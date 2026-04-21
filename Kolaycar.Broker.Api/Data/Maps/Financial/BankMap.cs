using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.Financial
{
    public class BankMap : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.ToTable("BANKS", "dbo");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(b => b.BankId).HasColumnName("BankId").IsRequired();
            builder.Property(b => b.BankVendorId).HasColumnName("BankVendorId").IsRequired();

            builder.Property(b => b.BankName).HasColumnName("BankName").IsRequired();
            builder.Property(b => b.BankDefinition).HasColumnName("BankDefinition").IsRequired();
            builder.Property(b => b.Account).HasColumnName("Account").IsRequired();
            builder.Property(b => b.IBAN).HasColumnName("IBAN").IsRequired();
            builder.Property(b => b.ReturnColumnName).HasColumnName("ReturnColumnName").HasDefaultValue("PROVISIONNUMBER");

            builder.Property(b => b.InstallmentActive).HasColumnName("InstallmentActive").IsRequired();
            builder.Property(b => b.ThreeDPaymentActive).HasColumnName("ThreeDPaymentActive").IsRequired();
            builder.Property(b => b.ThreeDPaymentRequired).HasColumnName("ThreeDPaymentRequired").IsRequired();
            builder.Property(b => b.AmexActive).HasColumnName("AmexActive").IsRequired();
        }
    }
}
