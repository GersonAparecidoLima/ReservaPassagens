using Microsoft.EntityFrameworkCore;
using TicketsBooking.Core.Entities;

namespace TicketsBooking.Infrastructure.Data.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

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

            // Adicione esta configuração para quebrar o ciclo de cascata:
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Trip)
                .WithMany(t => t.Bookings) // <-- Adicionei t.Bookings aqui dentro para fechar o circuito
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict); // <-- ISSO AQUI RESOLVE O ERRO
        }
    }
}