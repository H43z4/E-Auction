using Models.Domain.Base;
using Models.Domain.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class Application : UserModel
    {
        public int Id { get; set; }
     
        [StringLength(50)]
        public string AIN { get; set; }

        [ForeignKey("Customer")]
        public int CustomerId { get; set; }
        public virtual User Customer { get; set; }

        [ForeignKey("SeriesCategory")]
        public int SeriesCategoryId { get; set; }
        public virtual SeriesCategory SeriesCategory { get; set; }

        [ForeignKey("Series")]
        public int SeriesId { get; set; }
        public virtual Series Series { get; set; }

        [ForeignKey("SeriesNumber")]
        public int SeriesNumberId { get; set; }
        public virtual SeriesNumber SeriesNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string ChasisNumber { get; set; }

        [Required]
        [StringLength(70)]
        public string OwnerName { get; set; }

        [ForeignKey("ApplicationStatus")]
        public int ApplicationStatusId { get; set; }
        public virtual ApplicationStatus ApplicationStatus { get; set; }
    }
}
