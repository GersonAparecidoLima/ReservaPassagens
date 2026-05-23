using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketsBooking.Core.Entities;

namespace TicketsBooking.Infrastructure.Data.Mappings
{
    public class SeatMapping : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.ToTable("Seats");
            builder.HasKey(s => s.Id);

            builder.Property(s => s.SeatNumber)
                .IsRequired()
                .HasMaxLength(10);

            // Configuração do Token de Concorrência Otimista
            builder.Property(s => s.RowVersion)
                .IsRowVersion();

            builder.HasOne(s => s.Trip)
                .WithMany(t => t.Seats)
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}