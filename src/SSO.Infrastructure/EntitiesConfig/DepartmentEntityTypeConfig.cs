using SSO.Domain.Entities.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SSO.Infrastructure.Data.EntitiesConfig
{
    public class DepartmentEntityTypeConfig : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.Events);

            //builder.Property(b => b.Id)
            //    .UseHiLo($"{nameof(Department)}seq", EFContext.DEFAULT_SCHEMA);

            builder.Property(b => b.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(b => b.Name)
                .IsUnique(true);

            builder.Property(b => b.Description);

            builder.HasMany(b => b.Users)
                .WithOne(b => b.Department)
                .HasForeignKey(b => b.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            var navigation = builder.Metadata.FindNavigation(nameof(Department.Users));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
