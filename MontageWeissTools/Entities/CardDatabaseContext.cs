using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using System.IO;
using System.Text.Json;

namespace Montage.Weiss.Tools.Entities;

public class CardDatabaseContext : DbContext, ICardDatabase<WeissSchwarzCard>
{
    private readonly AppConfig _config;
    private readonly ILogger Log = Serilog.Log.ForContext<CardDatabaseContext>();

    public DbSet<WeissSchwarzCard> WeissSchwarzCards { get; set; }
    //public DbSet<MultiLanguageString> MultiLanguageStrings { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<ActivityLog> MigrationLog { get; set; }
    public DbSet<WeissSchwarzTrait> Traits { get; set; }
    public DatabaseFacade GetDatabase() => Database;
    public CardDatabaseContext (AppConfig config) {
        Log.Debug("Instantiating with {@AppConfig}.", config);

        _config = config;
    }

    public CardDatabaseContext()
    {
        Log.Debug("Instantiating with no arguments.");
        using var stream = File.Open(@"app.json", FileMode.OpenOrCreate);
        _config = JsonSerializer.Deserialize<AppConfig>(stream) ?? new();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var databasePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _config.DbName);
        Log.Debug("Loading Database: {databasePath}", databasePath);
        options.UseSqlite($"Data Source={databasePath}")
            .LogTo(Log.ForContext<CardDatabaseContext>().Warning, Microsoft.Extensions.Logging.LogLevel.Warning)
            .ConfigureWarnings(w => w.Log(RelationalEventId.PendingModelChangesWarning))
            .UseSeeding(SeedLogs)
            .UseAsyncSeeding(SeedLogsAsync);
    }

    internal async Task<WeissSchwarzCard?> FindNonFoil(WeissSchwarzCard card, CancellationToken ct = default)
    {
        return await WeissSchwarzCards.FindAsync(new[] { WeissSchwarzCard.RemoveFoil(card.Serial) }, ct);
    }

    internal IEnumerable<WeissSchwarzCard> FindFoils(WeissSchwarzCard card, CancellationToken ct = default)
    {
        var results = WeissSchwarzCards.Where(c => c.Serial.Contains(card.Serial) && c != card).ToList();
        Log.Information("Finding foils for: {card} || Found {amount} results.", card.Serial, results.Count);
        return results;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var options = new JsonSerializerOptions { };

        modelBuilder.Entity<WeissSchwarzCard>(b =>
        {
            b.HasKey(c => c.Serial);
            b.Property(c => c.Triggers)
                .HasConversion(arr => String.Join(',', arr.Select(t => t.ToString()))
                            , str => str.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToEnum<Trigger>() ?? Trigger.Soul).ToArray()
                            , new ValueComparer<Trigger[]>(
                                (c1, c2) => c1.SequenceEqual(c2)
                                , c => c.Aggregate(0, (a1, v1) => HashCode.Combine(a1, v1.GetHashCode()))
                                , c => c.ToArray()
                                )
                            );
            b.Property(c => c.Effect)
                .HasConversion(arr => JsonSerializer.Serialize(arr, options)
                            , str => JsonSerializer.Deserialize<string[]>(str, options) ?? Array.Empty<string>()
                            , new ValueComparer<string[]>(
                                    (c1, c2) => c1.SequenceEqual(c2)
                                ,   c => c.Aggregate(0, (a1, v1) => HashCode.Combine(a1, v1.GetHashCode()))
                                ,   c => c.ToArray()
                                )
                            );

            b.Property(c => c.Images)
                .HasConversion( arr => JsonSerializer.Serialize(arr.Select(uri => uri.ToString()).ToArray(), options)
                            ,   str => (JsonSerializer.Deserialize<string[]>(str, options) ?? Array.Empty<string>()).Select(s => new Uri(s)).ToList()
                            ,   new ValueComparer<List<Uri>>(
                                    (c1, c2) => c1.SequenceEqual(c2)
                                , c => c.Aggregate(0, (a1, v1) => HashCode.Combine(a1, v1.GetHashCode()))
                                , c => c.ToList()
                                )
                            );

            b.Property(c => c.Flavor)
                .IsRequired(false);

            b.OwnsMany(s => s.Traits, bb =>
            {
                bb.WithOwner().HasForeignKey("Serial");
                bb.HasKey("TraitID", "Serial");
                bb.Property<string>("EN").IsRequired(false);
                bb.Property<string>("JP").IsRequired(false);
            });

            b.OwnsOne(s => s.Name, bb =>
            {
                bb.ToTable("WeissSchwarzCards_Names");
                bb.Property<int>("Id").HasAnnotation("Sqlite:Autoincrement", true);
                bb.HasKey("Id");
                bb.WithOwner().HasPrincipalKey(s => s.Serial);
                bb.Property<string>("EN").IsRequired(false);
                bb.Property<string>("JP").IsRequired(false);
            });

            b   .HasMany(c => c.AdditionalInfo)
                .WithOne(i => i.Card)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WeissSchwarzCardOptionalInfo>(b =>
        {
            b   .HasOne(i => i.Card)
                .WithMany(c => c.AdditionalInfo)
                .HasForeignKey(i => i.Serial)
                .HasPrincipalKey(c => c.Serial);
            b.HasKey(i => new { i.Serial, i.Key });
        });

        modelBuilder.Entity<Setting>(b =>
        {
            b.HasKey(s => s.Key);
        });

        modelBuilder.Entity<ActivityLog>(b =>
        {
            b.HasKey(a => a.LogID);
        });
    }
    private void SeedLogs(DbContext context, bool wereChangesInvoked)
    {
        Task.Run(() => SeedLogsAsync(context, wereChangesInvoked, CancellationToken.None)).GetAwaiter().GetResult();
    }

    private async Task SeedLogsAsync(DbContext context, bool wereChangesInvoked, CancellationToken token)
    {
        var seededLogs = new[] {
            new ActivityLog
            {
                LogID = 1,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""EN"", ""VersionLessThan"": ""0.8.0""}",
                DateAdded = new DateTime(2021, 8, 10, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029)
            },
            new ActivityLog
            {
                LogID = 2,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""ALL"", ""VersionLessThan"": ""0.9.0""}",
                DateAdded = new DateTime(2021, 8, 11, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029)
            },
            new ActivityLog
            {
                LogID = 3,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""EN"", ""VersionLessThan"": ""0.10.0""}",
                DateAdded = new DateTime(2021, 12, 14, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029)
            },
            new ActivityLog
            {
                LogID = 4,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""EN"", ""VersionLessThan"": ""0.12.0""}",
                DateAdded = new DateTime(2022, 11, 28, 20, 51, 28, 983, DateTimeKind.Local).AddTicks(6076)
            },
            new ActivityLog
            {
                LogID = 5,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""EN"", ""VersionLessThan"": ""0.15.0""}",
                DateAdded = DateTime.FromBinary(-8584722569801376902)
            },
            new ActivityLog
            {
                LogID = 6,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""ALL"", ""VersionLessThan"": ""0.16.0""}",
                DateAdded = DateTime.FromBinary(-8584720777293269695)
            },
            new ActivityLog
            {
                LogID = 7,
                Activity = ActivityType.Delete,
                Target = @"{""Language"": ""EN"", ""VersionLessThan"": ""0.18.0""}",
                DateAdded = DateTime.FromBinary(-8584439178187962653)
            }
        };

        var activityLogs = context.Set<ActivityLog>();
        activityLogs.AddRange(seededLogs.Except(activityLogs, ActivityLog.EqualById));

        await context.SaveChangesAsync();
    }
}
