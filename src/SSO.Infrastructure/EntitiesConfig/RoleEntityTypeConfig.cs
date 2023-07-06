using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Domain.Entities.Roles;

namespace SSO.Infrastructure.Data.EntitiesConfig
{
    public class RoleEntityTypeConfig : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            //builder.ToTable(TableConstants.IdentityRoles, EFContext.DEFAULT_SCHEMA);

            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.Events);

            //builder.Property(b => b.Id)
            //    .UseHiLo($"{nameof(User)}seq", EFContext.DEFAULT_SCHEMA);

            builder.Property(b => b.Name)
                .HasMaxLength(200)
                .IsRequired();
        }
    }
}
