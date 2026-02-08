using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserIdentity.Entities;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(u => u.EmailConfirmed)
            .IsRequired();

        builder.Ignore(u => u.Profile);
        builder.Property<ProfileType?>("profileType")
            .HasColumnName("ProfileType");
        builder.Property<string?>("firstName")
            .HasColumnName("FirstName")
            .HasMaxLength(64);
        builder.Property<string?>("lastName")
            .HasColumnName("LastName")
            .HasMaxLength(64);
        builder.Property<string?>("cui")
            .HasColumnName("Cui")
            .HasMaxLength(10);
        builder.Property<string?>("companyName")
            .HasColumnName("CompanyName")
            .HasMaxLength(128);
        builder.Property<string?>("registrationNumber")
            .HasColumnName("RegistrationNumber")
            .HasMaxLength(20);
        builder.Property<string?>("address")
            .HasColumnName("Address")
            .HasMaxLength(256);
        builder.Property<string?>("county")
            .HasColumnName("County")
            .HasMaxLength(64);
        builder.Property<string?>("locality")
            .HasColumnName("Locality")
            .HasMaxLength(64);
        builder.Property<IReadOnlyList<string>?>("phoneNumbers")
                .HasColumnName("PhoneNumbers")
                .HasConversion(
                    v => string.Join(';', v ?? Array.Empty<string>()),
                    v => v.Split(';'))
                .Metadata
                .SetValueComparer(new ValueComparer<IReadOnlyList<string>?>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c != null ? string.Join(';', c).GetHashCode() : 0,
                    c => c ?? Array.Empty<string>()));
    }
}
