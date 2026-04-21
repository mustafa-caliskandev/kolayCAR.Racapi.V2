using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Location
{
    public class LocationVendorInstallmentMap : IEntityTypeConfiguration<LocationVendorInstallment>
    {
        public void Configure(EntityTypeBuilder<LocationVendorInstallment> builder)
        {
            builder.ToTable("LOCATIONVENDORINSTALLMENTS", "dbo");

            builder.HasKey(lv => lv.Id);

            builder.HasIndex(lv => new { lv.LocationId, lv.VendorId });

            builder.Property(lv => lv.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(lv => lv.LocationId).HasColumnName("LOCATIONID").IsRequired();
            builder.Property(lv => lv.VendorId).HasColumnName("VENDORID");
            builder.Property(lv => lv.MaxInstallmentCount).HasColumnName("MAXINSTALLMENTCOUNT").IsRequired();
        }
    }
}
