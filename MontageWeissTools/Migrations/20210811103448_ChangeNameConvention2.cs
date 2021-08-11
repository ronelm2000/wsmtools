using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.Weiss.Tools.Migrations
{
    public partial class ChangeNameConvention2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "WeissSchwarzCards");

            migrationBuilder.CreateTable(
                name: "WeissSchwarzCards_Names",
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
                    table.PrimaryKey("PK_WeissSchwarzCards_Names", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeissSchwarzCards_Names_WeissSchwarzCards_WeissSchwarzCardSerial",
                        column: x => x.WeissSchwarzCardSerial,
                        principalTable: "WeissSchwarzCards",
                        principalColumn: "Serial",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeissSchwarzCards_Names_WeissSchwarzCardSerial",
                table: "WeissSchwarzCards_Names",
                column: "WeissSchwarzCardSerial",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeissSchwarzCards_Names");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WeissSchwarzCards",
                type: "TEXT",
                nullable: true);
        }
    }
}
