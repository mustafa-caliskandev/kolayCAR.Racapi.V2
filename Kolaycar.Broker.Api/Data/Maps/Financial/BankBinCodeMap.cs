using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.Financial
{
    public class BankBinCodeMap : IEntityTypeConfiguration<BankBinCode>
    {
        public void Configure(EntityTypeBuilder<BankBinCode> builder)
        {
            builder.ToTable("BANKBINCODES", "dbo");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(b => b.BinStart).HasColumnName("BinStart").IsRequired();
            builder.Property(b => b.BinEnd).HasColumnName("BinEnd").IsRequired();
            builder.Property(b => b.Schema).HasColumnName("BinSchema").IsRequired().HasMaxLength(20);
            builder.Property(b => b.Type).HasColumnName("Type").IsRequired().HasMaxLength(20);
            builder.Property(b => b.SubType).HasColumnName("SubType").HasMaxLength(20);
            builder.Property(b => b.BankCountry).HasColumnName("BankCountry").HasMaxLength(50);
            builder.Property(b => b.BankName).HasColumnName("BankName").HasMaxLength(250);
            builder.Property(b => b.BrandName).HasColumnName("BrandName").HasMaxLength(250);
            builder.Property(b => b.Prepaid).HasColumnName("Prepaid").HasDefaultValue(false);
            builder.Property(b => b.InstallmentSupported).HasColumnName("InstallmentSupported").HasDefaultValue(true);
        }
    }
}
