using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using eauction.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Models.Domain.Identity;
using NotificationServices;

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
            [Display(Name = "Mobile Phone Number")]
            public string PhoneNumber { get; set; }

            [Required]
            [StringLength(13)]
            [Display(Name = "CNIC without dashes")]
            public string CNIC { get; set; }

            [StringLength(25)]
            public string NTN { get; set; }

            [StringLength(100)]
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
            [Display(Name = "Confirm password")]
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
            returnUrl = returnUrl ?? Url.Content("~/");

            if (Infrastructure.RegexUtilities.IsValidCNIC(Input.CNIC) == false)
            {
                ModelState.AddModelError(string.Empty, "CNIC should consists of valid 13 digits number.");
            }

            if (Infrastructure.RegexUtilities.IsValidPhoneNumber(Input.PhoneNumber) == false)
            {
                ModelState.AddModelError(string.Empty, "Phone number should be valid and at least 11 digits.");
            }

            if (ModelState.IsValid)
            {
                var existingUser = this._db.Users.SingleOrDefault(x => x.CNIC == Input.CNIC);

                if (existingUser != null)
                {
                    if (existingUser.EmailConfirmed)
                    {
                        ModelState.AddModelError(string.Empty, "User already registered.");

                        return Page();
                    }

                    /////   USER CREATED BUT OTP IS NOT CONFIRMED

                    var otp = this.GetOTP(existingUser);

                    try
                    {
                        await this.SendMessages(existingUser, otp);
                    }
                    catch
                    {
                    }

                    return RedirectToPage("ConfirmIdentity", new { userId = existingUser.Id });
                }

                var user = new Models.Domain.Identity.User
                {
                    UserName = Input.CNIC,
                    FullName = Input.Name,
                    CNIC = Input.CNIC,
                    NTN = Input.NTN,
                    Company = Input.Company,
                    PhoneNumber = Input.PhoneNumber,
                    Email = Input.Email,
                    UserTypeId = 1
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                
                if (result.Succeeded)
                {
                    var otp = this.GetOTP(user);

                    try
                    {
                        await this.SendMessages(user, otp);
                    }
                    catch
                    {
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("ConfirmIdentity", new { userId = user.Id });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        private async Task SendMessages(User user, string OTP)
        {
            var emailSetting = this._db.EmailSetting.Where(x => x.MessageTypeId == 1).FirstOrDefault();
            
            emailSetting.Receiver = user.Email;
            emailSetting.Body = emailSetting.Body.Replace("@@@", Environment.NewLine).Replace("#otp", OTP);

            await this.notificationManager.SendAccountConfirmationMessages(user.PhoneNumber, emailSetting);
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
