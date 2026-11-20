using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;

namespace KWMS.Data.Repositories
{





    public abstract class BaseRepository
    {

        protected static MySqlParameter P(string name, object? value) =>
            new MySqlParameter(name, value ?? DBNull.Value);


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


        protected T? GetOne<T>(string sql, Func<MySqlDataReader, T> map, params MySqlParameter[] parameters) where T : class
        {
            return GetList(sql, map, parameters).FirstOrDefault();
        }


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