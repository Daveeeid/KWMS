using MySqlConnector;

namespace KWMS.Data
{
    public static class DbHelper
    {




        public static bool IsDuplicateKey(MySqlException ex) =>
            ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry;
    }
}