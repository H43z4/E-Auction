using Models.Domain.Auction;
using Models.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Auction
{
    public class Series : Paging
    {
        public int Id { get; set; }

        public int DistrictId { get; set; }
        public string District { get; set; }
        
        public int AuctionYear { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int SeriesStatusId { get; set; }
        public string SeriesStatus { get; set; }

        [StringLength(100)]
        public string SeriesName { get; set; }
        public DateTime AuctionStartDateTime { get; set; }
        public DateTime AuctionEndDateTime { get; set; }
        public DateTime RegStartDateTime { get; set; }
        public DateTime RegEndDateTime { get; set; }
        public bool IsReauctioning { get; set; }
        public bool IsActive { get; set; }

        public List<SeriesStatusHistory> SeriesStatusHistory { get; set; }
    }
}
