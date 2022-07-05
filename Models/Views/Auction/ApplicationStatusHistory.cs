using System;

namespace Models.Views.Auction
{
    public class ApplicationStatusHistory
    {
        public int ApplicationId { get; set; }
        public string ApplicationStatus { get; set; }
        public string Comments { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}