﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Montage.Weiss.Tools.Entities;

namespace Montage.Weiss.Tools.Migrations
{
    [DbContext(typeof(CardDatabaseContext))]
    [Migration("20210129073227_AddsVersionTimestamp")]
    partial class AddsVersionTimestamp
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.Setting", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("Key");

                    b.ToTable("Settings");
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
                        .HasColumnType("TEXT");

                    b.Property<string>("Flavor")
                        .HasColumnType("TEXT");

                    b.Property<string>("Images")
                        .HasColumnType("TEXT");

                    b.Property<int?>("Level")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Power")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Rarity")
                        .HasColumnType("TEXT");

                    b.Property<string>("Remarks")
                        .HasColumnType("TEXT");

                    b.Property<int>("Side")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Soul")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Triggers")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<string>("VersionTimestamp")
                        .HasColumnType("TEXT");

                    b.HasKey("Serial");

                    b.ToTable("WeissSchwarzCards");
                });

            modelBuilder.Entity("Montage.Weiss.Tools.Entities.WeissSchwarzCard", b =>
                {
                    b.OwnsOne("Montage.Weiss.Tools.Entities.MultiLanguageString", "Name", b1 =>
                        {
                            b1.Property<string>("WeissSchwarzCardSerial")
                                .HasColumnType("TEXT");

                            b1.Property<string>("EN")
                                .HasColumnType("TEXT");

                            b1.Property<string>("JP")
                                .HasColumnType("TEXT");

                            b1.HasKey("WeissSchwarzCardSerial");

                            b1.ToTable("WeissSchwarzCards");

                            b1.WithOwner()
                                .HasForeignKey("WeissSchwarzCardSerial");
                        });

                    b.OwnsMany("Montage.Weiss.Tools.Entities.MultiLanguageString", "Traits", b1 =>
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

                            b1.HasIndex("WeissSchwarzCardSerial");

                            b1.ToTable("WeissSchwarzCards_Traits");

                            b1.WithOwner()
                                .HasForeignKey("WeissSchwarzCardSerial");
                        });

                    b.Navigation("Name");

                    b.Navigation("Traits");
                });
#pragma warning restore 612, 618
        }
    }
}
