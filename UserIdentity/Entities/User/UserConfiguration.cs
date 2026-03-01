using Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace UserIdentity.Entities;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(Constants.Tables.Users);

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
        builder.Property(u => u.ProfileType);
        builder.Property(u => u.FirstName)
            .HasMaxLength(64);
        builder.Property(u => u.LastName)
            .HasMaxLength(64);
        builder.Property(u => u.Cui)
            .HasMaxLength(10);
        builder.Property(u => u.CompanyName)
            .HasMaxLength(128);
        builder.Property(u => u.RegistrationNumber)
            .HasMaxLength(20);
        builder.Property(u => u.Address)
            .HasMaxLength(256);
        builder.Property(u => u.County)
            .HasMaxLength(64);
        builder.Property(u => u.Locality)
            .HasMaxLength(64);
        builder.Property(u => u.PhoneNumbers)
            .HasConversion(
                v => string.Join(';', v ?? Array.Empty<string>()),
                v => v.Split(';'),
                new ValueComparer<IReadOnlyList<string>?>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c != null ? string.Join(';', c).GetHashCode() : 0,
                    c => c ?? Array.Empty<string>()));
    }
}
