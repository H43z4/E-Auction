using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Bank
{
    public class AppSecurityDepositStatusHistory
    {
        public int Id { get; set; }

        public int AppSecurityDepositId { get; set; }
        public int AppSecurityDepositStatusId { get; set; }
        public string AppSecurityDepositStatus { get; set; }

        [StringLength(100)]
        public string DiaryNumber { get; set; }

        [StringLength(100)]
        public string Comments { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
