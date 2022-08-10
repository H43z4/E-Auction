using Models.Domain.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Base
{
    public class BaseModel
    {
        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public virtual User User { get; set; }

        [Required]
        public DateTime CreatedOn { get; set; }
        
        [ForeignKey("Modifier")]
        public int? ModifiedBy { get; set; }
        public virtual User Modifier { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool? IsSoftDeleted { get; set; }
    }
}
