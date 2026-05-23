using StackExchange.Redis;
using TicketsBooking.Core.Interfaces;

namespace TicketsBooking.Infrastructure.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis)
        {
            // Obtém a referência ao banco de dados em memória do Redis
            _database = redis.GetDatabase();
        }

        public async Task<bool> AcquireSeatLockAsync(Guid tripId, string seatNumber, string passengerId, TimeSpan expiration)
        {
            // Criamos uma chave única composta para o assento específico daquela viagem
            // Exemplo de chave resultante: "lock:trip:4f8d22...:seat:12A"
            string lockKey = $"lock:trip:{tripId}:seat:{seatNumber}";

            // Usamos o 'When.NotExists' para garantir a atomicidade. 
            // Só grava se NINGUÉM tiver travado essa poltrona antes.
            return await _database.StringSetAsync(lockKey, passengerId, expiration, When.NotExists);
        }

        public async Task ReleaseSeatLockAsync(Guid tripId, string seatNumber)
        {
            string lockKey = $"lock:trip:{tripId}:seat:{seatNumber}";
            await _database.KeyDeleteAsync(lockKey);
        }

        public async Task<string?> GetSeatLockStatusAsync(Guid tripId, string seatNumber)
        {
            string lockKey = $"lock:trip:{tripId}:seat:{seatNumber}";
            return await _database.StringGetAsync(lockKey);
        }
    }
}