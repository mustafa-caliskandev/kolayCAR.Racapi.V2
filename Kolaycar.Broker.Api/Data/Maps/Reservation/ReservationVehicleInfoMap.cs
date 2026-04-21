using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.Reservation
{
    public class ReservationVehicleInfoMap : IEntityTypeConfiguration<ReservationVehicleInfo>
    {
        public void Configure(EntityTypeBuilder<ReservationVehicleInfo> builder)
        {
            builder.ToTable("RESERVATIONVEHICLEINFOS", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(r => r.ReservationDetailId).HasColumnName("ReservationDetailId").IsRequired();

            builder.Property(r => r.VehicleBrandName).HasColumnName("VehicleBrandName").IsRequired();
            builder.Property(r => r.VehicleModelName).HasColumnName("VehicleModelName").IsRequired();
            builder.Property(r => r.VehicleName).HasColumnName("VehicleName").IsRequired();
            builder.Property(r => r.FuelName).HasColumnName("FuelName").IsRequired();
            builder.Property(r => r.TransmissionName).HasColumnName("TransmissionName").IsRequired();
            builder.Property(r => r.PersonName).HasColumnName("PersonName").IsRequired();
            builder.Property(r => r.CategoryName).HasColumnName("CategoryName").IsRequired();
            builder.Property(r => r.TypeName).HasColumnName("TypeName").IsRequired();

            builder.Property(r => r.DeliveryType).HasColumnName("DeliveryType");
            builder.Property(r => r.DailyPrice).HasColumnName("DailyPrice");
            builder.Property(r => r.Deposit).HasColumnName("Deposit");
            builder.Property(r => r.KmLimit).HasColumnName("KmLimit");
            builder.Property(r => r.OneWayFee).HasColumnName("OneWayFee");
            builder.Property(r => r.OfficeServicePrice).HasColumnName("OfficeServicePrice");
            builder.Property(r => r.VendorProfitMarkup).HasColumnName("VendorProfitMarkup");
            builder.Property(r => r.RentalDuration).HasColumnName("RentalDuration");

            builder.Property(r => r.ExtraJson).HasColumnName("ExtraJson");
            builder.Property(r => r.ExtraNames).HasColumnName("ExtraNames");
            builder.Property(r => r.ExtraAmount).HasColumnName("ExtraAmount");
            builder.Property(r => r.SpecialDailyPrice).HasColumnName("SpecialDailyPrice");
            builder.Property(r => r.SpecialOneWayFee).HasColumnName("SpecialOneWayFee");

            builder.Property(r => r.MinimumAge).HasColumnName("MinimumAge");
            builder.Property(r => r.MinimumLicenseAge).HasColumnName("MinimumLicenseAge");

            builder.HasOne(r => r.ReservationDetail).WithMany(rd => rd.ReservationVehicleInfos).HasForeignKey(r => r.ReservationDetailId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
