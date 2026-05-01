using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
        private readonly string connectionString;

        public DatabaseManager()
        {
            // Will need to be updated with final database schema and credentials
            connectionString = "Server=localhost;Database=patient_portal;User ID=root;Password=Plmko272SQLROOT97!;";
        }

        public bool TestConnection(out string message)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();
                message = "Database connection successful.";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Database connection failed: {ex.Message}";
                return false;
            }
        }
        
        private List<string> RunStringQuery(string sql)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<string> cols = new List<string>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        cols.Add(reader[i].ToString() ?? "");
                    }

                    results.Add(string.Join(" | ", cols));
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        private List<string> RunStringQuery(string sql, DateTime startDate, DateTime endDate)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    List<string> cols = new List<string>();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        cols.Add(reader[i].ToString() ?? "");
                    }

                    results.Add(string.Join(" | ", cols));
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }
    }
}
