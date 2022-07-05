using Microsoft.Reporting.WebForms;
using System.Collections.Generic;
using System.Data;

namespace dotNetReports
{
    public class ReportGenerator
    {
        public byte[] GenerateApprovalReport(string reportPath, System.Data.DataTable dt, ReportOutputFormat outputFormat, out string mimeType)
        {
            eauctionProductionDataSet ds = new eauctionProductionDataSet();

            var table = DataTableExtension.CopyTo(dt, ds.Approvals);

            var reportDataSource = new Microsoft.Reporting.WebForms.ReportDataSource("Approvals", ds.Tables["Approvals"]);

            CustomReport customReport = new CustomReport();
            //customReport.PageSetup = new PageSetup();   // There is default A4 Page setup in PageSetup Constructor
            //customReport.PageSetup = new PageSetup("17", "11", "0.5", "0.5", "0.5", "0.5");
            customReport.PageSetup = new PageSetup("15.5", "9.5", "0.5", "0.5", "0.5", "0.5");
            customReport.OutputFormat = outputFormat;
            customReport.ReportPath = reportPath;
            customReport.ReportDataSources.Add(reportDataSource);

            mimeType = customReport.MimeType;

            return customReport.GetReport();
        }
        public byte[] GenerateReport(string reportPath, System.Data.DataTable dt, ReportOutputFormat outputFormat)
        {

            eauctionProductionDataSet ds = new eauctionProductionDataSet();

            //int totalRows = dataTable.Rows.Count;

            //for (int i = 0; i < totalRows; i++)
            //{
            //    DataRow dr = ds.Winners.NewRow();

            //    dr["SeriesCategory"] = dataTable.Rows[i].ItemArray[0].ToString();
            //    dr["Series"] = dataTable.Rows[i].ItemArray[1].ToString();
            //    dr["SeriesNumber"] = dataTable.Rows[i].ItemArray[2].ToString();
            //    dr["ReservePrice"] = dataTable.Rows[i].ItemArray[3].ToString();
            //    dr["HighestBiddingPrice"] = dataTable.Rows[i].ItemArray[4].ToString();
            //    dr["WinnerAIN"] = dataTable.Rows[i].ItemArray[5].ToString();

            //    ds.Winners.Rows.Add(dr);
            //}

            foreach (DataRow row in dt.Rows)
            {
                DataRow dr = ds.Winners.NewRow();

                foreach (DataColumn column in dt.Columns)
                {
                    if (ds.Winners.Columns.Contains(column.ColumnName))
                    { 
                        dr[column.ColumnName] = row[column].ToString();
                    }
                }

                ds.Winners.Rows.Add(dr);
            }


            var reportDataSource = new ReportDataSource("Winners", ds.Tables["Winners"]);

            CustomReport customReport = new CustomReport();
            //customReport.PageSetup = new PageSetup();   // There is default A4 Page setup in PageSetup Constructor
            //customReport.PageSetup = new PageSetup("17", "11", "0.5", "0.5", "0.5", "0.5");
            customReport.PageSetup = new PageSetup("15.5", "9.5", "0.5", "0.5", "0.5", "0.5");
            customReport.OutputFormat = outputFormat;
            customReport.ReportPath = reportPath;
            customReport.ReportDataSources.Add(reportDataSource);

            return customReport.GetReport();
        }
        //public byte[] GenerateReport(string reportPath, List<Models.Views.Auction.Winners> winners, ReportOutputFormat outputFormat)
        //{
        //    var reportDataSource = new Microsoft.Reporting.WebForms.ReportDataSource();
        //    reportDataSource.Name = "eauctionProductionDataSet";
        //    reportDataSource.Value = winners;
        //    //var reportDataSource = new Microsoft.Reporting.WebForms.ReportDataSource(("eauctionProductionDataSet", dataTable);

        //    CustomReport customReport = new CustomReport();
        //    //customReport.PageSetup = new PageSetup();   // There is default A4 Page setup in PageSetup Constructor
        //    //customReport.PageSetup = new PageSetup("17", "11", "0.5", "0.5", "0.5", "0.5");
        //    customReport.PageSetup = new PageSetup("15.5", "9.5", "0.5", "0.5", "0.5", "0.5");
        //    customReport.OutputFormat = outputFormat;
        //    customReport.ReportPath = reportPath;
        //    customReport.ReportDataSources.Add(reportDataSource);

        //    return customReport.GetReport();
        //}
    }
}
