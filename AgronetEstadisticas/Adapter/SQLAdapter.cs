using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Diagnostics;
using System.Data;

using AgronetEstadisticas.Models;

namespace AgronetEstadisticas.Adapter
{
    public class SQLAdapter
    {
        public DataTable GetDatatable(string sqlString)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AgronetSQL"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            var results = new DataTable();
            using (var command = new SqlCommand(sqlString, connection))
            {
                connection.Open();

                using (var adapter = new SqlDataAdapter())
                {
                    adapter.SelectCommand = command;
                    adapter.Fill(results);
                }

                connection.Close();
            }

            return results;
        }

    }
}