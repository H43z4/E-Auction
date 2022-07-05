using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Views.Http
{
    public class AuctionNumberHttpResponse
    {
        public bool status { get; set; }
        public List<Models.Views.Auction.SeriesNumber> SeriesNumbers { get; set; }
    }
}
