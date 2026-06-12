using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBooking.Application.Services;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.API.Controllers
{
    [ApiController]
    [Route("reservas")]
    public class ReservasController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly BookingApplicationService _bookingService;

        public ReservasController(
            ApplicationDbContext dbContext,
            BookingApplicationService bookingService)
        {
            _dbContext = dbContext;
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservaRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Dados da requisição inválidos." });

            var result = await _bookingService.CreateBookingAsync(
                request.TripId,
                request.SeatNumber,
                request.PassengerName,
                request.PassengerDocument,
                request.Price);

            if (!result.Success)
                return Conflict(new { message = result.Message });

            return Created($"/reservas/{result.ReservationCode}", result);
        }

        [HttpGet("{codigo}")]
        public async Task<IActionResult> GetByCodigo(string codigo)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Trip)
                .Include(b => b.Seat)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.ReservationCode == codigo);

            if (booking == null)
                return NotFound(new { message = $"Reserva com código '{codigo}' não encontrada." });

            return Ok(new
            {
                id = booking.Id,
                codigoReserva = booking.ReservationCode,
                passageiro = booking.PassengerName,
                documento = booking.PassengerDocument,
                status = booking.Status.ToString(),
                viagem = booking.Trip == null ? null : new
                {
                    origem = booking.Trip.DeparturePlace,
                    destino = booking.Trip.ArrivalPlace,
                    partida = booking.Trip.DepartureTime
                },
                assento = booking.Seat?.SeatNumber
            });
        }

        [HttpDelete("{codigo}")]
        public async Task<IActionResult> Cancel(string codigo)
        {
            var result = await _bookingService.CancelBookingAsync(codigo);

            if (!result.Success)
            {
                var notFound = result.Message.Contains("não encontrada");
                return notFound
                    ? NotFound(new { message = result.Message })
                    : Conflict(new { message = result.Message });
            }

            return Ok(result);
        }
    }

    public record CreateReservaRequest(
        Guid TripId,
        string SeatNumber,
        string PassengerName,
        string PassengerDocument,
        string? PassengerEmail,
        decimal Price);
}