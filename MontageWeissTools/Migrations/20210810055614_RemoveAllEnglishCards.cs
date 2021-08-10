using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.Weiss.Tools.Migrations
{
    public partial class RemoveAllEnglishCards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MigrationLog",
                columns: table => new
                {
                    LogID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Activity = table.Column<int>(type: "INTEGER", nullable: false),
                    Target = table.Column<string>(type: "TEXT", nullable: true),
                    IsDone = table.Column<bool>(type: "INTEGER", nullable: false),
                    DateAdded = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrationLog", x => x.LogID);
                });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 1, 1, new DateTime(2021, 8, 10, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.8.0\"}" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MigrationLog");
        }
    }
}
