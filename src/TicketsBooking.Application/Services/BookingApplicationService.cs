using TicketsBooking.Core.Interfaces;

namespace TicketsBooking.Application.Services
{
    public class BookingApplicationService
    {
        private readonly ICacheService _cacheService;

        public BookingApplicationService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task<BookingResultDto> ReserveSeatTemporarilyAsync(Guid tripId, string seatNumber, string passengerId)
        {
            // Definimos o tempo regulamentar de retenção da poltrona (10 minutos)
            var expirationTime = TimeSpan.FromMinutes(10);

            // Tenta obter o lock atômico no Redis
            bool isLockAcquired = await _cacheService.AcquireSeatLockAsync(tripId, seatNumber, passengerId, expirationTime);

            if (!isLockAcquired)
            {
                return new BookingResultDto
                {
                    Success = false,
                    Message = "Este assento já está reservado por outro usuário ou já foi vendido."
                };
            }

            return new BookingResultDto
            {
                Success = true,
                Message = $"Assento reservado com sucesso! Você tem {expirationTime.Minutes} minutos para concluir o pagamento.",
                ExpiresAt = DateTime.UtcNow.Add(expirationTime)
            };
        }
    }

    // DTO auxiliar para responder à API
    public class BookingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}