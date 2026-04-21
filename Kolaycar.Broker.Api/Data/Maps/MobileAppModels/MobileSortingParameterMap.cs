using KolayCAR.Broker.API.Models.MobileAppDtos.MobileAppModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class MobileSortingParameterMap : IEntityTypeConfiguration<MobileSortingParameter>
    {
        public void Configure(EntityTypeBuilder<MobileSortingParameter> builder)
        {
            builder.ToTable("MOBILESORTINGPARAMETERS", "dbo");

            builder.HasKey(sp => sp.Id);

            builder.Property(sp => sp.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(sp => sp.DataType).HasColumnName("DATATYPE").IsRequired();
            builder.Property(sp => sp.Name).HasColumnName("NAME").IsRequired();
            builder.Property(sp => sp.Value).HasColumnName("VALUE").IsRequired();
            builder.Property(sp => sp.Active).HasColumnName("ACTIVE").HasDefaultValue(true);
        }
    }
}
