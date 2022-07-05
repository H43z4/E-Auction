using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataContext.Migrations
{
    public partial class epayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ePayAPIs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApiName = table.Column<string>(maxLength: 20, nullable: true),
                    RequestURL = table.Column<string>(nullable: true),
                    AccessToken = table.Column<string>(maxLength: 1000, nullable: true),
                    ClientId = table.Column<string>(maxLength: 100, nullable: true),
                    ClientSecretKey = table.Column<string>(maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    TokenExpiredOn = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ePayAPIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ePayApplication",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationId = table.Column<int>(nullable: false),
                    PSId = table.Column<string>(maxLength: 20, nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    PaymentStatusId = table.Column<int>(nullable: false),
                    AmountPaid = table.Column<int>(nullable: false),
                    PaidOn = table.Column<DateTime>(nullable: false),
                    BankCode = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ePayApplication", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ePayApplication_Application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Application",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ePayBankAccountInfo",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountHead = table.Column<string>(nullable: true),
                    AccountNumber = table.Column<string>(nullable: true),
                    AmountTransfer = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ePayBankAccountInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ePayApplication_ApplicationId",
                table: "ePayApplication",
                column: "ApplicationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ePayAPIs");

            migrationBuilder.DropTable(
                name: "ePayApplication");

            migrationBuilder.DropTable(
                name: "ePayBankAccountInfo");
        }
    }
}
