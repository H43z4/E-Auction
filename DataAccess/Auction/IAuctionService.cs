using System.Data;

namespace DataAccess.Auction
{
    interface IAuctionService
    {
        DataSet GetAuctionSeries(bool? IsActive = true);
        DataSet GetAuctionSeriesDetail(int seriesId);
    }
}
