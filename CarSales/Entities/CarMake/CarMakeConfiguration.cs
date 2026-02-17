using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarSales.Entities.CarMake;

internal sealed class CarMakeConfiguration : IEntityTypeConfiguration<CarMake>
{
    public void Configure(EntityTypeBuilder<CarMake> builder)
    {
        builder.ToTable("car_makes");
        builder.HasKey(cm => cm.Id);
        builder.Property(cm => cm.Name)
            .IsRequired()
            .HasMaxLength(64);
        builder.HasIndex(cm => cm.Name)
            .IsUnique();
    }
}