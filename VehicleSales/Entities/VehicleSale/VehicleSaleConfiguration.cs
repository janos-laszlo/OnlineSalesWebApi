using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VehicleSales.Entities.VehicleSale;

internal class VehicleSaleConfiguration : IEntityTypeConfiguration<VehicleSale>
{
    public void Configure(EntityTypeBuilder<VehicleSale> builder)
    {
        builder.ToTable("vehicle_sales");
        builder.HasKey(x => x.Id);

        builder.HasOne(v => v.Seller)
            .WithMany()
            .HasForeignKey(v => v.SellerId)
            .IsRequired();

        builder.ComplexProperty(
            vehicleSale => vehicleSale.Sale,
            saleBuilder =>
            {
                saleBuilder.Property(s => s.Title)
                    .HasColumnName(nameof(Sale.Title))
                    .HasColumnType("VARCHAR(100)")
                    .HasConversion(
                        title => title.Value,
                        value => SaleTitle.Create(value).Value)
                    .IsRequired();

                saleBuilder.Property(s => s.Description)
                    .HasColumnName(nameof(Sale.Description))
                    .HasColumnType("VARCHAR(5000)")
                    .HasConversion(
                        description => description.Value,
                        value => SaleDescription.Create(value).Value)
                    .IsRequired();

                saleBuilder.ComplexProperty(
                    s => s.SalePrice,
                    salePriceBuilder =>
                    {
                        salePriceBuilder.Property(m => m.AmountInCents)
                            .HasColumnName(nameof(Money.AmountInCents))
                            .IsRequired();
                        salePriceBuilder.Property(m => m.Currency)
                            .HasColumnName(nameof(Money.Currency))
                            .IsRequired();
                    });

                saleBuilder.ComplexProperty(
                    s => s.Location,
                    locationBuilder =>
                    {
                        locationBuilder.Property(m => m.County)
                            .HasColumnName(nameof(Location.County))
                            .HasColumnType("VARCHAR(100)")
                            .IsRequired();
                        locationBuilder.Property(m => m.Locality)
                            .HasColumnName(nameof(Location.Locality))
                            .HasColumnType("VARCHAR(100)")
                            .IsRequired();
                    });

                saleBuilder.Property(s => s.Status)
                    .HasColumnName(nameof(Sale.Status))
                    .IsRequired();

                saleBuilder.Property(s => s.CreatedAt)
                    .HasColumnName(nameof(Sale.CreatedAt))
                    .IsRequired();

                saleBuilder.Property(s => s.UpdatedAt)
                    .HasColumnName(nameof(Sale.UpdatedAt))
                    .IsRequired(false);
            });

        builder.OwnsOne(
            vehicleSale => vehicleSale.VehicleDetails,
            vehicleDetailsBuilder =>
            {
                vehicleDetailsBuilder.HasOne(v => v.VehicleModel)
                    .WithMany()
                    .HasForeignKey(v => v.VehicleModelId)
                    .IsRequired();
                vehicleDetailsBuilder.Property(v => v.VehicleModelId)
                    .HasColumnName(nameof(VehicleDetails.VehicleModelId));

                vehicleDetailsBuilder.Property(v => v.MileageInKilometers)
                    .HasColumnName(nameof(VehicleDetails.MileageInKilometers));

                vehicleDetailsBuilder.Property(v => v.HorsePower)
                    .HasColumnName(nameof(VehicleDetails.HorsePower));

                vehicleDetailsBuilder.Property(v => v.VehicleVersion)
                    .HasColumnName(nameof(VehicleDetails.VehicleVersion))
                    .HasColumnType("VARCHAR(100)");

                vehicleDetailsBuilder.Property(v => v.BodyType)
                    .HasColumnName(nameof(VehicleDetails.BodyType));

                vehicleDetailsBuilder.Property(v => v.EngineVolumeInCm3)
                    .HasColumnName(nameof(VehicleDetails.EngineVolumeInCm3));

                vehicleDetailsBuilder.Property(v => v.ExteriorColor)
                    .HasColumnName(nameof(VehicleDetails.ExteriorColor))
                    .HasColumnType("VARCHAR(30)")
                    .HasConversion(
                        value => value == null ? null : value.Value,
                        str => str == null ? null : MinLength3String.Create(str).Value);

                vehicleDetailsBuilder.Property(v => v.InteriorColor)
                    .HasColumnName(nameof(VehicleDetails.InteriorColor))
                    .HasColumnType("VARCHAR(30)")
                    .HasConversion(
                        value => value == null ? null : value.Value,
                        str => str == null ? null : MinLength3String.Create(str).Value);

                vehicleDetailsBuilder.Property(v => v.FuelType)
                    .HasColumnName(nameof(VehicleDetails.FuelType));

                vehicleDetailsBuilder.Property(v => v.VehicleManufacturingYear)
                    .HasColumnName(nameof(VehicleDetails.VehicleManufacturingYear))
                    .HasConversion(
                        value => value == null ? (int?)null : value.Value,
                        year => year == null ? null : VehicleManufacturingYear.Create(year, DateTimeOffset.Now.Year).Value);

                vehicleDetailsBuilder.Property(v => v.VehicleNumberOfDoors)
                    .HasColumnName(nameof(VehicleDetails.VehicleNumberOfDoors))
                    .HasConversion(
                        value => value == null ? (int?)null : value.Value,
                        doors => doors == null ? null : NumberBetween1And9.Create(doors).Value);

                vehicleDetailsBuilder.Property(v => v.VehicleCondition)
                    .HasColumnName(nameof(VehicleDetails.VehicleCondition));

                vehicleDetailsBuilder.Property(v => v.GearboxType)
                    .HasColumnName(nameof(VehicleDetails.GearboxType));

                vehicleDetailsBuilder.Property(v => v.SteeringWheelSide)
                    .HasColumnName(nameof(VehicleDetails.SteeringWheelSide));

                vehicleDetailsBuilder.Property(v => v.DriveType)
                    .HasColumnName(nameof(VehicleDetails.DriveType));

                vehicleDetailsBuilder.Property(v => v.NumberOfSeats)
                    .HasColumnName(nameof(VehicleDetails.NumberOfSeats))
                    .HasConversion(
                        value => value == null ? (int?)null : value.Value,
                        seats => seats == null ? null : NumberBetween1And9.Create(seats).Value);

                vehicleDetailsBuilder.Property(v => v.EmissionStandard)
                    .HasColumnName(nameof(VehicleDetails.EmissionStandard));

                vehicleDetailsBuilder.Property(v => v.HasServiceHistory)
                    .HasColumnName(nameof(VehicleDetails.HasServiceHistory));

                vehicleDetailsBuilder.Property(v => v.HasAccidentHistory)
                    .HasColumnName(nameof(VehicleDetails.HasAccidentHistory));

                vehicleDetailsBuilder.Property(v => v.Vin)
                    .HasColumnName(nameof(VehicleDetails.Vin))
                    .HasColumnType("VARCHAR(17)")
                    .HasConversion(
                        value => value == null ? null : value.Value,
                        vin => vin == null ? null : VIN.Create(vin).Value);

                vehicleDetailsBuilder.Property(v => v.NumberOfPreviousOwners)
                    .HasColumnName(nameof(VehicleDetails.NumberOfPreviousOwners));

                vehicleDetailsBuilder.Property(v => v.BatteryCapacityInKWh)
                    .HasColumnName(nameof(VehicleDetails.BatteryCapacityInKWh));

                vehicleDetailsBuilder.Property(v => v.RangeInKilometers)
                    .HasColumnName(nameof(VehicleDetails.RangeInKilometers));

                vehicleDetailsBuilder.Property(v => v.AverageFuelConsumptionInLitersPer100Km)
                    .HasColumnName(nameof(VehicleDetails.AverageFuelConsumptionInLitersPer100Km));

                vehicleDetailsBuilder.Property(v => v.AverageBatteryConsumptionInKWhPer100Km)
                    .HasColumnName(nameof(VehicleDetails.AverageBatteryConsumptionInKWhPer100Km));

                vehicleDetailsBuilder.Property(v => v.Mass)
                    .HasColumnName(nameof(VehicleDetails.Mass));

                vehicleDetailsBuilder.Property(v => v.MaximumLoad)
                    .HasColumnName(nameof(VehicleDetails.MaximumLoad));
            });
    }
}
