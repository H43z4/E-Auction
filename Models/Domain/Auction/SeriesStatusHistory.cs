using Models.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class SeriesStatusHistory : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("Series")]
        public int SeriesId { get; set; }
        public virtual Series Series { get; set; }

        [ForeignKey("SeriesStatus")]
        public int SeriesStatusId { get; set; }
        public SeriesStatus SeriesStatus { get; set; }

        [StringLength(100)]
        public string Comments { get; set; }
    }
}
