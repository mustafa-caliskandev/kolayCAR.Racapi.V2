using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.Reservation
{
    public class ReservationDetailMap : IEntityTypeConfiguration<ReservationDetail>
    {
        public void Configure(EntityTypeBuilder<ReservationDetail> builder)
        {
            builder.ToTable("RESERVATIONDETAILS", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(r => r.PaymentType).HasColumnName("PaymentType").IsRequired();

            builder.Property(r => r.LogDate).HasColumnName("LogDate").HasDefaultValueSql("(GETDATE())");

            builder.Property(r => r.ReservationToken).HasColumnName("ReservationToken").IsRequired();
            builder.Property(r => r.CouponCode).HasColumnName("CouponCode");
            builder.Property(r => r.SelectedExtrasJson).HasColumnName("SelectedExtrasJson");
            builder.Property(r => r.ReservationNote).HasColumnName("ReservationNote");
            builder.Property(r => r.IpAddress).HasColumnName("IpAddress");
            builder.Property(r => r.CurrencyId).HasColumnName("CurrencyId");
            builder.Property(r => r.SkyScannerRedirectId).HasColumnName("SkyScannerRedirectId");

            builder.Property(r => r.PickupLocation).HasColumnName("PickupLocation");
            builder.Property(r => r.PickupDate).HasColumnName("PickupDate");
            builder.Property(r => r.ReturnLocation).HasColumnName("ReturnLocation");
            builder.Property(r => r.ReturnDate).HasColumnName("ReturnDate");
            builder.Property(r => r.VendorName).HasColumnName("VendorName");

            builder.Property(r => r.ConfirmConditions).HasColumnName("ConfirmConditions").HasDefaultValue(true);
            builder.Property(r => r.InvoiceToDifferentAddress).HasColumnName("InvoiceToDifferentAddress").HasDefaultValue(false);
            builder.Property(r => r.FullCredit).HasColumnName("FullCredit").HasDefaultValue(false);

            builder.Property(r => r.CouponDiscountAmount).HasColumnName("CouponDiscountAmount").HasColumnType("DECIMAL(18, 2)").HasDefaultValue(0m);
            builder.Property(r => r.ServiceCharge).HasColumnName("ServiceCharge").HasColumnType("DECIMAL(18, 2)").HasDefaultValue(0m);
        }
    }
}
