using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Domain.Entities.AuditLogs;

namespace SSO.Infrastructure.Data.EntitiesConfig
{
    public class AuditLogEntityTypeConfig : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.Events);

            //builder.Property(b => b.Id)
            //    .UseHiLo($"{nameof(AuditLog)}seq", EFContext.DEFAULT_SCHEMA);
        }
    }
}
