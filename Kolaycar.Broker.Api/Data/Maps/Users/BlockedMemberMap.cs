using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Users
{
    public class BlockedMemberMap : IEntityTypeConfiguration<BlockedMember>
    {
        public void Configure(EntityTypeBuilder<BlockedMember> builder)
        {
            builder.ToTable("BLOCKEDMEMBERS", "dbo");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(b => b.MemberId).HasColumnName("MemberId").IsRequired();
            builder.Property(b => b.BlockedStartDate).HasColumnName("BlockedStartDate").HasDefaultValueSql("GETDATE()");
            builder.Property(b => b.BlockedEndDate).HasColumnName("BlockedEndDate");
        }
    }
}
