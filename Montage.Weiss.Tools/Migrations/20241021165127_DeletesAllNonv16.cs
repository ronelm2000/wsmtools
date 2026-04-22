using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montage.Weiss.Tools.Migrations
{
    /// <inheritdoc />
    public partial class DeletesAllNonv16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 6, 1, new DateTime(2024, 10, 22, 0, 45, 56, 150, DateTimeKind.Local).AddTicks(6113), false, "{\"Language\": \"ALL\", \"VersionLessThan\": \"0.16.0\"}" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 6);
        }
    }
}
