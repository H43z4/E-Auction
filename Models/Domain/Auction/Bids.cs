using Models.Domain.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class Bids : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        [ForeignKey("SeriesNumber")]
        public int SeriesNumberId { get; set; }
        public virtual SeriesNumber SeriesNumber { get; set; }

        public int BiddingPrice { get; set; }
    }
}
