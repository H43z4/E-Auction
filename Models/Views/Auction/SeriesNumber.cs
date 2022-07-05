using Models.Views.Base;

namespace Models.Views.Auction
{
    public class SeriesNumber : Paging
    {
        public int Id { get; set; }
        public int SeriesId { get; set; }
        public string AuctionNumber { get; set; }
        public int ReservePrice { get; set; }
    }
}
