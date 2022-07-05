using System.ComponentModel.DataAnnotations;

namespace Models.Domain.Oraganization
{
    public class District
    {
        public int Id { get; set; }

        [StringLength(100)]
        public string Name { get; set; }
    }
}
