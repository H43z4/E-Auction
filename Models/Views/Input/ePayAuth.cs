using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Models.Views.Input
{
    public class ePayAuth
    {
        [Required]
        public string secret { get; set; }

        [Required]
        public string apikey { get; set; }
    }
}
