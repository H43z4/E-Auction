using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using eauction.Data;
using Models.Views.Auction;
using eauction.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using eauction.Infrastructure;
using NotificationServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using eauction.Helpers;
using Models.Domain.Identity;

namespace eauction.Controllers
{
    [Authorize(Roles = "Special")]
    public class SpecialController : BaseController
    {
        private readonly IHubContext<NotificationHub> notificationHub;
        private readonly IWebHostEnvironment webHostEnvironment;

        public SpecialController(ILogger<HomeController> logger,
            IConfiguration configuration,
            ApplicationDbContext applicationDbContext,
            IHubContext<NotificationHub> hub,
            //ISmsSender smsSender,
            INotificationManager notificationManager,
            IWebHostEnvironment env)
                : base(logger, configuration, applicationDbContext, notificationManager)
        {
            //base(logger, configuration);
            this.notificationHub = hub;
            this.webHostEnvironment = env;
        }

        private async Task SendMessages(int customerId, string AIN)
        {
            User user = this.AuctionService.GetUser(customerId);

            var emailSetting = this.AuctionService.GetEmailSetting(2);  //"Application Approval"

            emailSetting.Receiver = user.Email;
            emailSetting.Body = emailSetting.Body.Replace("@@@", Environment.NewLine).Replace("#AIN", AIN);

            await this.notificationManager.SendMessage(user.PhoneNumber, emailSetting);
        }

        public IActionResult GetAuctionSeries()   //Registring Customers
        {
            var dataset = this.VIPService.GetAuctionSeries();

            var auctionSeries = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(dataset.Tables[0]);

            return View(auctionSeries);
        }

        public IActionResult GetAuctionSeriesNumber(int seriesId)
        {
            var dataset = this.VIPService.GetAuctionSeriesNumber(this.UserId, seriesId);

            if (dataset.Tables[0].Rows.Count == 0)
            {
                return View("Default");
            }

            ViewBag.series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(dataset.Tables[0]).FirstOrDefault();
            var auctionSeriesNumbers = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.SeriesNumber>(dataset.Tables[1]);

            return View(auctionSeriesNumbers);
        }

