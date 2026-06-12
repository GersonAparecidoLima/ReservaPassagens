using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.API.Controllers
{
    [ApiController]
    [Route("rotas")]
    public class RotasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public RotasController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rotas = await _dbContext.Routes
                .AsNoTracking()
                .Select(r => new
                {
                    id = r.Id,
                    nome = r.Name,
                    cidadeOrigem = r.DepartureCity,
                    cidadeDestino = r.ArrivalCity
                })
                .ToListAsync();

            return Ok(rotas);
        }
    }
}
