using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.Reservation
{
    public class ReservationDriverInfoMap : IEntityTypeConfiguration<ReservationDriverInfo>
    {
        public void Configure(EntityTypeBuilder<ReservationDriverInfo> builder)
        {
            builder.ToTable("RESERVATIONDRIVERINFOS", "dbo");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(r => r.ReservationDetailId).HasColumnName("ReservationDetailId").IsRequired();

            builder.Property(r => r.IdentityNumber).HasColumnName("IdentityNumber").IsRequired();
            builder.Property(r => r.Name).HasColumnName("Name").IsRequired();
            builder.Property(r => r.Surname).HasColumnName("Surname").IsRequired();
            builder.Property(r => r.Email).HasColumnName("Email").IsRequired();
            builder.Property(r => r.CountryPhoneCode).HasColumnName("CountryPhoneCode");
            builder.Property(r => r.PhoneNumber).HasColumnName("PhoneNumber");
            builder.Property(r => r.Gender).HasColumnName("Gender");
            builder.Property(r => r.FlightNumber).HasColumnName("FlightNumber");

            builder.Property(r => r.ContactPermission).HasColumnName("ContactPermission").HasDefaultValue(false);
            builder.Property(r => r.IsNonTurkishCitizen).HasColumnName("IsNonTurkishCitizen").HasDefaultValue(true);

            builder.Property(r => r.Birthday).HasColumnName("Birthday").HasDefaultValueSql("GETDATE()");

            builder.HasOne(r => r.ReservationDetail).WithMany(rd => rd.ReservationDriverInfos).HasForeignKey(r => r.ReservationDetailId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
