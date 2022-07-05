using System.ComponentModel.DataAnnotations;

namespace Models.Views.Auction
{
    public class NewBid
    {
        [StringLength(50)]
        public string AIN { get; set; }
        public int BidPrice { get; set; }
    }
}
