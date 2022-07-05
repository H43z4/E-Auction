using System;

namespace Models.Views.Notification
{
    public class NotificationStatusHistory
    {
        public int NotificationId { get; set; }
        public string NotificationStatus { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}