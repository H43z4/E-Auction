using System;

namespace Models.Views.Auction
{
    public class Winners
    {
        public int ApplicationId { get; set; }
        public string ApplicationStatus { get; set; }
        public DateTime AuctionEndDateTime { get; set; }
        public string YourAIN { get; set; }
        public string SeriesCategory { get; set; }
        public int SeriesCategoryId { get; set; }
        public int SeriesId { get; set; }
        public string Series { get; set; }
        public string SeriesNumber { get; set; }
        public int SeriesNumberId { get; set; }
        public int ReservePrice { get; set; }
        public int YourHighestBiddingPrice { get; set; }
        public int HighestBiddingPrice { get; set; }
        public string WinnerAIN { get; set; }
        public string WinnerChasisNumber { get; set; }
        public string ChasisNumber { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FatherHusbandName { get; set; }
        public string CNIC { get; set; }
        public string NTN { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
    }
}