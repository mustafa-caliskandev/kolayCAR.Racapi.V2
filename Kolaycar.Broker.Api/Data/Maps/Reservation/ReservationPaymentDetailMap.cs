using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Reservation
{
    public class ReservationPaymentDetailMap : IEntityTypeConfiguration<ReservationPaymentDetail>
    {
        public void Configure(EntityTypeBuilder<ReservationPaymentDetail> builder)
        {
            builder.ToTable("RESERVATIONPAYMENTDETAILS", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(r => r.ReservationDetailId).HasColumnName("ReservationDetailId").IsRequired();

            builder.Property(r => r.PaymentCode).HasColumnName("PaymentCode").IsRequired();
            builder.Property(r => r.CreditCardNumber).HasColumnName("CreaditCardNumber").IsRequired();
            builder.Property(r => r.CreditCardOwnerName).HasColumnName("CreditCardOwnerName").IsRequired();
            builder.Property(r => r.ExpireDate).HasColumnName("ExpireDate").IsRequired();
            builder.Property(r => r.Cvc).HasColumnName("Cvc").IsRequired();

            builder.Property(r => r.PaymentResultCode).HasColumnName("PaymentResultCode");
            builder.Property(r => r.PaymentResultMessage).HasColumnName("PaymentResultMessage");
            builder.Property(r => r.BankResultMessage).HasColumnName("BankResultMessage");
            builder.Property(r => r.ProvisionNumber).HasColumnName("ProvisionNumber");

            builder.Property(r => r.ExpireMonth).HasColumnName("ExpireMonth").IsRequired();
            builder.Property(r => r.ExpireYear).HasColumnName("ExpireYear").IsRequired();

            builder.Property(r => r.InstallmentCount).HasColumnName("InstallmentCount");
            builder.Property(r => r.InstallmentCommissionAmount).HasColumnName("InstallmentCommissionAmount");

            builder.Property(r => r.TotalPrice).HasColumnName("TotalPrice").HasColumnType("DECIMAL(18, 2)").IsRequired();
            builder.Property(r => r.PaidAmount).HasColumnName("PaidAmount").HasColumnType("DECIMAL(18, 2)").IsRequired();

            builder.Property(r => r.AdvencedPayment).HasColumnName("AdvencedPayment").HasDefaultValue(false);
            builder.Property(r => r.AdditionalProductPricePoa).HasColumnName("AdditionalProductPricePoa").HasDefaultValue(false);
            builder.Property(r => r.OneWayFeePoa).HasColumnName("OneWayFeePoa").HasDefaultValue(false);

            builder.Property(r => r.Refunded).HasColumnName("Refunded").HasDefaultValue(false);
            builder.Property(r => r.Returner).HasColumnName("Returner");
            builder.Property(r => r.RefundDate).HasColumnName("RefundDate");

            builder.HasOne(r => r.ReservationDetail).WithMany(rd => rd.ReservationPaymentDetails).HasForeignKey(r => r.ReservationDetailId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