        [HttpPost("SaveApplications")]
        public async Task<IActionResult> SaveApplications([FromBody]List<Models.Views.Input.Application> applications)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    status = false,
                    msg = string.Join("\n", ModelState.Values
                                        .SelectMany(x => x.Errors)
                                        .Select(x => x.ErrorMessage))
                });
            }

            try
            {
                var ds = this.AuctionService.SaveApplications(this.UserId, applications.Select(x => new
                {
                    x.Id,
                    AIN = "",
                    ApplicationStatusId = 0,
                    x.ChasisNumber,
                    CustomerId = 0,
                    x.OwnerName,
                    PSId = "",
                    AmountPaid = 0,
                    BankCode = "",
                    PaidOn = DateTime.Now,
                    PaymentStatusId = 0
                })
                    .ToList()
                    .ToDataTable());

                var ePayAPIs = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.EPay.ePayAPIs>(ds.Tables[0]).ToList();
                var ePayApplications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.EPay.ePayApplication>(ds.Tables[1]).ToList();

                EPayment.EPaymentService ePaymentService = new EPayment.EPaymentService(ePayAPIs);

                var ePayApps = await ePaymentService.GeneratePSIdAsync(ePayApplications);

                this.AuctionService.SavePSIds(ePayApps.Select(x => new
                {
                    Id = x.deptTransactionId,
                    AIN = "",
                    ApplicationStatusId = 0,
                    ChasisNumber = "",
                    CustomerId = 0,
                    OwnerName = "",
                    PSId = x.psId,
                    AmountPaid = 0,
                    BankCode = "",
                    PaidOn = DateTime.Now,
                    PaymentStatusId = 0
                })
                    .ToList()
                    .ToDataTable());
            }
            catch (SqlException ex)
            {
                return Json(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                });
            }
            catch
            {
                return Json(new
                {
                    status = false,
                    errCode = 0
                });
            }

            try
            {
                var user = this.AuctionService.GetUser(this.UserId);
                var emailSetting = this.AuctionService.GetEmailSetting(3);


                emailSetting.Body = emailSetting.Body.Replace("@@@", Environment.NewLine);
                emailSetting.Receiver = user.Email;

                await this.notificationManager.SendMessage(user.PhoneNumber, emailSetting);
            }
            catch
            {
            }

            return Json(new
            {
                status = true,
            });
        }

        public IActionResult GetApplications()
        {
            DataSet ds = this.AuctionService.GetApplications(this.UserId);

            var applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]);

            return View(applications);
        }

        public IActionResult FindApplications(Models.Views.Auction.Application applicationFilter)
        {
            DataSet ds = this.AuctionService.FindApplications_1(applicationFilter);
            var applicationsCount = System.Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

            var applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[1]).ToList();

            GenericPagedList<Models.Views.Auction.Application> genericPagedList = new GenericPagedList<Application>();

            genericPagedList.ListOfItems = applications;

            Pager pager = new Pager(applicationsCount, applicationFilter.PageNumber, applicationFilter.PageSize, 100);

            var recordsPerPage = new List<SelectListItem>
            {
                new SelectListItem { Value = "20", Text = "20" },
                new SelectListItem { Value = "50", Text = "50" },
            };

            var pageNumbers = new List<SelectListItem>();

            foreach (var pageNumber in pager.Pages)
            {
                var pageNo = pageNumber.ToString();
                pageNumbers.Add(new SelectListItem { Value = pageNo, Text = pageNo });
            };

            ViewBag.recordsPerPage = recordsPerPage;
            ViewBag.pageNumbers = pageNumbers;

            genericPagedList.Pager = pager;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Applications", genericPagedList);
            }

            var LOVs = this.AuctionService.GetLOVs();
            var seriesCategory = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.SeriesCategory>(LOVs.Tables[0]).ToList();
            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(LOVs.Tables[1]).ToList();
            var applicationStatus = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.ApplicationStatus>(LOVs.Tables[2]).ToList();

            ViewBag.SeriesCategory = new SelectList(seriesCategory, "Id", "Name");
            ViewBag.Series = new SelectList(series, "Id", "SeriesName");
            ViewBag.ApplicationStatus = new SelectList(applicationStatus, "Id", "Name");

            return View(genericPagedList);
        }

        public IActionResult GetApplicationDetail(int applicationId)
        {
            DataSet ds = this.AuctionService.GetApplicationDetail(applicationId);

            var application = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).FirstOrDefault();

            if (application != null)
            {
                application.ApplicationStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.ApplicationStatusHistory>(ds.Tables[1]).ToList();

                application.AppSecurityDeposit = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Bank.AppSecurityDeposit>(ds.Tables[2]).FirstOrDefault();

                if (application.ApplicationStatusId == 1)
                {
                    var banks = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Bank.Bank>(ds.Tables[4]).ToList();
                    var bankDocumentType = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Bank.BankDocumentType>(ds.Tables[5]).ToList();

                    application.AppSecurityDeposit = application.AppSecurityDeposit ?? new Models.Views.Bank.AppSecurityDeposit();

                    application.AppSecurityDeposit.BankSelectList = new SelectList(banks, "Id", "Name");
                    application.AppSecurityDeposit.BankDocumentTypeSelectList = new SelectList(bankDocumentType, "Id", "Name");
                }
                else if (application.ApplicationStatusId == 2)
                {
                    var appSecurityDepositStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Bank.AppSecurityDepositStatusHistory>(ds.Tables[3]).ToList();

                    application.AppSecurityDeposit.AppSecurityDepositStatusHistory = appSecurityDepositStatusHistory;
                }
            }

            return View(application);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveApplicationAsync(Models.Views.Bank.AppSecurityDeposit appSecurityDeposit)
        {
            int customerId = 0;
            string AIN = string.Empty;

            var appSecurityDepositObj = this.AuctionService.SaveSecurity(appSecurityDeposit, this.UserId);
            var result = this.AuctionService.ApproveApplication(appSecurityDeposit.ApplicationId, this.UserId, out customerId, out AIN);

            if (result == true)
            {
                await this.SendMessages(customerId, AIN);
            }

            return RedirectToAction(nameof(GetApplicationDetail), new { applicationId = appSecurityDeposit.ApplicationId });
        }

        [HttpPost]
        public async Task<IActionResult> GetPSId([FromBody]Models.Views.Input.Application application)
        {
            try
            {
                var ds = this.AuctionService.GetPSIdInputModel(application.Id.ToString());

                var ePayAPIs = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.EPay.ePayAPIs>(ds.Tables[0]).ToList();
                var ePayApplications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.EPay.ePayApplication>(ds.Tables[1]).ToList();

                EPayment.EPaymentService ePaymentService = new EPayment.EPaymentService(ePayAPIs);

                var ePayApps = await ePaymentService.GeneratePSIdAsync(ePayApplications);

                this.AuctionService.SavePSIds(ePayApps.Select(x => new
                {
                    Id = x.deptTransactionId,
                    AIN = "",
                    ApplicationStatusId = 0,
                    ChasisNumber = "",
                    CustomerId = 0,
                    OwnerName = "",
                    PSId = x.psId,
                    AmountPaid = 0,
                    BankCode = "",
                    PaidOn = DateTime.Now,
                    PaymentStatusId = 0
                })
                    .ToList()
                    .ToDataTable());


                return Json(new
                {
                    status = true,
                    psId = ePayApps.FirstOrDefault().psId
                });
            }
            catch
            {
                return Json(new
                {
                    status = false,
                    msg = "Try again."
                });
            }
        }
    }
}