using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.Weiss.Tools.Migrations
{
    public partial class InitialSetup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeissSchwarzCards",
                columns: table => new
                {
                    Serial = table.Column<string>(nullable: false),
                    Name_EN = table.Column<string>(nullable: true),
                    Name_JP = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    Color = table.Column<int>(nullable: false),
                    Side = table.Column<int>(nullable: false),
                    Rarity = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: true),
                    Cost = table.Column<int>(nullable: true),
                    Soul = table.Column<int>(nullable: true),
                    Power = table.Column<int>(nullable: true),
                    Triggers = table.Column<string>(nullable: true),
                    Flavor = table.Column<string>(nullable: true),
                    Effect = table.Column<string>(nullable: true),
                    Images = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeissSchwarzCards", x => x.Serial);
                });

            migrationBuilder.CreateTable(
                name: "WeissSchwarzCards_Traits",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EN = table.Column<string>(nullable: true),
                    JP = table.Column<string>(nullable: true),
                    WeissSchwarzCardSerial = table.Column<string>(nullable: false)
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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeissSchwarzCards_Traits");

            migrationBuilder.DropTable(
                name: "WeissSchwarzCards");
        }
    }
}
