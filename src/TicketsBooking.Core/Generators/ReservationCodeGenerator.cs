namespace TicketsBooking.Core.Generators
{
    public static class ReservationCodeGenerator
    {
        private static readonly Random _random = new();
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        // Gera código no formato ABC-12345
        public static string Generate()
        {
            var letters = new string(Enumerable.Range(0, 3)
                .Select(_ => Letters[_random.Next(Letters.Length)])
                .ToArray());

            var digits = _random.Next(10000, 99999).ToString();

            return $"{letters}-{digits}";
        }
    }
}
