using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ObjectUploadTracking;

internal sealed class ObjectUploadConfiguration : IEntityTypeConfiguration<ObjectUpload>
{
    /// <summary>
    /// 10 object keys with a maximum length of <see cref="ObjectKeyName.MaxLength"/> 
    /// characters each, plus 9 commas as separators.
    /// </summary>
    public const int ObjectKeysMaxLength = 10 * ObjectKeyName.MaxLength + 9;

    public void Configure(EntityTypeBuilder<ObjectUpload> builder)
    {
        builder.ToTable("object_uploads");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityId).IsRequired();

        builder.Property(x => x.Module)
            .IsRequired()
            .HasColumnType($"VARCHAR(100)");

        builder.Property(v => v.Directory)
            .HasColumnName(nameof(ObjectUpload.Directory))
            .HasColumnType($"VARCHAR({DirectoryName.MaxLength})")
            .HasConversion(
                value => value.Value,
                directory => DirectoryName.Create(directory).Value);

        builder.Property(v => v.ObjectKeys)
            .HasColumnName(nameof(ObjectUpload.ObjectKeys))
            .HasColumnType($"VARCHAR({ObjectKeysMaxLength})")
            .HasConversion(
                value => string.Join(',', value.Select(v => v.Value)),
                photoKeys => photoKeys.Split(',').Select(key => ObjectKeyName.Create(key).Value).ToList(),
                new ValueComparer<IReadOnlyList<ObjectKeyName>?>(
                    (c1, c2) => c1 == null && c2 == null
                        || (c1 != null && c2 != null && c1.Select(v => v.Value).SequenceEqual(c2.Select(v => v.Value))),
                    c => c == null ? 0 : c.Aggregate(0, (hash, v) => HashCode.Combine(hash, v.Value.GetHashCode())),
                    c => c == null ? null : c.Select(v => ObjectKeyName.Create(v.Value).Value).ToList()
                )
            );

        builder.Property(x => x.ExpiresAt).IsRequired();
    }
}
