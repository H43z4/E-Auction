using System.Collections.Generic;
using System.Data;

namespace dotNetReports
{
    public static class DataTableExtension
    {
        public static DataTable CopyTo(this DataTable sourceTable, DataTable destinationTable)
        {
            foreach (var sourceRow in sourceTable.Select())
            {
                System.Data.DataRow destinationRow = destinationTable.NewRow();

                foreach (DataColumn col in sourceTable.Columns)
                {
                    destinationRow[col.ColumnName] = sourceRow[col.ColumnName];
                }

                destinationTable.Rows.Add(destinationRow);
            }

            return destinationTable;
        }
    }
}
