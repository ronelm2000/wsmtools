using Microsoft.EntityFrameworkCore;
using Montage.Card.API.Entities;
using Montage.Card.API.Entities.Impls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Montage.Weiss.Tools.Entities
{
    public class CardDatabaseContext : DbContext, ICardDatabase<WeissSchwarzCard>
    {
        private readonly AppConfig _config;
        private readonly ILogger Log = Serilog.Log.ForContext<CardDatabaseContext>();

        public DbSet<WeissSchwarzCard> WeissSchwarzCards { get; set; }
        //public DbSet<MultiLanguageString> MultiLanguageStrings { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<ActivityLog> MigrationLog { get; set; }

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

        internal async Task<WeissSchwarzCard> FindNonFoil(WeissSchwarzCard card)
        {
            return await WeissSchwarzCards.FindAsync(WeissSchwarzCard.RemoveFoil(card.Serial));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<WeissSchwarzCard>(b =>
            {
                b   .HasKey(c => c.Serial);
                b   .Property(c => c.Triggers)
                    .HasConversion( arr => String.Join(',', arr.Select(t => t.ToString()))
                                ,   str => str.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.ToEnum<Trigger>().Value).ToArray()
                            );
                b   .Property(c => c.Effect)
                    .HasConversion( arr => JsonConvert.SerializeObject(arr)
                                ,   str => JsonConvert.DeserializeObject<string[]>(str)
                                    );
                b   .Property(c => c.Images)
                    .HasConversion( arr => JsonConvert.SerializeObject(arr.Select(uri => uri.ToString()).ToArray())
                                ,   str => JsonConvert.DeserializeObject<string[]>(str).Select(s => new Uri(s)).ToList()
                                    );
                
                b.OwnsMany(s => s.Traits, bb =>
                 {
                     bb.Property<int>("Id").HasAnnotation("Sqlite:Autoincrement", true);
                     bb.HasKey("Id");
                     bb.WithOwner().HasPrincipalKey(s => s.Serial);
                 });

                b   .OwnsOne(c => c.Name, bb =>
                {
                    bb.WithOwner();
                });

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
                    }
                );
            });
        }
    }
}
