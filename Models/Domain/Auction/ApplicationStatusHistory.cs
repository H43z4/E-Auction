using Models.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class ApplicationStatusHistory : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        [ForeignKey("ApplicationStatus")]
        public int ApplicationStatusId { get; set; }
        public ApplicationStatus ApplicationStatus { get; set; }

        [StringLength(100)]
        public string Comments { get; set; }
    }
}
