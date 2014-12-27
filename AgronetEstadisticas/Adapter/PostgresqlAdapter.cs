using System;
using System.Collections.Generic;
using System.Web;
using System.Configuration;
using Npgsql;

namespace AgronetEstadisticas.Adapter
{
    public class PostgresqlAdapter
    {
        public List<Dictionary<string, object>> GetResults(string sqlString)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AgronetPostgreSQL"].ConnectionString;
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);

            var results = new List<Dictionary<string, object>>();
            using (var command = new NpgsqlCommand(sqlString, connection))
            {
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var datarow = new Dictionary<string, object>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                datarow.Add(reader.GetName(i), reader.GetValue(i));
                            }
                            results.Add(datarow);
                        }
                        return results;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}