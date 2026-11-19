using MySqlConnector;

namespace KWMS.Data
{
    /// <summary>
    /// Small helpers to read a column BY NAME, including columns that may be NULL.
    /// </summary>
    public static class DataReaderExtensions
    {
        public static string ReadString(this MySqlDataReader reader, string column) =>
            reader.GetString(reader.GetOrdinal(column));

        public static int ReadInt(this MySqlDataReader reader, string column) =>
            reader.GetInt32(reader.GetOrdinal(column));

        public static decimal ReadDecimal(this MySqlDataReader reader, string column) =>
            reader.GetDecimal(reader.GetOrdinal(column));

        public static DateTime ReadDate(this MySqlDataReader reader, string column) =>
            reader.GetDateTime(reader.GetOrdinal(column));

        public static TimeSpan ReadTime(this MySqlDataReader reader, string column) =>
            (TimeSpan)reader.GetValue(reader.GetOrdinal(column));

        public static string? ReadStringOrNull(this MySqlDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index) ? null : reader.GetString(index);
        }

        public static int? ReadIntOrNull(this MySqlDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index) ? null : reader.GetInt32(index);
        }

        public static DateTime? ReadDateOrNull(this MySqlDataReader reader, string column)
        {
            int index = reader.GetOrdinal(column);
            return reader.IsDBNull(index) ? null : reader.GetDateTime(index);
        }
    }
}