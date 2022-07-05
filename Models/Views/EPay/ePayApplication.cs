using System;

namespace Models.Views.EPay
{
    public class ePayApplication
    {
        public string psId { get; set; }
        public int deptTransactionId { get; set; }
        public string ain { get; set; }
        public string consumerName { get; set; }
        public string mobileNo { get; set; }
        public string cnic { get; set; }
        public string ntn { get; set; }
        public string companyName { get; set; }
        public string emailAddress { get; set; }
        public string series { get; set; }
        public string registrationMark { get; set; }
        public string chassisNo { get; set; }
        public string ownerName { get; set; }
        public DateTime dueDate { get; set; }
        public int amountWithinDueDate { get; set; }
        public string accountHeadName { get; set; }
        public string accountNumber { get; set; }
        public string amountToTransfer { get; set; }
    }
}
