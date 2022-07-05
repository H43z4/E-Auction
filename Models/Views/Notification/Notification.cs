using Models.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Notification
{
    public class Notification : Paging
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Heading { get; set; }

        [Required]
        [StringLength(200)]
        public string Content { get; set; }
        public string Data { get; set; }

        public DateTime CreatedOn { get; set; }

        public int NotificationStatusId { get; set; }
        public string NotificationStatus { get; set; }

        public List<NotificationStatusHistory> NotificationStatusHistory { get; set; }

    }
}
