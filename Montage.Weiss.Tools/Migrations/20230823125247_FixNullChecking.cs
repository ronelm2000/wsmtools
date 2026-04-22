using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Montage.Weiss.Tools.Migrations
{
    /// <inheritdoc />
    public partial class FixNullChecking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValueJSON",
                table: "WeissSchwarzCardOptionalInfo",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValueJSON",
                table: "WeissSchwarzCardOptionalInfo",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
