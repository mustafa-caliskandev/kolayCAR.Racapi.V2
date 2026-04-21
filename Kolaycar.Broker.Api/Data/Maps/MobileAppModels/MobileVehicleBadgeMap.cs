using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehicleBadgeMap : IEntityTypeConfiguration<MobileVehicleBadge>
    {
        public void Configure(EntityTypeBuilder<MobileVehicleBadge> builder)
        {
            builder.ToTable("MOBILEVEHICLEBADGES", "dbo");

            builder.HasKey(vb => vb.Id);

            builder.Property(vb => vb.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(vb => vb.Order).HasColumnName("ORDER").IsRequired();
            builder.Property(vb => vb.LanguageId).HasColumnName("LANGUAGEID");
            builder.Property(vb => vb.ConditionId).HasColumnName("CONDITIONID");
            builder.Property(vb => vb.Text).HasColumnName("TEXT").IsRequired();
            builder.Property(vb => vb.BorderColor).HasColumnName("BORDERCOLOR");
            builder.Property(vb => vb.BackgroundColor).HasColumnName("BACKGROUNDCOLOR");
            builder.Property(vb => vb.TextColor).HasColumnName("TEXTCOLOR");
            builder.Property(vb => vb.IconPath).HasColumnName("ICONPATH");
            builder.Property(vb => vb.Active).HasColumnName("ACTIVE").HasDefaultValue(true);

        }
    }
}
