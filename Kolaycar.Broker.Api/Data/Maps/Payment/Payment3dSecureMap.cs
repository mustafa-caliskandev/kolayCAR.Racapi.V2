
using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Payment
{
    public class Payment3dSecureMap : IEntityTypeConfiguration<Payment3dSecure>
    {
        public void Configure(EntityTypeBuilder<Payment3dSecure> builder)
        {
            builder.ToTable("PAYMENT3DSECURE", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(p => p.BankId).HasColumnName("BankId").IsRequired();
            builder.Property(p => p.LanguageId).HasColumnName("LanguageId").IsRequired();
            builder.Property(p => p.CurrencyId).HasColumnName("CurrencyId").IsRequired();
            builder.Property(p => p.BankVendorId).HasColumnName("BankVendorId").IsRequired();
            builder.Property(p => p.CreditCardExpiredMonth).HasColumnName("CreditCardExpiredMonth").IsRequired();
            builder.Property(p => p.CreditCardExpiredYear).HasColumnName("CreditCardExpiredYear").IsRequired();
            builder.Property(p => p.InstallmentCount).HasColumnName("InstallmentCount").IsRequired();

            builder.Property(p => p.ReservaionToken).HasColumnName("ReservaionToken").IsRequired();
            builder.Property(p => p.CustomerMailAddress).HasColumnName("CustomerMailAddress");
            builder.Property(p => p.CreditCardHolder).HasColumnName("CreditCardHolder");
            builder.Property(p => p.CreditCardNumber).HasColumnName("CreditCardNumber");
            builder.Property(p => p.SecurityCode).HasColumnName("SecurityCode");
            builder.Property(p => p.PaymentAmount).HasColumnName("PaymentAmount");
            builder.Property(p => p.OrderNo).HasColumnName("OrderNo");
            builder.Property(p => p.IpAddress).HasColumnName("IpAddress");
            builder.Property(p => p.CallbackUrl).HasColumnName("CallbackUrl");

            builder.HasOne(p => p.Bank).WithMany(b => b.Payment3DSecures).HasForeignKey(p => p.BankId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
