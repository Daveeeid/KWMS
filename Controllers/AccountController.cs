using KWMS.Data.Repositories;
using KWMS.Models;
using MySqlConnector;

namespace KWMS.Controllers
{
    /// <summary>Actions every logged-in user may perform on their OWN account.</summary>
    public class AccountController
    {
        private readonly UserRepository _users = new UserRepository();

        public OperationResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            // The user is taken from the session, never from a text box,
            // so nobody can change someone else's password.
            User? sessionUser = SessionManager.CurrentUser;
            if (sessionUser == null) return OperationResult.Fail("You must be logged in.");

            if (currentPassword.Length == 0)
                return OperationResult.Fail("Please enter your current password.");
            if (!InputValidator.IsValidPassword(newPassword))
                return OperationResult.Fail("The new password must be 8 to 72 characters with at least one letter and one digit.");
            if (newPassword != confirmPassword)
                return OperationResult.Fail("The two new passwords do not match.");
            if (newPassword == currentPassword)
                return OperationResult.Fail("The new password must be different from the current one.");

            try
            {
                // The session does not keep the hash, so load it again to verify the current password
                User? dbUser = _users.GetByUserName(sessionUser.UserName);

                if (dbUser == null || dbUser.UserID != sessionUser.UserID
                    || !PasswordHasher.Verify(currentPassword, dbUser.PasswordHash))
                {
                    return OperationResult.Fail("The current password is incorrect.");
                }

                bool saved = _users.UpdatePasswordHash(sessionUser.UserID, PasswordHasher.Hash(newPassword));

                return saved
                    ? OperationResult.Ok("Your password was changed.")
                    : OperationResult.Fail("Your password could not be changed.");
            }
            catch (MySqlException)
            {
                return OperationResult.Fail("Cannot reach the database. Is MySQL running in XAMPP?");
            }
        }
    }
}