using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileVehiclePromotionMap : IEntityTypeConfiguration<MobileVehiclePromotion>
    {
        public void Configure(EntityTypeBuilder<MobileVehiclePromotion> builder)
        {
            builder.ToTable("MOBILEVEHICLEPROMOTIONS", "dbo");

            builder.HasKey(vp => vp.Id);

            builder.Property(vp => vp.Id).IsRequired().UseIdentityColumn();
            builder.Property(vp => vp.Order);
            builder.Property(vp => vp.Code).IsRequired();
            builder.Property(vp => vp.Text).IsRequired();
            builder.Property(vp => vp.Active).HasDefaultValue(true);
        }
    }
}
