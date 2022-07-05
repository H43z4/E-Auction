using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using eauction.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Domain.Identity;

namespace eauction.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmIdentityModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ConfirmIdentityModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "One time PIN")]
            public string OTP { get; set; }

            [Required]
            public int UserId { get; set; }
        }

        [ViewData]
        public string Message { get; set; }

        [ViewData]
        public bool Status { get; set; } = false;


        public IActionResult OnGet(int userId)
        {
            var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == userId);

            if (userIdentity != null)
            {
                if (userIdentity.OtpExpiryOn > DateTime.Now)
                {
                    this.Input = new InputModel();

                    Input.UserId = userId;

                    return Page();
                }
            }

            return BadRequest("Invalid authentication.");
        }

        public async Task<IActionResult> OnPostAsync_0()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == Input.UserId);

            if (userIdentity != null)
            {
                if (userIdentity.OTP == Input.OTP && userIdentity.OtpExpiryOn > DateTime.Now)
                {
                    var user = this._db.Users.SingleOrDefault(x => x.Id == Input.UserId);

                    if (user != null)
                    {
                        this._db.Entry<User>(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                        user.EmailConfirmed = true;
                        user.PhoneNumberConfirmed = true;

                        await this._db.SaveChangesAsync();

                        this.Message = "Thank you, your account has been confirmed.";
                    }
                }

                this.Message = "Could not verify your PIN code. Please, resend to verify again.";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = this._db.Users.SingleOrDefault(x => x.Id == Input.UserId);

            if (user != null)
            {
                if (!user.EmailConfirmed)
                {
                    var userIdentity = this._db.UserIdentity.SingleOrDefault(x => x.UserId == Input.UserId);

                    if (userIdentity != null)
                    {
                        if (userIdentity.OTP == Input.OTP && userIdentity.OtpExpiryOn >= DateTime.Now)
                        {
                            this._db.Entry<User>(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                            user.EmailConfirmed = true;
                            user.PhoneNumberConfirmed = true;

                            await this._db.SaveChangesAsync();

                            this.Status = true;
                            this.Message = "Thank you, your account has been confirmed.";

                            return Page();
                        }
                    }
                }

                this.Status = false;
                this.Message = "Incorrect PIN code.";
                
                return Page();
            }

            this.Status = false;
            this.Message = "Invalid request.";
            
            return Page();
        }

    }
}
