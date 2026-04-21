using KolayCAR.Broker.API.Models.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehicleListFastFilterMap : IEntityTypeConfiguration<MobileVehicleListFastFilter>
    {
        public void Configure(EntityTypeBuilder<MobileVehicleListFastFilter> builder)
        {
            builder.ToTable("MOBILEVEHICLEFASTFILTERS", "dbo");

            builder.HasKey(vf => vf.Id);

            builder.Property(vf => vf.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(vf => vf.Type).HasColumnName("TYPE").IsRequired();
            builder.Property(vf => vf.LanguageId).HasColumnName("LANGUAGEID").IsRequired();
            builder.Property(vf => vf.Order).HasColumnName("ORDER").IsRequired();
            builder.Property(vf => vf.Value).HasColumnName("VALUE").IsRequired();
            builder.Property(vf => vf.Active).HasColumnName("ACTIVE").HasDefaultValue(true);
        }
    }
}
