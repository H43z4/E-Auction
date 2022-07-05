using Models.Views.Auction;
using System.Collections.Generic;

namespace Models.Views.Http
{
    public class SeriesHttpResponse
    {
        public bool status { get; set; }
        public List<Series> Series { get; set; }
    }
}
