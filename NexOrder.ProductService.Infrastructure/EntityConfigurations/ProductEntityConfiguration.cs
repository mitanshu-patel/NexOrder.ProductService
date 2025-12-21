using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexOrder.ProductService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.ProductService.Infrastructure.EntityConfigurations
{
    public class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Description)
                   .HasMaxLength(200);

            builder.HasQueryFilter(v => !v.IsDeleted);

            builder.Property(v => v.CreatedAtUtc).HasDefaultValue(DateTime.UtcNow);
        }
    }
}
