using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text.RegularExpressions;
using TicketsBooking.Application.Services;
using TicketsBooking.Core.Entities;
using TicketsBooking.Core.Events;
using TicketsBooking.Core.Interfaces;
using TicketsBooking.Infrastructure.Data.Context;

namespace TicketsBooking.Application.Tests;

public class BookingApplicationServiceTests
{
    private const string ValidCpf = "529.982.247-25";
    private static readonly Regex CodeFormat = new(@"^[A-Z]{3}-\d{5}$");

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static BookingApplicationService CreateService(
        ApplicationDbContext db,
        bool cacheLock = true)
    {
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.AcquireSeatLockAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<TimeSpan>()))
            .ReturnsAsync(cacheLock);

        var publish = new Mock<IPublishEndpoint>();
        publish.Setup(p => p.Publish(
                It.IsAny<BookingCreatedEvent>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new BookingApplicationService(cache.Object, publish.Object, db);
    }

    [Fact]
    public async Task CreateBookingAsync_SeatAlreadyReserved_ReturnsFalse()
    {
        await using var db = CreateDbContext();

        var tripId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        db.Trips.Add(new Trip
        {
            Id = tripId,
            DeparturePlace = "SP",
            ArrivalPlace = "RJ",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(6),
            Price = 100
        });

        db.Seats.Add(new Seat
        {
            Id = seatId,
            TripId = tripId,
            SeatNumber = "10A",
            IsReserved = true,
            RowVersion = new byte[] { 1 }
        });

        await db.SaveChangesAsync();

        var service = CreateService(db);

        var result = await service.CreateBookingAsync(
            tripId, "10A", "Passageiro", ValidCpf, 100);

        Assert.False(result.Success);
        Assert.Contains("Assento já reservado", result.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_TripAlreadyDeparted_ReturnsFalse()
    {
        await using var db = CreateDbContext();

        var tripId = Guid.NewGuid();

        db.Trips.Add(new Trip
        {
            Id = tripId,
            DeparturePlace = "SP",
            ArrivalPlace = "RJ",
            DepartureTime = DateTime.UtcNow.AddHours(-1),
            ArrivalTime = DateTime.UtcNow.AddHours(5),
            Price = 100
        });

        db.Seats.Add(new Seat
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            SeatNumber = "10A",
            IsReserved = false,
            RowVersion = new byte[] { 1 }
        });

        await db.SaveChangesAsync();

        var service = CreateService(db);

        var result = await service.CreateBookingAsync(
            tripId, "10A", "Passageiro", ValidCpf, 100);

        Assert.False(result.Success);
        Assert.Contains("já ocorreu", result.Message);
    }

    [Fact]
    public async Task CreateBookingAsync_Success_ReturnsReservationCode()
    {
        await using var db = CreateDbContext();

        var tripId = Guid.NewGuid();

        db.Trips.Add(new Trip
        {
            Id = tripId,
            DeparturePlace = "SP",
            ArrivalPlace = "RJ",
            DepartureTime = DateTime.UtcNow.AddDays(1),
            ArrivalTime = DateTime.UtcNow.AddDays(1).AddHours(6),
            Price = 100
        });

        db.Seats.Add(new Seat
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            SeatNumber = "10A",
            IsReserved = false,
            RowVersion = new byte[] { 1 }
        });

        await db.SaveChangesAsync();

        var service = CreateService(db);

        var result = await service.CreateBookingAsync(
            tripId, "10A", "Passageiro", ValidCpf, 100);

        Assert.True(result.Success);
        Assert.NotNull(result.ReservationCode);
        Assert.Matches(CodeFormat, result.ReservationCode);
    }

    [Fact]
    public async Task CancelBookingAsync_DepartureInLessThan2Hours_ReturnsFalse()
    {
        await using var db = CreateDbContext();

        var tripId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        db.Trips.Add(new Trip
        {
            Id = tripId,
            DeparturePlace = "SP",
            ArrivalPlace = "RJ",
            DepartureTime = DateTime.UtcNow.AddMinutes(60),
            ArrivalTime = DateTime.UtcNow.AddHours(7),
            Price = 100
        });

        db.Seats.Add(new Seat
        {
            Id = seatId,
            TripId = tripId,
            SeatNumber = "10A",
            IsReserved = true,
            RowVersion = new byte[] { 1 }
        });

        db.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            SeatId = seatId,
            PassengerName = "Passageiro",
            PassengerDocument = ValidCpf,
            ReservationCode = "TST-00001",
            Status = BookingStatus.Confirmed
        });

        await db.SaveChangesAsync();

        var service = CreateService(db);

        var result = await service.CancelBookingAsync("TST-00001");

        Assert.False(result.Success);
        Assert.Contains("menos de 2 horas", result.Message);
    }

    [Fact]
    public async Task CancelBookingAsync_DepartureInMoreThan2Hours_CancelsSuccessfully()
    {
        await using var db = CreateDbContext();

        var tripId = Guid.NewGuid();
        var seatId = Guid.NewGuid();

        db.Trips.Add(new Trip
        {
            Id = tripId,
            DeparturePlace = "SP",
            ArrivalPlace = "RJ",
            DepartureTime = DateTime.UtcNow.AddHours(4),
            ArrivalTime = DateTime.UtcNow.AddHours(10),
            Price = 100
        });

        db.Seats.Add(new Seat
        {
            Id = seatId,
            TripId = tripId,
            SeatNumber = "10A",
            IsReserved = true,
            RowVersion = new byte[] { 1 }
        });

        db.Bookings.Add(new Booking
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            SeatId = seatId,
            PassengerName = "Passageiro",
            PassengerDocument = ValidCpf,
            ReservationCode = "TST-00002",
            Status = BookingStatus.Confirmed
        });

        await db.SaveChangesAsync();

        var service = CreateService(db);

        var result = await service.CancelBookingAsync("TST-00002");

        var booking = await db.Bookings.FirstAsync(b => b.ReservationCode == "TST-00002");
        var seat = await db.Seats.FirstAsync(s => s.Id == seatId);

        Assert.True(result.Success);
        Assert.Equal(BookingStatus.Canceled, booking.Status);
        Assert.False(seat.IsReserved);
    }
}