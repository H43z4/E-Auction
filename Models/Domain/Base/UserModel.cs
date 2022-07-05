using Models.Domain.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Models.Domain.Base
{
    public class UserModel
    {
        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public virtual User User { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public bool? IsSoftDeleted { get; set; }
    }
}
