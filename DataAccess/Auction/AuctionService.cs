using eauction.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Models.Domain.Auction;
using Models.Domain.Identity;
using Models.Domain.Mail;
using Models.Domain.Notification;
using Models.Views.Auction;
using Models.Views.Input;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataAccess.Auction
{
    public class AuctionService //: IAuctionService
    {
        readonly ApplicationDbContext db;
        private readonly string connectionString;

        public AuctionService(ApplicationDbContext _db)
        {
            this.db = _db;
            this.connectionString = _db.Database.GetDbConnection().ConnectionString;
        }

        public AuctionService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public DataSet GetLOVs()
        {
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetLOVs", null);

            return dataset;
        }

        public DataSet GetSeries(int seriesCategoryId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@SeriesCategoryId", seriesCategoryId);
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetSeries", sql);
            return dataset;
        }

        public DataSet GetAuctionSeries(int seriesStatusId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@SeriesStatusId", seriesStatusId);

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetAuctionSeries", sql);

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

        public DataSet GetSeriesSchedule()
        {
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetSeriesSchedule", null);
            return dataset;
        }

        public DataSet GetAuctionSeriesNumber(int customerId, int seriesId)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@SeriesId", seriesId);

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetAuctionSeriesNumber", sql);

            return dataset;
        }

        public DataSet GetSeriesNumber(int customerId, Models.Views.Auction.SeriesNumber seriesNumber)
        {
            SqlParameter[] sql = new SqlParameter[5];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@SeriesId", seriesNumber.SeriesId);
            sql[2] = new SqlParameter("@AuctionNumber", string.IsNullOrEmpty(seriesNumber.AuctionNumber) ? (object)DBNull.Value : seriesNumber.AuctionNumber);
            sql[3] = new SqlParameter("@PageNumber", seriesNumber.PageNumber);
            sql[4] = new SqlParameter("@PageSize", seriesNumber.PageSize);

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetSeriesNumber", sql);

            return dataset;
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

        public bool SaveSeries_0(Models.Views.Auction.Series series, int userId)
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

        public Models.Domain.Auction.Series SaveSeries_1(Models.Views.Auction.Series series, int userId)
        {
            Models.Domain.Auction.Series seriesObj = new Models.Domain.Auction.Series();

            if (series.Id == 0)
            {
                seriesObj.SeriesCategoryId = series.CategoryId;
                seriesObj.Name = series.SeriesName.ToUpper();
                seriesObj.RegEndDateTime = series.RegEndDateTime;
                seriesObj.RegStartDateTime = series.RegStartDateTime;
                seriesObj.AuctionEndDateTime = series.AuctionEndDateTime;
                seriesObj.AuctionStartDateTime = series.AuctionStartDateTime;
                seriesObj.SeriesStatusId = 1;
                seriesObj.DistrictId = 1;
                seriesObj.CreatedBy = userId;

                this.db.Series.Add(seriesObj);
            }
            else
            {
                seriesObj = this.db.Series.SingleOrDefault(x => x.Id == series.Id);

                this.db.Entry<Models.Domain.Auction.Series>(seriesObj).State = EntityState.Modified;

                if (seriesObj != null)
                {
                    seriesObj.RegEndDateTime = series.RegEndDateTime;
                    seriesObj.RegStartDateTime = series.RegStartDateTime;
                    seriesObj.AuctionEndDateTime = series.AuctionEndDateTime;
                    seriesObj.AuctionStartDateTime = series.AuctionStartDateTime;
                    seriesObj.Name = series.SeriesName.ToUpper();
                    seriesObj.SeriesCategoryId = series.CategoryId;
                }
            }

            this.db.SaveChanges();

            return seriesObj;
        }

        public DataSet SaveSeries(Models.Views.Auction.Series series, int userId, out string sqlException)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[10];
                sql[0] = new SqlParameter("@SeriesId", series.Id);
                sql[1] = new SqlParameter("@SeriesCategoryId", series.CategoryId);
                sql[2] = new SqlParameter("@SeriesName", series.SeriesName);
                sql[3] = new SqlParameter("@RegEndDateTime", series.RegEndDateTime);
                sql[4] = new SqlParameter("@RegStartDateTime", series.RegStartDateTime);
                sql[5] = new SqlParameter("@AuctionEndDateTime", series.AuctionEndDateTime);
                sql[6] = new SqlParameter("@AuctionStartDateTime", series.AuctionStartDateTime);
                sql[7] = new SqlParameter("@SeriesStatusId", series.SeriesStatusId);
                sql[8] = new SqlParameter("@IsReauctioning", series.IsReauctioning);
                sql[9] = new SqlParameter("@CreatedBy", userId);

                var ds = new Query.Execution(this.connectionString).Execute_DataSet("SaveSeries", sql);

                sqlException = string.Empty;

                return ds;
            }
            catch (SqlException ex)
            {
                sqlException = ex.Message;
            }

            return new DataSet();
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

        public DataSet SaveBid(int customerId, NewBid newBid)
        {
            SqlParameter[] sql = new SqlParameter[3];
            sql[0] = new SqlParameter("@CustomerId", customerId);
            sql[1] = new SqlParameter("@AIN", newBid.AIN);
            sql[2] = new SqlParameter("@BidPrice", newBid.BidPrice);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("SaveBidExt", sql);

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

                var ds = new Query.Execution(this.connectionString).Execute_DataSet("SaveBidExt", sql);

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

        public void SaveApplicationDetail(Models.Views.Auction.Application application, out string sqlException)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[3];
                sql[0] = new SqlParameter("@ApplicationId", application.Id);
                sql[1] = new SqlParameter("@ChasisNumber", application.ChasisNumber);
                sql[2] = new SqlParameter("@OwnerName", application.OwnerName);

                var applicationId = new Query.Execution(this.connectionString).Execute_Scaler("SaveApplicationDetail", sql);

                sqlException = string.Empty;
            }
            catch (SqlException ex)
            {
                sqlException = ex.Message;
            }
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

            //new Query.Execution(this.connectionString).Execute_Scaler("SaveApplicationBulk", sql);
            var ds = new Query.Execution(this.connectionString).Execute_DataSet("SaveApplicationBulk", sql);

            return ds;
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
            SqlParameter[] sql = new SqlParameter[8];
            sql[0] = new SqlParameter("@CustomerId", application.CustomerId > 0 ? application.CustomerId : (object)DBNull.Value);
            sql[1] = new SqlParameter("@ApplicationStatusId", application.ApplicationStatusId);
            sql[2] = new SqlParameter("@AIN", application.AIN != null ? application.AIN : (object)DBNull.Value);
            sql[3] = new SqlParameter("@PageNumber", application.PageNumber);
            sql[4] = new SqlParameter("@PageSize", application.PageSize);
            sql[5] = new SqlParameter("@Series", application.SeriesId > 0 ? application.SeriesId : (object)DBNull.Value);
            sql[6] = new SqlParameter("@Category", application.SeriesCategoryId > 0 ? application.SeriesCategoryId : (object)DBNull.Value);
            sql[7] = new SqlParameter("@SeriesNumber", application.SeriesNumber);
            //sql[7] = new SqlParameter("@RegMark", application.SeriesNumber != null ? application.SeriesNumber : (object)DBNull.Value);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetApplicationsFiltered", sql);

            return ds;
        }

        public SqlException SqlException { get; set; }

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

        public DataSet GetBidsMultipleSeries(int customerId, out string sqlException)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[1];
                sql[0] = new SqlParameter("@CustomerId", customerId);

                var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetBidsMultipleSeriesExt", sql);

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

        public DataSet GetWinnersBeforeApproval()
        {
            var ds = new Query.Execution(this.connectionString).Execute_DataSet("GetWinnersBeforeApproval", null);

            return ds;
        }

        public DataSet ApproveWinnersList(int seriesId, int userId)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@SeriesId", seriesId);
            sql[1] = new SqlParameter("@CreatedBy", userId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("ApproveWinnersList", sql);

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


        #region Dashboard
        /// <summary>
        /// AS RECEIVED BY MAJEED
        /// </summary>
        /// <param name="CategoryID"></param>
        /// <returns></returns>
        public DataSet GetDashboard(string CategoryID, string SeriesID, string FromDate, string ToDate)
        {
            SqlParameter[] sql = new SqlParameter[4];
            sql[0] = new SqlParameter("@CategID", CategoryID);
            sql[1] = new SqlParameter("@SeriesID", SeriesID);
            sql[2] = new SqlParameter("@FromDate", FromDate);
            sql[3] = new SqlParameter("@ToDate", ToDate);
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

        #endregion


        #region epayment

        public DataSet GetPSIdInputModel(string applications)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@ApplicationsCSVs", applications);
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("ePay_GetPSIdInputModelExt", sql);
            return dataset;
        }

        public void SavePSIds(DataTable applications)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@Applications", SqlDbType.Structured)
            {
                Value = applications,
                Direction = System.Data.ParameterDirection.Input,
                IsNullable = true,
                TypeName = "dbo.TYPE_Application"
            };

            new Query.Execution(this.connectionString).Execute_Scaler("ePay_SavePSIds", sql);
        }

        public void SavePSIdStatus(ePayStatusUpdate ePayStatusUpdate, out string sqlException)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[6];

                sql[0] = new SqlParameter("@deptTransactionId", ePayStatusUpdate.deptTransactionId);
                sql[1] = new SqlParameter("@psId", ePayStatusUpdate.psId);
                sql[2] = new SqlParameter("@amountPaid", ePayStatusUpdate.amountPaid);
                sql[3] = new SqlParameter("@paidDate", ePayStatusUpdate.paidDate);
                sql[4] = new SqlParameter("@paidTime", ePayStatusUpdate.paidTime);
                sql[5] = new SqlParameter("@bankCode", ePayStatusUpdate.bankCode);

                var result = new Query.Execution(this.connectionString).Execute_Scaler("ePay_SavePSIdStatus", sql);

                sqlException = string.Empty;
            }
            catch (Exception ex)
            {
                sqlException = ex.Message;
            }
        }

        public DataSet GetPayeesInfo(long applicationId)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@ApplicationsId", applicationId);
            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("ePay_GetPayeesInfo", sql);
            return dataset;
        }

        #endregion



        #region Reports

        public DataSet DownloadApprovals(int seriesCategoryId, int seriesId, string dateFrom, string dateTo)
        {
            SqlParameter[] sql = new SqlParameter[4];

            sql[0] = new SqlParameter("@SeriesCategoryId", seriesCategoryId);
            sql[1] = new SqlParameter("@SeriesId", seriesId);
            sql[2] = new SqlParameter("@DateFrom", dateFrom);
            sql[3] = new SqlParameter("@DateTo", dateTo);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("rpt_DownloadApprovals", sql);

            return ds;
        }

        public DataSet GetApplicationsApproved(int seriesCategoryId, int seriesId)
        {
            SqlParameter[] sql = new SqlParameter[2];

            sql[0] = new SqlParameter("@SeriesCategoryId", seriesCategoryId);
            sql[1] = new SqlParameter("@SeriesId", seriesId);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("rpt_GetApplicationsApproved", sql);

            return ds;
        }

        public DataSet GetApplicationsApproved(Models.Views.Auction.Application applicationFilter)
        {
            SqlParameter[] sql = new SqlParameter[2];

            sql[0] = new SqlParameter("@SeriesId", applicationFilter.SeriesId);
            sql[1] = new SqlParameter("@SeriesCategoryId", applicationFilter.SeriesCategoryId);


            //sql[0] = new SqlParameter("@SeriesCategoryId", SqlDbType.Int)
            //{
            //    Value = (object)applicationFilter.SeriesCategoryId ?? DBNull.Value,
            //    Direction = System.Data.ParameterDirection.Input,
            //};

            //sql[1] = new SqlParameter("@SeriesId", SqlDbType.Int)
            //{
            //    Value = (object)applicationFilter.SeriesId ?? DBNull.Value,
            //    Direction = System.Data.ParameterDirection.Input,
            //};

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("rpt_GetApplicationsApproved", sql);

            return ds;
        }

        public DataSet GetBidLogs(string categories, string series, string seriesNumbers, int pageNumber)
        {
            SqlParameter[] sql = new SqlParameter[4];

            sql[0] = new SqlParameter("@CategoryCSV", categories);
            sql[1] = new SqlParameter("@SeriesCSV", series);
            sql[2] = new SqlParameter("@SeriesNumberCSV", seriesNumbers);
            sql[3] = new SqlParameter("@PageNumber", pageNumber);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("rpt_GetBidLogs", sql);

            return ds;
        }


        #endregion
        /// <summary>
        /// ////////////////////////////////////////////////////////////////////
        /// DEVELOP
        /// </summary>



        /// <summary>
        /// ////////////////////////////////////////////////////////////////////
        /// </summary>


        #region EMotor

        public DataSet GetSeriesForDataTransfer()
        {
            SqlParameter[] sql = new SqlParameter[0];

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("GetSeriesForDataTransfer", sql);

            return dataset;
        }

        public DataSet GetAuctionNumberFromMvrs(string oraConnectionString, int catId, string series)
        {
            var P_CATEGORY = new OracleParameter("P_CATEGORY", OracleDbType.Int16, catId, ParameterDirection.Input);
            var P_CATCODE = new OracleParameter("P_CATCODE", OracleDbType.Varchar2, series, ParameterDirection.Input);
            var P_NUMBER = new OracleParameter("P_NUMBER", OracleDbType.RefCursor, ParameterDirection.Output);

            var command = new OracleConnection(oraConnectionString).CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = $"MVRS.PKG_EMOTOR.GETNUMBER";
            command.Parameters.AddRange(new OracleParameter[]
            {
                    P_CATEGORY,
                    P_CATCODE,
                    P_NUMBER
            });

            DataSet dataSet = new DataSet("Results");
            OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            oracleDataAdapter.Fill(dataSet);

            if (command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }

            return dataSet;
        }

        public DataSet GetSeriesNumbersFromRevampPool(int catId, string seriesName)
        {
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@SeriesCategoryId", catId);
            sql[1] = new SqlParameter("@SeriesName", seriesName);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("SRNRPL.GetNumbersforAuction", sql);

            return ds;
        }
        public DataSet GetCustomerCredit(string chassisNo)
        {
            SqlParameter[] sql = new SqlParameter[1];
            sql[0] = new SqlParameter("@ChasisNo", chassisNo);

            var dataset = new Query.Execution(this.connectionString).Execute_DataSet("SRNRPL.GetEAuctionCustomerCredit", sql);

            return dataset;
        }

        public DataSet SaveAuctionResultsToMvrs(int createdBy, List<Winners> winners)
        {
            
            SqlParameter[] sql = new SqlParameter[2];
            sql[0] = new SqlParameter("@CreatedBy", createdBy);
            sql[1] = new SqlParameter("@AuctionResult", winners);
            
            var ds = new Query.Execution(this.connectionString).Execute_DataSet("SRNRPL.SaveAuctionResult", sql);

            return ds;

            
        }

        
        public bool SaveWinnersToMvrs(string oraConnectionString, List<Winners> winners)
        {
            int size = winners.Count;

            var P_AIN_NUMBER = new OracleParameter("P_AIN_NUMBER", OracleDbType.Varchar2, size, winners.Select(x => x.WinnerAIN).ToArray(), ParameterDirection.Input);
            var P_CUSNAME = new OracleParameter("P_CUSNAME", OracleDbType.Varchar2, size, winners.Select(x => x.Name).ToArray(), ParameterDirection.Input);
            var P_CUSFATHERNAME = new OracleParameter("P_CUSFATHERNAME", OracleDbType.Varchar2, size, winners.Select(x => x.FatherHusbandName).ToArray(), ParameterDirection.Input);
            var P_CUSCNIC = new OracleParameter("P_CUSCNIC", OracleDbType.Varchar2, size, winners.Select(x => x.CNIC).ToArray(), ParameterDirection.Input);
            var P_CUSMOBILENO = new OracleParameter("P_CUSMOBILENO", OracleDbType.Varchar2, size, winners.Select(x => x.PhoneNumber).ToArray(), ParameterDirection.Input);
            var P_CUSEMAIL = new OracleParameter("P_CUSEMAIL", OracleDbType.Varchar2, size, winners.Select(x => x.Email).ToArray(), ParameterDirection.Input);
            var P_RESERVEPRICE = new OracleParameter("P_RESERVEPRICE", OracleDbType.Int32, size, winners.Select(x => x.ReservePrice).ToArray(), ParameterDirection.Input);
            var P_BIDDINGPRICE = new OracleParameter("P_BIDDINGPRICE", OracleDbType.Int32, size, winners.Select(x => x.HighestBiddingPrice).ToArray(), ParameterDirection.Input);
            var P_CAT_ID = new OracleParameter("P_CAT_ID", OracleDbType.Int32, size, winners.Select(x => x.SeriesCategoryId).ToArray(), ParameterDirection.Input);
            var P_NG_NUMBER = new OracleParameter("P_NG_NUMBER", OracleDbType.Varchar2, size, winners.Select(x => x.SeriesNumber).ToArray(), ParameterDirection.Input);
            var P_CAT_CODE = new OracleParameter("P_CAT_CODE", OracleDbType.Varchar2, size, winners.Select(x => x.Series).ToArray(), ParameterDirection.Input);
            var P_OWNER_NAME = new OracleParameter("P_OWNER_NAME", OracleDbType.Varchar2, size, winners.Select(x => x.OwnerName).ToArray(), ParameterDirection.Input);
            var P_CHASSIS_NO = new OracleParameter("P_CHASSIS_NO", OracleDbType.Varchar2, size, winners.Select(x => x.ChasisNumber).ToArray(), ParameterDirection.Input);

            P_AIN_NUMBER.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CUSNAME.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CUSFATHERNAME.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CUSCNIC.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CUSMOBILENO.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CUSEMAIL.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_RESERVEPRICE.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_BIDDINGPRICE.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CAT_ID.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_NG_NUMBER.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CAT_CODE.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_OWNER_NAME.CollectionType = OracleCollectionType.PLSQLAssociativeArray;
            P_CHASSIS_NO.CollectionType = OracleCollectionType.PLSQLAssociativeArray;

            var command = new OracleConnection(oraConnectionString).CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = $"MVRS.PKG_EMOTOR.AUCTIONRESULT";
            command.Parameters.AddRange(new OracleParameter[]
            {
                P_AIN_NUMBER,
                P_CUSNAME,
                P_CUSFATHERNAME,
                P_CUSCNIC,
                P_CUSMOBILENO,
                P_CUSEMAIL,
                P_RESERVEPRICE,
                P_BIDDINGPRICE,
                P_CAT_ID,
                P_NG_NUMBER,
                P_CAT_CODE,
                P_OWNER_NAME,
                P_CHASSIS_NO
            });

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            command.ExecuteNonQuery();

            if (command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }

            return true;
        }

        public DataSet GetCreditFromMvrs(string oraConnectionString, string chassisNo)
        {
            var P_CHASIS_NO = new OracleParameter("P_CHASIS_NO", OracleDbType.Varchar2, chassisNo, ParameterDirection.Input);
            var P_NUMBER = new OracleParameter("P_NUMBER", OracleDbType.RefCursor, ParameterDirection.Output);

            var command = new OracleConnection(oraConnectionString).CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = $"MVRS.PKG_EMOTOR.GET_CREDIT_INFO";
            command.Parameters.AddRange(new OracleParameter[]
            {
                    P_CHASIS_NO,
                    P_NUMBER
            });

            DataSet dataSet = new DataSet("Results");
            OracleDataAdapter oracleDataAdapter = new OracleDataAdapter(command);

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            oracleDataAdapter.Fill(dataSet);

            if (command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }

            return dataSet;
        }

        public void SaveCustomerPaymentToMvrs(Models.Views.Payment.Payee payee, string PSId, Int64 amountPaid)
        {
            try
            {
                SqlParameter[] sql = new SqlParameter[12];

                sql[0] = new SqlParameter("@SeriesCode", payee.Series);
                sql[1] = new SqlParameter("@SeriesNumberName", payee.SeriesNumber);
                sql[2] = new SqlParameter("@CategoryId", payee.SeriesCategoryId);
                sql[3] = new SqlParameter("@ChassisNo", payee.ChasisNumber);
                sql[4] = new SqlParameter("@Amount", amountPaid);
                sql[5] = new SqlParameter("@PSID", PSId);
                sql[6] = new SqlParameter("@bankCode", "BOP");
                sql[7] = new SqlParameter("@OwnerName", payee.OwnerName);
                sql[8] = new SqlParameter("@CNIC", payee.CNIC);
                sql[9] = new SqlParameter("@NTN", payee.NTN);
                sql[10] = new SqlParameter("@PaymentStatusId", 1); //Status Marked as "Credit"
                sql[11] = new SqlParameter("@CreatedBy", 1);


                var result = new Query.Execution(this.connectionString).Execute_Scaler("SRNRPL.SaveCustomerPaymentIntimation_EAuction", sql);

             
            }
            catch (Exception ex)
            {
                //sqlException = ex.Message;
            }
        }
        public int SavePaymentInfoToMvrs(string oraConnectionString, Models.Views.Payment.Payee payee, string PSId, Int64 amountPaid)
        {
            var P_CAT_CODE = new OracleParameter("P_CAT_CODE", OracleDbType.Varchar2, payee.Series, ParameterDirection.Input);
            var P_NG_NUMBER = new OracleParameter("P_NG_NUMBER", OracleDbType.Varchar2, payee.SeriesNumber, ParameterDirection.Input);
            var P_CAT_ID = new OracleParameter("P_CAT_ID", OracleDbType.Varchar2, payee.SeriesCategoryId, ParameterDirection.Input);
            var P_CUST_NAME = new OracleParameter("P_CUST_NAME", OracleDbType.Varchar2, payee.Name, ParameterDirection.Input);
            var P_CUST_CNIC = new OracleParameter("P_CUST_CNIC", OracleDbType.Varchar2, payee.CNIC, ParameterDirection.Input);
            var P_CUST_PHONE = new OracleParameter("P_CUST_PHONE", OracleDbType.Varchar2, payee.PhoneNumber, ParameterDirection.Input);
            var P_CUST_EMAIL = new OracleParameter("P_CUST_EMAIL", OracleDbType.Varchar2, payee.Email, ParameterDirection.Input);
            var P_OWN_NAME = new OracleParameter("P_OWN_NAME", OracleDbType.Varchar2, payee.OwnerName, ParameterDirection.Input);
            var P_OWN_CNIC = new OracleParameter("P_OWN_CNIC", OracleDbType.Varchar2, string.Empty, ParameterDirection.Input);
            var P_AMOUNT = new OracleParameter("P_AMOUNT", OracleDbType.Int64, amountPaid, ParameterDirection.Input);
            var P_RESERVE_PRICE = new OracleParameter("P_RESERVE_PRICE", OracleDbType.Int64, payee.ReservePrice, ParameterDirection.Input);
            var P_CHASSIS_NO = new OracleParameter("P_CHASSIS_NO", OracleDbType.Varchar2, payee.ChasisNumber, ParameterDirection.Input);
            var P_AUC_STATUS = new OracleParameter("P_AUC_STATUS", OracleDbType.Varchar2, string.Empty, ParameterDirection.Input);
            var P_TAT_ID = new OracleParameter("P_TAT_ID", OracleDbType.Int16, 0, ParameterDirection.Input);
            var P_PSID = new OracleParameter("P_PSID", OracleDbType.Varchar2, PSId, ParameterDirection.Input);
            var P_REMARKS = new OracleParameter("P_REMARKS", OracleDbType.Varchar2, string.Empty, ParameterDirection.Input);
            var P_RESULT = new OracleParameter("P_RESULT", OracleDbType.Int32, ParameterDirection.Output);

            var command = new OracleConnection(oraConnectionString).CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = $"MVRS.PKG_EMOTOR.PAYMENT_INTIMATION";
            command.Parameters.AddRange(new OracleParameter[]
            {
                P_CAT_CODE,
                P_NG_NUMBER,
                P_CAT_ID,
                P_CUST_NAME,
                P_CUST_CNIC,
                P_CUST_PHONE,
                P_CUST_EMAIL,
                P_OWN_NAME,
                P_OWN_CNIC,
                P_AMOUNT,
                P_RESERVE_PRICE,
                P_CHASSIS_NO,
                P_AUC_STATUS,
                P_TAT_ID,
                P_PSID,
                P_REMARKS,
                P_RESULT
            });

            if (command.Connection.State != ConnectionState.Open)
            {
                command.Connection.Open();
            }

            command.ExecuteNonQuery();

            if (command.Connection.State == ConnectionState.Open)
            {
                command.Connection.Close();
            }

            //return true;

            //return System.Convert.ToDecimal(P_RESULT.Value) > 0;

            return (int)(Oracle.ManagedDataAccess.Types.OracleDecimal)P_RESULT.Value.ToString();
        }

        #endregion


        #region User Management

        public DataSet GetUsers(Models.Views.Identity.User userFilter)
        {
            SqlParameter[] sql = new SqlParameter[5];
            //sql[0] = new SqlParameter("@CustomerId", application.CustomerId > 0 ? application.CustomerId : (object)DBNull.Value);
            sql[0] = new SqlParameter("@CNIC", userFilter.CNIC != null ? userFilter.CNIC : (object)DBNull.Value);
            sql[1] = new SqlParameter("@Email", userFilter.Email != null ? userFilter.Email : (object)DBNull.Value);
            sql[2] = new SqlParameter("@PhoneNumber", userFilter.PhoneNumber != null ? userFilter.PhoneNumber : (object)DBNull.Value);
            sql[3] = new SqlParameter("@PageNumber", userFilter.PageNumber);
            sql[4] = new SqlParameter("@PageSize", userFilter.PageSize);

            var ds = new Query.Execution(this.connectionString).Execute_DataSet("Identity.GetUsers", sql);

            return ds;
        }

        #endregion
    }
}
