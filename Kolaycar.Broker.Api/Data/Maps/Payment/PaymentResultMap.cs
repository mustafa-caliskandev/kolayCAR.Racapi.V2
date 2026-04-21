using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using KolayCAR.Broker.API.Models;

namespace KolayCAR.Broker.API.Data.Maps.Payment
{
    public class PaymentResultMap : IEntityTypeConfiguration<PaymentResult>
    {
        public void Configure(EntityTypeBuilder<PaymentResult> builder)
        {
            builder.ToTable("PAYMENTRESULTS", "dbo");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id).HasColumnName("Id").IsRequired().UseIdentityColumn();
            builder.Property(p => p.ResToken).HasColumnName("ResToken");
            builder.Property(p => p.PaymentCode).HasColumnName("PaymentCode");
            builder.Property(p => p.Result).HasColumnName("Result");
            builder.Property(p => p.Message).HasColumnName("Message");
            builder.Property(p => p.Date).HasColumnName("Date");
        }
    }
}
