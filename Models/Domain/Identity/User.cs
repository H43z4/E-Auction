using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Models.Domain.Auction;

namespace Models.Domain.Identity
{
    public class User : IdentityUser<int>
    {
        [ForeignKey("UserType")]
        public int UserTypeId { get; set; }
        public virtual UserType UserType { get; set; }

        [StringLength(50)]
        public string FullName { get; set; }

        [StringLength(50)]
        public string FatherHusbandName { get; set; }

        [Required]
        [StringLength(13)]
        public string CNIC { get; set; }

        [StringLength(25)]
        public string NTN { get; set; }

        [StringLength(100)]
        public string Company { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        public DateTime? PasswordExpiryDate { get; set; }

        public int? CreatedBy { get; set; }

        [Required]
        public System.DateTime CreatedOn { get; set; }

        public bool? IsSoftDeleted { get; set; }

    }
}
