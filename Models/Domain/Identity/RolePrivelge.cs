using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Identity
{
    public class RolePrivelge
    {
        public int Id { get; set; }

        [Index("IX_RoleIDAndLinkId", 1, IsUnique = true)]
        public int RoleId { get; set; }

        [Index("IX_RoleIDAndPrivelgeId", 2, IsUnique = true)]
        public int PrivelgeId { get; set; }

        [ForeignKey("RoleId")]
        public virtual CustomRole CustomRole { get; set; }

        [ForeignKey("PrivelgeId")]
        public virtual Privelge Privelge { get; set; }
    }
}
