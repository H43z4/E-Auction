using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Domain.Auction
{
    public class CustomMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SqlCode { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
    }
}
