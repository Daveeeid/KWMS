using KWMS.Data;
using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{

    public class CompanyController
    {
        private readonly CompanyRepository _companies = new CompanyRepository();
        private readonly UserRepository _users = new UserRepository();

        private const string NoPermission = "You do not have permission to perform this action.";
        private const string DatabaseDown = "Cannot reach the database. Is MySQL running in XAMPP?";

        private static bool IsAdmin() => SessionManager.IsInRole(UserRole.Admin);




        public DataResult<List<Company>> GetCompanies(string? searchText, RecordStatus? status)
        {
            if (!IsAdmin()) return DataResult<List<Company>>.Fail(NoPermission);

            try
            {
                return DataResult<List<Company>>.Ok(_companies.GetAll(searchText?.Trim(), status));
            }
            catch (MySqlException)
            {
                return DataResult<List<Company>>.Fail(DatabaseDown);
            }
        }




        public OperationResult RegisterCompany(Company company, string managerUserName, string managerEmail,
                                               string password, string confirmPassword)
        {
            if (!IsAdmin()) return OperationResult.Fail(NoPermission);

            CleanCompany(company);
            managerUserName = managerUserName.Trim();
            managerEmail = managerEmail.Trim();

            List<string> errors = ValidateCompany(company);

            if (!InputValidator.IsValidUserName(managerUserName))
                errors.Add("Manager username must be 3 to 30 characters: letters, digits or underscore only.");
            if (!InputValidator.IsValidEmail(managerEmail))
                errors.Add("Please enter a valid manager e-mail address.");
            if (!InputValidator.IsValidPassword(password))
                errors.Add("Manager password must be 8 to 72 characters with at least one letter and one digit.");
            else if (password != confirmPassword)
                errors.Add("The two passwords do not match.");

            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            try
            {

                if (_companies.RegistrationNumberExists(company.RegistrationNumber))
                    errors.Add("A company with this registration number already exists.");
                if (_users.UserNameExists(managerUserName))
                    errors.Add("That manager username is already taken.");
                if (_users.EmailExists(managerEmail))
                    errors.Add("That manager e-mail address is already registered.");

                if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

                User manager = new User
                {
                    UserName = managerUserName,
                    Email = managerEmail,
                    PasswordHash = PasswordHasher.Hash(password)
                };

                _companies.RegisterWithManager(company, manager);

                return OperationResult.Ok(
                    $"Company '{company.CompanyName}' was registered with manager account '{managerUserName}'.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail(
                    "The registration number, manager username or e-mail was just used by someone else. Nothing was saved.");
            }
            catch (MySqlException ex)
            {
                return OperationResult.Fail("Database error. Nothing was saved.\n\n" + ex.Message);
            }
        }




        public OperationResult UpdateCompany(Company company)
        {
            if (!IsAdmin()) return OperationResult.Fail(NoPermission);

            CleanCompany(company);
            List<string> errors = ValidateCompany(company);
            if (errors.Count > 0) return OperationResult.Fail(ValidationHelper.FormatErrors(errors));

            try
            {

                if (_companies.RegistrationNumberExists(company.RegistrationNumber, company.CompanyID))
                    return OperationResult.Fail("Another company already uses this registration number.");

                if (!_companies.Update(company))
                    return OperationResult.Fail("The company could not be found. It may have been removed.");

                return OperationResult.Ok($"Company '{company.CompanyName}' was updated.");
            }
            catch (MySqlException ex) when (DbHelper.IsDuplicateKey(ex))
            {
                return OperationResult.Fail("Another company already uses this registration number.");
            }
            catch (MySqlException)
            {
                return OperationResult.Fail(DatabaseDown);
            }
        }




        public OperationResult SetCompanyStatus(int companyId, RecordStatus status)
        {
            if (!IsAdmin()) return OperationResult.Fail(NoPermission);

            try
            {
                if (!_companies.SetStatus(companyId, status))
                    return OperationResult.Fail("The company could not be found.");

                return OperationResult.Ok(status == RecordStatus.Active
                    ? "The company is now active."
                    : "The company was deactivated. It will not accept new subscriptions.");
            }
            catch (MySqlException)
            {
                return OperationResult.Fail(DatabaseDown);
            }
        }




        private static void CleanCompany(Company company)
        {
            company.CompanyName = company.CompanyName.Trim();
            company.RegistrationNumber = company.RegistrationNumber.Trim();
            company.Phone = ValidationHelper.NullIfEmpty(company.Phone);
            company.Address = ValidationHelper.NullIfEmpty(company.Address);
        }

        private static List<string> ValidateCompany(Company company)
        {
            List<string> errors = new List<string>();

            if (company.CompanyName.Length < 2 || company.CompanyName.Length > 100)
                errors.Add("Company name must be 2 to 100 characters.");
            if (company.RegistrationNumber.Length < 3 || company.RegistrationNumber.Length > 50)
                errors.Add("Registration number must be 3 to 50 characters.");
            if (company.Phone != null && (!InputValidator.IsValidPhone(company.Phone) || company.Phone.Length > 20))
                errors.Add("Phone must contain 9 to 15 digits (optionally starting with +), at most 20 characters.");
            if (company.Address != null && company.Address.Length > 150)
                errors.Add("Address must be at most 150 characters.");

            return errors;
        }
    }
}