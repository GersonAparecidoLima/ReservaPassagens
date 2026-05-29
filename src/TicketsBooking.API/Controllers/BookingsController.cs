using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketsBooking.Application.Services;
using TicketsBooking.Core.Entities;
using TicketsBooking.Core.Interfaces;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly BookingApplicationService _bookingService;
        private readonly ILogger<BookingsController> _logger; // Injetado para o log do Delete funcionar

        // Atualizado o construtor para receber o Logger
        public BookingsController(
            ApplicationDbContext dbContext,
            BookingApplicationService bookingService,
            ILogger<BookingsController> logger)
        {
            _dbContext = dbContext;
            _bookingService = bookingService;
            _logger = logger;
        }

        #region READ (Consultas - OK manter no Controller ou via Queries)

        [HttpGet("trips")]
        public async Task<IActionResult> GetAvailableTrips()
        {
            var trips = await _dbContext.Trips
                .Include(t => t.Seats)
                .AsNoTracking()
                .Select(t => new
                {
                    t.Id,
                    Origin = t.DeparturePlace,
                    Destination = t.ArrivalPlace,
                    t.DepartureTime,
                    t.Price,
                    AvailableSeats = t.Seats.Where(s => !s.IsReserved).Select(s => s.SeatNumber)
                })
                .ToListAsync();

            return Ok(trips);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _dbContext.Bookings.AsNoTracking().ToListAsync();
            return Ok(bookings);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetBookingById(Guid id)
        {
            var booking = await _dbContext.Bookings
                .Include(b => b.Trip)
                .Include(b => b.Seat)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null) return NotFound($"Reserva com ID {id} não encontrada.");

            return Ok(booking);
        }

        #endregion

        #region WRITE (Comandos delegados para o Application Service)

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.SeatNumber))
                return BadRequest("Dados da requisição inválidos.");

            var result = await _bookingService.ConfirmBookingAndPayAsync(
                request.TripId, request.SeatNumber, request.PassengerName, request.PassengerDocument, request.Price);

            if (!result.Success) return Conflict(new { message = result.Message });

            return Accepted(new { message = result.Message, expiresAt = result.ExpiresAt });
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingRequest request)
        {
            if (request == null) return BadRequest("Dados inválidos.");

            // O correto é delegar para o serviço que conhece as regras de validação de alteração
            var success = await _bookingService.UpdatePassengerInfoAsync(id, request.PassengerName, request.PassengerDocument);

            if (!success) return NotFound(new { Message = "Reserva não encontrada." });

            return Ok(new { Message = "Reserva atualizada com sucesso." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            // 1. Busca a reserva trazendo junto o Assento relacionado (usando Include) e usando o _dbContext correto
            var booking = await _dbContext.Bookings
                .Include(b => b.Seat)
                .FirstOrDefaultAsync(b => b.Id == id);

            // 2. Se a reserva não existir, retorna 404 Not Found
            if (booking == null)
            {
                return NotFound(new { message = $"Reserva com o ID {id} não encontrada." });
            }

            // 3. Regra Sênior: Se o assento associado existir no banco, libera ele!
            if (booking.Seat != null)
            {
                booking.Seat.IsReserved = false;
                _logger.LogInformation("Assento {SeatNumber} foi liberado com sucesso.", booking.Seat.SeatNumber);
            }

            // 4. Remove a reserva do banco
            _dbContext.Bookings.Remove(booking);

            // 5. Salva tudo em uma única transação atômica no SQL Server
            await _dbContext.SaveChangesAsync();

            // 6. Retorna 204 No Content (Padrão REST para remoções de sucesso)
            return NoContent();
        }

        #endregion
    }

    // DTOs auxiliares corrigidos
    public record CreateBookingRequest(Guid TripId, string SeatNumber, string PassengerName, string PassengerDocument, decimal Price);
    public record UpdateBookingRequest(string PassengerName, string PassengerDocument);
    public record UpdateStatusRequest(BookingStatus NewStatus);
}