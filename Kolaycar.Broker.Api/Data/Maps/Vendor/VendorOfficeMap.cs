
using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Vendor
{
    public class VendorOfficeMap : IEntityTypeConfiguration<VendorOffice>
    {
        public void Configure(EntityTypeBuilder<VendorOffice> builder)
        {
            builder.ToTable("VENDOROFFICES", "dbo");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Id).HasColumnName("Id").UseIdentityColumn();
            builder.Property(v => v.LocationId).HasColumnName("LocationId").IsRequired();
            builder.Property(v => v.VendorId).HasColumnName("VendorId").IsRequired();
            builder.Property(v => v.OpeningTime).HasColumnName("OpeningTime");
            builder.Property(v => v.ClosingTime).HasColumnName("ClosingTime");
            builder.Property(v => v.FlightCardRequired).HasColumnName("FlightCardRequired").HasDefaultValue(false);
            builder.Property(v => v.ReservationsCancellable).HasColumnName("ReservationsCancellable").HasDefaultValue(true);
        }
    }
}
