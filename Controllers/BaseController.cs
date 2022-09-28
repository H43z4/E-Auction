using System;
using System.Security.Claims;
using DataAccess.Auction;
using eauction.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NotificationServices;
using SmsService;

namespace eauction.Controllers
{
    public class BaseController : Controller
    {
        private readonly ILogger<BaseController> _logger;
        IConfiguration configuration;
        //private readonly string connectionString;
        private readonly ApplicationDbContext applicationDbContext;
        //protected readonly ISmsSender smsSender;
        protected readonly INotificationManager notificationManager;
        public int UserId
        {
            get
            {
                //var claimsIdentity = User.Identity as ClaimsIdentity;

                //if (claimsIdentity != null)
                //{
                //    var Claim = claimsIdentity.FindFirst("Id");

                //    if (Claim != null && !String.IsNullOrEmpty(Claim.Value))
                //    {
                //        return Convert.ToInt32(Claim.Value);
                //    }
                //}
                //var userId = this.User.GetUserId();
                return Convert.ToInt32(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
            }
            set { }
        }

        public AuctionService AuctionService
        {
            get
            {
                return new AuctionService(this.applicationDbContext);
            }

            private set { }
        }

        public AuctionService MvrsRevampService
        {
            get
            {
                return new AuctionService(this.configuration.GetConnectionString("MvrsRevamp"));
            }

            private set { }
        }

        public VIPService VIPService
        {
            get
            {
                return new VIPService(this.applicationDbContext);
            }

            private set { }
        }


        //public BaseController()
        //{

        //}
        public BaseController(ILogger<HomeController> logger, IConfiguration configuration, ApplicationDbContext applicationDbContext, INotificationManager notificationManager)
        {
            _logger = logger;
            this.configuration = configuration;
            //this.connectionString = configuration.GetConnectionString("DefaultConnection");
            //this.auctionService = new AuctionService(this.applicationDbContext, this.connectionString);

            this.applicationDbContext = applicationDbContext;
            //this.emailSender = emailSender;
            this.notificationManager = notificationManager;
            //this.smsSender = smsSender;
        }
    }
}