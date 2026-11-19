using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    /// <summary>
    /// Shared ADO.NET plumbing. Every method opens a connection, runs ONE parameterized
    /// command, and disposes everything with 'using'. SQL is never built by joining
    /// user text into a string, so SQL injection is not possible.
    /// </summary>
    public abstract class BaseRepository
    {
        // Short helper to create a parameter. A C# null becomes SQL NULL.
        protected static MySqlParameter P(string name, object? value) =>
            new MySqlParameter(name, value ?? DBNull.Value);

        // Runs a SELECT and converts every row to an object using 'map'.
        protected List<T> GetList<T>(string sql, Func<MySqlDataReader, T> map, params MySqlParameter[] parameters)
        {
            List<T> items = new List<T>();

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(map(reader));
                        }
                    }
                }
            }
            return items;
        }

        // Runs a SELECT and returns the first row, or null if nothing matched.
        protected T? GetOne<T>(string sql, Func<MySqlDataReader, T> map, params MySqlParameter[] parameters) where T : class
        {
            return GetList(sql, map, parameters).FirstOrDefault();
        }

        // UPDATE / DELETE. Returns the number of rows matched. 0 means "nothing matched".
        protected int ExecuteUpdate(string sql, params MySqlParameter[] parameters)
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    return command.ExecuteNonQuery();
                }
            }
        }

        // INSERT. Returns the new AUTO_INCREMENT id, or 0 if no row was inserted.
        protected int ExecuteInsert(string sql, params MySqlParameter[] parameters)
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    int rows = command.ExecuteNonQuery();
                    return rows == 0 ? 0 : (int)command.LastInsertedId;
                }
            }
        }

        // SELECT COUNT(*) ... returns true when the count is above zero.
        protected bool Exists(string sql, params MySqlParameter[] parameters)
        {
            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddRange(parameters);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        // The two methods below run inside a transaction the caller already started.
        protected static int ExecuteUpdateIn(MySqlConnection connection, MySqlTransaction transaction,
                                             string sql, params MySqlParameter[] parameters)
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddRange(parameters);
                return command.ExecuteNonQuery();
            }
        }

        protected static int ExecuteInsertIn(MySqlConnection connection, MySqlTransaction transaction,
                                             string sql, params MySqlParameter[] parameters)
        {
            using (MySqlCommand command = new MySqlCommand(sql, connection, transaction))
            {
                command.Parameters.AddRange(parameters);
                int rows = command.ExecuteNonQuery();
                return rows == 0 ? 0 : (int)command.LastInsertedId;
            }
        }
    }
}