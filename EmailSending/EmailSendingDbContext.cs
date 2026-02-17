using Microsoft.EntityFrameworkCore;

namespace EmailSending;

internal sealed class EmailSendingDbContext(
    DbContextOptions<EmailSendingDbContext> options) : DbContext(options)
{
    public DbSet<Email> Emails { get; private set; }

    override protected void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmailSendingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
