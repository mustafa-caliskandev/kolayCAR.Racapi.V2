using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileAppSettingsMap : IEntityTypeConfiguration<MobileAppSetting>
    {
        public void Configure(EntityTypeBuilder<MobileAppSetting> builder)
        {
            builder.ToTable("MOBILEAPPSETTINGS", "dbo");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(s => s.Parameter).HasColumnName("PARAMETER");
            builder.Property(s => s.Value).HasColumnName("VALUE");
            builder.Property(s => s.IconPath).HasColumnName("ICONPATH");
            builder.Property(s => s.Order).HasColumnName("ORDER");
            builder.Property(s => s.LanguageId).HasColumnName("LANGUAGEID");
            builder.Property(s => s.Type).HasColumnName("TYPE");
        }
    }
}
