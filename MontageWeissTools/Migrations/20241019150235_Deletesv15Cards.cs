using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montage.Weiss.Tools.Migrations
{
    /// <inheritdoc />
    public partial class Deletesv15Cards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 5, 1, new DateTime(2024, 10, 19, 22, 58, 25, 339, DateTimeKind.Local).AddTicks(8906), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.15.0\"}" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 5);
        }
    }
}
