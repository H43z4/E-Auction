using System;
using System.Collections.Generic;

namespace Models.Views.Auction
{
    public class Bids
    {
        public List<Application> Applications { get; set; }
        public List<LastBid> LastBids { get; set; }
        public List<HighestBid> HighestBids { get; set; }
        public int CustomerId { get; set; }
        public string SysDate { get; set; }
    }

    public class LastBid
    { 
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int SeriesNumberId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int LastBiddingPrice { get; set; }
    }

    public class HighestBid
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int SeriesNumberId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int HighestBiddingPrice { get; set; }
        public string AIN { get; set; }
    }
}
