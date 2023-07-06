using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Domain.Entities.Logs;

namespace SSO.Infrastructure.Data.EntitiesConfig
{
    public class LogEntityTypeConfig : IEntityTypeConfiguration<Log>
    {
        public void Configure(EntityTypeBuilder<Log> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.Events);

            //builder.Property(b => b.Id)
            //    .UseHiLo($"{nameof(Log)}seq", EFContext.DEFAULT_SCHEMA);

            builder.Property(b => b.Level)
                .HasMaxLength(128);
        }
    }
}
