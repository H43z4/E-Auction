using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Domain.Identity
{
    public class CustomUserRole : IdentityUserRole<int>
    {

        [Required]
        public System.DateTime CreatedOn { get; set; }

        public bool? IsSoftDeleted { get; set; }
    }
}
