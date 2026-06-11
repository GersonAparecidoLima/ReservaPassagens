namespace TicketsBooking.Core.Entities
{
    public enum BookingStatus
    {
        PendingPayment = 1,
        Confirmed = 2,
        Canceled = 3
    }

    public class Booking
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public Guid SeatId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerDocument { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public BookingStatus Status { get; set; } = BookingStatus.PendingPayment;
        public string ReservationCode { get; set; } = string.Empty;

        // Propriedades de Navegação
        public Trip? Trip { get; set; }
        public Seat? Seat { get; set; }
    }
}