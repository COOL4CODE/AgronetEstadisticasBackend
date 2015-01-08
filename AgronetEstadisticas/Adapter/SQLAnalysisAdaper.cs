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