using KWMS.Data.Repositories;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text;

namespace KWMS.Controller
{
    internal class DatabaseTestController
    {
        private readonly DatabaseCheckRepository _repository = new DatabaseCheckRepository();


        private readonly Dictionary<string, string[]> _expectedColumns =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["Companies"] = new[]
                {
                    "CompanyID", "CompanyName", "RegistrationNumber",
                    "Phone", "Address", "Status", "CreatedAt"
                },
                ["Users"] = new[]
                {
                    "UserID", "CompanyID", "UserName", "Email",
                    "PasswordHash", "Role", "AccountStatus", "CreatedAt"
                }
            };





        public string RunConnectionTest()
        {
            StringBuilder report = new StringBuilder();

            try
            {

                string version = _repository.GetServerVersion();
                report.AppendLine("✔ Connected to KWMS_DB successfully.");
                report.AppendLine($"  Server version: {version}");
                report.AppendLine();


                var actualTables = _repository.GetColumnsOfTables("Companies", "Users");

                foreach (var expected in _expectedColumns)
                {
                    string tableName = expected.Key;

                    if (!actualTables.ContainsKey(tableName))
                    {
                        report.AppendLine($"✘ Table '{tableName}' was NOT found.");
                        continue;
                    }

                    List<string> actualColumns = actualTables[tableName];
                    List<string> missing = expected.Value
                        .Where(col => !actualColumns.Contains(col, StringComparer.OrdinalIgnoreCase))
                        .ToList();

                    if (missing.Count == 0)
                    {
                        report.AppendLine($"✔ Table '{tableName}' found with all expected columns.");
                    }
                    else
                    {
                        report.AppendLine($"✘ Table '{tableName}' is missing columns: {string.Join(", ", missing)}");
                    }

                    report.AppendLine($"  Actual columns: {string.Join(", ", actualColumns)}");
                    report.AppendLine();
                }
            }
            catch (MySqlException ex)
            {
                report.AppendLine("✘ Database error:");
                report.AppendLine(ex.Message);
                report.AppendLine();
                report.AppendLine("Check that MySQL is started in XAMPP, and that the");
                report.AppendLine("connection string in App.config is correct.");
            }
            catch (Exception ex)
            {
                report.AppendLine("✘ Unexpected error:");
                report.AppendLine(ex.Message);
            }

            return report.ToString();
        }
    }
}
