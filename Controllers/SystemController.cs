using eauction.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.Views.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NotificationServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace eauction.Controllers
{
    [Authorize(Roles = "System")]
    public class SystemController : BaseController
    {
        private readonly IConfiguration configuration;

        public SystemController(ILogger<HomeController> logger,
            IConfiguration configuration,
            ApplicationDbContext applicationDbContext,
            INotificationManager notificationManager)
            : base(logger, configuration, applicationDbContext, notificationManager)
        {
            this.configuration = configuration;
        }

        public IActionResult Synchronize()
        {
            var dataset = this.AuctionService.GetSeriesBeforeRegistration();  //  Newly Opened (Idle)

            var auctionSeries = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(dataset.Tables[0]);

            return View(auctionSeries);
        }

        public async Task<JsonResult> DownloadSeries()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.AccessToken);

                    var uri = new Uri($"{this.configuration.GetSection("EntrepriseServer:GetAuctionSeries").Value}");

                    //GET Method  
                    var seriesHttpResponse = await client.GetAsync(uri)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            var content = task.Result.Content.ReadAsStringAsync().Result;
                            return JsonConvert.DeserializeObject<SeriesHttpResponse>(content, new IsoDateTimeConverter { DateTimeFormat = "dd-MM-yyyy HH:mm:ss" });
                        });

                    if (!seriesHttpResponse.status)
                    {
                        return new JsonResult(new { status = false });
                    }

                    var series = new List<Models.Domain.Auction.Series>();

                    foreach (var item in seriesHttpResponse.Series.OrderBy(x => x.SeriesName))
                    {
                        series.Add(new Models.Domain.Auction.Series()
                        {
                            AuctionEndDateTime = item.AuctionEndDateTime,
                            AuctionStartDateTime = item.AuctionStartDateTime,
                            CreatedBy = this.UserId,
                            DistrictId = 1,
                            Name = item.SeriesName,
                            SeriesCategoryId = item.CategoryId,
                            RegEndDateTime = item.AuctionEndDateTime,
                            RegStartDateTime = item.AuctionStartDateTime,
                            SeriesStatusId = 1
                        });
                    }

                    this.AuctionService.SaveSeries(series);
                }

                return new JsonResult(new { status = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false });
            }
        }

        public async Task<JsonResult> DownloadNumbers(int seriesCategoryId, int seriesId, string series)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.AccessToken);

                    //var uri = new Uri($"http://localhost:5555/EMotor/GetAuctionNumber?catId={seriesCategoryId}&series={series}");

                    var uri = new Uri($"{this.configuration.GetSection("EntrepriseServer:GetAuctionNumber").Value}?catId={seriesCategoryId}&series={series}");

                    //GET Method  
                    var auctionNumberHttpResponse = await client.GetAsync(uri)
                        .ContinueWith((task) =>
                        {
                            task.Result.EnsureSuccessStatusCode();

                            var content = task.Result.Content.ReadAsStringAsync().Result;
                            return JsonConvert.DeserializeObject<AuctionNumberHttpResponse>(content, new IsoDateTimeConverter { DateTimeFormat = "dd-MM-yyyy" });
                        });

                    if (!auctionNumberHttpResponse.status)
                    {
                        return new JsonResult(new { status = false });
                    }

                    var seriesNumbers = new List<Models.Domain.Auction.SeriesNumber>();

                    foreach (var item in auctionNumberHttpResponse.SeriesNumbers.OrderBy(x => x.AuctionNumber))
                    {
                        seriesNumbers.Add(new Models.Domain.Auction.SeriesNumber()
                        {
                            AuctionNumber = item.AuctionNumber,
                            CreatedBy = this.UserId,
                            IsAuctionable = true,
                            ReservePrice = item.ReservePrice,
                            SeriesId = seriesId,
                        });
                    }

                    this.AuctionService.SaveSeriesNumber(seriesNumbers);
                }

                return new JsonResult(new { status = true });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { status = false });
            }
        }

        [AllowAnonymous]
        public Task<JsonResult> GetSeriesNumbers(string secret)
        {
            var mysecret = this.configuration.GetSection("JwtConfig:secret").Value;

            if (secret != mysecret)
            {
                return Task.FromResult(Json(new { status = false, msg = -1 }));
            }

            try
            {
                var seriesDataset = this.AuctionService.GetSeriesForDataTransfer();

                var openingSeries = seriesDataset.Tables[0].AsEnumerable().Select(x => new
                {
                    Id = Convert.ToInt32(x["Id"].ToString()),
                    CategoryId = Convert.ToInt32(x["CategoryId"].ToString()),
                    SeriesName = x["SeriesName"].ToString(),
                    AuctionEndDateTime = Convert.ToDateTime(x["AuctionEndDateTime"].ToString())
                });

                var oraConnectionString = this.configuration.GetSection("MVRS:DefaultConnection").Value;

                foreach (var series in openingSeries)
                {
                    var ds = this.AuctionService.GetAuctionNumberFromMvrs(oraConnectionString, series.CategoryId, series.SeriesName);

                    var newSeriesNumbers = ds.Tables[0].AsEnumerable().Select(x => new
                    {
                        AuctionNumber = x["AUCTIONED_NUMBER"].ToString(),
                        ReservePrice = Convert.ToInt32(x["AUCTION_PRICE"]),
                    });

                    var seriesNumbers = new List<Models.Domain.Auction.SeriesNumber>();

                    foreach (var item in newSeriesNumbers.OrderBy(x => x.AuctionNumber))
                    {
                        seriesNumbers.Add(new Models.Domain.Auction.SeriesNumber()
                        {
                            AuctionNumber = item.AuctionNumber,
                            CreatedBy = 121,
                            IsAuctionable = true,
                            ReservePrice = item.ReservePrice,
                            SeriesId = series.Id,
                            AuctionEndDateTime = series.AuctionEndDateTime
                        });
                    }

                    this.AuctionService.SaveSeriesNumber(seriesNumbers);
                }

                return Task.FromResult(Json(new { status = true }));
            }
            catch (Exception ex)
            {
                return Task.FromResult(Json(new { status = false, msg = ex.Message }));
            }
        }

        [AllowAnonymous]
        public Task<JsonResult> ApproveWinnersList(string secret)
        {
            var mysecret = this.configuration.GetSection("JwtConfig:secret").Value;

            if (secret != mysecret)
            {
                return Task.FromResult(Json(new { status = false, msg = -1 }));
            }

            var winnersDataSet = this.AuctionService.GetWinnersBeforeApproval();
            
            var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(winnersDataSet.Tables[0]).ToList();

            var oraConnectionString = this.configuration.GetSection("MVRS:DefaultConnection").Value;

            try
            {
                this.AuctionService.SaveWinnersToMvrs(oraConnectionString, winners);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            foreach (var series in winners.GroupBy(x => x.SeriesId).Select(x => x.Key))
            { 
                DataSet ds = this.AuctionService.ApproveWinnersList(series, 0);
            }

            return Task.FromResult(Json(new { status = true }));
        }

        [HttpGet]
        public IActionResult UploadResults()
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

        [HttpPost]
        public Task<JsonResult> UploadResults(int seriesId)
        {
            var winnersDataSet = this.AuctionService.GetWinnersSeriesWise(seriesId);

            var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(winnersDataSet.Tables[0]).ToList();

            var oraConnectionString = this.configuration.GetSection("MVRS:DefaultConnection").Value;

            this.AuctionService.SaveWinnersToMvrs(oraConnectionString, winners);

            return Task.FromResult(Json(new { status = true }));
        }
    }
}