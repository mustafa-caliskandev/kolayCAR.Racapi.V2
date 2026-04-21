using KolayCAR.Broker.API.Models.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehicleSortingOptionMap : IEntityTypeConfiguration<MobileVehicleSortingOption>
    {
        public void Configure(EntityTypeBuilder<MobileVehicleSortingOption> builder)
        {
            builder.ToTable("MOBILEVEHICLESORTINGOPTIONS", "dbo");

            builder.HasKey(so => so.Id);

            builder.Property(so => so.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(so => so.Order).HasColumnName("ORDER").IsRequired();
            builder.Property(so => so.LanguageId).HasColumnName("LANGUAGEID").IsRequired();
            builder.Property(so => so.SortType).HasColumnName("SORTTYPE").IsRequired();
            builder.Property(so => so.IconPath).HasColumnName("ICONPATH");
            builder.Property(so => so.ValueName).HasColumnName("VALUENAME").IsRequired();
            builder.Property(so => so.Name).HasColumnName("NAME").IsRequired();
            builder.Property(so => so.Default).HasColumnName("DEFAULT").HasDefaultValue(false);
            builder.Property(so => so.Active).HasColumnName("ACTIVE").HasDefaultValue(true);
        }
    }
}
