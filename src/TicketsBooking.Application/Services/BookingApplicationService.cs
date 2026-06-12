using MassTransit;
using Microsoft.EntityFrameworkCore;
using TicketsBooking.Core.Entities;
using TicketsBooking.Core.Events;
using TicketsBooking.Core.Generators;
using TicketsBooking.Core.Interfaces;
using TicketsBooking.Core.Validators;
using TicketsBooking.Application.DTOs;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.Application.Services
{
    public class BookingApplicationService
    {
        private readonly ICacheService _cacheService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ApplicationDbContext _dbContext; // Injetado para gerenciar o banco de dados

        public BookingApplicationService(
            ICacheService cacheService,
            IPublishEndpoint publishEndpoint,
            ApplicationDbContext dbContext) // Adicionado no construtor
        {
            _cacheService = cacheService;
            _publishEndpoint = publishEndpoint;
            _dbContext = dbContext;
        }

        public async Task<BookingResultDto> ConfirmBookingAndPayAsync(Guid tripId, string seatNumber, string passengerName, string passengerDocument, decimal price)
        {
            var passengerId = passengerDocument; // Usando o documento como identificador único temporário
            var expirationTime = TimeSpan.FromMinutes(10);

            // 1. Tenta pegar a trava no Redis para evitar overbooking
            bool isLockAcquired = await _cacheService.AcquireSeatLockAsync(tripId, seatNumber, passengerId, expirationTime);

            if (!isLockAcquired)
            {
                return new BookingResultDto
                {
                    Success = false,
                    Message = "Assento indisponível ou já reservado."
                };
            }

            // 2. Simula a criação do ID da reserva que iria para o banco relacional futuramente
            var bookingId = Guid.NewGuid();

            // 3. Publica o evento de forma ASSÍNCRONA no RabbitMQ
            await _publishEndpoint.Publish(new BookingCreatedEvent
            {
                BookingId = bookingId,
                TripId = tripId,
                SeatNumber = seatNumber,
                Amount = price,
                PassengerName = passengerName,
                PassengerDocument = passengerDocument,
                CreatedAt = DateTime.UtcNow
            });

            return new BookingResultDto
            {
                Success = true,
                Message = "Reserva garantida! Seu pagamento foi enviado para processamento na fila.",
                ExpiresAt = DateTime.UtcNow.Add(expirationTime)
            };
        }

        // --- NOVO MÉTODO ADICIONADO ---
        public async Task<bool> UpdatePassengerInfoAsync(
           Guid bookingId,
           string passengerName,
           string passengerDocument)
        {
            if (string.IsNullOrWhiteSpace(passengerName) ||
                string.IsNullOrWhiteSpace(passengerDocument))
            {
                return false;
            }

            var booking = await _dbContext.Bookings
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return false;

            booking.PassengerName = passengerName;
            booking.PassengerDocument = passengerDocument;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<BookingResultDto> CreateBookingAsync(
            Guid tripId, string seatNumber,
            string passengerName, string passengerDocument, decimal price)
        {
            if (!CpfValidator.IsValid(passengerDocument))
                return new BookingResultDto { Success = false, Message = "CPF inválido." };

            var seat = await _dbContext.Seats
                .Include(s => s.Trip)
                .FirstOrDefaultAsync(s => s.TripId == tripId && s.SeatNumber == seatNumber);

            if (seat == null)
                return new BookingResultDto { Success = false, Message = "Assento não encontrado." };

            if (seat.Trip!.DepartureTime <= DateTime.UtcNow)
                return new BookingResultDto { Success = false, Message = "Não é possível reservar: a viagem já ocorreu." };

            if (seat.IsReserved)
                return new BookingResultDto { Success = false, Message = "Assento já reservado." };

            var expirationTime = TimeSpan.FromMinutes(10);

            bool lockAcquired = await _cacheService.AcquireSeatLockAsync(tripId, seatNumber, passengerDocument, expirationTime);
            if (!lockAcquired)
                return new BookingResultDto { Success = false, Message = "Assento indisponível no momento. Tente novamente." };

            string reservationCode;
            int attempts = 0;
            do
            {
                reservationCode = ReservationCodeGenerator.Generate();
                if (++attempts > 5)
                    return new BookingResultDto { Success = false, Message = "Não foi possível gerar código único. Tente novamente." };
            }
            while (await _dbContext.Bookings.AnyAsync(b => b.ReservationCode == reservationCode));

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                SeatId = seat.Id,
                PassengerName = passengerName,
                PassengerDocument = passengerDocument,
                ReservationCode = reservationCode,
                Status = BookingStatus.Confirmed,
                CreatedAt = DateTime.UtcNow
            };

            seat.IsReserved = true;
            _dbContext.Bookings.Add(booking);
            await _dbContext.SaveChangesAsync();

            await _publishEndpoint.Publish(new BookingCreatedEvent
            {
                BookingId = booking.Id,
                TripId = tripId,
                SeatNumber = seatNumber,
                Amount = price,
                PassengerName = passengerName,
                PassengerDocument = passengerDocument,
                CreatedAt = booking.CreatedAt
            });

            return new BookingResultDto
            {
                Success = true,
                Message = "Reserva criada com sucesso.",
                ReservationCode = reservationCode,
                ExpiresAt = DateTime.UtcNow.Add(expirationTime)
            };
        }

        public async Task<BookingResultDto> CancelBookingAsync(string reservationCode)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Seat)
                .Include(b => b.Trip)
                .FirstOrDefaultAsync(b => b.ReservationCode == reservationCode);

            if (booking == null)
                return new BookingResultDto { Success = false, Message = "Reserva não encontrada." };

            if (booking.Status == BookingStatus.Canceled)
                return new BookingResultDto { Success = false, Message = "Reserva já está cancelada." };

            if (booking.Trip != null && booking.Trip.DepartureTime <= DateTime.UtcNow.AddHours(2))
                return new BookingResultDto { Success = false, Message = "Cancelamento não permitido: faltam menos de 2 horas para a partida." };

            booking.Status = BookingStatus.Canceled;

            if (booking.Seat != null)
                booking.Seat.IsReserved = false;

            await _dbContext.SaveChangesAsync();

            return new BookingResultDto
            {
                Success = true,
                Message = "Reserva cancelada com sucesso.",
                ReservationCode = booking.ReservationCode
            };
        }
    }
}