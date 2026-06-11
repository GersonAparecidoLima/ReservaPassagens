using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TicketsBooking.Core.Entities;

namespace TicketsBooking.Infrastructure.Data.Mappings
{
    public class RouteMapping : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.ToTable("Routes");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.DepartureCity)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(r => r.ArrivalCity)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
