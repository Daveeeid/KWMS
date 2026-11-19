using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace KWMS.Data.Repositories
{
    internal class DatabaseCheckRepository
    {
        public string GetServerVersion()
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand("SELECT VERSION();", connection))
                {
                    object? result = command.ExecuteScalar();
                    return result?.ToString() ?? "Unknown";
                }
            }
        }

        // Returns a dictionary: table name -> list of its column names
        public Dictionary<string, List<string>> GetColumnsOfTables(params string[] tableNames)
        {
            Dictionary<string, List<string>> tables =
                new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            const string sql = @"
                SELECT TABLE_NAME, COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME IN ('Companies', 'Users')
                ORDER BY TABLE_NAME, ORDINAL_POSITION;";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string tableName = reader.GetString("TABLE_NAME");
                        string columnName = reader.GetString("COLUMN_NAME");

                        if (!tables.ContainsKey(tableName))
                        {
                            tables[tableName] = new List<string>();
                        }
                        tables[tableName].Add(columnName);
                    }
                }
            }

            return tables;
        }
    }
}
