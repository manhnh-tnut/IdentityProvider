using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSO.Domain.Entities.Branches;

namespace SSO.Infrastructure.Data.EntitiesConfig
{
    public class BranchEntityTypeConfig : IEntityTypeConfiguration<Branch>
    {
        public void Configure(EntityTypeBuilder<Branch> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Ignore(b => b.Events);

            //builder.Property(b => b.Id)
            //    .UseHiLo($"{nameof(Branch)}seq", EFContext.DEFAULT_SCHEMA);

            builder.Property(b => b.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasIndex(b => b.Name)
                .IsUnique(true);

            builder.Property(b => b.Description);

            builder.HasMany(b => b.Users)
                .WithOne(b => b.Branch)
                .HasForeignKey(b => b.BranchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.Departments)
                .WithMany(b => b.Branches);
                //.UsingEntity(b => b.ToTable("BranchesDepartments"));

            var navigation = builder.Metadata.FindNavigation(nameof(Branch.Users));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}
