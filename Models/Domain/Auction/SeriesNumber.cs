using Models.Domain.Base;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class SeriesNumber : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("Series")]
        public int SeriesId { get; set; }
        public virtual Series Series { get; set; }
        public string AuctionNumber { get; set; }
        public int ReservePrice { get; set; }
        public bool IsAuctionable { get; set; }
        public DateTime? AuctionEndDateTime { get; set; }
    }
}
