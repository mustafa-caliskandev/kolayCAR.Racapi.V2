using KolayCAR.Broker.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace KolayCAR.Broker.API.Data.Maps.Logs
{
    public class FindeksLogMap : IEntityTypeConfiguration<FindeksLog>
    {
        public void Configure(EntityTypeBuilder<FindeksLog> builder)
        {
            builder.ToTable("FINDEKSLOGS", "dbo");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Id).HasColumnName("ID").IsRequired().UseIdentityColumn();
            builder.Property(f => f.LogDate).HasColumnName("LOGDATE").HasDefaultValueSql("(GETDATE())");
            builder.Property(f => f.ReservationToken).HasColumnName("RESTOKEN").IsRequired();
            builder.Property(f => f.Tckn).HasColumnName("TCKN");
            builder.Property(f => f.Request).HasColumnName("REQUEST");
            builder.Property(f => f.Response).HasColumnName("RESPONSE");
            builder.Property(f => f.StepName).HasColumnName("STEPNAME");
            builder.Property(f => f.IsSuitable).HasColumnName("ISSUITABLE");

        }
    }
}
