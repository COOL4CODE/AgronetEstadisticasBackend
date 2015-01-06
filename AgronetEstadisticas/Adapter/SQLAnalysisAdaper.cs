using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Diagnostics;
using MdxClient;
using System.Data;
using Microsoft.AnalysisServices.AdomdClient;

using AgronetEstadisticas.Models;

namespace AgronetEstadisticas.Adapter
{
    public class SQLAnalysisAdaper
    {

        public Table GetTable(string sqlString, string connectionName)
        {
            CellSet cs = this.GetCellSet(sqlString, connectionName);
            Table table = new Table { columns = new List<Column>(), columnsgroups = new List<ColumnGroup>(), rows = new DataTable() };
            int totalColumns = 0;
            int countColumnsInAxe0 = 0;
            int countColumnsInAxe1 = 0;

            TupleCollection tuplesOnColumns = cs.Axes[0].Set.Tuples;
            for (int col = 0; col < tuplesOnColumns.Count; col++)
            {
                Column columnInAxe0 = new Column { text = tuplesOnColumns[col].Members[0].Caption, datafield = "data" + totalColumns };
                table.columns.Add(columnInAxe0);
                table.rows.Columns.Add(columnInAxe0.datafield);
                countColumnsInAxe0++;
                totalColumns++;
            }

            TupleCollection tuplesOnRows = cs.Axes[1].Set.Tuples;
            for (int i = 0; i < tuplesOnRows[0].Members.Count; i++)
            {
                Column columnInAxe1 = new Column { text = "", datafield = "data" + totalColumns };
                table.columns.Add(columnInAxe1);
                table.rows.Columns.Add(columnInAxe1.datafield);
                countColumnsInAxe1++;
                totalColumns++;
            }

            for (int row = 0; row < tuplesOnRows.Count; row++)
            {
                table.rows.Rows.Add();
                for (int colAxe0 = 0; colAxe0 < countColumnsInAxe0; colAxe0++)
                {
                    table.rows.Rows[row][colAxe0] = cs.Cells[colAxe0, row].Value;
                }
                for (int colAxe1 = 0; colAxe1 < countColumnsInAxe1; colAxe1++)
                {
                    table.rows.Rows[row][colAxe1 + countColumnsInAxe0] = tuplesOnRows[row].Members[colAxe1].Caption;
                }
            }
            return table;
        }

        public DataTable GetDataTable(string connectionName, string mdx, List<MdxParameter> paramerters)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            DataTable dataTable = new DataTable();

            MdxConnection connection = new MdxConnection(connectionString);
            using (connection)
            {
                connection.Open();
                using (MdxCommand command = connection.CreateCommand())
                {
                    command.CommandText = mdx;
                    foreach (var p in paramerters)
                    {
                        command.Parameters.Add(p);
                    }
                    using (MdxDataAdapter dataAdapter = new MdxDataAdapter())
                    {
                        dataAdapter.SelectCommand = command;
                        dataAdapter.Fill(dataTable);
                    }

                    connection.Close();
                }
            }

            return dataTable;
        }

        private CellSet GetCellSet(string sqlString, string connectionName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            CellSet results = null;

            AdomdConnection connection = new AdomdConnection(connectionString);
            using (connection)
            {
                connection.Open();
                
                using (AdomdCommand command = connection.CreateCommand())
                {
                    command.CommandText = sqlString;
                    results = command.ExecuteCellSet();                    
                }

                connection.Close();
            }

            return results;
        }

    }
}