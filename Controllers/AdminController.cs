using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using eauction.Data;
using eauction.Helpers;
using eauction.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.Domain.Identity;
using Models.Domain.Notification;
using Models.Views.Auction;
using Models.Views.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NotificationServices;

namespace eauction.Controllers
{
    //[Authorize(Roles = "System,SuperAdmin,Admin")]
    [Authorize]
    public class AdminController : BaseController
    {
        private readonly IConfiguration configuration;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AdminController(ILogger<HomeController> logger, 
            IConfiguration configuration, 
            ApplicationDbContext applicationDbContext,
            INotificationManager notificationManager,
            IWebHostEnvironment env)
            : base(logger, configuration, applicationDbContext, notificationManager)
        {
            this.configuration = configuration;
            this.webHostEnvironment = env;
            
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        }

        //private async Task SendMessages(int customerId, string AIN)
        //{
        //    User user = this.AuctionService.GetUser(customerId);

        //    var msg = "Your application has been approved In e-auction" + Environment.NewLine +
        //        "AIN:" + AIN;

        //    var result = await this.smsSender.SendSms(user.PhoneNumber, msg);

        //    var emailSetting = this.AuctionService.GetEmailSetting();
        //    emailSetting.Receiver = user.Email;

        //    //HtmlEncoder.Default.Encode(callbackUrl) adds ampersand in query string paramters which google chrome does not parse correctly.

        //    //emailSetting.Body = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.";

        //    emailSetting.Subject = "Application Approved.";
        //    emailSetting.Body = msg;

        //    var emailSender = new MailSender() { EmailSetting = emailSetting };

        //    await emailSender.SendEmailAsync(string.Empty, string.Empty, string.Empty);
        //}

        private async Task SendMessages(int customerId, string AIN)
        {
            User user = this.AuctionService.GetUser(customerId);

            var emailSetting = this.AuctionService.GetEmailSetting(2);  //"Application Approval"

            emailSetting.Receiver = user.Email;
            emailSetting.Body = emailSetting.Body.Replace("@@@", Environment.NewLine).Replace("#AIN", AIN);

            await this.notificationManager.SendMessage(user.PhoneNumber, emailSetting);
        }


        private async Task<bool> SendPushNotification(Models.Views.Notification.Notification notification)
        {
            try
            {
                using (var client = new HttpClient())
                {

                    var uri = new Uri($"{this.configuration.GetSection("OneSignal:SendToAll").Value}");
                    var app_id = this.configuration.GetSection("OneSignal:app_id").Value;
                    var authorization = this.configuration.GetSection("OneSignal:authorization").Value;

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);

                    var data = JsonConvert.SerializeObject(new
                    {
                        app_id,
                        included_segments = new string[] { "All" },
                        contents = new { en = notification.Content },
                        data = new { en = notification.Data },
                        headings = new { en = notification.Heading }
                    });

                    var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

                    var httpStatusResponse = await client.PostAsync(uri, stringContent)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            return task.Result;
                        });
                    
                    if (!httpStatusResponse.IsSuccessStatusCode)
                    {
                        return false;
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> SendPushNotification(Notification notification, List<string> customerAppIds)
        {
            try
            {
                using (var client = new HttpClient())
                {

                    var uri = new Uri($"{this.configuration.GetSection("OneSignal:SendToAll").Value}");
                    var app_id = this.configuration.GetSection("OneSignal:app_id").Value;
                    var authorization = this.configuration.GetSection("OneSignal:authorization").Value;

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authorization);

                    var data = JsonConvert.SerializeObject(new
                    {
                        app_id,
                        include_player_ids = customerAppIds.ToArray(),
                        contents = new { en = notification.Content },
                        data = new { customkey = notification.Data },
                        headings = new { en = notification.Heading }
                    });

                    var stringContent = new StringContent(data, Encoding.UTF8, "application/json");

                    var httpStatusResponse = await client.PostAsync(uri, stringContent)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            return task.Result;
                        });

                    var response = httpStatusResponse.Content.ReadAsStringAsync().Result;

                    if (!httpStatusResponse.IsSuccessStatusCode)
                    {
                        return false;
                    }

                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult FindApplications_0(Models.Views.Auction.Application applicationFilter)
        {
            //applicationFilter ??= new Models.Views.Auction.Application();

            DataSet ds = this.AuctionService.FindApplications(applicationFilter);

            var applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).ToList();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Applications", applications);
            }

