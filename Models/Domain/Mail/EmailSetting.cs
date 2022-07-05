using Models.Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Domain.Mail
{
    public class EmailSetting
    {
        public int Id { get; set; }

        [StringLength(50)]
        public string MessageType { get; set; }
        public int MessageTypeId { get; set; }

        [StringLength(100)]
        public string Sender { get; set; }
        [StringLength(100)]
        public string Password { get; set; }
        [StringLength(100)]
        public string Receiver { get; set; }
        [StringLength(100)]
        public string Subject { get; set; }
        [StringLength(1000)]
        public string Body { get; set; }
        [StringLength(100)]
        public string Host { get; set; }
        public int Port { get; set; } = 587;
        [StringLength(100)]
        public string SenderName { get; set; }
    }
}
