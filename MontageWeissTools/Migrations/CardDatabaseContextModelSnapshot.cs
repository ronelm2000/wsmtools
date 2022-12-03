﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Montage.Weiss.Tools.Entities;

#nullable disable

namespace Montage.Weiss.Tools.Migrations
{
    [DbContext(typeof(CardDatabaseContext))]
    partial class CardDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("Montage.Card.API.Entities.Impls.ActivityLog", b =>
                {
                    b.Property<int>("LogID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Activity")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDone")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Target")
                        .HasColumnType("TEXT");

                    b.HasKey("LogID");

                    b.ToTable("MigrationLog", (string)null);

                    b.HasData(
                        new
                        {
                            LogID = 1,
                            Activity = 1,
                            DateAdded = new DateTime(2021, 8, 10, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029),
                            IsDone = false,
                            Target = "{\"Language\": \"EN\", \"VersionLessThan\": \"0.8.0\"}"
                        },
                        new
                        {
                            LogID = 2,
                            Activity = 1,
                            DateAdded = new DateTime(2021, 8, 11, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029),
                            IsDone = false,
                            Target = "{\"Language\": \"ALL\", \"VersionLessThan\": \"0.9.0\"}"
                        },
                        new
                        {
                            LogID = 3,
                            Activity = 1,
                            DateAdded = new DateTime(2021, 12, 14, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029),
                            IsDone = false,
                            Target = "{\"Language\": \"EN\", \"VersionLessThan\": \"0.10.0\"}"
                        },
                        new
                        {
                            LogID = 4,
                            Activity = 1,
                            DateAdded = new DateTime(2022, 11, 28, 20, 51, 28, 983, DateTimeKind.Local).AddTicks(6076),
                            IsDone = false,
                            Target = "{\"Language\": \"EN\", \"VersionLessThan\": \"0.12.0\"}"
                        });
                });

            modelBuilder.Entity("Montage.Card.API.Entities.Impls.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("Settings", (string)null);
                });

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.WeissSchwarzCard", b =>
                {
                    b.Property<string>("Serial")
                        .HasColumnType("TEXT");

                    b.Property<int>("Color")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Cost")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Effect")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Flavor")
                        .HasColumnType("TEXT");

                    b.Property<string>("Images")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Power")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Rarity")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Remarks")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Side")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Soul")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Triggers")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionTimestamp")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Serial");

                    b.ToTable("WeissSchwarzCards", (string)null);
                });

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.WeissSchwarzCardOptionalInfo", b =>
                {
                    b.Property<string>("Serial")
                        .HasColumnType("TEXT");

                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("ValueJSON")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Serial", "Key");

                    b.ToTable("WeissSchwarzCardOptionalInfo", (string)null);
                });

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.WeissSchwarzCard", b =>
                {
                    b.OwnsMany("Montage.Weiss.Tools.Entities.WeissSchwarzCard.Traits#Montage.Weiss.Tools.Entities.WeissSchwarzTrait", "Traits", b1 =>
                        {
                            b1.Property<Guid>("TraitID")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("TEXT");

                            b1.Property<string>("Serial")
                                .HasColumnType("TEXT");

                            b1.Property<string>("EN")
                                .HasColumnType("TEXT");

                            b1.Property<string>("JP")
                                .HasColumnType("TEXT");

                            b1.HasKey("TraitID", "Serial");

                            b1.HasIndex("Serial");

                            b1.ToTable("Traits", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("Serial");
                        });

                    b.OwnsOne("Montage.Weiss.Tools.Entities.WeissSchwarzCard.Name#Montage.Card.API.Entities.Impls.MultiLanguageString", "Name", b1 =>
                        {
                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("INTEGER")
                                .HasAnnotation("Sqlite:Autoincrement", true);

                            b1.Property<string>("EN")
                                .HasColumnType("TEXT");

                            b1.Property<string>("JP")
                                .HasColumnType("TEXT");

                            b1.Property<string>("WeissSchwarzCardSerial")
                                .IsRequired()
                                .HasColumnType("TEXT");

                            b1.HasKey("Id");

                            b1.HasIndex("WeissSchwarzCardSerial")
                                .IsUnique();

                            b1.ToTable("WeissSchwarzCards_Names", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("WeissSchwarzCardSerial");
                        });

                    b.Navigation("Name")
                        .IsRequired();

                    b.Navigation("Traits");
                });

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.WeissSchwarzCardOptionalInfo", b =>
                {
                    b.HasOne("Montage.Weiss.Tools.Entities.WeissSchwarzCard", "Card")
                        .WithMany("AdditionalInfo")
                        .HasForeignKey("Serial")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");
                });

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.WeissSchwarzCard", b =>
                {
                    b.Navigation("AdditionalInfo");
                });
#pragma warning restore 612, 618
        }
    }
}
