using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataContext.Migrations
{
    public partial class _3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                table: "Series",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                table: "Series",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SeriesLogs",
                columns: table => new
                {
                    SeriesLogsId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedBy = table.Column<int>(nullable: true),
                    ModifiedOn = table.Column<DateTime>(nullable: true),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    SeriesId = table.Column<int>(nullable: false),
                    DistrictId = table.Column<int>(nullable: false),
                    AuctionYear = table.Column<int>(nullable: false),
                    SeriesCategoryId = table.Column<int>(nullable: false),
                    SeriesStatusId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    AuctionStartDateTime = table.Column<DateTime>(nullable: false),
                    AuctionEndDateTime = table.Column<DateTime>(nullable: false),
                    RegStartDateTime = table.Column<DateTime>(nullable: false),
                    RegEndDateTime = table.Column<DateTime>(nullable: false),
                    IsReauctioning = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesLogs", x => x.SeriesLogsId);
                    table.ForeignKey(
                        name: "FK_SeriesLogs_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesLogs_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesLogs_Users_ModifiedBy",
                        column: x => x.ModifiedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesLogs_SeriesCategory_SeriesCategoryId",
                        column: x => x.SeriesCategoryId,
                        principalTable: "SeriesCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesLogs_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesLogs_SeriesStatus_SeriesStatusId",
                        column: x => x.SeriesStatusId,
                        principalTable: "SeriesStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Series_ModifiedBy",
                table: "Series",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesLogs_CreatedBy",
                table: "SeriesLogs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesLogs_DistrictId",
                table: "SeriesLogs",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesLogs_ModifiedBy",
                table: "SeriesLogs",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesLogs_SeriesCategoryId",
                table: "SeriesLogs",
                column: "SeriesCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesLogs_SeriesId",
                table: "SeriesLogs",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesLogs_SeriesStatusId",
                table: "SeriesLogs",
                column: "SeriesStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Series_Users_ModifiedBy",
                table: "Series",
                column: "ModifiedBy",
                principalSchema: "Identity",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Series_Users_ModifiedBy",
                table: "Series");

            migrationBuilder.DropTable(
                name: "SeriesLogs");

            migrationBuilder.DropIndex(
                name: "IX_Series_ModifiedBy",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                table: "Series");
        }
    }
}
