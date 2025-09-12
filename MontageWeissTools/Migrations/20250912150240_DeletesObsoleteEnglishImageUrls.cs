using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montage.Weiss.Tools.Migrations
{
    /// <inheritdoc />
    public partial class DeletesObsoleteEnglishImageUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 7, 1, new DateTime(2025, 9, 12, 22, 57, 46, 681, DateTimeKind.Local).AddTicks(3155), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.18.0\"}" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 7);
        }
    }
}
