using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Newtonsoft.Json;
using System.IO;

namespace Montage.Weiss.Tools.Entities;

public class CardDatabaseContext : DbContext, ICardDatabase<WeissSchwarzCard>
{
    private readonly AppConfig _config;
    private readonly ILogger Log = Serilog.Log.ForContext<CardDatabaseContext>();

    public DbSet<WeissSchwarzCard> WeissSchwarzCards { get; set; }
    //public DbSet<MultiLanguageString> MultiLanguageStrings { get; set; }
    public DbSet<Setting> Settings { get; set; }
    public DbSet<ActivityLog> MigrationLog { get; set; }
    public DatabaseFacade GetDatabase() => Database;
    public CardDatabaseContext (AppConfig config) {
        Log.Debug("Instantiating with {@AppConfig}.", config);

        _config = config;
    }

    public CardDatabaseContext()
    {
        Log.Debug("Instantiating with no arguments.");
        using (StreamReader file = File.OpenText(@"app.json"))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            _config = JToken.ReadFrom(reader).ToObject<AppConfig>();
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Data Source={_config.DbName}");
    }

    internal async Task<WeissSchwarzCard> FindNonFoil(WeissSchwarzCard card, CancellationToken ct = default)
    {
        return await WeissSchwarzCards.FindAsync(new[] { WeissSchwarzCard.RemoveFoil(card.Serial) }, ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeissSchwarzCard>(b =>
        {
            b.HasKey(c => c.Serial);
            b.Property(c => c.Triggers)
                .HasConversion(arr => String.Join(',', arr.Select(t => t.ToString()))
                            , str => str.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToEnum<Trigger>().Value).ToArray()
                        );
            b.Property(c => c.Effect)
                .HasConversion(arr => JsonConvert.SerializeObject(arr)
                            , str => JsonConvert.DeserializeObject<string[]>(str)
                                );
            b.Property(c => c.Images)
                .HasConversion(arr => JsonConvert.SerializeObject(arr.Select(uri => uri.ToString()).ToArray())
                            , str => JsonConvert.DeserializeObject<string[]>(str).Select(s => new Uri(s)).ToList()
                                );

            b.OwnsMany(s => s.Traits, bb =>
                 {
                     bb.Property<int>("Id").HasAnnotation("Sqlite:Autoincrement", true);
                     bb.HasKey("Id");
                     bb.ToTable("WeissSchwarzCards_Traits");
                     bb.WithOwner().HasPrincipalKey(s => s.Serial);
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
        });

        modelBuilder.Entity<WeissSchwarzCardOptionalInfo>(b =>
        {
            b   .HasOne(i => i.Card)
                .WithMany(c => c.AdditionalInfo)
                .HasForeignKey(i => i.Serial)
                .HasPrincipalKey(c => c.Serial);
            b   .HasKey(i => new { i.Serial, i.Key });
        });

        modelBuilder.Entity<Setting>(b =>
        {
            b.HasKey(s => s.Key);
        });

        modelBuilder.Entity<ActivityLog>(b =>
        {
            b.HasKey(a => a.LogID);
            b.HasData(
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
                }
            );
        });
    }
}
