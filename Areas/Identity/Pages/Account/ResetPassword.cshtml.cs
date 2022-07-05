using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eauction.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Models.Domain.Identity;

namespace eauction.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _db;

        public ResetPasswordModel(UserManager<User> userManager, SignInManager<User> signInManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public int UserId { get; set; }

            [Required]
            [Display(Name = "One time PIN")]
            public string OTP { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet(int userId = 0)
        {
            if (userId == 0)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                Input = new InputModel
                {
                    UserId = userId
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
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
                        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var result = await _signInManager.UserManager.ResetPasswordAsync(user, code, Input.Password);

                        if (result.Succeeded)
                        {
                            return RedirectToPage("./ResetPasswordConfirmation");
                        }

                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid user account.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Pin code.");
                }
            }

            return Page();
        }
    }
}
