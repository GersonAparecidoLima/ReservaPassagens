using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketsBooking.Core.Entities;
using TicketsBooking.Core.Events;
using TicketsBooking.Infrastructure.Data;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.Infrastructure.Messaging
{
    public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly ILogger<BookingCreatedConsumer> _logger;
        private readonly ApplicationDbContext _context;

        public BookingCreatedConsumer(
            ILogger<BookingCreatedConsumer> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("--- [Mensageria] Mensagem recebida da fila ---");
            _logger.LogInformation("Processando pagamento para a Reserva: {BookingId}", message.BookingId);

            // Simula gateway de pagamento
            await Task.Delay(2000);

            // Busca o assento
            var seat = await _context.Seats
                .FirstOrDefaultAsync(s =>
                    s.TripId == message.TripId &&
                    s.SeatNumber == message.SeatNumber);

            if (seat == null)
            {
                _logger.LogWarning("Assento não encontrado.");
                return;
            }

            // Cria a reserva
            var booking = new Booking
            {
                Id = message.BookingId,
                TripId = message.TripId,
                SeatId = seat.Id,
                PassengerName = message.PassengerName,
                PassengerDocument = message.PassengerDocument,
                CreatedAt = DateTime.UtcNow,
                Status = BookingStatus.Confirmed
            };

            // Salva no banco
            _context.Bookings.Add(booking);

            // Marca assento como reservado
            seat.IsReserved = true;

            await _context.SaveChangesAsync();

            _logger.LogInformation("+++ Reserva salva com sucesso no banco! +++");
        }
    }
}