using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Models.Views.Auction
{
    public class SeriesStatusWise
    {
        public SelectList SeriesStatusSelectList { get; set; }
        public List<Series> Series { get; set; }
    }
}
