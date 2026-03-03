using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VehicleSales.Entities.VehicleMake;

internal sealed class VehicleMakeConfiguration : IEntityTypeConfiguration<VehicleMake>
{
    public void Configure(EntityTypeBuilder<VehicleMake> builder)
    {
        builder.ToTable(Tables.VehicleMakes);
        builder.HasKey(cm => cm.Id);
        builder.Property(cm => cm.Name)
            .IsRequired()
            .HasMaxLength(64);
        builder.HasIndex(cm => cm.Name)
            .IsUnique();
    }
}