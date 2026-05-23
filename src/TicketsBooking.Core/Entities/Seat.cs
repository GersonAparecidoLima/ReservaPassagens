namespace TicketsBooking.Core.Entities
{
    public class Seat
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public string SeatNumber { get; set; } = string.Empty; // Ex: "12A", "24"
        public bool IsReserved { get; set; }

        // Token de concorrência para evitar que dois comprem o mesmo assento
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        // Propriedades de Navegação
        public Trip? Trip { get; set; }
    }
}