using Models.Oraganization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Identity
{
    public class RoleDistrict
    {
        public int Id { get; set; }

        [Index("IX_RoleIdAndDistrictId", 1, IsUnique = true)]

        [ForeignKey("CustomRole")]
        public int RoleId { get; set; }
        public virtual CustomRole CustomRole { get; set; }

        [Index("IX_RoleIdAndDistrictId", 2, IsUnique = true)]
        [ForeignKey("District")]
        public int DistrictId { get; set; }
        public virtual District District { get; set; }
    }
}
