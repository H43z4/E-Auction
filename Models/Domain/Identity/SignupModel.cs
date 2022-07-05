using System.ComponentModel.DataAnnotations;

namespace Models.Domain.Identity
{
    public class SignupModel
    {
        [Required]
        [StringLength(15)]
        [Display(Name = "Mobile Phone Number")]
        public string PhoneNumber { get; set; }

        [StringLength(13)]
        [Display(Name = "CNIC")]
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
}
