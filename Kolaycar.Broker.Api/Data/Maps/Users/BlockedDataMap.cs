using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Users
{
    public class BlockedDataMap : IEntityTypeConfiguration<BlockedData>
    {
        public void Configure(EntityTypeBuilder<BlockedData> builder)
        {
            builder.ToTable("BLOCKEDDATAS", "dbo");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(b => b.Email).HasColumnName("Email");
            builder.Property(b => b.PassportNumber).HasColumnName("PassportNumber");
            builder.Property(b => b.OtherInfos).HasColumnName("OtherInfos");
        }
    }
}
