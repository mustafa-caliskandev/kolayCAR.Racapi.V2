using KolayCAR.Broker.API.Models.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileFindeksUrlMap : IEntityTypeConfiguration<MobileFindeksUrl>
    {
        public void Configure(EntityTypeBuilder<MobileFindeksUrl> builder)
        {
            builder.ToTable("MOBILEFINDEKSURL", "dbo");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(s => s.DriverLicenseDate).HasColumnName("DRIVERLICENSEDATE");
            builder.Property(s => s.ReservationToken).HasColumnName("RESERVATIONTOKEN");
            builder.Property(s => s.BirthDate).HasColumnName("BIRTHDATE");
            builder.Property(s => s.UniqueId).HasColumnName("UNIQUEID");
            builder.Property(s => s.Tckn).HasColumnName("TCKN");
            builder.Property(s => s.VendorId).HasColumnName("VENDORID");
        }
    }
}
