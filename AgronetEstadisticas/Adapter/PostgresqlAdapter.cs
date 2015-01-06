using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using System.Data;
using Npgsql;

namespace AgronetEstadisticas.Adapter
{
    public class PostgresqlAdapter
    {
        public DataTable GetDataTable(string sqlString)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AgronetPostgreSQL"].ConnectionString;
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);

            var results = new DataTable();
            using (var command = new NpgsqlCommand(sqlString, connection))
            {
                connection.Open();

                using (var adapter = new NpgsqlDataAdapter())
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