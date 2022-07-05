using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Identity
{
    public class UserIdentity
    {
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [StringLength(6)]
        public string OTP { get; set; }

        public DateTime OtpExpiryOn { get; set; }

        public string MobilePhoneAppId { get; set; }

        public DateTime MobilePhoneAppIdCreatedOn { get; set; }
    }
}
