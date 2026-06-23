using HorseEntryNotifier.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HorseEntryNotifier.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<RegisteredHorse> RegisteredHorses => Set<RegisteredHorse>();
    public DbSet<NotificationRecord> Notifications => Set<NotificationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var dateConverter = new ValueConverter<DateOnly, string>(
            value => value.ToString("yyyy-MM-dd"),
            value => DateOnly.Parse(value));

        modelBuilder.Entity<RegisteredHorse>(entity =>
        {
            entity.ToTable("RegisteredHorses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.HorseName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.NormalizedHorseName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Memo).HasMaxLength(1000);
            entity.Property(x => x.NominatorName).HasMaxLength(200);
            entity.HasIndex(x => x.NormalizedHorseName).IsUnique();
        });

        modelBuilder.Entity<NotificationRecord>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.NotificationKey).HasMaxLength(500).IsRequired();
            entity.Property(x => x.HorseId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.HorseName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RaceId).HasMaxLength(200).IsRequired();
            entity.Property(x => x.RaceDate).HasConversion(dateConverter).HasMaxLength(10);
            entity.Property(x => x.RaceName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.JockeyId).HasMaxLength(100);
            entity.Property(x => x.JockeyName).HasMaxLength(200);
            entity.Property(x => x.NotificationType).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PayloadHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.NotificationKey).IsUnique();
            entity.HasIndex(x => new { x.RaceId, x.HorseId });
        });
    }
}
