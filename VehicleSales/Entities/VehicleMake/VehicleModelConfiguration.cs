using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VehicleSales.Entities.VehicleMake;

internal sealed class VehicleModelConfiguration : IEntityTypeConfiguration<VehicleModel>
{
    public void Configure(EntityTypeBuilder<VehicleModel> builder)
    {
        builder.ToTable(Tables.VehicleModels);
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.HasIndex(cm => new { cm.Name, cm.VehicleMakeId })
            .IsUnique();

        builder
            .HasOne(cm => cm.VehicleMake)
            .WithMany(cm => cm.VehicleModels)
            .HasForeignKey(cm => cm.VehicleMakeId)
            .IsRequired();
    }
}
