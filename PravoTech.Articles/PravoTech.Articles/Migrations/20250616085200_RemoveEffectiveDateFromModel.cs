using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PravoTech.Articles.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEffectiveDateFromModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Articles_EffectiveDate",
                table: "Articles");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "Articles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "Articles",
                type: "datetime2",
                nullable: false,
                computedColumnSql: "ISNULL([UpdatedAt], [CreatedAt])",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_EffectiveDate",
                table: "Articles",
                column: "EffectiveDate");
        }
    }
}
