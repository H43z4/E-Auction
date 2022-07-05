using Models.Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models.Domain.Notification
{
    public class Notification : UserModel
    {
        public int Id { get; set; }
        public string Heading { get; set; }
        public string Content { get; set; }
        public string Data { get; set; }

        [ForeignKey("NotificationStatus")]
        public int NotificationStatusId { get; set; }
        public virtual NotificationStatus NotificationStatus { get; set; }
    }
}
