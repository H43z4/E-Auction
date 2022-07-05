using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccess.Auction;
using eauction.Data;
using eauction.Infrastructure;
using eauction.SignalR;
using iText.Html2pdf;
using iText.StyledXmlParser.Css.Media;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Models.Views.Auction;
using Models.Views.Input;
using NotificationServices;

namespace eauction.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AuctionController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext applicationDbContext;
        private readonly IHubContext<NotificationHub> notificationHub;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly INotificationManager notificationManager;

        public int UserId
        {
            get
            {
                return System.Convert.ToInt32(this.User.FindFirstValue(ClaimTypes.NameIdentifier));
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

        public AuctionController(ApplicationDbContext applicationDbContext,
            IConfiguration configuration,
            IHubContext<NotificationHub> hub,
            IWebHostEnvironment env,
            INotificationManager notificationManager)
        {
            this.configuration = configuration;
            this.applicationDbContext = applicationDbContext;
            this.notificationHub = hub;
            this.webHostEnvironment = env;
            this.notificationManager = notificationManager;
        }

        public JsonResult Error()
        {
            return new JsonResult(new
            {
                status = false,
                msg = "We are not able to process your request at the moment. Sorry, for inconvenience."
            });
        }

        private Task BroadcastHighestBid(int customerId, int seriesNumberId, int highestBidPrice, string time)
        {
            return this.notificationHub.Clients.All.SendAsync("ReceiveMessage", customerId, seriesNumberId, highestBidPrice, time);
        }

        [HttpPost("GetAuctionSeries")]
        public Task<JsonResult> GetAuctionSeries(int seriesStatusId = 2)
        {
            try
            {
                var dataset = this.AuctionService.GetAuctionSeries(seriesStatusId);

                var auctionSeries = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(dataset.Tables[0])
                    .Select(x => new 
                    {
                        x.AuctionEndDateTime,
                        x.AuctionStartDateTime,
                        x.CategoryId,
                        x.CategoryName,
                        x.Id,
                        x.PageNumber,
                        x.PageSize,
                        x.RegEndDateTime,
                        x.RegStartDateTime,
                        x.SeriesName,
                        x.SeriesStatus
                    });

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    auctionSeries
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetAuctionSeriesNumber")]
        public Task<JsonResult> GetAuctionSeriesNumber([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                var dataset = this.AuctionService.GetAuctionSeriesNumber(this.UserId, genericParamtersList.seriesId);

                var auctionSeriesNumbers = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.SeriesNumber>(dataset.Tables[1]);

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    auctionSeriesNumbers
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("SaveApplications")]
        public async Task<JsonResult> SaveApplications([FromBody]List<Models.Views.Input.Application> applications)
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
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
                return new JsonResult(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new
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

            return new JsonResult(new
            {
                status = true,
            });
        }

        [HttpPost("GetApplications")]
        public Task<JsonResult> GetApplications()
        {
            try
            {
                DataSet ds = this.AuctionService.GetApplications(this.UserId);

                var applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]);

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    applications
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetChallan_000000000")]
        public Task<JsonResult> GetChallan([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                DataSet ds = this.AuctionService.GetApplicationDetail(genericParamtersList.applicationId);

                var application = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).FirstOrDefault();

                if (application != null)
                {
                    if (application.CustomerId != this.UserId || application.ApplicationStatusId != 1)
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = false,
                            msg = "Invalid application."
                        }));
                    }

                    var pdfFile = $"{this.webHostEnvironment.WebRootPath}/Reports/pdfFiles/{application.AIN}.pdf";

                    if (!System.IO.File.Exists(pdfFile))
                    {
                        var htmlFile = $"{this.webHostEnvironment.WebRootPath}/Reports/Template/challanForm.html";

                        var html = System.IO.File.ReadAllText(htmlFile);

                        var user = this.AuctionService.GetUser(this.UserId);

                        html = html.Replace("@User", user.FullName)
                            .Replace("@AIN", application.AIN);

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

                    //// return resulted pdf document
                    //FileResult fileResult = new FileContentResult(System.IO.File.ReadAllBytes(pdfFile), "application/pdf");
                    //fileResult.FileDownloadName = $"{application.AIN}.pdf";
                    //return fileResult;

                    var challanFormLink = $"{this.Request.Scheme}://{this.Request.Host.Value}{this.Request.PathBase.Value}/Reports/pdfFiles/{application.AIN}.pdf";

                    return Task.FromResult(new JsonResult(new
                    {
                        status = true,
                        challanFormLink
                    }));
                }

                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    msg = "Invalid application."
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetChallan")]
        public async Task<JsonResult> GetPSID_Temp([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                var ds = this.AuctionService.GetPSIdInputModel(genericParamtersList.applicationId.ToString());

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


                return new JsonResult(new
                {
                    status = false,
                    psId = "Your PSID has been generated. Please, refresh."
                });
            }
            catch
            {
                return this.Error();
            }
        }

        public Task<JsonResult> GetBids_0()
        {
            try
            {
                string sqlException = string.Empty;

                DataSet ds = this.AuctionService.GetBids(this.UserId, out sqlException);

                if (ds.Tables.Count == 0)
                {
                    if (!string.IsNullOrEmpty(sqlException))
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = false,
                            errCode = 99,
                            msg = sqlException
                        }));
                    }
                }

                Bids bids = new Bids()
                {
                    Applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).ToList(),
                    HighestBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.HighestBid>(ds.Tables[1]).ToList(),
                    LastBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.LastBid>(ds.Tables[2]).ToList(),
                };

                var yourBids = bids.Applications.GroupJoin(bids.HighestBids, a => a.SeriesNumberId, b => b.SeriesNumberId,
                    (a, b) => new
                    {
                        applications = a,
                        highestBids = b.FirstOrDefault() ?? new HighestBid()
                    })
                    .GroupJoin(bids.LastBids, a => a.applications.Id, b => b.ApplicationId,
                    (a, b) => new 
                    {
                        applications = a.applications,
                        highestBids = a.highestBids,
                        yourLastBid = b.FirstOrDefault() ?? new LastBid()
                    })
                    .Select(x => new
                    {
                        x.applications.AIN,
                        x.applications.ApplicationStatus,
                        x.applications.ReservePrice,
                        x.applications.Series,
                        x.applications.SeriesCategory,
                        x.applications.SeriesNumber,
                        x.applications.SeriesNumberId,
                        x.highestBids.HighestBiddingPrice,
                        highestBidCreatedOn = x.highestBids.HighestBiddingPrice > 0 ? Infrastructure.DateTimeTimeAgo.TimeAgo(x.highestBids.CreatedOn) : string.Empty,
                        highestBidder = x.highestBids.HighestBiddingPrice > 0 ? x.highestBids.CreatedBy == this.UserId ? "Your bidding" : "Other person" : string.Empty,
                        yourLastBiddingPrice = x.yourLastBid.LastBiddingPrice,
                        yourLastBidCreatedOn = x.yourLastBid.LastBiddingPrice > 0 ? Infrastructure.DateTimeTimeAgo.TimeAgo(x.yourLastBid.CreatedOn) : string.Empty,
                    });

                var auctionEndDateTime = bids.Applications.FirstOrDefault()?.AuctionEndDateTime;

                var remainingTime = auctionEndDateTime - DateTime.Now;

                var disclaimer = $"{this.configuration.GetSection("KeyValues:Disclaimer").Value}";

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    disclaimer,
                    //auctionEndDateTime = bids.Applications.FirstOrDefault()?.AuctionEndDateTime,
                    //sysDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"),
                    remainingTime = (long)remainingTime.Value.TotalMilliseconds,
                    customerId = this.UserId,
                    yourBids
                }));
            }
            catch (SqlException ex)
            {
                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                }));
            }
            catch (Exception ex) when (ex.InnerException is SqlException)
            {
                //var sqlException = ex as SqlException;

                //if (sqlException != null)
                //{
                //    return Task.FromResult(new JsonResult(new
                //    {
                //        status = false,
                //        errCode = sqlException.Number,
                //        msg = ex.Message
                //    }));
                //}

                //if(ex.err)

                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetBids")]
        public Task<JsonResult> GetBids()
        {
            try
            {
                string sqlException = string.Empty;

                DataSet ds = this.AuctionService.GetBids(this.UserId, out sqlException);

                if (ds.Tables.Count == 0)
                {
                    if (!string.IsNullOrEmpty(sqlException))
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = false,
                            errCode = 99,
                            msg = sqlException
                        }));
                    }
                }

                Bids bids = new Bids()
                {
                    Applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).ToList(),
                    HighestBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.HighestBid>(ds.Tables[1]).ToList(),
                    LastBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.LastBid>(ds.Tables[2]).ToList(),
                };

                var yourBids = bids.Applications.GroupJoin(bids.HighestBids, a => a.SeriesNumberId, b => b.SeriesNumberId,
                    (a, b) => new
                    {
                        applications = a,
                        highestBids = b.FirstOrDefault() ?? new HighestBid()
                    })
                    .GroupJoin(bids.LastBids, a => a.applications.Id, b => b.ApplicationId,
                    (a, b) => new
                    {
                        applications = a.applications,
                        highestBids = a.highestBids,
                        yourLastBid = b.FirstOrDefault() ?? new LastBid()
                    })
                    .Select(x => new
                    {
                        x.applications.AIN,
                        x.applications.ApplicationStatus,
                        x.applications.ReservePrice,
                        x.applications.Series,
                        x.applications.SeriesCategory,
                        x.applications.SeriesNumber,
                        x.applications.SeriesNumberId,
                        x.highestBids.HighestBiddingPrice,
                        //highestBidCreatedOn = x.highestBids.HighestBiddingPrice > 0 ? Infrastructure.DateTimeTimeAgo.TimeAgo(x.highestBids.CreatedOn) : string.Empty,
                        highestBidCreatedOn = x.highestBids.HighestBiddingPrice > 0 ? x.highestBids.CreatedOn.ToString("dd-MM-yyyy HH:mm:ss:fff") : string.Empty,
                        highestBidder = x.highestBids.HighestBiddingPrice > 0 ? x.highestBids.CreatedBy == this.UserId ? "Your bidding" : "Other person" : string.Empty,
                        yourLastBiddingPrice = x.yourLastBid.LastBiddingPrice,
                        yourLastBidCreatedOn = x.yourLastBid.LastBiddingPrice > 0 ? Infrastructure.DateTimeTimeAgo.TimeAgo(x.yourLastBid.CreatedOn) : string.Empty,
                        //auctionEndDateTime = x.applications.AuctionEndDateTime,
                        //remainingTime = (long)(x.applications.AuctionEndDateTime - DateTime.Now).TotalMilliseconds
                    });

                var auctionEndDateTime = bids.Applications.FirstOrDefault()?.AuctionEndDateTime;

                var remainingTime = auctionEndDateTime - DateTime.Now;

                var disclaimer = $"{this.configuration.GetSection("KeyValues:Disclaimer").Value}";

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    disclaimer,
                    auctionEndDateTime = bids.Applications.FirstOrDefault()?.AuctionEndDateTime,
                    //sysDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"),
                    remainingTime = (long)remainingTime.Value.TotalMilliseconds,
                    customerId = this.UserId,
                    yourBids
                }));
            }
            catch (SqlException ex)
            {
                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                }));
            }
            catch (Exception ex) when (ex.InnerException is SqlException)
            {
                //var sqlException = ex as SqlException;

                //if (sqlException != null)
                //{
                //    return Task.FromResult(new JsonResult(new
                //    {
                //        status = false,
                //        errCode = sqlException.Number,
                //        msg = ex.Message
                //    }));
                //}

                //if(ex.err)

                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetBidsMultipleSeries")]
        public Task<JsonResult> GetBidsMultipleSeries()
        {
            try
            {
                string sqlException = string.Empty;

                DataSet ds = this.AuctionService.GetBidsMultipleSeries(this.UserId, out sqlException);

                if (ds.Tables.Count == 0)
                {
                    if (!string.IsNullOrEmpty(sqlException))
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = false,
                            errCode = 99,
                            msg = sqlException
                        }));
                    }
                }

                Bids bids = new Bids()
                {
                    Applications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Application>(ds.Tables[0]).ToList(),
                    HighestBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.HighestBid>(ds.Tables[1]).ToList(),
                    LastBids = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.LastBid>(ds.Tables[2]).ToList(),
                };

                var yourBids = bids.Applications.GroupJoin(bids.HighestBids, a => a.SeriesNumberId, b => b.SeriesNumberId,
                    (a, b) => new
                    {
                        applications = a,
                        highestBids = b.FirstOrDefault() ?? new HighestBid()
                    })
                    .GroupJoin(bids.LastBids, a => a.applications.Id, b => b.ApplicationId,
                    (a, b) => new
                    {
                        applications = a.applications,
                        highestBids = a.highestBids,
                        yourLastBid = b.FirstOrDefault() ?? new LastBid()
                    })
                    .Select(x => new
                    {
                        x.applications.AIN,
                        x.applications.ApplicationStatus,
                        x.applications.ReservePrice,
                        x.applications.Series,
                        x.applications.SeriesCategory,
                        x.applications.SeriesNumber,
                        x.applications.SeriesNumberId,
                        x.highestBids.HighestBiddingPrice,
                        //highestBidCreatedOn = x.highestBids.HighestBiddingPrice > 0 ? Infrastructure.DateTimeTimeAgo.TimeAgo(x.highestBids.CreatedOn) : string.Empty,
                        highestBidCreatedOn = x.highestBids.HighestBiddingPrice > 0 ? x.highestBids.CreatedOn.ToString("dd-MM-yyyy HH:mm:ss:fff") : string.Empty,
                        highestBidder = x.highestBids.HighestBiddingPrice > 0 ? x.highestBids.CreatedBy == this.UserId ? "Your bidding" : "Other person" : string.Empty,
                        yourLastBiddingPrice = x.yourLastBid.LastBiddingPrice,
                        yourLastBidCreatedOn = x.yourLastBid.LastBiddingPrice > 0 ? Infrastructure.DateTimeTimeAgo.TimeAgo(x.yourLastBid.CreatedOn) : string.Empty,
                        //auctionEndDateTime = x.applications.AuctionEndDateTime,
                        remainingTime = (long)(x.applications.AuctionEndDateTime - DateTime.Now).TotalMilliseconds
                    });

                //var auctionEndDateTime = bids.Applications.FirstOrDefault()?.AuctionEndDateTime;

                //var remainingTime = auctionEndDateTime - DateTime.Now;

                var disclaimer = $"{this.configuration.GetSection("KeyValues:Disclaimer").Value}";

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    disclaimer,
                    //auctionEndDateTime = bids.Applications.FirstOrDefault()?.AuctionEndDateTime,
                    //sysDate = DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"),
                    //remainingTime = (long)remainingTime.Value.TotalMilliseconds,
                    customerId = this.UserId,
                    yourBids
                }));
            }
            catch (SqlException ex)
            {
                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                }));
            }
            catch (Exception ex) when (ex.InnerException is SqlException)
            {
                //var sqlException = ex as SqlException;

                //if (sqlException != null)
                //{
                //    return Task.FromResult(new JsonResult(new
                //    {
                //        status = false,
                //        errCode = sqlException.Number,
                //        msg = ex.Message
                //    }));
                //}

                //if(ex.err)

                return Task.FromResult(this.Error());
            }
        }


        [HttpPost("SaveBid")]
        public Task<JsonResult> SaveBid([FromBody]NewBid newBid)
        {
            try
            {
                string sqlException = string.Empty;

                DataSet ds = this.AuctionService.SaveBid(this.UserId, newBid, out sqlException);

                if (ds.Tables.Count == 0)
                {
                    if (!string.IsNullOrEmpty(sqlException))
                    {
                        return Task.FromResult(new JsonResult(new
                        {
                            status = false,
                            errCode = 99,
                            msg = sqlException
                        }));
                    }
                }

                var highestBid = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.HighestBid>(ds.Tables[0])
                    .FirstOrDefault(x => x.AIN == newBid.AIN);

                //var time = Infrastructure.DateTimeTimeAgo.TimeAgo(highestBid.CreatedOn);
                var time = highestBid.CreatedOn.ToString("dd-MM-yyyy HH:mm:ss:fff");

                this.BroadcastHighestBid(this.UserId, highestBid.SeriesNumberId, highestBid.HighestBiddingPrice, time);

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    highestBid.SeriesNumberId,
                    msg = $"Your bid has successfully been submitted at {time}"
                }));
            }
            catch (SqlException ex)
            {
                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                }));
            }
            catch(Exception ex)
            {
                return Task.FromResult(this.Error()); 
            }
        }

        [HttpPost("GetWinners")]
        public Task<JsonResult> GetWinners()
        {
            try
            {
                //DataSet ds = this.AuctionService.GetWinners(this.UserId);

                //var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds.Tables[0])
                //    .ToList()
                //    .Select(x => new
                //        {
                //            x.HighestBiddingPrice,
                //            x.ReservePrice,
                //            x.Series,
                //            x.SeriesCategory,
                //            x.SeriesNumber,
                //            x.WinnerAIN,
                //            x.YourAIN,
                //            x.YourHighestBiddingPrice
                //        });

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    winners = new List<Models.Views.Auction.Winners>()
                }));
            }
            catch (SqlException ex)
            {
                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    errCode = ex.Number,
                    msg = ex.Message
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetNotifications")]
        public Task<JsonResult> GetNotifications()
        {
            try
            {
                DataSet ds = this.AuctionService.GetNotifications();

                var notifications = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Notification.Notification>(ds.Tables[0])
                    .ToList()
                    .Select(x => new
                    {
                        x.Content,
                        x.Data,
                        x.Heading,
                        Dated = x.CreatedOn,
                    });

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    notifications
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }


        [HttpPost("GetClosedSeries")]
        public Task<JsonResult> GetClosedSeries()
        {
            try
            {
                DataSet ds = this.AuctionService.GetSeriesClosedForBidding();

                var series = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0])
                    .Select(x => new
                    {
                        x.Id,
                        Name = $"{x.CategoryName} [{x.SeriesName}]"
                    });

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    series
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        public Task<JsonResult> DownloadWinners_0([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                DataSet ds = this.AuctionService.GetWinnersSeriesWise(genericParamtersList.seriesId);

                var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds.Tables[0]);

                if (winners.Count() > 0)
                {
                    var pdfFile = $"{this.webHostEnvironment.WebRootPath}/Reports/pdfFiles/Winners/{genericParamtersList.seriesId}.pdf";

                    if (!System.IO.File.Exists(pdfFile))
                    {
                        var htmlFile = $"{this.webHostEnvironment.WebRootPath}/Reports/Template/winners.html";

                        var html = System.IO.File.ReadAllText(htmlFile);

                        //html = Engine.Razor.Run(html, typeof(List<Models.Views.Auction.Winners>), winners);

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

                    //// return resulted pdf document
                    //FileResult fileResult = new FileContentResult(System.IO.File.ReadAllBytes(pdfFile), "application/pdf");
                    //fileResult.FileDownloadName = $"{application.AIN}.pdf";
                    //return fileResult;

                    var winnersLink = $"{this.Request.Scheme}://{this.Request.Host.Value}{this.Request.PathBase.Value}/Reports/pdfFiles/Winners/{genericParamtersList.seriesId}.pdf";

                    return Task.FromResult(new JsonResult(new
                    {
                        status = true,
                        winnersLink
                    }));
                }

                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    msg = "Not available."
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }

        [HttpPost("DownloadWinners")]
        public Task<JsonResult> DownloadWinners([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                if (genericParamtersList.seriesId > 0)
                {
                    var pdfFile = $"{this.webHostEnvironment.WebRootPath}\\Reports\\pdfFiles\\Winners\\{genericParamtersList.seriesId}.pdf";

                    if (!System.IO.File.Exists(pdfFile))
                    {
                        DataSet ds1 = this.AuctionService.GetWinnersSeriesWise(genericParamtersList.seriesId);

                        var winners = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Winners>(ds1.Tables[0])
                        .OrderBy(x => x.SeriesNumber)
                        .ToList();

                        this.GeneratePdf(genericParamtersList.seriesId, winners);
                    }

                    var winnersLink = $"{this.Request.Scheme}://{this.Request.Host.Value}{this.Request.PathBase.Value}/Reports/pdfFiles/Winners/{genericParamtersList.seriesId}.pdf";

                    return Task.FromResult(new JsonResult(new
                    {
                        status = true,
                        winnersLink
                    }));
                }

                return Task.FromResult(new JsonResult(new
                {
                    status = false,
                    msg = "Coming soon."
                }));
            }
            catch(Exception ex)
            {
                return Task.FromResult(this.Error());
            }
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



        [HttpPost("GetSchedule")]
        public Task<JsonResult> GetSchedule()
        {
            try
            {
                var ds = this.AuctionService.GetSeriesSchedule();

                var schedule = Infrastructure.DataTableExtension.DataTableToList<Models.Views.Auction.Series>(ds.Tables[0])
                    .Select(x => new
                    {
                        x.AuctionEndDateTime,
                        x.AuctionStartDateTime,
                        x.CategoryName,
                        x.RegEndDateTime,
                        x.RegStartDateTime,
                        x.SeriesName
                    });

                return Task.FromResult(new JsonResult(new
                {
                    status = true,
                    schedule
                }));
            }
            catch
            {
                return Task.FromResult(this.Error());
            }
        }


        #region EPayment

        [HttpPost("PaymentIntimation")]
        public async Task<JsonResult> SavePaymentStatus(ePayStatusUpdate ePayStatusUpdate)
        {
            if (!ModelState.IsValid)
            {
                var modelStateErrors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);

                return new JsonResult(new
                {
                    status = false,
                    msg = "Data validation failed.",
                    errors = modelStateErrors
                });
            }

            try
            {
                string sqlException = string.Empty;

                this.AuctionService.SavePSIdStatus(ePayStatusUpdate, out sqlException);
                
                if (!string.IsNullOrEmpty(sqlException))
                {
                    return await Task.FromResult(new JsonResult(new
                    {
                        status = false,
                        msg = sqlException
                    }));
                }

                return new JsonResult(new
                {
                    status = true,
                    msg = "OK"
                });
            }
            catch (SqlException ex)
            {
                return new JsonResult(new
                {
                    status = false,
                    msg = ex.Message
                });
            }
            catch (Exception ex)
            {
                return await Task.FromResult(this.Error());
            }
        }

        [HttpPost("GetPSId")]
        public async Task<IActionResult> GetPSId([FromBody]GenericParamtersList genericParamtersList)
        {
            try
            {
                var ds = this.AuctionService.GetPSIdInputModel(genericParamtersList.applicationId.ToString());

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


                return new JsonResult(new
                {
                    status = true,
                    psId = ePayApps.FirstOrDefault().psId
                });
            }
            catch
            {
                return new JsonResult(new
                {
                    status = false,
                    msg = "Try again."
                });
            }
        }

        #endregion
    }
}