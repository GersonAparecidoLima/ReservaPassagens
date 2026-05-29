using MassTransit;
using Microsoft.EntityFrameworkCore; // Adicionado para o FirstOrDefaultAsync
using TicketsBooking.Core.Events;
using TicketsBooking.Core.Interfaces;
using TicketsBooking.Application.DTOs;
using TicketsBooking.Infrastructure.Data.Context; // Adicionado para o ApplicationDbContext

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
    }
}