            ViewBag.ApplicationStatus = new SelectList(this.AuctionService.GetApplicationStatus(), "Id", "Name");

            return View(applications);
        }



        [Authorize(Roles = "Admin")]
        public IActionResult FindApplicationsMRA(string ain)
        {
            Models.Views.Auction.Application applicationFilter = new Application();

            applicationFilter.AIN = ain;
            applicationFilter.ApplicationStatusId = 0;
            applicationFilter.PageNumber = 1;
            applicationFilter.PageSize = 1;

            GenericPagedList<Models.Views.Auction.Application> genericPagedList = new GenericPagedList<Application>()
            {
                ListOfItems = new List<Application>()
            };

            if (Request.IsAjaxRequest())
            {
                DataSet ds = this.AuctionService.FindApplications_1(applicationFilter);

                var applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[1]).ToList();

                genericPagedList.ListOfItems = applications;

                return PartialView("_Applications", genericPagedList);
            }

            return View(genericPagedList);
        }


        [Authorize(Roles = "System,SuperAdmin,Admin")]
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
                else if (application.ApplicationStatusId == 5)
                {
                    var appSecurityDepositStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Bank.AppSecurityDepositStatusHistory>(ds.Tables[3]).ToList();

                    application.AppSecurityDeposit.AppSecurityDepositStatusHistory = appSecurityDepositStatusHistory;
                }
            }

            return View(application);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveApplicationAsync_0(int applicationId)
        {
            int customerId = 0;
            string AIN = string.Empty;

            var result = this.AuctionService.ApproveApplication(applicationId, this.UserId, out customerId, out AIN);

            if (result == true)
            {
                await this.SendMessages(customerId, AIN);
            }

            return RedirectToAction(nameof(GetApplicationDetail), new { applicationId = applicationId });
        }


        [Authorize(Roles = "Admin")]
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

            //return RedirectToAction(nameof(GetApplicationDetail), new { applicationId = appSecurityDeposit.ApplicationId });
            var actionresult = RedirectToAction(nameof(GetApplicationDetail), new { applicationId = appSecurityDeposit.ApplicationId });

            return actionresult;
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult SaveSecurity(Models.Views.Bank.AppSecurityDeposit appSecurityDeposit)
        {
            var appSecurityDepositObj = this.AuctionService.SaveSecurity(appSecurityDeposit, this.UserId);

            return RedirectToAction(nameof(GetApplicationDetail), new { applicationId = appSecurityDeposit.ApplicationId });
        }

        /// <summary>
        /// removed in new version
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost]
        private IActionResult SaveApplicationDetail(Models.Views.Auction.Application application)
        {
            string sqlException = string.Empty;

            this.AuctionService.SaveApplicationDetail(application, out sqlException);

            if (!string.IsNullOrEmpty(sqlException))
            {
                TempData["Error"] = $"{sqlException}";

                return RedirectToAction(nameof(EditApplication), new { applicationId = application.Id });
            }
            else
            {
                return RedirectToAction(nameof(GetApplicationDetail), new { applicationId = application.Id });
            }

            //return RedirectToAction(nameof(EditApplication), new { applicationId = application.Id });
        }


        [Authorize(Roles = "Admin")]
        public IActionResult EditApplication(int applicationId)
        {
            DataSet ds = this.AuctionService.GetApplicationDetail(applicationId);

            var application = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).FirstOrDefault();

            return View(application);
        }


        [Authorize(Roles = "System,SuperAdmin,Admin")]
        public IActionResult GetWinners()
        {
            DataSet ds = this.AuctionService.GetWinnersDistinct();

            var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds.Tables[0]);

            return View(winners);
        }



        [Authorize(Roles = "System,SuperAdmin")]
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
            //var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(LOVs.Tables[1]).ToList();
            var applicationStatus = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.ApplicationStatus>(LOVs.Tables[2]).ToList();

            ViewBag.SeriesCategory = new SelectList(seriesCategory, "Id", "Name");
            //ViewBag.Series = new SelectList(series, "Id", "SeriesName");
            //ViewBag.Series = series;
            ViewBag.ApplicationStatus = new SelectList(applicationStatus, "Id", "Name");

            return View(genericPagedList);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetCustomerApplication(int customerId)
        {
            DataSet ds = this.AuctionService.GetCustomerApplication(customerId);

            CustomerApplication customerApplication = new CustomerApplication();

            customerApplication.Customer = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Customer>(ds.Tables[0]).FirstOrDefault();
            customerApplication.Applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[1]).ToList();
            var applicationStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.ApplicationStatusHistory>(ds.Tables[2]).ToList();

            foreach (var app in customerApplication.Applications)
            {
                app.ApplicationStatusHistory.AddRange(applicationStatusHistory.Where(x => x.ApplicationId == app.Id));
            }

            return View(customerApplication);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetAuctionSeries(int? seriesStatusId)
        {
            var ds = this.AuctionService.GetAuctionSeries(seriesStatusId);
            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).ToList();

            return PartialView("_AuctionSeries", series);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetAuctionSeriesStatusWise()
        {
            var ds = this.AuctionService.GetAuctionSeriesStatusWise();

            SeriesStatusWise seriesStatusWise = new SeriesStatusWise();
            var seriesStatus = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.SeriesStatus>(ds.Tables[0]).ToList();
            seriesStatusWise.SeriesStatusSelectList = new SelectList(seriesStatus, "Id", "Name");
            seriesStatusWise.Series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[1]).ToList();

            return View(seriesStatusWise);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetSeriesDetail(int seriesId)
        {
            DataSet ds = this.AuctionService.GetSeriesDetail(seriesId);

            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).FirstOrDefault();

            if (series != null)
            {
                series.SeriesStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.SeriesStatusHistory>(ds.Tables[1]).ToList();
            }

            var seriesStatusSelectList = new List<SelectListItem>();
            this.AuctionService.GetSeriesStatus().ForEach(x => seriesStatusSelectList.Add(new SelectListItem(x.Name, x.Id.ToString())));

            ViewBag.SeriesStatusSelectList = seriesStatusSelectList;

            return View(series);
        }



        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetWinnersSeriesWise(int seriesId)
        {
            DataSet ds = this.AuctionService.GetWinnersSeriesWise(seriesId);

            var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds.Tables[0]);

            if (Request.IsAjaxRequest())
            {
                ViewBag.SeriesId = seriesId;

                return PartialView("_Winners", winners);
            }

            var LOVs = this.AuctionService.GetLOVs();
            var seriesCategory = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.SeriesCategory>(LOVs.Tables[0]).ToList();
            //var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(LOVs.Tables[1])
            //    .Where(x => x.SeriesStatusId != 5 && x.AuctionEndDateTime < DateTime.Now)  //   5 = Series Closed
            //    .ToList();
            var applicationStatus = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.ApplicationStatus>(LOVs.Tables[2]).ToList();

            ViewBag.SeriesCategory = new SelectList(seriesCategory, "Id", "Name");
            //ViewBag.Series = series; 

            return View(winners);
        }

        
        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetSeriesScheduler()
        {
            var ds = this.AuctionService.GetSeriesSchedule();

            GenericPagedList<Series> genericPagedList = new GenericPagedList<Series>();
            genericPagedList.ListOfItems = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).ToList();

            return View(genericPagedList);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult EditSeries(int seriesId = 0)
        {
            DataSet ds = this.AuctionService.GetSeriesDetail(seriesId);

            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).FirstOrDefault();

            //if (series != null)
            //{
            //    series.SeriesStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.SeriesStatusHistory>(ds.Tables[1]).ToList();
            //}

            if (series is null)
            {
                series = new Series()
                {
                    RegStartDateTime = DateTime.Now.Date.Add(new TimeSpan(8, 0, 0)),
                    RegEndDateTime = DateTime.Now.Date.AddDays(2).Add(new TimeSpan(23, 59, 59)),
                    AuctionStartDateTime = DateTime.Now.Date.AddDays(5).Add(new TimeSpan(8, 0, 0)),
                    AuctionEndDateTime = DateTime.Now.Date.AddDays(7).Add(new TimeSpan(23, 59, 59)),
                };
            }

            var seriesCategories = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.SeriesCategory>(ds.Tables[2]).ToList();

            var seriesCategorySelectList = new List<SelectListItem>();
            seriesCategories.ForEach(x => seriesCategorySelectList.Add(new SelectListItem(x.Name, x.Id.ToString())));

            ViewBag.SeriesCategorySelectList = seriesCategorySelectList;

            return View(series);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        [HttpPost]
        public IActionResult SaveSeries(Models.Views.Auction.Series series)
        {
            string sqlException = string.Empty;

            var ds = this.AuctionService.SaveSeries(series, this.UserId, out sqlException);

            if (!string.IsNullOrEmpty(sqlException))
            {
                TempData["Error"] = $"{series.CategoryName} [{series.SeriesName}]  - {sqlException}";

                return RedirectToAction(nameof(EditSeries), new { seriesId = series?.Id });
            }
            else
            {
                series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).ToList().FirstOrDefault();

                TempData["SeriesSaved"] = $"{series.CategoryName} [{series.SeriesName}]";

                return RedirectToAction(nameof(GetSeriesScheduler));
            }
        }

        [Authorize(Roles = "System,SuperAdmin")]
        [HttpPost]
        [ActionName("ApproveWinnersList")]
        public IActionResult ApproveWinnersList(int seriesId)
        {
            DataSet ds = this.AuctionService.ApproveWinnersList(seriesId, this.UserId);

            if (ds.Tables.Count > 0)    // Assume that approval of winners list is successfull then push results to MVRS too
            {
                try
                {
                    var winnersDataSet = this.AuctionService.GetWinnersSeriesWise(seriesId);

                    var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(winnersDataSet.Tables[0]).ToList();

                    var oraConnectionString = this.configuration.GetSection("MVRS:DefaultConnection").Value;

                    this.AuctionService.SaveWinnersToMvrs(oraConnectionString, winners);
                }
                catch
                {
                }
            }

            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).ToList();

            TempData["ApprovedSeries"] = series.FirstOrDefault().SeriesName;

            return RedirectToAction(nameof(GetWinnersSeriesWise));
        }

        
        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetNotifications()
        {
            DataSet ds = this.AuctionService.GetNotifications();

            var notifications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Notification.Notification>(ds.Tables[0]);

            return View(notifications);
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetNotificationDetail(int notificationId = 0)
        {
            DataSet ds = this.AuctionService.GetNotificationDetail(notificationId);

            var notification = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Notification.Notification>(ds.Tables[0]).FirstOrDefault();

            var notificationStatus = Infrastructure.DataTableExtension.DataTableToList<NotificationStatus>(ds.Tables[2]).ToList();

            var notificationStatusSelectList = new List<SelectListItem>();

            if (notification != null)
            {
                notification.NotificationStatusHistory = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Notification.NotificationStatusHistory>(ds.Tables[1]).ToList();

                notificationStatusSelectList.AddRange(
                    notificationStatus.Where(x => x.Id == 2)
                    .Select(x => new SelectListItem(x.Name, x.Id.ToString())));
            }
            else
            {
                notificationStatusSelectList.AddRange(
                    notificationStatus.Where(x => x.Id == 1)
                    .Select(x => new SelectListItem(x.Name, x.Id.ToString())));
            }

            ViewBag.NotificationStatusSelectList = notificationStatusSelectList;

            return View(notification ?? new Models.Views.Notification.Notification());
        }

        
        
        [Authorize(Roles = "System,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> SaveNotificationAsync(Models.Views.Notification.Notification notification)
        {
            if (ModelState.IsValid)
            {
                notification.Id = this.AuctionService.SaveNotification(notification, this.UserId).Id;

                if (notification.NotificationStatusId == 1)
                {
                    await this.SendPushNotification(notification);
                }
            }

            return RedirectToAction(nameof(GetNotificationDetail), new { notificationId = notification.Id });
        }


        #region Reports

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetApplicationsApproved(Models.Views.Auction.Application applicationFilter)
        {
            DataSet ds = this.AuctionService.GetApplicationsApproved(applicationFilter);

            var applicationsApproved = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Reports.Application>(ds.Tables[0]).ToList();

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ApplicationsApproved", applicationsApproved);
            }

            var LOVs = this.AuctionService.GetLOVs();
            var seriesCategory = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.SeriesCategory>(LOVs.Tables[0]).ToList();

            ViewBag.SeriesCategory = new SelectList(seriesCategory, "Id", "Name");

            return View(applicationsApproved);
        }

        //[Authorize(Roles = "System,SuperAdmin")]
        public IActionResult DownloadApprovals_0(Models.Views.Auction.Application applicationFilter)
        {
            DataSet ds = this.AuctionService.GetApplicationsApproved(applicationFilter);

            string mimeType;

            var file = new dotNetReports.ReportGenerator()
                .GenerateApprovalReport($"{AppDomain.CurrentDomain.BaseDirectory}/Reports/Approvals.rdlc", 
                ds.Tables[0], 
                dotNetReports.ReportOutputFormat.Excel,
                out mimeType);

            //var applicationsApproved = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Reports.Application>(ds.Tables[0]).ToList();
            //Infrastructure.Extensions.DataTableExtension.CopyTo(ds.Tables[0], printingDataSet.VehicleInfo);

            //printingDataSet.VehicleInfo.Rows[0]["Period"] = DateTime.Now.ToString("MMM yyyy");
            //printingDataSet.VehicleInfo.Rows[0]["User"] = this.User.Identity.Name;

            //var fileContentResult = this.GenerateReport("VehicleInfo", printingDataSet.Tables["VehicleInfo"], ReportOutputFormat.Excel, $"{batchNo}.xls");

            return File(file, mimeType, "Approvals.xls");
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetApprovals()
        {
            return View();
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult DownloadApprovals(int seriesCategoryId, int seriesId, string dateFrom, string dateTo)
        {
            DataSet ds = this.AuctionService.DownloadApprovals(seriesCategoryId, seriesId, dateFrom, dateTo);

            string mimtype = "";
            int extension = 1;
            var path = $"{this.webHostEnvironment.WebRootPath}\\Reports\\Template\\Approvals.rdlc";

            LocalReport localReport = new LocalReport(path);
            localReport.AddDataSource("Approvals", ds.Tables[0]);
            var result = localReport.Execute(RenderType.ExcelOpenXml, extension, null, mimtype);

            //return File(result.MainStream, "application/pdf");
            return File(result.MainStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MRA Approvals.xlsx");
        }

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetBidLogs(string categories, string series, string seriesNumbers, int pageNumber = 1)
        {
            DataSet ds = this.AuctionService.GetBidLogs(categories, series, seriesNumbers, pageNumber);

            var bidLogs = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds.Tables[0]).ToList();

            GenericPagedList<Models.Views.Auction.Winners> genericPagedList = new GenericPagedList<Winners>()
            {
                ListOfItems = bidLogs,
                Pager = new PagerBootStrapped(bidLogs.Count, pageNumber, 100)
            };

            if (Request.IsAjaxRequest())
            {
                return PartialView("_GetBidLogs", genericPagedList);
            }

            var LOVs = this.AuctionService.GetLOVs();
            var seriesCategory = Infrastructure.DataTableExtension.DataTableToList<Models.Domain.Auction.SeriesCategory>(LOVs.Tables[0]).ToList();

            ViewBag.SeriesCategory = new SelectList(seriesCategory, "Id", "Name");

            return View("GetBidLogs", genericPagedList);
        }

        #endregion


        #region User Management

        [Authorize(Roles = "System,SuperAdmin")]
        public IActionResult GetUsers(Models.Views.Identity.User userFilter)
        {
            userFilter.CNIC = string.IsNullOrEmpty(userFilter.CNIC) ? userFilter.CNIC : RegexUtilities.Clean(userFilter.CNIC);
            userFilter.PhoneNumber = string.IsNullOrEmpty(userFilter.PhoneNumber) ? userFilter.PhoneNumber : RegexUtilities.Clean(userFilter.PhoneNumber);

            DataSet ds = this.AuctionService.GetUsers(userFilter);

            var usersCount = System.Convert.ToInt32(ds.Tables[0].Rows[0][0].ToString());

            var users = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Identity.User>(ds.Tables[1]).ToList();

            GenericPagedList<Models.Views.Identity.User> genericPagedList = new GenericPagedList<Models.Views.Identity.User>();

            genericPagedList.ListOfItems = users;

            PagerBootStrapped pager = new PagerBootStrapped(usersCount, userFilter.PageNumber, userFilter.PageSize);

            genericPagedList.Pager = pager;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_Users", genericPagedList);
            }

            return View(genericPagedList);
        }

        #endregion
    }
}