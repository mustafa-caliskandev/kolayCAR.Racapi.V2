using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehicleFeatureMap : IEntityTypeConfiguration<MobileVehicleFeature>
    {
        public void Configure(EntityTypeBuilder<MobileVehicleFeature> builder)
        {
            builder.ToTable("MOBILEVEHICLEFEATURES", "dbo");

            builder.HasKey(vf => vf.Id);

            builder.Property(vf => vf.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(vf => vf.Type).HasColumnName("TYPE");
            builder.Property(vf => vf.Order).HasColumnName("ORDER");
            builder.Property(vf => vf.IconPath).HasColumnName("ICEONPATH");
            builder.Property(vf => vf.Value).HasColumnName("VALUE");
            builder.Property(vf => vf.Text).HasColumnName("TEXT");
            builder.Property(vf => vf.Active).HasColumnName("ACTIVE").HasDefaultValue(true);
        }
    }
}
