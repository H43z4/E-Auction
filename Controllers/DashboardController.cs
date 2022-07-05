using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using eauction.Data;
using eauction.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Hosting;
using NotificationServices;
using Microsoft.AspNetCore.Authorization;

namespace eauction.Controllers
{
    [Authorize(Roles = "System,SuperAdmin,Special")]
    public class DashboardController : BaseController
    {
        private readonly IHubContext<NotificationHub> notificationHub;
        private readonly IWebHostEnvironment webHostEnvironment;

        public DashboardController(ILogger<HomeController> logger,
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

        public IActionResult Dashboard(string CateogryList, string SeriesList, string txtFromDate, string txtToDate)   //Registring Customers
        {
            if (CateogryList == null)
            {
                CateogryList = "0";
            }
            //if (SeriesList == null)
            //{
            //    SeriesList = "0";
            //}
            //if (txtFromDate==null)
            //{
            //    txtFromDate = "";
            //}
            //if (txtToDate == null)
            //{
            //    txtToDate = "";
            //}
            var dataset = this.AuctionService.GetDashboard(CateogryList, "0", "", "");
            var dashboard = new Models.Views.Dashboard.Dashboard();
            dashboard.totalCounters = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.TotalCounters>(dataset.Tables[0]);
            dashboard.SeriesCategoryList = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.SeriesCategory>(dataset.Tables[1]);
            dashboard.TopTenRevenueSeries = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.TotalRevenueSeriesWise>(dataset.Tables[2]);
            //dashboard.TopTenRevenueNumbers = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.TopTenRevenueNumbers>(dataset.Tables[6]);
            dashboard.TopTenApplicationSeries = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.TopTenApplicationsSeries>(dataset.Tables[3]);
            dashboard.TopTenApplicationsNumbers = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.TopTenApplicationsNumbers>(dataset.Tables[4]);
            ViewBag.CatID = CateogryList;
            //TempData["SeriesID"] = SeriesList;
            //ViewBag.FromDate = txtFromDate;
            //ViewBag.ToDate = txtToDate;
            return View(dashboard);
        }

        public IActionResult LiveBid(string CateogryList, string SeriesList, string RegNo)
        {
            if (CateogryList == null)
            {
                CateogryList = "0";
            }
            if (SeriesList == null)
            {
                SeriesList = "0";
            }
            if (RegNo == null)
            {
                RegNo = "";
            }
            var dataset = this.AuctionService.GetLiveBid(CateogryList, SeriesList, RegNo);
            var objLiveBitList = new Models.Views.Dashboard.LiveBidList();
            objLiveBitList.BidList = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.LiveBidData>(dataset.Tables[0]);
            ViewBag.CatID = CateogryList;
            TempData["SeriesID"] = SeriesList;
            ViewBag.RegNo = RegNo;
            return View(objLiveBitList);
        }

        public JsonResult GetSeriesByCatID(string cateid)
        {
            var dataset = this.AuctionService.GetSeriesByCategoryID(cateid);
            var SeriesList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(Infrastructure.DataTableExtension.DataTableToList<Models.Views.Dashboard.SeriesDropdown>(dataset.Tables[0]));
            return Json(SeriesList.Items);
        }

    }
}