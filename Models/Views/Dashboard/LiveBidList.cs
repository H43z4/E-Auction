using System;
using System.Collections.Generic;
using System.Text;

namespace Models.Views.Dashboard
{
    public class LiveBidList
    {
        public IEnumerable<LiveBidData> BidList { get; set; }
    }

    public class LiveBidData
    {
        public string RegNo { get; set; }
        public int ReservePrice { get; set; }
        public int maxbid { get; set; }
        public string TimeLeft { get; set; }
    }
}
