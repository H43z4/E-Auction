using System.ComponentModel.DataAnnotations;

namespace Models.Views.Input
{
    public class Application
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Chassis Number")]
        public string ChasisNumber { get; set; }

        [Required]
        [StringLength(70)]
        [Display(Name = "Vehicle Owner Name")]
        public string OwnerName { get; set; }
    }
}
