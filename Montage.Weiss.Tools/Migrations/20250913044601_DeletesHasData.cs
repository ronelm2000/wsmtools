using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Montage.Weiss.Tools.Migrations
{
    /// <inheritdoc />
    public partial class DeletesHasData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2021, 8, 10, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.8.0\"}" },
                    { 2, 1, new DateTime(2021, 8, 11, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029), false, "{\"Language\": \"ALL\", \"VersionLessThan\": \"0.9.0\"}" },
                    { 3, 1, new DateTime(2021, 12, 14, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.10.0\"}" },
                    { 4, 1, new DateTime(2022, 11, 28, 20, 51, 28, 983, DateTimeKind.Local).AddTicks(6076), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.12.0\"}" },
                    { 5, 1, new DateTime(2024, 10, 19, 22, 58, 25, 339, DateTimeKind.Local).AddTicks(8906), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.15.0\"}" },
                    { 6, 1, new DateTime(2024, 10, 22, 0, 45, 56, 150, DateTimeKind.Local).AddTicks(6113), false, "{\"Language\": \"ALL\", \"VersionLessThan\": \"0.16.0\"}" },
                    { 7, 1, new DateTime(2025, 9, 12, 22, 57, 46, 681, DateTimeKind.Local).AddTicks(3155), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.18.0\"}" }
                });
        }
    }
}
