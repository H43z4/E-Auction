using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Domain.Identity;
using eauction.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace eauction.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ProfileModel : PageModel
    {
        private readonly ApplicationDbContext db;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public ProfileModel(ApplicationDbContext db,
            SignInManager<User> signInManager,
            UserManager<User> userManager)
        {
            this.db = db;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty]
        public UserModel UserProfile { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class UserModel
        {
            [EmailAddress]
            public string Email { get; set; }

            //public int? CustomerTypeId { get; set; }
            //public CustomerType CustomerType { get; set; }

            [StringLength(50)]
            [Display(Name = "Father/ Husband Name")]
            public string FatherHusbandName { get; set; }

            [StringLength(15)]
            public string CNIC { get; set; }

            [StringLength(50)]
            public string NTN { get; set; }

            [StringLength(50)]
            [Required]
            public string Name { get; set; }

            [StringLength(100)]
            [Required]
            public string Address { get; set; }

            [StringLength(50)]
            [Required]
            [Display(Name = "Phone Number")]
            public string PhoneNumber { get; set; }
        }

        public Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = this.db.Users.SingleOrDefault(x => x.Id == userId);

            this.UserProfile = new UserModel()
            {
                Address = user.Address,
                CNIC = user.CNIC,
                Email = user.Email,
                FatherHusbandName = user.FatherHusbandName,
                //Name = user.Name,
                Name = user.UserName,
                NTN = user.NTN,
                PhoneNumber = user.PhoneNumber
            };

            return Task.FromResult(this.UserProfile);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(UserProfile.CNIC) && string.IsNullOrEmpty(UserProfile.NTN))
            {
                ModelState.AddModelError("AIN_REQUIREMENT", "Anyone of CNIC or NTN is also required .");

                return Page();
            }

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(UserProfile.CNIC))
                {
                    UserProfile.CNIC = Infrastructure.RegexUtilities.Clean(UserProfile.CNIC);
                }

                if (!string.IsNullOrEmpty(UserProfile.PhoneNumber))
                {
                    UserProfile.PhoneNumber = Infrastructure.RegexUtilities.Clean(UserProfile.PhoneNumber);
                }

                var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var user = this.db.Users.SingleOrDefault(x => x.Id == userId);

                this.db.Entry<User>(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;

                user.Address = UserProfile.Address;
                user.CNIC = UserProfile.CNIC;
                user.FatherHusbandName = UserProfile.FatherHusbandName;
                //user.Name = UserProfile.Name;
                user.UserName = UserProfile.Name;
                user.NTN = UserProfile.NTN;
                user.PhoneNumber = UserProfile.PhoneNumber;

                var rowsAffected = this.db.SaveChanges();

                if (rowsAffected < 1)
                {
                    ModelState.AddModelError(string.Empty, "Could not save.");
                }
                else
                {
                    //var loggedInUser = User as ClaimsPrincipal;
                    //var identity = loggedInUser.Identity as ClaimsIdentity;
                    ////var claim = (from c in loggedInUser.Claims
                    ////             where c.Type == "aaa"
                    ////             select c).FirstOrDefault();
                    //var claim = loggedInUser.Claims.FirstOrDefault(x => x.Type == "ContactName");

                    //identity.RemoveClaim(claim);

                    //IdentityUser loggedInUser = await _userManager.GetUserAsync(Request.HttpContext.User);
                    //var loggedInUser = await _userManager.GetUserAsync(User);


                    //await _signInManager.SignOutAsync();
                    await _signInManager.RefreshSignInAsync(user);

                    ViewData["Success"] = "Profile saved successfully.";
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
