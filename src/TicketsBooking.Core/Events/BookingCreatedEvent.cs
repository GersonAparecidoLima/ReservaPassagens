namespace TicketsBooking.Core.Events
{
    // Usamos 'record' por ser imutável e perfeito para transferência de dados
    public record BookingCreatedEvent
    {
        public Guid BookingId { get; init; }
        public Guid TripId { get; init; }
        public string SeatNumber { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public string PassengerName { get; init; } = string.Empty;
        public string PassengerDocument { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}