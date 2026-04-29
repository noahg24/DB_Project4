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
            connectionString = "Server=localhost;Database=patient_portal;User ID=root;Password=Plmko272SQLROOT97!;";
        }

        public bool InsertPatient(
            string patientId,
            string patientName,
            DateTime dob,
            string insuranceName,
            string insurancePolicy,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    INSERT INTO Patient
                        (p_patid, p_patname, p_dob, p_insname, p_inspol)
                    VALUES
                        (@patientId, @patientName, @dob, @insuranceName, @insurancePolicy);";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@patientId", patientId);
                command.Parameters.AddWithValue("@patientName", patientName);
                command.Parameters.AddWithValue("@dob", dob);
                command.Parameters.AddWithValue("@insuranceName",
                    string.IsNullOrWhiteSpace(insuranceName) ? DBNull.Value : insuranceName);
                command.Parameters.AddWithValue("@insurancePolicy",
                    string.IsNullOrWhiteSpace(insurancePolicy) ? DBNull.Value : insurancePolicy);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Patient added successfully.";
                    return true;
                }

                message = "Patient was not added.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public bool DeletePatient(string patientId, out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    DELETE FROM Patient
                    WHERE p_patid = @patientId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@patientId", patientId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Patient deleted successfully.";
                    return true;
                }

                message = "No patient found with that ID.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
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
                    FROM Therapist
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

        public List<string> SearchPatientsByName(string patientName)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT p_patid, p_patname, p_dob, p_insname, p_inspol
                    FROM Patient
                    WHERE p_patname LIKE @patientName
                    ORDER BY p_patname;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@patientName", "%" + patientName + "%");

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string patientId = reader["p_patid"].ToString() ?? "";
                    string name = reader["p_patname"].ToString() ?? "";
                    string dob = Convert.ToDateTime(reader["p_dob"]).ToString("yyyy-MM-dd");
                    string insurance = reader["p_insname"] == DBNull.Value ? "None" : reader["p_insname"].ToString()!;
                    string policy = reader["p_inspol"] == DBNull.Value ? "None" : reader["p_inspol"].ToString()!;

                    results.Add(
                        $"ID: {patientId} | Name: {name} | DOB: {dob} | Insurance: {insurance} | Policy: {policy}"
                    );
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
                    FROM Accounting
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
                    FROM PatientSession, Accounting
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
                    FROM PatientSession, Accounting
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

        // Payments by WHO? methods
        public List<string> GetOutOfNetworkInsurancePayments()
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT *
                    FROM Accounting
                    WHERE a_payer NOT IN (
                            SELECT i_insname
                            FROM InsuranceNetwork
                        )
                    AND a_payer NOT IN (
                            SELECT p_patid
                            FROM Patient
                        );";

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(FormatAccountingRow(reader));
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public List<string> GetInNetworkInsurancePayments()
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT *
                    FROM Accounting
                    WHERE a_payer IN (
                            SELECT i_insname
                            FROM InsuranceNetwork
                        );";

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(FormatAccountingRow(reader));
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public List<string> GetSelfPayPayments()
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT *
                    FROM Accounting
                    WHERE a_payer IN (
                            SELECT p_patid
                            FROM Patient
                        );";

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    results.Add(FormatAccountingRow(reader));
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        private string FormatAccountingRow(MySqlDataReader reader)
        {
            string txId = reader["a_txid"].ToString() ?? "";
            string sessId = reader["a_sessid"].ToString() ?? "";
            string txDate = Convert.ToDateTime(reader["a_txdate"]).ToString("yyyy-MM-dd");
            decimal balDue = Convert.ToDecimal(reader["a_baldue"]);
            decimal amtColld = Convert.ToDecimal(reader["a_amtcolld"]);
            string payer = reader["a_payer"].ToString() ?? "";

            return $"Transaction ID: {txId} | Session ID: {sessId} | Date: {txDate} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Payer: {payer}";
        }
    }
}
