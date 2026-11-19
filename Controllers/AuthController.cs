using KWMS.Data;
using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    /// <summary>Handles login, resident registration and first-run admin creation.</summary>
    public class AuthController
    {
        private readonly UserRepository _users = new UserRepository();
        private readonly ResidentRepository _residents = new ResidentRepository();

        // The three districts of Kigali City. The Register form fills its drop-down from this list.
        public static readonly string[] KigaliDistricts = { "Gasabo", "Kicukiro", "Nyarugenge" };

        private const string DefaultAdminUserName = "admin";
        private const string DefaultAdminEmail = "admin@kwms.local";
        private const string DefaultAdminPassword = "Admin@12345";   // TEMPORARY, documented in the README

        // ---------------------------------------------------------------
        // First-run setup
        // ---------------------------------------------------------------
        /// <summary>
        /// Called once at startup. If no ADMIN exists, creates one.
        /// Result.Message is empty when nothing was created.
        /// </summary>
        public OperationResult EnsureDefaultAdmin()
        {
            try
            {
                if (_users.AnyAdminExists()) return OperationResult.Ok();

                User admin = new User
                {
                    CompanyID = null,
                    UserName = DefaultAdminUserName,
                    Email = DefaultAdminEmail,
                    PasswordHash = PasswordHasher.Hash(DefaultAdminPassword),
                    Role = UserRole.Admin,
                    AccountStatus = RecordStatus.Active
                };
                _users.Insert(admin);

                return OperationResult.Ok(
                    "First-time setup: an administrator account was created.\n\n" +
                    $"Username: {DefaultAdminUserName}\nPassword: {DefaultAdminPassword}\n\n" +
                    "This is a temporary password for development and demonstration.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail(
                    "Could not create the default admin because the user name 'admin' or the e-mail " +
                    "'admin@kwms.local' is already used by a non-admin account.");
            }
            catch (MySqlException ex)
            {
                return OperationResult.Fail("Cannot reach the database. Is MySQL running in XAMPP?\n\n" + ex.Message);
            }
        }

        // ---------------------------------------------------------------
        // Login
        // ---------------------------------------------------------------
        public OperationResult Login(string userName, string password)
        {
            userName = userName.Trim();

            if (userName.Length == 0 || password.Length == 0)
                return OperationResult.Fail("Please enter your username and password.");

            try
            {
                User? user = _users.GetByUserName(userName);

                // Same message for "no such user" and "wrong password", so an attacker
                // cannot discover which usernames exist.
                if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
                    return OperationResult.Fail("Invalid username or password.");

                // The status is checked only AFTER the password is correct (same reason).
                if (!user.IsActive)
                    return OperationResult.Fail("This account is inactive. Please contact your administrator.");

                // Rule 5 depends on this: managers and workers must have a company
                if (user.RequiresCompany && user.CompanyID == null)
                    return OperationResult.Fail("This account is not linked to a company. Please contact your administrator.");

                SessionManager.Start(user);
                return OperationResult.Ok();
            }
            catch (MySqlException)
            {
                return OperationResult.Fail("Cannot reach the database. Is MySQL running in XAMPP?");
            }
            catch (InvalidDataException)
            {
                return OperationResult.Fail("This account has no valid role. Please contact your administrator.");
            }
        }

        // ---------------------------------------------------------------
        // Resident registration
        // ---------------------------------------------------------------
        public OperationResult RegisterResident(string userName, string email, string password,
                                                string confirmPassword, Resident resident)
        {
            // Clean the input first
            userName = userName.Trim();
            email = email.Trim();
            resident.FullName = resident.FullName.Trim();
            resident.Sector = resident.Sector.Trim();
            resident.Phone = NullIfEmpty(resident.Phone);
            resident.HouseAddress = NullIfEmpty(resident.HouseAddress);

            // Validate everything and collect ALL problems, so the user can fix them in one go
            List<string> errors = new List<string>();

            if (resident.FullName.Length < 2 || resident.FullName.Length > 100)
                errors.Add("Full name must be 2 to 100 characters.");
            if (!InputValidator.IsValidUserName(userName))
                errors.Add("Username must be 3 to 30 characters: letters, digits or underscore only.");
            if (!InputValidator.IsValidEmail(email))
                errors.Add("Please enter a valid e-mail address.");
            if (resident.Phone != null && !InputValidator.IsValidPhone(resident.Phone))
                errors.Add("Phone must contain 9 to 15 digits (optionally starting with +).");
            if (!KigaliDistricts.Contains(resident.District))
                errors.Add("Please select your district.");
            if (resident.Sector.Length < 2 || resident.Sector.Length > 50)
                errors.Add("Sector must be 2 to 50 characters.");
            if (resident.HouseAddress != null && resident.HouseAddress.Length > 150)
                errors.Add("House address must be at most 150 characters.");
            if (resident.HouseholdSize < 1)
                errors.Add("Household size must be at least 1.");
            if (!InputValidator.IsValidPassword(password))
                errors.Add("Password must be 8 to 72 characters with at least one letter and one digit.");
            else if (password != confirmPassword)
                errors.Add("The two passwords do not match.");

            if (errors.Count > 0) return OperationResult.Fail(FormatErrors(errors));

            try
            {
                // Friendly duplicate checks (the database UNIQUE constraints are the safety net below)
                if (_users.UserNameExists(userName)) errors.Add("That username is already taken.");
                if (_users.EmailExists(email)) errors.Add("That e-mail address is already registered.");
                if (errors.Count > 0) return OperationResult.Fail(FormatErrors(errors));

                User user = new User
                {
                    UserName = userName,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password)   // the plain password is never stored
                };

                // Creates the Users row AND the Residents row in one transaction (Stage 3)
                _residents.Register(user, resident);
                return OperationResult.Ok("Your account was created. You can now log in.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail("That username or e-mail was just registered by someone else. Please choose another.");
            }
            catch (MySqlException ex)
            {
                return OperationResult.Fail("Database error. Your account was not created.\n\n" + ex.Message);
            }
        }

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string FormatErrors(List<string> errors) =>
            string.Join(Environment.NewLine, errors.Select(e => "• " + e));
    }
}