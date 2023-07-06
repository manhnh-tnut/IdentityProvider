using SSO.Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SSO.Infrastructure.Data.EntitiesConfig
{
    public class UserEntityTypeConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            //builder.ToTable(TableConstants.IdentityUsers, EFContext.DEFAULT_SCHEMA);

            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.Events);

            //builder.Property(b => b.Id)
            //    .UseHiLo($"{nameof(User)}seq", EFContext.DEFAULT_SCHEMA);

            builder.Property(b => b.UserName)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(b => b.UserName)
                .IsUnique(true);

            builder.HasMany(b => b.Avatars)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            var navigation = builder.Metadata.FindNavigation(nameof(User.Avatars));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
