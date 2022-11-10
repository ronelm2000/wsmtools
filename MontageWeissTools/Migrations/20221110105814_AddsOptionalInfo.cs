using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.Weiss.Tools.Migrations
{
    public partial class AddsOptionalInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Settings",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "WeissSchwarzCardOptionalInfo",
                columns: table => new
                {
                    Serial = table.Column<string>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    ValueJSON = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeissSchwarzCardOptionalInfo", x => new { x.Serial, x.Key });
                    table.ForeignKey(
                        name: "FK_WeissSchwarzCardOptionalInfo_WeissSchwarzCards_Serial",
                        column: x => x.Serial,
                        principalTable: "WeissSchwarzCards",
                        principalColumn: "Serial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 3, 1, new DateTime(2021, 12, 14, 10, 2, 57, 51, DateTimeKind.Local).AddTicks(8029), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.10.0\"}" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeissSchwarzCardOptionalInfo");

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 3);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
