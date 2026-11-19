using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using MySqlConnector;


namespace KWMS.Data
{
    internal class DatabaseConnection
    {
        // Read the connection string once, from App.config
        private static readonly string _connectionString = LoadConnectionString();

        private static string LoadConnectionString()
        {
            ConnectionStringSettings? settings = ConfigurationManager.ConnectionStrings["KWMS_DB"];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'KWMS_DB' was not found in App.config.");
            }

            return settings.ConnectionString;
        }

        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }
    }
}
