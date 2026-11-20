using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{

    public class UserController
    {
        private readonly UserRepository _users = new UserRepository();

        public DataResult<List<User>> GetUsers(string? searchText, UserRole? role)
        {
            if (!SessionManager.IsInRole(UserRole.Admin))
                return DataResult<List<User>>.Fail("You do not have permission to view users.");

            try
            {

                return DataResult<List<User>>.Ok(_users.GetAll(searchText?.Trim(), role));
            }
            catch (MySqlException)
            {
                return DataResult<List<User>>.Fail("Cannot reach the database. Is MySQL running in XAMPP?");
            }
            catch (InvalidDataException)
            {
                return DataResult<List<User>>.Fail("At least one user in the database has no valid role. Please check the Users table.");
            }
        }
    }
}