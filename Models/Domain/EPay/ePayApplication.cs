using Models.Domain.Auction;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.EPay
{
    public class ePayApplication
    {
        public int Id { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        [StringLength(20)]
        public string PSId { get; set; }

        public DateTime CreatedOn { get; set; }

        public int PaymentStatusId { get; set; }

        public int? AmountPaid { get; set; }

        public DateTime? PaidOn { get; set; }

        [StringLength(10)]
        public string BankCode { get; set; }
    }
}