using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
        public List<string> GetAllInsuranceNetworkProviders()
        {
            return RunStringQuery(@"
                SELECT i_insname
                FROM InsuranceNetwork
                ORDER BY i_insname;");
        }

        public bool InsertInsuranceProvider(string insuranceName, out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    INSERT INTO InsuranceNetwork
                        (i_insname)
                    VALUES
                        (@insuranceName);";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@insuranceName", insuranceName);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Insurance provider added successfully.";
                    return true;
                }

                message = "Insurance provider was not added.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public bool DeleteInsuranceProvider(string insuranceName, out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    DELETE FROM InsuranceNetwork
                    WHERE i_insname = @insuranceName;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@insuranceName", insuranceName);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Insurance provider deleted successfully.";
                    return true;
                }

                message = "No insurance provider found with that name.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }
    }
}