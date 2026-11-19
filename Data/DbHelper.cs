using MySqlConnector;

namespace KWMS.Data
{
    public static class DbHelper
    {
        /// <summary>
        /// True when MySQL rejected a statement because of a UNIQUE violation (error 1062).
        /// Controllers use this to show a friendly message instead of a raw database error.
        /// </summary>
        public static bool IsDuplicateKey(MySqlException ex) =>
            ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry;
    }
}