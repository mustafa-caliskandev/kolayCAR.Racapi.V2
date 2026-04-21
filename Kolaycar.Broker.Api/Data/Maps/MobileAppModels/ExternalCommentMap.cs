using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.MobileAppModels
{
    public class ExternalCommentMap : IEntityTypeConfiguration<ExternalComment>
    {
        public void Configure(EntityTypeBuilder<ExternalComment> builder)
        {
            builder.ToTable("EXTERNALCOMMENTS", "dbo");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(e => e.LocationId).HasColumnName("LocationId").IsRequired();
            builder.Property(e => e.VendorId).HasColumnName("VendorId").IsRequired();
            builder.Property(e => e.LanguageId).HasColumnName("LanguageId").IsRequired();

            builder.Property(e => e.Score).HasColumnName("Score").HasColumnType("DECIMAL(18, 2)").HasDefaultValue(0M);

            builder.Property(e => e.CustomerName).HasColumnName("CustomerName");
            builder.Property(e => e.CustomerSurname).HasColumnName("CustomerSurname");
            builder.Property(e => e.Comment).HasColumnName("Comment");
            builder.Property(e => e.Source).HasColumnName("Source").HasMaxLength(100);

            builder.Property(e => e.CommentDate).HasColumnName("CommentDate").HasDefaultValueSql("(GETDATE())");

            builder.Property(e => e.ShowOnWebsite).HasColumnName("ShowOnWebsite").HasDefaultValue(false);
        }
    }
}
