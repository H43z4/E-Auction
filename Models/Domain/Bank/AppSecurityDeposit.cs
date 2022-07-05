using Models.Domain.Auction;
using Models.Domain.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models.Domain.Bank
{
    public class AppSecurityDeposit : UserModel
    {
        public int Id { get; set; }

        [ForeignKey("Application")]
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }

        [ForeignKey("AppSecurityDepositStatus")]
        public int AppSecurityDepositStatusId { get; set; }
        public virtual AppSecurityDepositStatus AppSecurityDepositStatus { get; set; }

        [Required]
        [ForeignKey("Bank")]
        public int BankId { get; set; }
        public virtual Bank Bank { get; set; }

        [Required]
        [ForeignKey("BankDocumentType")]
        public int BankDocumentTypeId { get; set; }
        public virtual BankDocumentType BankDocumentType { get; set; }

        [Required]
        [StringLength(100)]
        public string DocumentIdValue { get; set; }

        public int Worth { get; set; }

        [StringLength(100)]
        public string Remarks { get; set; }
    }
}
