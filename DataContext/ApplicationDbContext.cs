using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models.Domain.Auction;
using Models.Domain.Bank;
using Models.Domain.EPay;
using Models.Domain.Identity;
using Models.Domain.Mail;
using Models.Domain.MobileApplication;
using Models.Domain.Notification;
using Models.Domain.Oraganization;
using System.Linq;

namespace eauction.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, CustomRole, int, CustomUserClaim, CustomUserRole, CustomUserLogin, CustomRoleClaim, CustomUserToken>
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<EmailSetting> EmailSetting { get; set; }
        public DbSet<APK> APK { get; set; }
        public DbSet<CustomMessage> CustomMessage { get; set; }
        public DbSet<UserType> UserType { get; set; }
        public DbSet<UserIdentity> UserIdentity { get; set; }
        public DbSet<Application> Application { get; set; }
        public DbSet<ApplicationStatus> ApplicationStatus { get; set; }
        public DbSet<ApplicationStatusHistory> ApplicationStatusHistory { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<SeriesLogs> SeriesLogs { get; set; }
        public DbSet<SeriesCategory> SeriesCategory { get; set; }
        public DbSet<SeriesStatus> SeriesStatus { get; set; }
        public DbSet<SeriesStatusHistory> SeriesStatusHistory { get; set; }
        public DbSet<SeriesNumber> SeriesNumber { get; set; }
        public DbSet<Bids> Bids { get; set; }
        public DbSet<District> District { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<NotificationStatus> NotificationStatus { get; set; }
        public DbSet<NotificationStatusHistory> NotificationStatusHistory { get; set; }
        
        public DbSet<AppSecurityDeposit> AppSecurityDeposit { get; set; }
        public DbSet<BankDocumentType> BankDocumentType { get; set; }
        public DbSet<Bank> Bank { get; set; }
        public DbSet<AppSecurityDepositStatus> AppSecurityDepositStatus { get; set; }
        public DbSet<AppSecurityDepositStatusHistory> AppSecurityDepositStatusHistory { get; set; }


        public DbSet<ePayAPIs> ePayAPIs { get; set; }
        public DbSet<ePayBankAccountInfo> ePayBankAccountInfo { get; set; }
        public DbSet<ePayApplication> ePayApplication { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //this.SeedData(builder);

            builder.Entity<User>().HasAlternateKey(x => x.CNIC);
            builder.Entity<User>().HasAlternateKey(x => x.PhoneNumber);
            builder.Entity<User>().HasAlternateKey(x => x.Email);

            builder.Entity<UserType>(x => x.ToTable("UserType", "Identity"));
            builder.Entity<UserIdentity>(x => x.ToTable("UserIdentity", "Identity"));
            builder.Entity<User>(x => x.ToTable("Users", "Identity"));
            builder.Entity<CustomRole>(x => x.ToTable("Roles", "Identity"));
            builder.Entity<CustomUserClaim>(x => x.ToTable("UserClaims", "Identity"));
            builder.Entity<CustomUserRole>(x => x.ToTable("UserRoles", "Identity"));
            builder.Entity<CustomUserLogin>(x => x.ToTable("UserLogins", "Identity"));
            builder.Entity<CustomRoleClaim>(x => x.ToTable("RoleClaims", "Identity"));
            builder.Entity<CustomUserToken>(x => x.ToTable("UserTokens", "Identity"));

            //builder.Entity<User>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<User>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<CustomRole>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<CustomUserRole>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<Customer>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<Application>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<ApplicationStatusHistory>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<Series>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");
            //builder.Entity<SeriesDetail>().Property("CreatedOn").HasDefaultValueSql("GETDATE()");

            //builder.Entity<Customer>().HasMany<Application>().WithOne(x => x.Customer).OnDelete(DeleteBehavior.NoAction);
            //builder.Entity<Category>().HasMany<Application>().WithOne(x => x.Category).OnDelete(DeleteBehavior.NoAction);
            //builder.Entity<Series>().HasMany<Application>().WithOne(x => x.Series).OnDelete(DeleteBehavior.NoAction);
            //builder.Entity<ApplicationStatus>().HasMany<Application>().WithOne(x => x.ApplicationStatus).OnDelete(DeleteBehavior.NoAction);

            var entityTypes = builder.Model.GetEntityTypes();

            foreach (var relationship in entityTypes.SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.NoAction;
            }

            foreach (var entity in entityTypes)
            {
                entity.FindProperty("CreatedOn")?.SetDefaultValueSql("GETDATE()");
            }

            builder.Entity<SeriesNumber>().HasIndex(x => new { x.SeriesId, x.AuctionNumber }).IsUnique();
        }
    }
}
