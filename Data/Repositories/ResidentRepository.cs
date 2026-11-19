using KWMS.Models;
using MySqlConnector;

namespace KWMS.Data.Repositories
{
    public class ResidentRepository : BaseRepository
    {
        private readonly UserRepository _userRepository = new UserRepository();

        private const string SelectAll =
            @"SELECT ResidentID, UserID, FullName, Phone, District, Sector, HouseAddress, HouseholdSize, CreatedAt
              FROM Residents ";

        private static Resident MapResident(MySqlDataReader r) => new Resident
        {
            ResidentID = r.ReadInt("ResidentID"),
            UserID = r.ReadInt("UserID"),
            FullName = r.ReadString("FullName"),
            Phone = r.ReadStringOrNull("Phone"),
            District = r.ReadString("District"),
            Sector = r.ReadString("Sector"),
            HouseAddress = r.ReadStringOrNull("HouseAddress"),
            HouseholdSize = r.ReadInt("HouseholdSize"),
            CreatedAt = r.ReadDate("CreatedAt")
        };

        public Resident? GetById(int residentId) =>
            GetOne(SelectAll + "WHERE ResidentID = @id;", MapResident, P("@id", residentId));

        public Resident? GetByUserId(int userId) =>
            GetOne(SelectAll + "WHERE UserID = @id;", MapResident, P("@id", userId));

        /// <summary>
        /// Creates the Users row AND the Residents row in ONE transaction.
        /// If either insert fails, neither is saved. Returns the new ResidentID.
        /// </summary>
        public int Register(User user, Resident resident)
        {
            // Rule 1: a resident account always has the RESIDENT role and no company
            user.Role = UserRole.Resident;
            user.CompanyID = null;
            user.AccountStatus = RecordStatus.Active;

            const string sql = @"INSERT INTO Residents (UserID, FullName, Phone, District, Sector, HouseAddress, HouseholdSize)
                                 VALUES (@userId, @fullName, @phone, @district, @sector, @houseAddress, @householdSize);";

            using (MySqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                using (MySqlTransaction transaction = connection.BeginTransaction())
                {
                    int userId = _userRepository.Insert(user, connection, transaction);

                    int residentId = ExecuteInsertIn(connection, transaction, sql,
                        P("@userId", userId), P("@fullName", resident.FullName), P("@phone", resident.Phone),
                        P("@district", resident.District), P("@sector", resident.Sector),
                        P("@houseAddress", resident.HouseAddress), P("@householdSize", resident.HouseholdSize));

                    transaction.Commit();
                    return residentId;
                }
            }
        }

        // The resident can only update their own row (ResidentID AND UserID must match)
        public bool UpdateProfile(Resident resident)
        {
            const string sql = @"UPDATE Residents
                                 SET FullName = @fullName, Phone = @phone, District = @district, Sector = @sector,
                                     HouseAddress = @houseAddress, HouseholdSize = @householdSize
                                 WHERE ResidentID = @residentId AND UserID = @userId;";
            return ExecuteUpdate(sql,
                P("@fullName", resident.FullName), P("@phone", resident.Phone),
                P("@district", resident.District), P("@sector", resident.Sector),
                P("@houseAddress", resident.HouseAddress), P("@householdSize", resident.HouseholdSize),
                P("@residentId", resident.ResidentID), P("@userId", resident.UserID)) > 0;
        }
    }
}