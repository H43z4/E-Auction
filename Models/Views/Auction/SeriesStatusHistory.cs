using System;

namespace Models.Views.Auction
{
    public class SeriesStatusHistory
    {
        public int SeriesId { get; set; }
        public string SeriesStatus { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}