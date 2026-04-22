using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montage.Weiss.Tools.Migrations
{
    /// <inheritdoc />
    public partial class RevampsTraitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeissSchwarzCards_Traits");

            migrationBuilder.AlterColumn<string>(
                name: "VersionTimestamp",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Triggers",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Rarity",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Images",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Effect",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ValueJSON",
                table: "WeissSchwarzCardOptionalInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                table: "MigrationLog",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "Traits",
                columns: table => new
                {
                    TraitID = table.Column<Guid>(type: "TEXT", nullable: false),
                    Serial = table.Column<string>(type: "TEXT", nullable: false),
                    EN = table.Column<string>(type: "TEXT", nullable: true),
                    JP = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traits", x => new { x.TraitID, x.Serial });
                    table.ForeignKey(
                        name: "FK_Traits_WeissSchwarzCards_Serial",
                        column: x => x.Serial,
                        principalTable: "WeissSchwarzCards",
                        principalColumn: "Serial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MigrationLog",
                columns: new[] { "LogID", "Activity", "DateAdded", "IsDone", "Target" },
                values: new object[] { 4, 1, new DateTime(2022, 11, 28, 20, 51, 28, 983, DateTimeKind.Local).AddTicks(6076), false, "{\"Language\": \"EN\", \"VersionLessThan\": \"0.12.0\"}" });

            migrationBuilder.CreateIndex(
                name: "IX_Traits_Serial",
                table: "Traits",
                column: "Serial");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Traits");

            migrationBuilder.DeleteData(
                table: "MigrationLog",
                keyColumn: "LogID",
                keyValue: 4);

            migrationBuilder.AlterColumn<string>(
                name: "VersionTimestamp",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Triggers",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Remarks",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Rarity",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Images",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Effect",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "ValueJSON",
                table: "WeissSchwarzCardOptionalInfo",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Target",
                table: "MigrationLog",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "WeissSchwarzCards_Traits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EN = table.Column<string>(type: "TEXT", nullable: true),
                    JP = table.Column<string>(type: "TEXT", nullable: true),
                    WeissSchwarzCardSerial = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeissSchwarzCards_Traits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeissSchwarzCards_Traits_WeissSchwarzCards_WeissSchwarzCardSerial",
                        column: x => x.WeissSchwarzCardSerial,
                        principalTable: "WeissSchwarzCards",
                        principalColumn: "Serial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeissSchwarzCards_Traits_WeissSchwarzCardSerial",
                table: "WeissSchwarzCards_Traits",
                column: "WeissSchwarzCardSerial");
        }
    }
}
