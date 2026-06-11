namespace TicketsBooking.Core.Entities
{
    public class Route
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DepartureCity { get; set; } = string.Empty;
        public string ArrivalCity { get; set; } = string.Empty;

        public ICollection<Trip> Trips { get; set; } = new List<Trip>();
    }
}
