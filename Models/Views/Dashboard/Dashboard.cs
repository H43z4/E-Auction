using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Models.Views.Dashboard
{
	public class Dashboard
	{
		public IEnumerable<TotalCounters> totalCounters { get; set; }
		public IEnumerable<TotalRevenueSeriesWise> TopTenRevenueSeries { get; set; }
		public IEnumerable<TopTenRevenueNumbers> TopTenRevenueNumbers { get; set; }
		public IEnumerable<TopTenApplicationsSeries> TopTenApplicationSeries { get; set; }
		public IEnumerable<TopTenApplicationsNumbers> TopTenApplicationsNumbers { get; set; }
		public IEnumerable<SeriesCategory> SeriesCategoryList { get; set; }
	}

	public class TotalCounters
	{
		public int TotalApplications { get; set; }
		public int TotalApprovedApplications { get; set; }
		public int TotalBidders { get; set; }
		public int TotalActiveSeries { get; set; }
		public int TotalNumberSold { get; set; }
		public int TotalNumbers { get; set; }
		public int TotalRevenue { get; set; }
	}

	public class TotalRevenueSeriesWise
	{
		public int Revenue { get; set; }
		public string SeriesName { get; set; }
	}
	public class TopTenRevenueNumbers
	{
		public int Revenue { get; set; }
		public string SeriesNumber { get; set; }
	}
	public class TopTenApplicationsSeries
	{
		public string SeriesName { get; set; }
		public int TotalApplications { get; set; }
		public int NotApproved { get; set; }
		public int Approved { get; set; }
	}
	public class TopTenApplicationsNumbers
	{
		public string SeriesName { get; set; }
		public string AuctionNumber { get; set; }
		public int TotalApplications { get; set; }
		public int NotApproved { get; set; }
		public int Approved { get; set; }
	}
	public class SeriesCategory
	{
		public string ActiveSeries { get; set; }
		public string Category { get; set; }
	}

	public class SeriesDropdown
	{
		public int SeriesID { get; set; }
		public string SeriesName { get; set; }
	}
}

