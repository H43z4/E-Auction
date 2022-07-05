using System.ComponentModel.DataAnnotations;

namespace Models.Views.Auction
{
    public class GenericParamtersList
    {
        public string email { get; set; }
        public string password { get; set; }
        
        public string CNIC { get; set; }
        public string NTN { get; set; }
        public string phoneNumber { get; set; }
        public string address { get; set; }
        public string fatherHusbandName { get; set; }
        public string name { get; set; }


        public int userId { get; set; }
        public string player_id { get; set; }

        [StringLength(6)]
        public string OTP { get; set; }

        public string apkVersion { get; set; }

        public int applicationId { get; set; }

        public int seriesId { get; set; }
        public int seriesNumberId { get; set; }
        public string seriesNumberIdCSVs { get; set; }
    }
    public class ChangePassword
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel
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
}
