using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public class DatabaseManager
    {
        private readonly string connectionString;

        public DatabaseManager()
        {
            // Will need to be updated with final database schema and credentials
            connectionString = "Server=localhost;Database=test_schema;User ID=root;Password=Plmko272SQLROOT97!;";
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

        public List<string> SearchTherapistsByName(string name)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT t_theraid, t_theraname
                    FROM therapist
                    WHERE t_theraname LIKE @name
                    ORDER BY t_theraname;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", "%" + name + "%");

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string therapistId = reader["t_theraid"].ToString() ?? "";
                    string therapistName = reader["t_theraname"].ToString() ?? "";

                    results.Add($"ID: {therapistId} | Name: {therapistName}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        // Unpaid balance methods
        public List<UnpaidBalanceResult> GetLifetimeUnpaidBalances()
        {
            List<UnpaidBalanceResult> results = new List<UnpaidBalanceResult>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT a_sessid,
                           SUM(a_baldue) AS bal_due,
                           SUM(a_amtcolld) AS amt_colld
                    FROM accounting
                    GROUP BY a_sessid
                    HAVING SUM(a_baldue) <> SUM(a_amtcolld)
                    ORDER BY a_sessid;";

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string sessionId = reader["a_sessid"].ToString() ?? "";
                    decimal balanceDue = reader["bal_due"] != DBNull.Value ? Convert.ToDecimal(reader["bal_due"]) : 0;
                    decimal amountCollected = reader["amt_colld"] != DBNull.Value ? Convert.ToDecimal(reader["amt_colld"]) : 0;

                    results.Add(new UnpaidBalanceResult(sessionId, balanceDue, amountCollected));
                }
            }
            catch (Exception ex)
            {
                results.Add(new UnpaidBalanceResult($"Database error: {ex.Message}", 0, 0));
            }

            return results;
        }

        public List<UnpaidBalanceResult> GetYearEndUnpaidBalances(int year)
        {
            List<UnpaidBalanceResult> results = new List<UnpaidBalanceResult>();

            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = startDate.AddYears(1);

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT s_sessid AS session_id,
                        SUM(a_baldue) AS balance_due,
                        SUM(a_amtcolld) AS amount_collected
                    FROM therapy_session, accounting
                    WHERE s_sessdate >= @startDate
                    AND s_sessdate < @endDate
                    AND s_sessid = a_sessid
                    GROUP BY s_sessid
                    HAVING SUM(a_baldue) <> SUM(a_amtcolld)
                    ORDER BY s_sessid;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string sessionId = reader["session_id"].ToString() ?? "";
                    decimal balanceDue = reader["balance_due"] != DBNull.Value ? Convert.ToDecimal(reader["balance_due"]) : 0;
                    decimal amountCollected = reader["amount_collected"] != DBNull.Value ? Convert.ToDecimal(reader["amount_collected"]) : 0;

                    results.Add(new UnpaidBalanceResult(sessionId, balanceDue, amountCollected));
                }
            }
            catch (Exception ex)
            {
                results.Add(new UnpaidBalanceResult($"Database error: {ex.Message}", 0, 0));
            }

            return results;
        }

        public List<UnpaidBalanceResult> GetMonthEndUnpaidBalances(int year, int month)
        {
            List<UnpaidBalanceResult> results = new List<UnpaidBalanceResult>();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1);

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT s_sessid AS session_id,
                        SUM(a_baldue) AS balance_due,
                        SUM(a_amtcolld) AS amount_collected
                    FROM therapy_session, accounting
                    WHERE s_sessdate >= @startDate
                    AND s_sessdate < @endDate
                    AND s_sessid = a_sessid
                    GROUP BY s_sessid
                    HAVING SUM(a_baldue) <> SUM(a_amtcolld)
                    ORDER BY s_sessid;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string sessionId = reader["session_id"].ToString() ?? "";
                    decimal balanceDue = reader["balance_due"] != DBNull.Value ? Convert.ToDecimal(reader["balance_due"]) : 0;
                    decimal amountCollected = reader["amount_collected"] != DBNull.Value ? Convert.ToDecimal(reader["amount_collected"]) : 0;

                    results.Add(new UnpaidBalanceResult(sessionId, balanceDue, amountCollected));
                }
            }
            catch (Exception ex)
            {
                results.Add(new UnpaidBalanceResult($"Database error: {ex.Message}", 0, 0));
            }

            return results;
        }
    }
}
