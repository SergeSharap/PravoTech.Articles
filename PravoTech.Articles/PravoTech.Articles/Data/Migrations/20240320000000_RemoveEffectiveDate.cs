using Microsoft.EntityFrameworkCore.Migrations;

namespace PravoTech.Articles.Data.Migrations
{
    public partial class RemoveEffectiveDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Articles");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Articles");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "COALESCE([UpdatedAt], [CreatedAt])");
        }
    }
} 