using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataContext.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AuctionEndDateTime",
                table: "SeriesNumber",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReauctioning",
                table: "Series",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuctionEndDateTime",
                table: "SeriesNumber");

            migrationBuilder.DropColumn(
                name: "IsReauctioning",
                table: "Series");
        }
    }
}
