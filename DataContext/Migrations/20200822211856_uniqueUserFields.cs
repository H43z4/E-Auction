using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataContext.Migrations
{
    public partial class uniqueUserFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomMessage_Users_CreatedBy",
                table: "CustomMessage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomMessage",
                table: "CustomMessage");

            migrationBuilder.DropIndex(
                name: "IX_CustomMessage_CreatedBy",
                table: "CustomMessage");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "CustomMessage");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CustomMessage");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "CustomMessage");

            migrationBuilder.DropColumn(
                name: "IsSoftDeleted",
                table: "CustomMessage");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "Identity",
                table: "Users",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "Identity",
                table: "Users",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Email",
                schema: "Identity",
                table: "Users",
                column: "Email");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_PhoneNumber",
                schema: "Identity",
                table: "Users",
                column: "PhoneNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomMessage",
                table: "CustomMessage",
                column: "SqlCode");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Email",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_PhoneNumber",
                schema: "Identity",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CustomMessage",
                table: "CustomMessage");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "Identity",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "Identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 256);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "CustomMessage",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "CustomMessage",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "CustomMessage",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<bool>(
                name: "IsSoftDeleted",
                table: "CustomMessage",
                type: "bit",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CustomMessage",
                table: "CustomMessage",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMessage_CreatedBy",
                table: "CustomMessage",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomMessage_Users_CreatedBy",
                table: "CustomMessage",
                column: "CreatedBy",
                principalSchema: "Identity",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
