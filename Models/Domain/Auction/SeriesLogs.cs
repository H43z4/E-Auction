using Models.Domain.Base;
using Models.Domain.Oraganization;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class SeriesLogs : BaseModel
    {
        [Key]
        public int SeriesLogsId { get; set; }

        [ForeignKey("Series")]
        public int SeriesId { get; set; }
        public virtual Series Series { get; set; }

        [ForeignKey("District")]
        public int DistrictId { get; set; }
        public virtual District District { get; set; }
        
        public int AuctionYear { get; set; }

        [ForeignKey("SeriesCategory")]
        public int SeriesCategoryId { get; set; }
        public virtual SeriesCategory SeriesCategory { get; set; }

        [ForeignKey("SeriesStatus")]
        public int SeriesStatusId { get; set; }
        public virtual SeriesStatus SeriesStatus { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
        public DateTime AuctionStartDateTime { get; set; }
        public DateTime AuctionEndDateTime { get; set; }
        public DateTime RegStartDateTime { get; set; }
        public DateTime RegEndDateTime { get; set; }
        public bool? IsReauctioning { get; set; }
    }
}
