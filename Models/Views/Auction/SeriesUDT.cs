using Models.Domain.Auction;
using Models.Views.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Models.Views.Auction
{
    public class SeriesUDT
    {
        //public int SeriesId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        [StringLength(100)]
        public string SeriesName { get; set; }

    }
}
