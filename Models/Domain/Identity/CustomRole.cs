using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Domain.Identity
{
    public class CustomRole : IdentityRole<int>
    {
        public CustomRole(string name) { Name = name; }

        [Required]
        public DateTime CreatedOn { get; set; }

        public bool? IsSoftDeleted { get; set; }
    }
}
