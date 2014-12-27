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
        public List<Dictionary<string, object>> GetResults(string sqlString)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["AgronetSQL"].ConnectionString;
            SqlConnection connection = new SqlConnection(connectionString);

            var results = new List<Dictionary<string, object>>();
            using (var command = new SqlCommand(sqlString, connection))
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