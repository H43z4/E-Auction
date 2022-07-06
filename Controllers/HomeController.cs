using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using eauction.TemplateModels;
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
using System.IO;
using iText.Html2pdf;
using iText.StyledXmlParser.Css.Media;
using System.Collections.Generic;
using eauction.Infrastructure;
using NotificationServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using dotNetReports;
using eauction.Helpers;

namespace eauction.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        private readonly IHubContext<NotificationHub> notificationHub;
        private readonly IWebHostEnvironment webHostEnvironment;

        public HomeController(ILogger<HomeController> logger, 
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private Task BroadcastHighestBid(int customerId, int seriesNumberId, int highestBidPrice, string timeStamp, long remainingTime)
        {
            return this.notificationHub.Clients.All.SendAsync("ReceiveMessage", customerId, seriesNumberId, highestBidPrice, timeStamp, remainingTime);
        }

        [AllowAnonymous]
        public IActionResult Default()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Howtodo()
        {
            return View();
        }



        //[AllowAnonymous]
        public IActionResult GetAuctionSeries(int seriesStatusId = 2)   //Registring Customers
        {
            var dataset = this.AuctionService.GetAuctionSeries(seriesStatusId);

            //if (dataset.Tables[0].Rows.Count == 0)
            //{
            //    return View(new Models.Views.Auction.Series());
            //}

            var auctionSeries = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(dataset.Tables[0]);

            return View(auctionSeries);
        }

        //[AllowAnonymous]
        public IActionResult GetAuctionSeriesNumber(int seriesId)
        {
            var dataset = this.AuctionService.GetAuctionSeriesNumber(this.UserId, seriesId);

            if (dataset.Tables[0].Rows.Count == 0)
            {
                //ViewBag.series = new Models.Views.Auction.Series();
                //return View(new Models.Views.Auction.SeriesNumber());

                return View("Default");
            }

            ViewBag.series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(dataset.Tables[0]).FirstOrDefault();
            var auctionSeriesNumbers = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.SeriesNumber>(dataset.Tables[1]);

            return View(auctionSeriesNumbers);
        }

        public IActionResult GetSeriesNumber(Models.Views.Auction.SeriesNumber seriesNumberFilter)
        {
            seriesNumberFilter.PageSize = 50;

            var dataset = this.AuctionService.GetSeriesNumber(this.UserId, seriesNumberFilter);

            var seriesNumbersCount = System.Convert.ToInt32(dataset.Tables[0].Rows[0][0].ToString());
            var seriesNumbers = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.SeriesNumber>(dataset.Tables[1]).ToList();

            GenericPagedList<Models.Views.Auction.SeriesNumber> genericPagedList = new GenericPagedList<SeriesNumber>();

            genericPagedList.ListOfItems = seriesNumbers;

            PagerBootStrapped pager = new PagerBootStrapped(seriesNumbersCount, seriesNumberFilter.PageNumber, seriesNumberFilter.PageSize);

            genericPagedList.Pager = pager;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_SeriesNumber", genericPagedList);
            }

            return View(genericPagedList);
        }


        [HttpPost]
        public IActionResult SaveApplication_0(int seriesNumberId)
        {
            try
            {
                this.AuctionService.SaveApplication(this.UserId, seriesNumberId);
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

            //return RedirectToAction(nameof(GetApplications));

            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public IActionResult SaveApplications_1(string seriesNumberIdCSVs)
        {
            try
            {
                this.AuctionService.SaveApplications(this.UserId, seriesNumberIdCSVs);
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

            //return RedirectToAction(nameof(GetApplications));
            TempData["newlySubmittedApps"] = seriesNumberIdCSVs;

            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
        public IActionResult SaveApplications_2([FromBody]List<Models.Views.Input.Application> applications)
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
                this.AuctionService.SaveApplications(this.UserId, applications.Select(x => new { x.Id, x.ChasisNumber, x.OwnerName }).ToList().ToDataTable());
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
            var seriesNumberIdCSVs = string.Join(",", applications.Select(x => x.Id));
            //return RedirectToAction(nameof(GetApplications)); 
            TempData["newlySubmittedApps"] = seriesNumberIdCSVs;

            return Json(new
            {
                status = true,
            });
        }

        [HttpPost]
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

        public Task<ViewResult> GetApplications()
        {
            DataSet ds = this.AuctionService.GetApplications(this.UserId);

            var applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]);

            return Task.FromResult(View(applications));
        }

        public IActionResult GetChallan(int applicationId)
        {
            DataSet ds = this.AuctionService.GetApplicationDetail(applicationId);

            var application = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).FirstOrDefault();

            if (application != null)
            {
                if (application.CustomerId != this.UserId || application.ApplicationStatusId != 1)
                {
                    return BadRequest();
                }

                var pdfFile = $"{this.webHostEnvironment.WebRootPath}/Reports/pdfFiles/{application.AIN}.pdf";

                if (!System.IO.File.Exists(pdfFile))
                {
                    var htmlFile = $"{this.webHostEnvironment.WebRootPath}/Reports/Template/challanForm.html";

                    var html = System.IO.File.ReadAllText(htmlFile);

                    //int userid = 132;
                    //var user = this.AuctionService.GetUser(userid);

                    var user = this.AuctionService.GetUser(this.UserId);

                    html = html.Replace("@User", user.FullName)
                        .Replace("@AIN", application.AIN);

                    FileStream pdfDocFile;

                    //using (FileStream htmlSource = System.IO.File.Open(htmlFile, FileMode.Open))
                    using (FileStream pdfDest = System.IO.File.Open(pdfFile, FileMode.OpenOrCreate))
                    {
                        ConverterProperties converterProperties = new ConverterProperties();
                        converterProperties.SetBaseUri("../../");
                        converterProperties.SetMediaDeviceDescription(new MediaDeviceDescription(MediaType.PRINT));

                        //HtmlConverter.ConvertToPdf(htmlSource, pdfDest, converterProperties);
                        HtmlConverter.ConvertToPdf(html, pdfDest, converterProperties);

                        pdfDocFile = pdfDest;
                    }
                }

                //// return resulted pdf document
                FileResult fileResult = new FileContentResult(System.IO.File.ReadAllBytes(pdfFile), "application/pdf");
                fileResult.FileDownloadName = $"{application.AIN}.pdf";
                return fileResult;

            }

            return BadRequest();
        }

        public IActionResult GetBids()
        {
            string sqlException = string.Empty;

            DataSet ds = this.AuctionService.GetBidsMultipleSeries(this.UserId, out sqlException);

            if (!string.IsNullOrEmpty(sqlException))
            {
                ViewBag.BiddingStatus = sqlException;

                return View("BiddingStatus");
            }

            Bids bids = new Bids()
            {
                Applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).ToList(),
                HighestBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.HighestBid>(ds.Tables[1]).ToList(),
                LastBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.LastBid>(ds.Tables[2]).ToList(),
                CustomerId = this.UserId,
                SysDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss")
            };

            foreach (var app in bids.Applications)
            {
                app.RemainingTime = (int)(app.AuctionEndDateTime - DateTime.Now).TotalSeconds;
            }

            return View(bids);
        }

        [HttpPost]
        public IActionResult SaveBid(NewBid newBid)
        {
            DataSet ds;

            try
            {
                ds = this.AuctionService.SaveBid(this.UserId, newBid);
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
                });
            }

            var highestBid = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.HighestBid>(ds.Tables[0])
                .FirstOrDefault(x => x.AIN == newBid.AIN);

            //var time = Infrastructure.DateTimeTimeAgo.TimeAgo(highestBid.CreatedOn);
            var timeStamp = highestBid.CreatedOn.ToString("dd-MM-yyyy HH:mm:ss:fff");
            //var remainingTime = (int)(highestBid.ClosingTime - DateTime.Now).TotalSeconds;
            var remainingTime = (long)(highestBid.ClosingTime - DateTime.Now).TotalMilliseconds;

            this.BroadcastHighestBid(this.UserId, highestBid.SeriesNumberId, highestBid.HighestBiddingPrice, timeStamp, remainingTime);

            return Json(new 
            {   
                status = true,
                highestBid.SeriesNumberId,
                timeStamp,
                remainingTime
            });
        }

        public IActionResult GetWinners_0()
        {
            DataSet ds = this.AuctionService.GetWinners(this.UserId);

            var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds.Tables[0]);

            return View(winners);
        }

        public IActionResult GetWinners()
        {
            DataSet ds = this.AuctionService.GetSeriesClosedForBidding();

            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0])
                .Select(x => new 
                {
                    x.Id,
                    Name = $"{x.CategoryName} [{x.SeriesName}]"
                });

            ViewBag.SeriesClosedForBiddingSelectList = new SelectList(series, "Id", "Name");

            return View(series);
        }

        public IActionResult DownloadWinners(int seriesId)
        {
            if (seriesId > 0)
            {
                var pdfFile = $"{this.webHostEnvironment.WebRootPath}\\Reports\\pdfFiles\\Winners\\{seriesId}.pdf";

                if (!System.IO.File.Exists(pdfFile))
                {
                    DataSet ds1 = this.AuctionService.GetWinnersSeriesWise(seriesId);

                    var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds1.Tables[0])
                        .OrderBy(x => x.SeriesNumber)
                        .ToList();

                    this.GeneratePdf(seriesId, winners);
                }

                //// return resulted pdf document
                FileResult fileResult = new FileContentResult(System.IO.File.ReadAllBytes(pdfFile), "application/pdf");
                fileResult.FileDownloadName = $"{seriesId}.pdf";
                return fileResult;
            }

            DataSet ds = this.AuctionService.GetSeriesClosedForBidding();

            var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0])
                .Select(x => new
                {
                    x.Id,
                    Name = $"{x.CategoryName} [{x.SeriesName}]"
                });

            ViewBag.SeriesClosedForBiddingSelectList = new SelectList(series, "Id", "Name");

            return View(series);
        }
        
        void GeneratePdf(int seriesId, List<Models.Views.Auction.Winners> winners)
        {
            var pdfFile = $"{this.webHostEnvironment.WebRootPath}/Reports/pdfFiles/Winners/{seriesId}.pdf";

            //if (!System.IO.File.Exists(pdfFile))
            //{

            //}
            var htmlFile = $"{this.webHostEnvironment.WebRootPath}/Reports/Template/winners.html";

            var html = System.IO.File.ReadAllText(htmlFile);

            var winnerTableRowTemplate = string.Empty;
            int row = 1;

            foreach (var winner in winners)
            {
                //winnerTableRowTemplate += $"<tr><td>{row++}</td><td>{winner.SeriesCategory}</td><td>{winner.Series}</td><td class='text-right'>{winner.SeriesNumber}</td><td class='text-right'>{winner.ReservePrice.ToString("N0")}</td><td class='text-right'>{winner.HighestBiddingPrice.ToString("N0")}</td><td>{winner.WinnerAIN}</td><td>{winner.OwnerName}</td></tr>";
                winnerTableRowTemplate += $"<tr style='width: 100%;background: {(row % 2 == 1 ? "#fff" : "#ededed")};'>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{row++}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.SeriesCategory}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.Series}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.SeriesNumber}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.ReservePrice.ToString("N0")}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.HighestBiddingPrice.ToString("N0")}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.WinnerAIN.Remove(9, 4).Insert(9, "****")}</td>" +
                    //$"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.WinnerAIN}</td>" +
                    $"<td style='padding:10px 8px;font-family:Lucida Grande,Lucida Sans,Lucida Sans Unicode,Arial,Helvetica,Verdana,sans-serif;font-size:13px;color: #787878;border-right:1px solid #787878;text-align: center;'>{winner.OwnerName}</td></tr>";
            }

            var winnerObj = winners.FirstOrDefault();

            html = html.Replace("@Series", $"{winnerObj.SeriesCategory} [{winnerObj.Series}]");
            html = html.Replace("@TABLE_BODY", winnerTableRowTemplate);

            FileStream pdfDocFile;

            //using (FileStream htmlSource = System.IO.File.Open(htmlFile, FileMode.Open))
            using (FileStream pdfDest = System.IO.File.Open(pdfFile, FileMode.OpenOrCreate))
            {
                ConverterProperties converterProperties = new ConverterProperties();
                converterProperties.SetMediaDeviceDescription(new MediaDeviceDescription(MediaType.PRINT));

                //HtmlConverter.ConvertToPdf(htmlSource, pdfDest, converterProperties);
                HtmlConverter.ConvertToPdf(html, pdfDest, converterProperties);

                pdfDocFile = pdfDest;
            }
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


                //return Json(new
                //{
                //    status = true,
                //    psId = ePayApps.ToList()
                //});

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
            catch (Exception ex)
            {
                return Json(new
                {
                    status = false,
                    msg = "Try again." + ex.Message
                });
            }
        }

        public IActionResult GetSeriesScheduler()
        {
            var ds = this.AuctionService.GetSeriesSchedule();

            GenericPagedList<Series> genericPagedList = new GenericPagedList<Series>();
            genericPagedList.ListOfItems = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0]).ToList();

            return View(genericPagedList);
        }

    }
}
