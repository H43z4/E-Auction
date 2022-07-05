using Models.Domain.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Notification
{
    public class NotificationStatusHistory : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("Notification")]
        public int NotificationId { get; set; }
        public virtual Notification Notification { get; set; }

        [ForeignKey("NotificationStatus")]
        public int NotificationStatusId { get; set; }
        public NotificationStatus NotificationStatus { get; set; }

        [StringLength(100)]
        public string Comments { get; set; }
    }

}
