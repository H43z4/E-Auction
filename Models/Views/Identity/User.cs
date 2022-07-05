using Models.Views.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Identity
{
    public class User : Paging
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string FullName { get; set; }

        [StringLength(50)]
        public string FatherHusbandName { get; set; }

        [Required]
        [StringLength(13)]
        public string CNIC { get; set; }

        [StringLength(20)]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        [StringLength(25)]
        public string NTN { get; set; }

        [StringLength(100)]
        public string Company { get; set; }

        public DateTime? PasswordExpiryDate { get; set; }

        public int? CreatedBy { get; set; }

        [Required]
        public System.DateTime CreatedOn { get; set; }

        public bool? IsSoftDeleted { get; set; }
    }
}
