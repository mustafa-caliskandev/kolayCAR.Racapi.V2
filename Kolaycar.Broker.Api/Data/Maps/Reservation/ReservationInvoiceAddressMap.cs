using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Reservation
{
    public class ReservationInvoiceAddressMap : IEntityTypeConfiguration<ReservationInvoiceAddress>
    {
        public void Configure(EntityTypeBuilder<ReservationInvoiceAddress> builder)
        {
            builder.ToTable("RESERVATIONINVOICEADDRESSES", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(r => r.ReservationDetailId).HasColumnName("ReservationDetailId").IsRequired();

            builder.Property(r => r.Address).HasColumnName("Address");
            builder.Property(r => r.Title).HasColumnName("Title");
            builder.Property(r => r.TaxOffice).HasColumnName("TaxOffice");
            builder.Property(r => r.TaxNumber).HasColumnName("TaxNumber");
            builder.Property(r => r.Country).HasColumnName("Country");
            builder.Property(r => r.City).HasColumnName("City");
            builder.Property(r => r.District).HasColumnName("District");
            builder.Property(r => r.ZipCode).HasColumnName("ZipCode");

            builder.HasOne(r => r.ReservationDetail).WithMany(rd => rd.ReservationInvoiceAddresses).HasForeignKey(r => r.ReservationDetailId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
