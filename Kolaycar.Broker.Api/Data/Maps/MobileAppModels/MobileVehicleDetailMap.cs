using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehicleDetailMap : IEntityTypeConfiguration<MobileVehicleDetail>
    {
        public void Configure(EntityTypeBuilder<MobileVehicleDetail> builder)
        {
            builder.ToTable("MOBILEVEHICLEDETAILS", "dbo");

            builder.HasKey(vd => vd.Id);

            builder.Property(vd => vd.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(vd => vd.Type).HasColumnName("TYPE");
            builder.Property(vd => vd.Order).HasColumnName("ORDER");
            builder.Property(vd => vd.IconPath).HasColumnName("ICEONPATH");
            builder.Property(vd => vd.Value).HasColumnName("VALUE");
            builder.Property(vd => vd.Text).HasColumnName("TEXT");
            builder.Property(vd => vd.Active).HasColumnName("ACTIVE").HasDefaultValue(true);
        }
    }
}
