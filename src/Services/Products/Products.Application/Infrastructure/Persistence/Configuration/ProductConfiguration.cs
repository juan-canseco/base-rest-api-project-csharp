using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Products.Application.Domain;

namespace Products.Application.Infrastructure.Persistence.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Name)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Property<DateTime>("CreatedAt");
            builder.Property<string>("CreatedBy");

            builder
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey("CreatedBy");

            builder.Property<DateTime>("UpdatedAt");
            builder.Property<string>("UpdatedBy");

            builder.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey("UpdatedBy");

        }
    }
}
