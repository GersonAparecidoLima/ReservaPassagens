using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBooking.Infrastructure.Data.Context;
using TicketsBooking.Core.Entities;

namespace TicketsBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("seed")]
        public async Task<IActionResult> SeedDatabase()
        {
            // 1. Verifica se já existem viagens para não duplicar dados
            if (await _dbContext.Trips.AnyAsync())
            {
                return Ok(new { message = "O banco de dados já possui dados de teste carregados." });
            }

            var routeId = Guid.Parse("aaaaaaaa-1111-2222-3333-bbbbbbbbbbbb");

            var route = new TicketsBooking.Core.Entities.Route
            {
                Id = routeId,
                Name = "São Paulo x Rio de Janeiro",
                DepartureCity = "São Paulo",
                ArrivalCity = "Rio de Janeiro"
            };


            // 2. Instancia uma viagem de teste com ID fixo e conhecido
            var tripId = Guid.Parse("9b85f3b2-2222-5555-9999-111111111111");
            var trip = new Trip
            {
                Id = tripId,
                RouteId = routeId,
                Route = route,
                VehicleNumber = "Bus-JCA-2026",
                DeparturePlace = "São Paulo - Tietê",
                ArrivalPlace = "Rio de Janeiro - Novo Rio",
                DepartureTime = DateTime.UtcNow.AddDays(1),
                ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(6),
                Price = 450.00m,
                Seats = new List<Seat>()
            };

            // 3. Cria uma lista de assentos para essa viagem (incluindo o 14C do exemplo)
            var seatNumbers = new[] { "12A", "12B", "14C", "14D", "15A", "15B" };
            foreach (var num in seatNumbers)
            {
                trip.Seats.Add(new Seat
                {
                    Id = Guid.NewGuid(),
                    TripId = tripId,
                    SeatNumber = num,
                    IsReserved = false
                });
            }

            // 4. Salva no banco de dados via EF Core
            await _dbContext.Routes.AddAsync(route);
            await _dbContext.Trips.AddAsync(trip);
            await _dbContext.SaveChangesAsync();

            return Created("", new
            {
                message = "Dados de teste inicializados com sucesso via API!",
                tripId = tripId,
                availableSeats = seatNumbers
            });
        }
    }
}