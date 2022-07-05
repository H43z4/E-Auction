using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Views.Bank
{
    public class AppSecurityDeposit : Paging
    {
        public int Id { get; set; }

        public int ApplicationId { get; set; }

        public int AppSecurityDepositStatusId { get; set; }
        public string AppSecurityDepositStatus { get; set; }

        [Required]
        public int BankId { get; set; }
        public string Bank { get; set; }
        public SelectList BankSelectList { get; set; }

        [Required]
        public int BankDocumentTypeId { get; set; }
        public string BankDocumentType { get; set; }
        public SelectList BankDocumentTypeSelectList { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Pay Order/ Challan No.")]
        public string DocumentIdValue { get; set; }

        public int Worth { get; set; }

        [StringLength(100)]
        public string DiaryNumber { get; set; }

        [StringLength(100)]
        public string Remarks { get; set; }

        public List<AppSecurityDepositStatusHistory> AppSecurityDepositStatusHistory { get; set; }
    }
}
