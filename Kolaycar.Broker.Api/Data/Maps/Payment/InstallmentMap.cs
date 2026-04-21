
using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KolayCAR.Broker.API.Data.Maps.Payment
{
    public class InstallmentMap : IEntityTypeConfiguration<Installment>
    {
        public void Configure(EntityTypeBuilder<Installment> builder)
        {
            builder.ToTable("INSTALLMENTS", "dbo");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(i => i.PaymentSettingId).HasColumnName("PaymentSettingId").IsRequired();
            builder.Property(i => i.InstallmentCount).HasColumnName("InstallmentCount").IsRequired();

            builder.Property(i => i.InstallmentTotalAmount).HasColumnName("InstallmentTotalAmount").IsRequired();
            builder.Property(i => i.InstallmentAmount).HasColumnName("InstallmentAmount").IsRequired();

            builder.Property(i => i.Comment).HasColumnName("Comment");

            builder.Property(i => i.IsActive).HasColumnName("IsActive");

            builder.HasOne(i => i.PaymentSetting).WithMany(ps => ps.Installments).HasForeignKey(i => i.PaymentSettingId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
