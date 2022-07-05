using eauction.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models.Domain.Auction;
using Models.Domain.Bank;
using Models.Domain.Identity;
using Models.Domain.Notification;
using System;
using System.Collections.Generic;

namespace DataContext.Seeding
{
    public class Sample
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var db = new ApplicationDbContext(
                serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>()))
            {
                CustomRole [] roles = new CustomRole[3]
                { 
                    new CustomRole("System"), 
                    new CustomRole("SuperAdmin"),
                    new CustomRole("Admin")
                };

                var applicationStatus = new List<ApplicationStatus>()
                    {
                        new ApplicationStatus() { Id = 1, Name = "Received" },
                        new ApplicationStatus() { Id = 2, Name = "Approved" },
                        new ApplicationStatus() { Id = 3, Name = "Withdrawn By Applicant" },
                        new ApplicationStatus() { Id = 4, Name = "Rejected" },
                        new ApplicationStatus() { Id = 5, Name = "Processed" },
                    };

                var appSecurityDepositStatus = new List<Models.Domain.Bank.AppSecurityDepositStatus>()
                    {
                        new AppSecurityDepositStatus() { Id = 1, Name = "Received" },
                        new AppSecurityDepositStatus() { Id = 2, Name = "Returned" },
                    };

                var notificationStatus = new List<Models.Domain.Notification.NotificationStatus>()
                    {
                        new NotificationStatus() { Id = 1, Name = "Active" },
                        new NotificationStatus() { Id = 2, Name = "Closed" },
                    };

                var seriesStatus = new List<SeriesStatus>()
                    {
                        new SeriesStatus() { Id = 1, Name = "Newly Opened (Idle)" },
                        new SeriesStatus() { Id = 2, Name = "Registring Customers" },
                        new SeriesStatus() { Id = 3, Name = "Bidding" },
                        new SeriesStatus() { Id = 4, Name = "Bidding Closed" },
                        new SeriesStatus() { Id = 5, Name = "Series Closed" },
                    };

                var seriesCategory = new List<SeriesCategory>()
                    {
                        new SeriesCategory() { Id = 1, Name = "Motor Car / موٹر کار" },
                        new SeriesCategory() { Id = 3, Name = "Motor Cycle / موٹر سائیکل" },
                        new SeriesCategory() { Id = 4, Name = "Commercial / کمرشل" },
                    };

                var userType = new List<UserType>()
                    {
                        new UserType() { Id = 1, Name = "Individual" },
                        new UserType() { Id = 2, Name = "Company" },
                    };


                if (!db.Roles.AnyAsync().Result)
                    db.Roles.AddRange(roles);

                if (!db.ApplicationStatus.AnyAsync().Result)
                    db.ApplicationStatus.AddRange(applicationStatus);

                if (!db.AppSecurityDepositStatus.AnyAsync().Result)
                    db.AppSecurityDepositStatus.AddRange(appSecurityDepositStatus);

                if (!db.NotificationStatus.AnyAsync().Result)
                    db.NotificationStatus.AddRange(notificationStatus);

                if (!db.ApplicationStatus.AnyAsync().Result)
                    db.ApplicationStatus.AddRange(applicationStatus);

                if (!db.SeriesStatus.AnyAsync().Result)
                    db.SeriesStatus.AddRange(seriesStatus);

                if (!db.SeriesCategory.AnyAsync().Result)
                    db.SeriesCategory.AddRange(seriesCategory);

                if (!db.UserType.AnyAsync().Result)
                    db.UserType.AddRange(userType);

                /*
DELETE FROM ApplicationStatusHistory
DELETE FROM AppSecurityDepositStatusHistory
DELETE FROM AppSecurityDeposit
DELETE FROM Application

DELETE FROM SeriesNumber
DELETE FROM SeriesStatusHistory
DELETE FROM Series

DELETE FROM [Identity].UserIdentity

DELETE FROM [Identity].UserRoles
DELETE FROM [Identity].Users

                CREATE TYPE [dbo].[TYPE_Application] AS TABLE(
	                [Id] [int] NOT NULL,
	                [ChasisNumber] [varchar](50) NOT NULL,
	                [OwnerName] [varchar](70) NOT NULL
                )

INSERT INTO [dbo].[EmailSetting]([MessageType], [Sender], [Password], [Receiver], [Subject], [Body], [Host], [Port], [SenderName], [MessageTypeId]) VALUES (N'Email Confirmation', N'no-reply-excise@punjab.gov.pk', N'YdesrbnV9t', NULL, N'e-auction OTP', N'Your one time PIN is #otp @@@Use this PIN for verification of your account in e-auction.', N'103.226.216.248', 25, N'e-auction OTP', 1);
INSERT INTO [dbo].[EmailSetting]([MessageType], [Sender], [Password], [Receiver], [Subject], [Body], [Host], [Port], [SenderName], [MessageTypeId]) VALUES (N'Application Approval', N'no-reply-excise@punjab.gov.pk', N'YdesrbnV9t', NULL, N'Application Approval', N'Your application has been approved In e-auction.@@@ AIN: #AIN', N'103.226.216.248', 25, N'e-auction OTP', 2);
INSERT INTO [dbo].[EmailSetting]([MessageType], [Sender], [Password], [Receiver], [Subject], [Body], [Host], [Port], [SenderName], [MessageTypeId]) VALUES (N'Application Submission', N'no-reply-excise@punjab.gov.pk', N'YdesrbnV9t', NULL, N'Application Submission', N'Dear Citizen, your application for attractive registration mark has been SUCCESSFULLY sent to Motor Registration Authority. @@@You are requested to print the challan form to deposit participation fee. @@@The payment receipt of this challan form along with pay order in the name of MRA of District you desire to visit may be submitted in MRA office within 02 days after closing date of registration. @@@Once, your application is approved by MRA you can participate in the E-Auction proceedings. @@@Wish you best of luck', N'103.226.216.248', 25, N'e-auction', 3);
                     
                INSERT INTO [dbo].[APK]([Version]) VALUES (N'1');

                INSERT INTO [dbo].[District]([Name]) VALUES (N'Punjab');

                INSERT INTO [dbo].[BankDocumentType]([Id], [Name]) VALUES (1, N'Pay Order');
                INSERT INTO [dbo].[BankDocumentType]([Id], [Name]) VALUES (2, N'Cheque');


                INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51000, N'Already applied', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51001, N'Currently, applications are not being accepted.', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51002, N'Complete your profile before you apply. CNIC or NTN is required to generate AIN.', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51003, N'Atleast bid 500 or higher.', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51004, N'Invalid bidding application', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51005, N'Atleast bid 500 higher than the last Bid', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51006, N'Cannot bid less than reserve price.', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51007, N'Bidding not started yet.', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51008, N'Atleast bid 500 higher than the highest bid', NULL);
INSERT INTO [dbo].[CustomMessage]([SqlCode], [Message], [Description]) VALUES (51009, N'Bidding closed.', NULL);

                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (1, N'Al Baraka Bank');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (2, N'Al Baraka Bank (Pakistan) Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (3, N'Allied Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (4, N'Allied Islamic Bank');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (5, N'Askari Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (6, N'Askari Bank Ltd');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (7, N'Bank Al Habib Islamic Banking');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (8, N'Bank Alfalah Islamic');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (9, N'Bank Alfalah Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (10, N'Bank Al-Habib Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (11, N'Bank of Punjab Islamic Banking');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (12, N'BankIslami Pakistan Limited');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (13, N'BankIslami Pakistan Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (14, N'Citi Bank');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (15, N'Deutsche Bank A.G.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (16, N'Dubai Islamic Bank');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (17, N'Dubai Islamic Bank Pakistan Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (18, N'Faysal Bank (Islamic)');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (19, N'Faysal Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (20, N'First Women Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (21, N'Habib Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (22, N'Habib Metropolitan Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (23, N'HBL Islamic Banking');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (24, N'Industrial and Commercial Bank of China');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (25, N'Industrial Development Bank of Pakistan');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (26, N'JS Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (27, N'MCB Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (28, N'MCB Islamic Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (29, N'MCB Islamic Banking');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (30, N'Meezan Bank Limited');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (31, N'Meezan Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (32, N'National Bank of Pakistan');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (33, N'S.M.E. Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (34, N'Samba Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (35, N'Silk Bank Limited');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (36, N'Sindh Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (37, N'Soneri Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (38, N'Soneri Mustaqeem Islamic Bank');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (39, N'Standard Chartered Bank (Pakistan) Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (40, N'State Bank of Pakistan');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (41, N'Summit Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (42, N'The Bank of Khyber.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (43, N'The Bank of Punjab.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (44, N'The Bank of Tokyo-Mitsubishi UFJ');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (45, N'The Punjab Provincial Cooperative Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (46, N'UBL Islamic Banking');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (47, N'United Bank Limited.');
                INSERT INTO [dbo].[Bank]([Id], [Name]) VALUES (48, N'Zarai Taraqiati Bank Limited.');


                 */

                db.SaveChanges();
            }
        }
    }
}
