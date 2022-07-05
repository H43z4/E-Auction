using eauction.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models.Domain.Auction;
using Models.Domain.Identity;
using Models.Domain.Mail;
using Models.Domain.Notification;
using Models.Views.Auction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataAccess.Auction
{
    public class VIPService //: IAuctionService
    {
        readonly ApplicationDbContext db;
        private readonly string connectionString;

        public VIPService(ApplicationDbContext _db)
        {
            this.db = _db;
            this.connectionString = _db.Database.GetDbConnection().ConnectionString;
        }

        public DataSet GetAuctionSeries()
        {
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("vipGetAuctionSeries", null);

            return dataset;
        }

        public DataSet GetSeriesBeforeRegistration()
        {
            SqlParameter[] sql = new SqlParameter[0];

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetSeriesBeforeRegistration", sql);

            return dataset;
        }

        public DataSet GetAuctionSeries(int? seriesStatusId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            
            sql[0] = new SqlParameter("@SeriesStatusId", SqlDbType.Int)
            {
                Value = (object)seriesStatusId ?? DBNull.Value,
                Direction = System.Data.ParameterDirection.Input,
            };

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetAuctionSeries", sql);

            return dataset;
        }

        public DataSet GetAuctionSeriesStatusWise()
        {
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetAuctionSeriesStatusWise", null);

            return dataset;
        }
        

        public DataSet GetAuctionSeriesNumber(int customerId, int seriesId)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@SeriesId", seriesId);

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("vipGetAuctionSeriesNumber", sql);

            return dataset;
        }

        public DataSet SaveApplications(int customerId, DataTable applications)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@Applications", SqlDbType.Structured)
            {
                Value = applications,
                Direction = System.Data.ParameterDirection.Input,
                IsNullable = true,
                TypeName = "dbo.TYPE_Application"
            };

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("vipSaveApplicationBulk", sql);

            return ds;
        }

        public EmailSetting GetEmailSetting(int msgTypeId)
        {
            return this.db.EmailSetting.Where(x => x.MessageTypeId == msgTypeId).FirstOrDefault();
        }

        public void SaveSeries(List<Models.Domain.Auction.Series> series)
        {
            this.db.Series.AddRange(series);
            this.db.SaveChanges();
        }

        public bool SaveSeries(Models.Views.Auction.Series series, int userId)
        {
            bool result = false;

            var srs = this.db.Series.SingleOrDefault(x => x.Id == series.Id);

            if (srs != null)
            {

                using (var transaction = this.db.Database.BeginTransaction())
                {
                    this.db.Entry<Models.Domain.Auction.Series>(srs).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                    srs.RegEndDateTime = series.RegEndDateTime;
                    srs.RegStartDateTime = series.RegStartDateTime;
                    srs.AuctionEndDateTime = series.AuctionEndDateTime;
                    srs.AuctionStartDateTime = series.AuctionStartDateTime;
                    srs.SeriesStatusId = series.SeriesStatusId;

                    this.db.SeriesStatusHistory.Add(new Models.Domain.Auction.SeriesStatusHistory()
                    {
                        SeriesId = series.Id,
                        SeriesStatusId = series.SeriesStatusId,
                        CreatedBy = userId
                    });

                    var rowsAffected = this.db.SaveChanges();

                    result = rowsAffected > 0;

                    transaction.Commit();
                }
            }

            return result;
        }

        public void SaveSeriesNumber(List<Models.Domain.Auction.SeriesNumber> seriesNumbers)
        {
            this.db.SeriesNumber.AddRange(seriesNumbers);
            this.db.SaveChanges();
        }

        public bool ApproveApplication(int applicationId, int userId, out int customerId, out string AIN)
        {
            bool result = false;

            var application = this.db.Application.SingleOrDefault(x => x.Id == applicationId);

            customerId = application.CustomerId;
            AIN = application.AIN;

            if (application != null)
            {
                using (var transaction = this.db.Database.BeginTransaction())
                {
                    this.db.Entry<Models.Domain.Auction.Application>(application).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    application.ApplicationStatusId = 2;

                    this.db.ApplicationStatusHistory.Add(new Models.Domain.Auction.ApplicationStatusHistory()
                    {
                        Application = application,
                        ApplicationStatusId = 2,
                        CreatedBy = userId
                    });

                    var rowsAffected = this.db.SaveChanges();

                    result = rowsAffected > 0;

                    transaction.Commit();
                }
            }

            return result;
        }

        public decimal SaveBid_v0(int customerId, NewBid newBid)
        {
            SqlParameter[] sql = new SqlParameter[3];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@AIN", newBid.AIN);
            sql[2] = new SqlParameter("@BidPrice", newBid.BidPrice);

            var id = new Query.Execution(this.connectionString).Execute_Scaler("SaveBid", sql);

            //return (decimal)applicationId;
            return 0;
        }

        public DataSet SaveBid(int customerId, NewBid newBid)
        {
            SqlParameter[] sql = new SqlParameter[3];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@AIN", newBid.AIN);
            sql[2] = new SqlParameter("@BidPrice", newBid.BidPrice);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("SaveBid", sql);

            return ds;
        }

        public DataSet SaveBid(int customerId, NewBid newBid, out string sqlException)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[3];
                sql[0] = new SqlParameter("@CustomerId", customerId);
                sql[1] = new SqlParameter("@AIN", newBid.AIN);
                sql[2] = new SqlParameter("@BidPrice", newBid.BidPrice);

                var ds = new Query.Execution(this.connectionString).Execute_DataSet("SaveBid", sql);

                sqlException = string.Empty;
                
                return ds;
            }
            catch (SqlException ex)
            {
                //this.SqlException = $"{ex.Number}---{ex.Message}";
                //this.SqlException = $"{ex.Number}---{ex.Message}";
                sqlException = ex.Message;
            }

            return new DataSet();
        }


        public decimal SaveApplication(int customerId, int seriesNumberId)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@SeriesNumberId", seriesNumberId);

            var applicationId = new Query.Execution(this.connectionString).Execute_Scaler("SaveApplication", sql);

            //return (decimal)applicationId;
            return 0;
        }

        public decimal SaveApplications(int customerId, string seriesNumberIdCSVs)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@SeriesNumberIdCSVs", seriesNumberIdCSVs);

            var applicationId = new Query.Execution(this.connectionString).Execute_Scaler("SaveApplicationMultiple", sql);

            //return (decimal)applicationId;
            return 0;
        }


        public DataSet GetApplications(int customerId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@CustomerId", customerId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetApplications", sql);

            return ds;
        }

        public DataSet GetWinners(int customerId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@CustomerId", customerId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetWinners", sql);

            return ds;
        }

        public DataSet GetWinnersDistinct()
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@CustomerId", DBNull.Value);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetWinnersDistinct", sql);

            return ds;
        }

        public DataSet GetCustomerApplication(int customerId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@CustomerId", customerId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetCustomerApplication", sql);

            return ds;
        }

        public DataSet GetApplicationDetail(int applicationId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@ApplicationId", applicationId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetApplicationDetail", sql);

            return ds;
        }

        public List<ApplicationStatus> GetApplicationStatus()
        {
            return this.db.ApplicationStatus.ToList();
        }

        public DataSet GetSeriesDetail(int seriesId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@SeriesId", seriesId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetSeriesDetail", sql);

            return ds;
        }

        public List<SeriesStatus> GetSeriesStatus()
        {
            return this.db.SeriesStatus.ToList();
        }

        public Models.Views.Auction.Application FindApplication(string AIN)
        {
            var application = this.db.SeriesCategory //.Where(x => x.AIN == AIN)
                .Join(this.db.Series, a => a.Id, b => b.SeriesCategoryId,
                (a, b) => new
                {
                    seriesCategory = a,
                    series = b
                })

                .Join(this.db.SeriesNumber, a => a.series.Id, b => b.SeriesId,
                (a, b) => new
                {
                    seriesCategory = a.seriesCategory,
                    series = a.series,
                    seriesNumber = b
                })

                .Join(this.db.Application.Where(x => x.AIN == AIN), a => a.seriesNumber.Id, b => b.SeriesNumberId,
                (a, b) => new
                {
                    seriesCategory = a.seriesCategory,
                    series = a.series,
                    seriesNumber = a.seriesNumber,
                    application = b
                })

                .Join(this.db.Users, a => a.application.CustomerId, b => b.Id,
                (a, b) => new
                {
                    seriesCategory = a.seriesCategory,
                    series = a.series,
                    seriesNumber = a.seriesNumber,
                    application = a.application,
                    customer = b
                })

                .Select(x => new Models.Views.Auction.Application()
                {
                    AIN = x.application.AIN,
                    ApplicationStatusId = x.application.ApplicationStatusId,
                    Customer = x.customer.NormalizedUserName,
                    ReservePrice = x.seriesNumber.ReservePrice,
                    Series = x.series.Name,
                    SeriesCategory = x.seriesCategory.Name,
                    SeriesNumber = x.seriesNumber.AuctionNumber
                })
                .ToList()
                .FirstOrDefault();

            return application;
        }

        public User GetUser(int userId)
        {
            return this.db.Users.SingleOrDefault(x => x.Id == userId);
        }

        public List<Models.Views.Auction.Application> FindApplications_0(Models.Views.Auction.Application app)
        {
            //System.Linq.Expressions.Expression<Func<Application, bool>> expression = new System.Linq.Expressions.Expression<Func<Application, bool>>()
            //Func<Application, bool> func = new Func<Application, bool>(x => x.AIN == app.AIN);
            //Func<Application, bool> func = x => x.AIN == app.AIN;

            Func<Models.Domain.Auction.Application, bool> applicationFilterFunc = x => x.ApplicationStatusId == app.ApplicationStatusId;

            if (!string.IsNullOrEmpty(app.AIN))
            {
                applicationFilterFunc += x => x.AIN == app.AIN;
            }

            System.Linq.Expressions.Expression<Func<Models.Domain.Auction.Application, bool>> applicationFilter = x => x.ApplicationStatusId == app.ApplicationStatusId;
            //System.Linq.Expressions.Expression<Func<Application, bool>> applicationFilter = applicationFilterFunc;

            var applications = this.db.SeriesCategory
                .Join(this.db.Series, a => a.Id, b => b.SeriesCategoryId,
                (a, b) => new
                {
                    seriesCategory = a,
                    series = b
                })

                .Join(this.db.SeriesNumber, a => a.series.Id, b => b.SeriesId,
                (a, b) => new
                {
                    seriesCategory = a.seriesCategory,
                    series = a.series,
                    seriesNumber = b
                })

                .Join(this.db.Application.Where(applicationFilter), a => a.seriesNumber.Id, b => b.SeriesNumberId,
                (a, b) => new
                {
                    seriesCategory = a.seriesCategory,
                    series = a.series,
                    seriesNumber = a.seriesNumber,
                    application = b
                })

                .Join(this.db.Users, a => a.application.CustomerId, b => b.Id,
                (a, b) => new
                {
                    seriesCategory = a.seriesCategory,
                    series = a.series,
                    seriesNumber = a.seriesNumber,
                    application = a.application,
                    customer = b
                })
                
                .Select(x => new Models.Views.Auction.Application()
                {
                    Id = x.application.Id,
                    AIN = x.application.AIN,
                    ApplicationStatusId = x.application.ApplicationStatusId,
                    CustomerId = x.application.CustomerId,
                    Customer = x.customer.NormalizedUserName,
                    ReservePrice = x.seriesNumber.ReservePrice,
                    Series = x.series.Name,
                    SeriesCategory = x.seriesCategory.Name,
                    SeriesNumber = x.seriesNumber.AuctionNumber
                })
                .OrderBy(x => x.Id)
                .Skip((app.PageNumber - 1) * app.PageSize)
                .Take(app.PageSize)
                .ToList();

            return applications;
        }

        public DataSet GetSeriesClosedForBidding()
        {
            SqlParameter[] sql = new SqlParameter[0];

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetSeriesClosedForBidding", sql);

            return dataset;
        }

        public DataSet FindApplications(Models.Views.Auction.Application application)
        {
            SqlParameter[] sql = new SqlParameter[5];
            sql[0] = new SqlParameter("@CustomerId", application.CustomerId > 0 ? application.CustomerId : (object)DBNull.Value);
            sql[1] = new SqlParameter("@ApplicationStatusId", application.ApplicationStatusId);
            sql[2] = new SqlParameter("@AIN", application.AIN != null ? application.AIN : (object)DBNull.Value);
            sql[3] = new SqlParameter("@PageNumber", application.PageNumber);
            sql[4] = new SqlParameter("@PageSize", application.PageSize);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetApplications", sql);

            return ds;
        }

        public DataSet FindApplications_1(Models.Views.Auction.Application application)
        {
            SqlParameter[] sql = new SqlParameter[5];
            sql[0] = new SqlParameter("@CustomerId", application.CustomerId > 0 ? application.CustomerId : (object)DBNull.Value);
            sql[1] = new SqlParameter("@ApplicationStatusId", application.ApplicationStatusId);
            sql[2] = new SqlParameter("@AIN", application.AIN != null ? application.AIN : (object)DBNull.Value);
            sql[3] = new SqlParameter("@PageNumber", application.PageNumber);
            sql[4] = new SqlParameter("@PageSize", application.PageSize);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetApplicationsFiltered", sql);

            return ds;
        }

        public SqlException SqlException { get; set; }

        public DataSet GetBids_0(int customerId)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[1];
                sql[0] = new SqlParameter("@CustomerId", customerId);

                var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetBids", sql);

                return ds;
            }
            catch (SqlException ex)
            {
                this.SqlException = ex;
            }

            return new DataSet();

            //SqlParameter[] sql = new SqlParameter[1];
            //sql[0] = new SqlParameter("@CustomerId", customerId);

            //var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetBids", sql);

            //return ds;
        }
        
        public DataSet GetBids(int customerId, out string sqlException)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[1];
                sql[0] = new SqlParameter("@CustomerId", customerId);

                var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetBids", sql);

                sqlException = string.Empty;

                return ds;
            }
            catch (SqlException ex)
            {
                //this.SqlException = $"{ex.Number}---{ex.Message}";
                //this.SqlException = $"{ex.Number}---{ex.Message}";
                sqlException = ex.Message;
            }

            return new DataSet();

            //SqlParameter[] sql = new SqlParameter[1];
            //sql[0] = new SqlParameter("@CustomerId", customerId);

            //var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetBids", sql);

            //return ds;
        }

        public DataSet GetNotifications()
        {
            SqlParameter[] sql = new SqlParameter[0];

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetNotifications", sql);

            return ds;
        }

        public DataSet GetNotificationDetail(int notificationId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@NotificationId", notificationId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetNotificationDetail", sql);

            return ds;
        }

        public Notification SaveNotification(Models.Views.Notification.Notification notification, int userId)
        {
            Notification notificationObj = null;

            try
            {
                using (var transaction = this.db.Database.BeginTransaction())
                {
                    if (notification.Id == 0)
                    {
                        notificationObj = new Notification() 
                        {
                            CreatedBy = userId,
                            Content = notification.Content,
                            Data = notification.Data,
                            Heading = notification.Heading,
                            NotificationStatusId = notification.NotificationStatusId,
                        };

                        this.db.Notification.Add(notificationObj);

                        this.db.NotificationStatusHistory.Add(new NotificationStatusHistory()
                        {
                            CreatedBy = userId,
                            Notification = notificationObj,
                            NotificationStatusId = notification.NotificationStatusId
                        });
                    }
                    else
                    {
                        notificationObj = this.db.Notification.SingleOrDefault(x => x.Id == notification.Id);

                        if (notificationObj != null)
                        {
                            notificationObj.Content = notification.Content;
                            notificationObj.Data = notification.Data;
                            notificationObj.Heading = notification.Heading;
                            notificationObj.NotificationStatusId = notification.NotificationStatusId;

                            this.db.Entry<Notification>(notificationObj).State = EntityState.Modified;
                        }

                        this.db.NotificationStatusHistory.Add(new NotificationStatusHistory()
                        {
                            CreatedBy = userId,
                            NotificationId = notification.Id,
                            NotificationStatusId = notification.NotificationStatusId
                        });
                    }

                    this.db.SaveChanges();

                    transaction.Commit();
                }
            }
            catch
            {
                
            }

            return notificationObj;
        }

        public DataSet GetWinnersSeriesWise(int seriesId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@SeriesId", seriesId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetWinnersSeriesWise", sql);

            return ds;
        }

        public Models.Domain.Bank.AppSecurityDeposit SaveSecurity_0(Models.Views.Bank.AppSecurityDeposit appSecurityDeposit, int userId)
        {
            Models.Domain.Bank.AppSecurityDeposit appSecurityDepositObj = null;
            
            using (var transaction = this.db.Database.BeginTransaction())
            {
                appSecurityDepositObj = new Models.Domain.Bank.AppSecurityDeposit()
                {
                    ApplicationId = appSecurityDeposit.ApplicationId,
                    AppSecurityDepositStatusId = 1,
                    BankDocumentTypeId = appSecurityDeposit.BankDocumentTypeId,
                    BankId = appSecurityDeposit.BankId,
                    CreatedBy = userId,
                    DocumentIdValue = appSecurityDeposit.DocumentIdValue,
                    Worth = appSecurityDeposit.Worth
                };

                var appSecurityDepositStatusHistory = new Models.Domain.Bank.AppSecurityDepositStatusHistory()
                {
                    AppSecurityDeposit = appSecurityDepositObj,
                    AppSecurityDepositStatusId = 1, // Received
                    CreatedBy = userId,
                    DiaryNumber = appSecurityDeposit.DiaryNumber
                };

                this.db.AppSecurityDeposit.Add(appSecurityDepositObj);
                this.db.AppSecurityDepositStatusHistory.Add(appSecurityDepositStatusHistory);

                this.db.SaveChanges();

                transaction.Commit();
            }

            return appSecurityDepositObj;
        }

        public Models.Domain.Bank.AppSecurityDeposit SaveSecurity(Models.Views.Bank.AppSecurityDeposit appSecurityDeposit, int userId)
        {
            Models.Domain.Bank.AppSecurityDeposit appSecurityDepositObj = null;

            using (var transaction = this.db.Database.BeginTransaction())
            {
                if (appSecurityDeposit.Id == 0)
                {
                    appSecurityDepositObj = new Models.Domain.Bank.AppSecurityDeposit()
                    {
                        ApplicationId = appSecurityDeposit.ApplicationId,
                        AppSecurityDepositStatusId = 1,
                        BankDocumentTypeId = appSecurityDeposit.BankDocumentTypeId,
                        BankId = appSecurityDeposit.BankId,
                        CreatedBy = userId,
                        DocumentIdValue = appSecurityDeposit.DocumentIdValue,
                        Worth = appSecurityDeposit.Worth
                    };

                    var appSecurityDepositStatusHistory = new Models.Domain.Bank.AppSecurityDepositStatusHistory()
                    {
                        AppSecurityDeposit = appSecurityDepositObj,
                        AppSecurityDepositStatusId = 1, // Received
                        CreatedBy = userId,
                        DiaryNumber = appSecurityDeposit.DiaryNumber
                    };

                    this.db.AppSecurityDeposit.Add(appSecurityDepositObj);
                    this.db.AppSecurityDepositStatusHistory.Add(appSecurityDepositStatusHistory);
                }
                else
                {
                    appSecurityDepositObj = this.db.AppSecurityDeposit.SingleOrDefault(x => x.Id == appSecurityDeposit.Id);

                    if (appSecurityDepositObj != null)
                    {
                        appSecurityDepositObj.AppSecurityDepositStatusId = appSecurityDeposit.AppSecurityDepositStatusId;

                        this.db.Entry<Models.Domain.Bank.AppSecurityDeposit>(appSecurityDepositObj).State = EntityState.Modified;
                    }

                    var appSecurityDepositStatusHistory = new Models.Domain.Bank.AppSecurityDepositStatusHistory()
                    {
                        AppSecurityDepositId = appSecurityDeposit.Id,
                        AppSecurityDepositStatusId = appSecurityDeposit.AppSecurityDepositStatusId,
                        CreatedBy = userId,
                        DiaryNumber = appSecurityDeposit.DiaryNumber
                    };

                    this.db.AppSecurityDepositStatusHistory.Add(appSecurityDepositStatusHistory);
                }

                this.db.SaveChanges();

                transaction.Commit();
            }

            return appSecurityDepositObj;
        }

        /// <summary>
        /// AS RECEIVED BY MAJEED
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public DataSet GetDashboard(string CategoryID)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@CategID", CategoryID);
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("SP_Get_Dashboard_Test", sql);
            return dataset;
        }

        public DataSet GetSeriesByCategoryID(string CatID)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@CategID", CatID);
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("sp_getSeries_AgainstCateg", sql);
            return dataset;
        }
        public DataSet GetLiveBid(string CateogryID, string SeriesID, string RegNo)
        {
            SqlParameter[] sql = new SqlParameter[3];
            sql[0] = new SqlParameter("@CategID", CateogryID);
            sql[1] = new SqlParameter("@SeriesID", SeriesID);
            sql[2] = new SqlParameter("@SerialNumber", RegNo);
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("sp_get_LiveBidding", sql);
            return dataset;
        }
    }
}
