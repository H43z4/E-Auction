using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using eauction.Data;
using eauction.SignalR;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using NotificationServices;
using Models.Domain.Auction;
using System;
using System.Linq;

namespace eauction.Controllers
{
    public class LOVController : BaseController
    {
        private readonly IHubContext<NotificationHub> notificationHub;
        private readonly IWebHostEnvironment webHostEnvironment;

        public LOVController(ILogger<HomeController> logger,
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

        public Task<JsonResult> GetSeries(int seriesCategoryId)
        {
            try
            {
                var ds = this.AuctionService.GetSeries(seriesCategoryId);

                var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0])
                    .Where(x => x.SeriesStatusId != 5 && x.AuctionEndDateTime < DateTime.Now)  //   5 = Series Closed
                    .Select(x => new 
                    {
                        x.Id,
                        x.SeriesName
                    })
                    .ToList();

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    series
                }));
            }
            catch
            {
                return this.Error();
            }
        }

        public Task<JsonResult> GetSeriesAll(int seriesCategoryId)
        {
            try
            {
                var ds = this.AuctionService.GetSeries(seriesCategoryId);

                var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0])
                    //.Where(x => x.SeriesStatusId != 5 && x.AuctionEndDateTime < DateTime.Now)  //   5 = Series Closed
                    .Select(x => new
                    {
                        x.Id,
                        x.SeriesName
                    })
                    .ToList();

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    series
                }));
            }
            catch
            {
                return this.Error();
            }
        }

        private Task<JsonResult> Error()
        {
            return Task.FromResult(new JsonResult(new
            {
                status = false,
                msg = "Error",
            }));
        }
    }
}