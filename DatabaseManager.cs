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

        // Patient management methods
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

        public bool UpdatePatientInfo(
            string patientId,
            string newName,
            string newInsuranceName,
            string newInsurancePolicy,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    UPDATE Patient
                    SET p_patname = @newName,
                        p_insname = @newInsuranceName,
                        p_inspol = @newInsurancePolicy
                    WHERE p_patid = @patientId;";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@patientId", patientId);
                command.Parameters.AddWithValue("@newName", newName);
                command.Parameters.AddWithValue("@newInsuranceName",
                    string.IsNullOrWhiteSpace(newInsuranceName) ? DBNull.Value : newInsuranceName);
                command.Parameters.AddWithValue("@newInsurancePolicy",
                    string.IsNullOrWhiteSpace(newInsurancePolicy) ? DBNull.Value : newInsurancePolicy);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Patient information updated successfully.";
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

        public string GetSinglePatientById(string patientId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT p_patid, p_patname, p_dob, p_insname, p_inspol
                    FROM Patient
                    WHERE p_patid = @patientId
                    LIMIT 1;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@patientId", patientId);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string id = reader["p_patid"].ToString() ?? "";
                    string name = reader["p_patname"].ToString() ?? "";
                    string dob = Convert.ToDateTime(reader["p_dob"]).ToString("yyyy-MM-dd");
                    string insurance =
                        reader["p_insname"] == DBNull.Value
                        ? "None"
                        : reader["p_insname"].ToString() ?? "";

                    string policy =
                        reader["p_inspol"] == DBNull.Value
                        ? "None"
                        : reader["p_inspol"].ToString() ?? "";

                    return $"ID: {id} | Name: {name} | DOB: {dob} | Insurance: {insurance} | Policy: {policy}";
                }

                return "";
            }
            catch (Exception ex)
            {
                return $"Database error: {ex.Message}";
            }
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

        public List<UnpaidBalanceResult> GetCustomRangeUnpaidBalances(
            DateTime startDate,
            DateTime endDate)
        {
            List<UnpaidBalanceResult> results = new List<UnpaidBalanceResult>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT ps_sessid AS session_id,
                        SUM(a_baldue) AS balance_due,
                        SUM(a_amtcolld) AS amount_collected
                    FROM PatientSession, Accounting
                    WHERE ps_sessdate >= @startDate
                    AND ps_sessdate <= @endDate
                    AND ps_sessid = a_sessid
                    GROUP BY ps_sessid
                    HAVING SUM(a_baldue) <> SUM(a_amtcolld)
                    ORDER BY ps_sessid;";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string sessionId = reader["session_id"].ToString() ?? "";

                    decimal balanceDue =
                        reader["balance_due"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["balance_due"]);

                    decimal amountCollected =
                        reader["amount_collected"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["amount_collected"]);

                    results.Add(
                        new UnpaidBalanceResult(
                            sessionId,
                            balanceDue,
                            amountCollected));
                }
            }
            catch (Exception ex)
            {
                results.Add(
                    new UnpaidBalanceResult(
                        $"Database error: {ex.Message}",
                        0,
                        0));
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

        // Treatment analysis methods
        public List<string> GetTreatmentCountPerPatient()
        {
            return RunStringQuery(@"
                SELECT tr_patid, p_patname, COUNT(*) AS treatment_count
                FROM Treatment, Patient
                WHERE tr_patid = p_patid
                GROUP BY tr_patid, p_patname
                ORDER BY treatment_count DESC;");
        }

        public List<string> GetTop5PatientsTreatments()
        {
            return RunStringQuery(@"
                SELECT tr_patid, p_patname, COUNT(*) AS treatment_count
                FROM Treatment, Patient
                WHERE tr_patid = p_patid
                GROUP BY tr_patid, p_patname
                ORDER BY treatment_count DESC
                LIMIT 5;");
        }

        public List<string> GetOngoingTreatments()
        {
            return RunStringQuery(@"
                SELECT DISTINCT tr_patid, p_patname
                FROM Treatment, Patient
                WHERE tr_enddate IS NULL
                    AND tr_patid = p_patid
                ORDER BY p_patname;");
        }

        public List<string> GetAverageTreatmentsPerPatient()
        {
            return RunStringQuery(@"
                SELECT AVG(treatment_count) AS avg_treatments_per_patient
                FROM (
                    SELECT tr_patid, COUNT(*) AS treatment_count
                    FROM Treatment
                    GROUP BY tr_patid
                ) t1;");
        }

        public List<string> GetPeakMonthsForTreatment()
        {
            return RunStringQuery(@"
                SELECT DATE_FORMAT(tr_startdate, '%b %Y') AS month,
                    COUNT(*) AS treatments_started
                FROM Treatment
                GROUP BY YEAR(tr_startdate), MONTH(tr_startdate)
                ORDER BY treatments_started DESC;");
        }

        public List<string> GetIncompleteTreatments()
        {
            return RunStringQuery(@"
                SELECT DISTINCT t1.tr_patid, p_patname
                FROM Treatment t1, Treatment t2, Patient p
                WHERE t1.tr_patid = t2.tr_patid
                AND t1.tr_treatid <> t2.tr_treatid
                AND t1.tr_startdate < IFNULL(t2.tr_enddate, CURDATE())
                AND t2.tr_startdate < t1.tr_startdate
                AND t1.tr_patid = p.p_patid;");
        }

        public List<string> GetPatientsNeverPursuedTreatment()
        {
            return RunStringQuery(@"
                SELECT p_patid, p_patname
                FROM Patient
                WHERE p_patid NOT IN (
                    SELECT ps_patid
                    FROM PatientSession
                );");
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
    }
}
