using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace dotNetReports
{
    public enum ReportOutputFormat
    {
        pdf, Excel, Word
    }

    public enum ReportOutputFileExtension
    {
        pdf, xlsx, docx, jpeg
    }

    public class PageSetup
    {
        public string PageWidth, PageHeight, MarginTop, MarginLeft, MarginRight, MarginBottom;

        public PageSetup()
        {
            this.PageWidth = "8.5in";
            this.PageHeight = "11in";
            this.MarginTop = "0.5in";
            this.MarginLeft = "1in";
            this.MarginRight = "1in";
            this.MarginBottom = "0.5in";
        }

        public PageSetup(string PageWidth, string PageHeight, string MarginTop, string MarginLeft, string MarginRight, string MarginBottom)
        {
            this.PageWidth = PageWidth;
            this.PageHeight = PageHeight;
            this.MarginTop = MarginTop;
            this.MarginLeft = MarginLeft;
            this.MarginRight = MarginRight;
            this.MarginBottom = MarginBottom;
        }

        public override string ToString()
        {
            string pageSetup = "<PageWidth>" + this.PageWidth + "</PageWidth>" +
                                "  <PageHeight>" + this.PageHeight + "</PageHeight>" +
                                "  <MarginTop>" + this.MarginTop + "</MarginTop>" +
                                "  <MarginLeft>" + this.MarginLeft + "</MarginLeft>" +
                                "  <MarginRight>" + this.MarginRight + "</MarginRight>" +
                                "  <MarginBottom>" + this.MarginBottom + "</MarginBottom>";
            return pageSetup;
        }
    }


    public class CustomReport
    {
        public ReportOutputFormat OutputFormat { get; set; }

        public string ExportFileName { get; set; }

        private string mimeType;

        public string MimeType
        {
            get
            {
                return mimeType;
            }
            private set
            {
                mimeType = value;
            }
        }

        private string encoding;

        public string Encoding
        {
            get
            {
                return encoding;
            }
            private set
            {
                encoding = value;
            }
        }

        private string fileNameExtension;

        public string FileNameExtension
        {
            get
            {
                return fileNameExtension;
            }
            private set
            {
                fileNameExtension = value;
            }
        }

        private string[] streams;

        public string[] Streams
        {
            get
            {
                return streams;
            }
            private set
            {
                streams = value;
            }
        }

        private Microsoft.Reporting.WebForms.Warning[] warnings;

        public Microsoft.Reporting.WebForms.Warning[] Warnings
        {
            get
            {
                return warnings;
            }
            private set
            {
                warnings = value;
            }
        }

        public string ReportPath { get; set; }

        public PageSetup PageSetup { get; set; }

        public IList<ReportDataSource> ReportDataSources { get; set; }

        public CustomReport()
        {
            this.ReportDataSources = new List<ReportDataSource>();
        }

        public byte[] GetReport()
        {
            LocalReport localReport = new LocalReport();
            localReport.ReportPath = this.ReportPath;

            foreach (var source in this.ReportDataSources)
            {
                localReport.DataSources.Add(source);
            }

            string deviceInfo = "<DeviceInfo>" +
                                "  <OutputFormat>" + this.OutputFormat.ToString() + "</OutputFormat>" +
                                this.PageSetup.ToString() +
                                "</DeviceInfo>";


            //Render the report             
            return localReport.Render(
                this.OutputFormat.ToString(),
                deviceInfo,
                out mimeType,
                out encoding,
                out fileNameExtension,
                out streams,
                out warnings
            );
        }
    }
}