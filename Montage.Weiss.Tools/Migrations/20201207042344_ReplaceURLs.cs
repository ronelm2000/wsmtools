using Microsoft.EntityFrameworkCore.Migrations;

namespace Montage.Weiss.Tools.Migrations
{
    public partial class ReplaceURLs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE WeissSchwarzCards " +
                "SET Images = REPLACE(" +
                "Images, " +
                "'https://s3-ap-northeast-1.amazonaws.com/static.ws-tcg.com/wordpress/wp-content/cardimages/', " +
                "'https://ws-tcg.com/wordpress/wp-content/images/cardlist/'" +
                ")");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE WeissSchwarzCards " +
                "SET Images = REPLACE(" +
                "Images, " +
                "'https://ws-tcg.com/wordpress/wp-content/images/cardlist/', " +
                "'https://s3-ap-northeast-1.amazonaws.com/static.ws-tcg.com/wordpress/wp-content/cardimages/'" +
                ")");
        }
    }
}
