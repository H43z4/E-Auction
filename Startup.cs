using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using eauction.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Models.Domain.Identity;
using eauction.SignalR;
using eauction.Helpers;
using SmsService;
using Microsoft.AspNetCore.Identity;
using System;
using NotificationServices;

namespace eauction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<CustomRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();
            //    .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

            //services.AddIdentity<User, CustomRole>(options => options.SignIn.RequireConfirmedAccount = true)
            //        .AddRoles<CustomRole>()
            //        .AddRoleManager<RoleManager<CustomRole>>()
            //        .AddEntityFrameworkStores<ApplicationDbContext>()
            //        .AddDefaultTokenProviders()
            //        .AddClaimsPrincipalFactory<CustomUserClaimsPrincipalFactory>();

            //services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, CustomRole>>();

            //services.ConfigureApplicationCookie(options =>
            //{
            //    options.LoginPath = "/Identity/Account/Login";
            //    options.LogoutPath = "/Identity/Account/Logout";
            //});

            services.AddTokenAuthentication(Configuration);

            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";

                // Default Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Default Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            });

            /*
                 As the doc has said that asp.net core 3.0+ template use these new methodsAddControllersWithViews,AddRazorPages,AddControllers instead of AddMvc.

                However, AddMvc continues to behave as it has in previous releases.AddMvc() is really just a wrapper around a bunch of other methods that register services. See Source:

                https://github.com/aspnet/AspNetCore/blob/0303c9e90b5b48b309a78c2ec9911db1812e6bf3/src/Mvc/Mvc/src/MvcServiceCollectionExtensions.cs#L27

                You could either use AddMvc to register for MVC, Razor Pages,API or use individual AddControllersWithViews(for MVC only) and AddRazorPages (for Razor Pages only).
             */

            //services.AddControllersWithViews();
            //services.AddRazorPages();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "");
                })
                //.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0)
                .ConfigureApiBehaviorOptions(options => 
                {
                    options.SuppressModelStateInvalidFilter = true;
                });

            services.AddDbContext<ApplicationDbContext>(ServiceLifetime.Scoped);

            services.AddSignalR();

            //var section = Configuration.AsEnumerable().Where(x => x.Key == "Email");
            //var section2 = Configuration.GetSection("EmailSetting");

            //Console.WriteLine(section?.FirstOrDefault());

            //services.Configure<EmailSetting>()
            //services.AddScoped<ISmsSender, SmsSender>();
            services.AddTransient<INotificationManager, NotificationManager>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseDeveloperExceptionPage();
            //app.UseDatabaseErrorPage();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=GetApplications}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapHub<NotificationHub>("/notificationHub");
            });
        }
    }
}
