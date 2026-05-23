namespace TicketsBooking.Application.DTOs
{
    public class BookingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}