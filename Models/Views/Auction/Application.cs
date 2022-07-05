using Models.Views.Bank;
using Models.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Auction
{
    public class Application : Paging
    {
        public int Id { get; set; }

        public string AIN { get; set; }

        public int CustomerId { get; set; }
        public string Customer { get; set; }

        public int SeriesCategoryId { get; set; }
        public string SeriesCategory { get; set; }

        public int SeriesId { get; set; }
        public string Series { get; set; }

        public int SeriesNumberId { get; set; }

        [Display(Name = "Registration Mark")]
        public string SeriesNumber { get; set; }
        public int ReservePrice { get; set; }

        //[Required]
        [StringLength(50)]
        public string ChasisNumber { get; set; }

        //[Required]
        [StringLength(70)]
        public string OwnerName { get; set; }

        [StringLength(20)]
        public string PSId { get; set; }

        public int PaymentStatusId { get; set; }

        public DateTime? PaidOn { get; set; }

        public int ApplicationStatusId { get; set; }
        public string ApplicationStatus { get; set; }

        public DateTime RegStartDateTime { get; set; }
        public DateTime RegEndDateTime { get; set; }

        public DateTime AuctionStartDateTime { get; set; }
        public DateTime AuctionEndDateTime { get; set; }

        public int RemainingTime { get; set; }

        public List<ApplicationStatusHistory> ApplicationStatusHistory { get; set; }

        public AppSecurityDeposit AppSecurityDeposit { get; set; }
    }
}
