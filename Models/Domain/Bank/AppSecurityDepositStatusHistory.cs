using Models.Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models.Domain.Bank
{
    public class AppSecurityDepositStatusHistory : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("AppSecurityDeposit")]
        public int AppSecurityDepositId { get; set; }
        public virtual AppSecurityDeposit AppSecurityDeposit { get; set; }

        [ForeignKey("AppSecurityDepositStatus")]
        public int AppSecurityDepositStatusId { get; set; }
        public AppSecurityDepositStatus AppSecurityDepositStatus { get; set; }

        [StringLength(100)]
        public string DiaryNumber { get; set; }

        [StringLength(100)]
        public string Comments { get; set; }
    }
}
