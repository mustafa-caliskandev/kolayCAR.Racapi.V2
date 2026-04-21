using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Payment
{
    public class PaymentSettingMap : IEntityTypeConfiguration<PaymentSetting>
    {
        public void Configure(EntityTypeBuilder<PaymentSetting> builder)
        {
            builder.ToTable("PAYMENTSETTINGS", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(p => p.BankId).HasColumnName("BankId").IsRequired();

            builder.Property(p => p.LogDate).HasColumnName("LogDate").HasDefaultValueSql("(getdate())");

            builder.Property(p => p.ReservationToken).HasColumnName("ReservationToken").IsRequired();
            builder.Property(p => p.BinNumber).HasColumnName("BinNumber");
            builder.Property(p => p.PaymentAmount).HasColumnName("PaymentAmount");
            builder.Property(p => p.CustomerInfo).HasColumnName("CustomerInfo");
            builder.Property(p => p.Code).HasColumnName("Code");
            builder.Property(p => p.Message).HasColumnName("Message");
            builder.Property(p => p.ProvisionNumber).HasColumnName("ProvisionNumber");

            builder.Property(p => p.AdvencedPaymentActive).HasColumnName("AdvencedPaymentActive").HasDefaultValue(false);

            builder.HasOne(p => p.Bank).WithMany(b => b.PaymentSettings).HasForeignKey(p => p.BankId).OnDelete(DeleteBehavior.NoAction);

        }
    }
}
