using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DraftEntities;

internal sealed class DraftEntityConfiguration : IEntityTypeConfiguration<DraftEntity>
{
    public void Configure(EntityTypeBuilder<DraftEntity> builder)
    {
        builder.ToTable("draft_entities");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(DraftEntity.NameMaxLength);
        builder.Property(e => e.JsonValue).IsRequired().HasColumnType("text");
        builder.Property(e => e.CreatedAt).IsRequired();
    }
}
