using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.Weiss.Tools.Migrations
{
    public partial class AddsVersionTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VersionTimestamp",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true);
            migrationBuilder.Sql("UPDATE WeissSchwarzCards SET VersionTimestamp='0.6.1'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VersionTimestamp",
                table: "WeissSchwarzCards");
        }
    }
}
