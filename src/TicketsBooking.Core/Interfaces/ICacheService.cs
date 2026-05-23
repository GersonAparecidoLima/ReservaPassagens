namespace TicketsBooking.Core.Interfaces
{
    public interface ICacheService
    {
        // Tenta reservar um assento no Redis por um tempo determinado (Lock)
        Task<bool> AcquireSeatLockAsync(Guid tripId, string seatNumber, string passengerId, TimeSpan expiration);

        // Libera o assento manualmente caso o usuário cancele o fluxo
        Task ReleaseSeatLockAsync(Guid tripId, string seatNumber);

        // Verifica o status atual do assento no cache
        Task<string?> GetSeatLockStatusAsync(Guid tripId, string seatNumber);
    }
}