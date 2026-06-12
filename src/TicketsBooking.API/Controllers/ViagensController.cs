using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.API.Controllers
{
    [ApiController]
    [Route("viagens")]
    public class ViagensController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public ViagensController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? origem,
            [FromQuery] string? destino,
            [FromQuery] DateTime? data)
        {
            var query = _dbContext.Trips
                .Include(t => t.Seats)
                .Include(t => t.Route)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(origem))
            {
                query = query.Where(t =>
                    (t.Route != null ? t.Route.DepartureCity : t.DeparturePlace)
                    .Contains(origem));
            }

            if (!string.IsNullOrWhiteSpace(destino))
            {
                query = query.Where(t =>
                    (t.Route != null ? t.Route.ArrivalCity : t.ArrivalPlace)
                    .Contains(destino));
            }

            if (data.HasValue)
            {
                query = query.Where(t => t.DepartureTime.Date == data.Value.Date);
            }

            var viagens = await query
                .Select(t => new
                {
                    id = t.Id,
                    origem = t.Route != null ? t.Route.DepartureCity : t.DeparturePlace,
                    destino = t.Route != null ? t.Route.ArrivalCity : t.ArrivalPlace,
                    partida = t.DepartureTime,
                    chegada = t.ArrivalTime,
                    preco = t.Price,
                    assentosDisponiveis = t.Seats.Count(s => !s.IsReserved)
                })
                .ToListAsync();

            return Ok(viagens);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var viagem = await _dbContext.Trips
                .Include(t => t.Seats)
                .Include(t => t.Route)
                .AsNoTracking()
                .Where(t => t.Id == id)
                .Select(t => new
                {
                    id = t.Id,
                    origem = t.Route != null ? t.Route.DepartureCity : t.DeparturePlace,
                    destino = t.Route != null ? t.Route.ArrivalCity : t.ArrivalPlace,
                    partida = t.DepartureTime,
                    chegada = t.ArrivalTime,
                    preco = t.Price,
                    assentos = t.Seats.Select(s => new
                    {
                        numero = s.SeatNumber,
                        disponivel = !s.IsReserved
                    })
                })
                .FirstOrDefaultAsync();

            if (viagem == null)
                return NotFound(new { message = $"Viagem com ID {id} não encontrada." });

            return Ok(viagem);
        }
    }
}