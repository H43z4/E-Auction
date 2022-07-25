using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Models.Domain.Identity;
using System.Threading.Tasks.Dataflow;
using eauction.Data;
using eauction.Infrastructure;

namespace eauction.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly ApplicationDbContext _db;

        public LoginModel(SignInManager<User> signInManager, 
            ILogger<LoginModel> logger,
            UserManager<User> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(15)]
            [Display(Name = "CNIC")]
            public string CNIC { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            //[Display(Name = "Remember me?")]
            //public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl = returnUrl ?? Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            Input.CNIC = RegexUtilities.Clean(Input.CNIC);  //  remove dashes

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                //var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                // Once, UserName is changed _signInManager.PasswordSignInAsync cannot be used after password is changed.
                ////////////////////////////////

                var user = this._db.Users.SingleOrDefault(x => x.CNIC == Input.CNIC && x.IsSoftDeleted != true);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                }
                else
                {
                    var result = await _signInManager.PasswordSignInAsync(user, Input.Password, false, lockoutOnFailure: false);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");

                        //var userIsAdmin = await this._userManager.IsInRoleAsync(user, "Admin");
                        var userRoles = await this._userManager.GetRolesAsync(user);

                        //if (user.IsInRole("Admin"))
                        //if (userIsAdmin)
                        if (userRoles.Contains("SuperAdmin"))
                        {
                            return LocalRedirect("/Admin/FindApplications");
                        }
                        else
                        if (userRoles.Contains("Admin"))
                        {
                            return LocalRedirect("/Admin/FindApplicationsMRA");
                        }

                        return LocalRedirect("/Home/GetApplications");
                    }
                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = false });
                    }
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");
                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        if (!user.EmailConfirmed)
                        {
                            ModelState.AddModelError(string.Empty, "Email not confirmed.");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid credentials.");
                        }
                    }
                }

            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
