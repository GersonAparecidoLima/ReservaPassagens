using Microsoft.EntityFrameworkCore;
using TicketsBooking.Core.Entities;

namespace TicketsBooking.Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Trip> Trips => Set<Trip>();
        public DbSet<Seat> Seats => Set<Seat>();
        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Aplica as configurações separadas da pasta Mappings
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            base.OnModelCreating(modelBuilder);

            // 2. Configuração do decimal (deixe aqui se não houver um arquivo TripMapping)
            modelBuilder.Entity<Trip>()
                .Property(t => t.Price)
                .HasColumnType("decimal(18,2)");

            // Quebra o ciclo de cascata entre Booking e Trip
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings)
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento Trip -> Route (FK nullable: RouteId pode ser null em dados antigos)
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Route)
                .WithMany(r => r.Trips)
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Índice único filtrado: só exige unicidade quando ReservationCode não é vazio
            // Isso protege registros legados que ainda não possuem código gerado
            modelBuilder.Entity<Booking>()
                .HasIndex(b => b.ReservationCode)
                .IsUnique()
                .HasFilter("[ReservationCode] <> ''");
        }
    }
}