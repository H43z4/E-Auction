using Models.Views.Base;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Auction
{
    public class Customer : Paging
    {
        public int Id { get; set; }

        public int CustomerTypeId { get; set; }
        public string CustomerType { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string FatherHusbandName { get; set; }

        [StringLength(13)]
        public string CNIC { get; set; }

        [StringLength(50)]
        public string Phone { get; set; }

        [StringLength(50)]
        public string Email { get; set; }

        [StringLength(100)]
        public string Address { get; set; }

        [StringLength(50)]
        public string NTN { get; set; }

        [StringLength(50)]
        public string ChasisNumber { get; set; }
    }

}
