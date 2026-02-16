using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarSales.Entities.CarMake;

internal sealed class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
{
    public void Configure(EntityTypeBuilder<CarModel> builder)
    {
        builder.ToTable("car_models");
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Name)
            .IsRequired()
            .HasMaxLength(32);

        builder
            .HasOne(cm => cm.CarMake)
            .WithMany(cm => cm.CarModels)
            .HasForeignKey(cm => cm.CarMakeId)
            .IsRequired();
    }
}
