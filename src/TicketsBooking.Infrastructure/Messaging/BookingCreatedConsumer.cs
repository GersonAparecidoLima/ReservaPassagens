using MassTransit;
using Microsoft.Extensions.Logging;
using TicketsBooking.Core.Events;

namespace TicketsBooking.Infrastructure.Messaging
{
    public class BookingCreatedConsumer : IConsumer<BookingCreatedEvent>
    {
        private readonly ILogger<BookingCreatedConsumer> _logger;

        public BookingCreatedConsumer(ILogger<BookingCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<BookingCreatedEvent> context)
        {
            var message = context.Message;

            _logger.LogInformation("--- [Mensageria] Mensagem recebida da fila ---");
            _logger.LogInformation("Processando pagamento para a Reserva: {BookingId}", message.BookingId);
            _logger.LogInformation("Passageiro: {PassengerName} | Valor: R$ {Amount}", message.PassengerName, message.Amount);

            // Simula a comunicação com o Gateway de Pagamento (Stripe, Cielo, etc.)
            await Task.Delay(2000);

            _logger.LogInformation("+++ [Mensageria] Pagamento Processado com Sucesso para a Reserva: {BookingId} +++", message.BookingId);

            // Aqui, em um cenário real, dispararíamos outro evento: 'PaymentConfirmedEvent'
        }
    }
}