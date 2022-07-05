using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Domain.Identity;
using eauction.Data;
using System.Linq;
using NotificationServices;
using eauction.Infrastructure;

namespace eauction.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly INotificationManager notificationManager;
        private readonly ApplicationDbContext _db;

        public ForgotPasswordModel(INotificationManager notificationManager, ApplicationDbContext db)
        {
            this.notificationManager = notificationManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(15)]
            [Display(Name = "CNIC")]
            public string CNIC { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Input.CNIC = RegexUtilities.Clean(Input.CNIC);  //  remove dashes

            if (Infrastructure.RegexUtilities.IsValidCNIC(Input.CNIC) == false)
            {
                ModelState.AddModelError(string.Empty, "CNIC should consists of valid 13 digits number.");
            }

            if (ModelState.IsValid)
            {
                var existingUser = this._db.Users.SingleOrDefault(x => x.CNIC == Input.CNIC);

                if (existingUser == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid CNIC.");

                    return Page();
                }

                if (existingUser.EmailConfirmed == false)
                {
                    ModelState.AddModelError(string.Empty, "Account not confirmed.");

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

                //return RedirectToPage("ConfirmIdentity", new { userId = existingUser.Id });

                return RedirectToPage("ResetPassword", new { userId = existingUser.Id });
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
