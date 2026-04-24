using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VehicleSales.Entities.Translation;

internal sealed class TranslationConfiguration : IEntityTypeConfiguration<Translation>
{
    public void Configure(EntityTypeBuilder<Translation> builder)
    {
        builder.ToTable(Tables.Translations);
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Key)
            .IsRequired()
            .HasMaxLength(128);
        builder.Property(t => t.Value)
            .IsRequired()
            .HasMaxLength(1024);
    }
}
