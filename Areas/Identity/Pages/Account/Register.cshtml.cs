using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using eauction.Data;
using eauction.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Domain.Identity;
using NotificationServices;
using SmsService;

namespace eauction.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly INotificationManager notificationManager;
        private readonly ApplicationDbContext _db;

        public RegisterModel(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ILogger<RegisterModel> logger,
            INotificationManager notificationManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            this.notificationManager = notificationManager;
            _db = db;

        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(15)]
            [Display(Name = "Mobile Number (+923031234567)")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(15)]
            [Display(Name = "CNIC")]
            public string CNIC { get; set; }

            [StringLength(25)]
            public string NTN { get; set; }

            [StringLength(100)]
            [Display(Name = "Company Name")]
            public string Company { get; set; }

            [Required]
            [StringLength(50)]
            public string Name { get; set; }

            [Required]
            [EmailAddress]
            [StringLength(100)]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Page();
            }

            Input.CNIC = RegexUtilities.Clean(Input.CNIC);  //  remove dashes
            Input.PhoneNumber = RegexUtilities.Clean(Input.PhoneNumber);  //  remove dashes

            if (Infrastructure.RegexUtilities.IsValidCNIC(Input.CNIC) == false)
            {
                ModelState.AddModelError(string.Empty, "CNIC should consists of valid 13 digits number.");
                return Page();
            }

            var parameters = new SqlParameter[3]
            {
                new SqlParameter("@CNIC", Input.CNIC),
                new SqlParameter("@Email", Input.Email),
                new SqlParameter("@PhoneNumber", Input.PhoneNumber),
            };

            //var parameters = new SqlParameter[3]
            //{
            //    new SqlParameter("@CNIC", SqlDbType.VarChar, 13) { Value = Input.CNIC },
            //new SqlParameter("@Email", Input.Email),
            //    new SqlParameter("@PhoneNumber", Input.PhoneNumber),
            //};

            var existingUsers = this._db.Users.FromSqlRaw("EXEC [Identity].GetExistingUsers @CNIC, @Email, @PhoneNumber", parameters).ToList();

            if (existingUsers.Count() > 0)
            {
                ModelState.AddModelError(string.Empty, "This CNIC / Email / Phone Number has already been taken.");
                return Page();
            }

            var user = new Models.Domain.Identity.User
            {
                UserName = Input.CNIC,
                FullName = Input.Name,
                CNIC = Input.CNIC,
                NTN = Input.NTN,
                Company = Input.Company,
                PhoneNumber = "0" + Input.PhoneNumber,
                Email = Input.Email,
                UserTypeId = 1
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("FullName", Input.Name));

                var otp = this.GetOTP(user);

                try
                {
                    await this.SendMessages(user, otp);
                }
                catch
                {
                }

                return RedirectToPage("ConfirmIdentity", new { userId = user.Id });
            }
            else 
            {
                result.Errors.Select(x => x.Description).ToList().ForEach(error => 
                {
                    ModelState.AddModelError(string.Empty, error);
                });
            }

            return Page();
        }

        private async Task SendMessages(User user, string OTP)
        {
            var emailSetting = this._db.EmailSetting.Where(x => x.MessageTypeId == 1).FirstOrDefault();
            
            emailSetting.Receiver = user.Email;
            emailSetting.Body = emailSetting.Body.Replace("@@@", Environment.NewLine).Replace("#otp", OTP);

            await this.notificationManager.SendMessage(user.PhoneNumber, emailSetting);
        }

        private string GetOTP(User user)
        {
            string otp = DateTime.Now.ToString("ffff");
            
            var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == user.Id);

            if (userIdentity != null)
            {
                this._db.Entry<UserIdentity>(userIdentity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                userIdentity.OTP = otp;
                userIdentity.OtpExpiryOn = DateTime.Now.AddHours(1);

                this._db.SaveChanges();
            }
            else
            {
                this._db.UserIdentity.Add(new UserIdentity()
                {
                    UserId = user.Id,
                    OTP = otp,
                    OtpExpiryOn = DateTime.Now.AddHours(1)
                });

                this._db.SaveChanges();
            }

            return otp;
        }
    }
}
