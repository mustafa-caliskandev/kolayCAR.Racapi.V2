using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Reservation
{
    public class ReservationSelectedExtraMap : IEntityTypeConfiguration<ReservationSelectedExtra>
    {
        public void Configure(EntityTypeBuilder<ReservationSelectedExtra> builder)
        {
            builder.ToTable("RESERVATIONSELECTEDEXTRAS", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(r => r.ReservationDetailId).HasColumnName("ReservationDetailId").IsRequired();

            builder.Property(r => r.ExtraRentalType).HasColumnName("ExtraRentalType").IsRequired();
            builder.Property(r => r.RentalDuration).HasColumnName("RentalDuration").IsRequired();

            builder.Property(r => r.Name).HasColumnName("Name").IsRequired();

            builder.Property(r => r.Price).HasColumnName("Price").IsRequired().HasColumnType("DECIMAL(18, 2)");

            builder.HasOne(r => r.ReservationDetail).WithMany(rd => rd.ReservationSelectedExtras).HasForeignKey(r => r.ReservationDetailId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
