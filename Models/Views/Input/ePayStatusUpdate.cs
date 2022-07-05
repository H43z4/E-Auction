using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Input
{
    public class ePayStatusUpdate
    {
        [Required]
        public string psidStatus { get; set; }

        [Required]
        public string deptTransactionId { get; set; }

        [Required]
        public string psId { get; set; }

        [Required]
        public string amountPaid { get; set; }
        
        [Required]
        public string paidDate { get; set; }
        
        [Required]
        public string paidTime { get; set; }
        
        [Required]
        public string bankCode { get; set; }
    }
}
