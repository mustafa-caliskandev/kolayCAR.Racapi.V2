using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehicleListFilterMap : IEntityTypeConfiguration<MobileVehicleListFilter>
    {
        public void Configure(EntityTypeBuilder<MobileVehicleListFilter> builder)
        {
            builder.ToTable("MOBILEVEHICLELISTFILTERS", "dbo");

            builder.HasKey(vf => vf.Id);

            builder.Property(vf => vf.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(vf => vf.Type).HasColumnName("TYPE").IsRequired();
            builder.Property(vf => vf.LanguageId).HasColumnName("LANGUAGEID").IsRequired();
            builder.Property(vf => vf.Order).HasColumnName("ORDER").IsRequired();
            builder.Property(vf => vf.IconPath).HasColumnName("ICONPATH").IsRequired();
            builder.Property(vf => vf.Header).HasColumnName("HEADER").IsRequired();
            builder.Property(vf => vf.Active).HasColumnName("ACTIVE").HasDefaultValue(true);
        }
    }
}
