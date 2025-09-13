using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaperTrail.Core.Models;

namespace PaperTrail.Core.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public AppDbContext() { }

    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    private static string DbPath =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PaperTrailContractTracker", "contracts.db");

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DbPath)!);
            options.UseSqlite($"Data Source={DbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
            d => d.ToDateTime(TimeOnly.MinValue),
            d => DateOnly.FromDateTime(d));

        modelBuilder.Entity<Contract>(entity =>
        {
            entity.Property(e => e.EffectiveDate).HasConversion(dateOnlyConverter);
            entity.Property(e => e.RenewalDate).HasConversion(dateOnlyConverter);
            entity.Property(e => e.TerminationDate).HasConversion(dateOnlyConverter);
            entity.HasMany(e => e.Attachments).WithOne(a => a.Contract).HasForeignKey(a => a.ContractId);
            entity.HasMany(e => e.Reminders).WithOne(r => r.Contract).HasForeignKey(r => r.ContractId);
        });

        modelBuilder.Entity<Party>()
            .HasMany(p => p.Contracts)
            .WithOne(c => c.Counterparty)
            .HasForeignKey(c => c.CounterpartyId);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.Entity is Contract || e.Entity is Party || e.Entity is Attachment || e.Entity is Reminder))
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "CreatedUtc"))
                    entry.Property("CreatedUtc").CurrentValue = DateTime.UtcNow;
            }
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedUtc"))
                    entry.Property("UpdatedUtc").CurrentValue = DateTime.UtcNow;
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
}
