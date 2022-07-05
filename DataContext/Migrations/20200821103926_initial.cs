using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DataContext.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Identity");

            migrationBuilder.CreateTable(
                name: "ApplicationStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSecurityDepositStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSecurityDepositStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankDocumentType",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankDocumentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "District",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_District", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailSetting",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageType = table.Column<string>(maxLength: 50, nullable: true),
                    Sender = table.Column<string>(maxLength: 100, nullable: true),
                    Password = table.Column<string>(maxLength: 100, nullable: true),
                    Receiver = table.Column<string>(maxLength: 100, nullable: true),
                    Subject = table.Column<string>(maxLength: 100, nullable: true),
                    Body = table.Column<string>(maxLength: 300, nullable: true),
                    Host = table.Column<string>(maxLength: 100, nullable: true),
                    Port = table.Column<int>(nullable: false),
                    SenderName = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSetting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeriesCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SeriesStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserType",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: true),
                    SecurityStamp = table.Column<string>(nullable: true),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    UserTypeId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    FatherHusbandName = table.Column<string>(maxLength: 50, nullable: true),
                    CNIC = table.Column<string>(maxLength: 13, nullable: false),
                    NTN = table.Column<string>(maxLength: 25, nullable: true),
                    Company = table.Column<string>(maxLength: 100, nullable: true),
                    Address = table.Column<string>(maxLength: 100, nullable: true),
                    PasswordExpiryDate = table.Column<DateTime>(nullable: true),
                    CreatedBy = table.Column<int>(nullable: true),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.UniqueConstraint("AK_Users_CNIC", x => x.CNIC);
                    table.ForeignKey(
                        name: "FK_Users_UserType_UserTypeId",
                        column: x => x.UserTypeId,
                        principalSchema: "Identity",
                        principalTable: "UserType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CustomMessage",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    SqlCode = table.Column<int>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomMessage_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    Heading = table.Column<string>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Data = table.Column<string>(nullable: true),
                    NotificationStatusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notification_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Notification_NotificationStatus_NotificationStatusId",
                        column: x => x.NotificationStatusId,
                        principalTable: "NotificationStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Series",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    DistrictId = table.Column<int>(nullable: false),
                    AuctionYear = table.Column<int>(nullable: false),
                    SeriesCategoryId = table.Column<int>(nullable: false),
                    SeriesStatusId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: true),
                    AuctionStartDateTime = table.Column<DateTime>(nullable: false),
                    AuctionEndDateTime = table.Column<DateTime>(nullable: false),
                    RegStartDateTime = table.Column<DateTime>(nullable: false),
                    RegEndDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Series", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Series_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Series_District_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "District",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Series_SeriesCategory_SeriesCategoryId",
                        column: x => x.SeriesCategoryId,
                        principalTable: "SeriesCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Series_SeriesStatus_SeriesStatusId",
                        column: x => x.SeriesStatusId,
                        principalTable: "SeriesStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserIdentity",
                schema: "Identity",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(nullable: false),
                    OTP = table.Column<int>(nullable: false),
                    OtpExpiryOn = table.Column<DateTime>(nullable: false),
                    MobilePhoneAppId = table.Column<string>(nullable: true),
                    MobilePhoneAppIdCreatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdentity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIdentity_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserLogins",
                schema: "Identity",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    RoleId = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "Identity",
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                schema: "Identity",
                columns: table => new
                {
                    UserId = table.Column<int>(nullable: false),
                    LoginProvider = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 128, nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NotificationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    NotificationId = table.Column<int>(nullable: false),
                    NotificationStatusId = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationStatusHistory_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationStatusHistory_Notification_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notification",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_NotificationStatusHistory_NotificationStatus_NotificationStatusId",
                        column: x => x.NotificationStatusId,
                        principalTable: "NotificationStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SeriesNumber",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    SeriesId = table.Column<int>(nullable: false),
                    AuctionNumber = table.Column<string>(nullable: true),
                    ReservePrice = table.Column<int>(nullable: false),
                    IsAuctionable = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesNumber", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeriesNumber_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesNumber_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SeriesStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    SeriesId = table.Column<int>(nullable: false),
                    SeriesStatusId = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeriesStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeriesStatusHistory_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesStatusHistory_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeriesStatusHistory_SeriesStatus_SeriesStatusId",
                        column: x => x.SeriesStatusId,
                        principalTable: "SeriesStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Application",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    AIN = table.Column<string>(maxLength: 50, nullable: true),
                    CustomerId = table.Column<int>(nullable: false),
                    SeriesCategoryId = table.Column<int>(nullable: false),
                    SeriesId = table.Column<int>(nullable: false),
                    SeriesNumberId = table.Column<int>(nullable: false),
                    ChasisNumber = table.Column<string>(maxLength: 50, nullable: false),
                    ApplicationStatusId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Application", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Application_ApplicationStatus_ApplicationStatusId",
                        column: x => x.ApplicationStatusId,
                        principalTable: "ApplicationStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Application_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Application_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Application_SeriesCategory_SeriesCategoryId",
                        column: x => x.SeriesCategoryId,
                        principalTable: "SeriesCategory",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Application_Series_SeriesId",
                        column: x => x.SeriesId,
                        principalTable: "Series",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Application_SeriesNumber_SeriesNumberId",
                        column: x => x.SeriesNumberId,
                        principalTable: "SeriesNumber",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ApplicationStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    ApplicationId = table.Column<int>(nullable: false),
                    ApplicationStatusId = table.Column<int>(nullable: false),
                    Comments = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationStatusHistory_Application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Application",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationStatusHistory_ApplicationStatus_ApplicationStatusId",
                        column: x => x.ApplicationStatusId,
                        principalTable: "ApplicationStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationStatusHistory_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppSecurityDeposit",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    ApplicationId = table.Column<int>(nullable: false),
                    AppSecurityDepositStatusId = table.Column<int>(nullable: false),
                    BankId = table.Column<int>(nullable: false),
                    BankDocumentTypeId = table.Column<int>(nullable: false),
                    DocumentIdValue = table.Column<string>(maxLength: 100, nullable: false),
                    Worth = table.Column<int>(nullable: false),
                    Remarks = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSecurityDeposit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSecurityDeposit_AppSecurityDepositStatus_AppSecurityDepositStatusId",
                        column: x => x.AppSecurityDepositStatusId,
                        principalTable: "AppSecurityDepositStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSecurityDeposit_Application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Application",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSecurityDeposit_BankDocumentType_BankDocumentTypeId",
                        column: x => x.BankDocumentTypeId,
                        principalTable: "BankDocumentType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSecurityDeposit_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSecurityDeposit_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bids",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    ApplicationId = table.Column<int>(nullable: false),
                    SeriesNumberId = table.Column<int>(nullable: false),
                    BiddingPrice = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bids", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bids_Application_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Application",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bids_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bids_SeriesNumber_SeriesNumberId",
                        column: x => x.SeriesNumberId,
                        principalTable: "SeriesNumber",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AppSecurityDepositStatusHistory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()"),
                    IsSoftDeleted = table.Column<bool>(nullable: true),
                    AppSecurityDepositId = table.Column<int>(nullable: false),
                    AppSecurityDepositStatusId = table.Column<int>(nullable: false),
                    DiaryNumber = table.Column<string>(maxLength: 100, nullable: true),
                    Comments = table.Column<string>(maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSecurityDepositStatusHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSecurityDepositStatusHistory_AppSecurityDeposit_AppSecurityDepositId",
                        column: x => x.AppSecurityDepositId,
                        principalTable: "AppSecurityDeposit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSecurityDepositStatusHistory_AppSecurityDepositStatus_AppSecurityDepositStatusId",
                        column: x => x.AppSecurityDepositStatusId,
                        principalTable: "AppSecurityDepositStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AppSecurityDepositStatusHistory_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "Identity",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Application_ApplicationStatusId",
                table: "Application",
                column: "ApplicationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_CreatedBy",
                table: "Application",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Application_CustomerId",
                table: "Application",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_SeriesCategoryId",
                table: "Application",
                column: "SeriesCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_SeriesId",
                table: "Application",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_Application_SeriesNumberId",
                table: "Application",
                column: "SeriesNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistory_ApplicationId",
                table: "ApplicationStatusHistory",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistory_ApplicationStatusId",
                table: "ApplicationStatusHistory",
                column: "ApplicationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationStatusHistory_CreatedBy",
                table: "ApplicationStatusHistory",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDeposit_AppSecurityDepositStatusId",
                table: "AppSecurityDeposit",
                column: "AppSecurityDepositStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDeposit_ApplicationId",
                table: "AppSecurityDeposit",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDeposit_BankDocumentTypeId",
                table: "AppSecurityDeposit",
                column: "BankDocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDeposit_BankId",
                table: "AppSecurityDeposit",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDeposit_CreatedBy",
                table: "AppSecurityDeposit",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDepositStatusHistory_AppSecurityDepositId",
                table: "AppSecurityDepositStatusHistory",
                column: "AppSecurityDepositId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDepositStatusHistory_AppSecurityDepositStatusId",
                table: "AppSecurityDepositStatusHistory",
                column: "AppSecurityDepositStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_AppSecurityDepositStatusHistory_CreatedBy",
                table: "AppSecurityDepositStatusHistory",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_ApplicationId",
                table: "Bids",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_CreatedBy",
                table: "Bids",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_SeriesNumberId",
                table: "Bids",
                column: "SeriesNumberId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomMessage_CreatedBy",
                table: "CustomMessage",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_CreatedBy",
                table: "Notification",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_NotificationStatusId",
                table: "Notification",
                column: "NotificationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationStatusHistory_CreatedBy",
                table: "NotificationStatusHistory",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationStatusHistory_NotificationId",
                table: "NotificationStatusHistory",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationStatusHistory_NotificationStatusId",
                table: "NotificationStatusHistory",
                column: "NotificationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_CreatedBy",
                table: "Series",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Series_DistrictId",
                table: "Series",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_SeriesCategoryId",
                table: "Series",
                column: "SeriesCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Series_SeriesStatusId",
                table: "Series",
                column: "SeriesStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesNumber_CreatedBy",
                table: "SeriesNumber",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesNumber_SeriesId_AuctionNumber",
                table: "SeriesNumber",
                columns: new[] { "SeriesId", "AuctionNumber" },
                unique: true,
                filter: "[AuctionNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesStatusHistory_CreatedBy",
                table: "SeriesStatusHistory",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesStatusHistory_SeriesId",
                table: "SeriesStatusHistory",
                column: "SeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_SeriesStatusHistory_SeriesStatusId",
                table: "SeriesStatusHistory",
                column: "SeriesStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                schema: "Identity",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "Identity",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                schema: "Identity",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentity_UserId",
                schema: "Identity",
                table: "UserIdentity",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                schema: "Identity",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                schema: "Identity",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "Identity",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "Identity",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserTypeId",
                schema: "Identity",
                table: "Users",
                column: "UserTypeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationStatusHistory");

            migrationBuilder.DropTable(
                name: "AppSecurityDepositStatusHistory");

            migrationBuilder.DropTable(
                name: "Bids");

            migrationBuilder.DropTable(
                name: "CustomMessage");

            migrationBuilder.DropTable(
                name: "EmailSetting");

            migrationBuilder.DropTable(
                name: "NotificationStatusHistory");

            migrationBuilder.DropTable(
                name: "SeriesStatusHistory");

            migrationBuilder.DropTable(
                name: "RoleClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserClaims",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserIdentity",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserLogins",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserRoles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "UserTokens",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AppSecurityDeposit");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Roles",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "AppSecurityDepositStatus");

            migrationBuilder.DropTable(
                name: "Application");

            migrationBuilder.DropTable(
                name: "BankDocumentType");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "NotificationStatus");

            migrationBuilder.DropTable(
                name: "ApplicationStatus");

            migrationBuilder.DropTable(
                name: "SeriesNumber");

            migrationBuilder.DropTable(
                name: "Series");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "Identity");

            migrationBuilder.DropTable(
                name: "District");

            migrationBuilder.DropTable(
                name: "SeriesCategory");

            migrationBuilder.DropTable(
                name: "SeriesStatus");

            migrationBuilder.DropTable(
                name: "UserType",
                schema: "Identity");
        }
    }
}
