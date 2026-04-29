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

        // Therapist methods
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

        public List<string> GetTreatmentCountPerTherapist(DateTime startDate, DateTime endDate)
        {
            return RunStringQuery(@"
                SELECT tr_theraid, t_theraname, COUNT(*) AS treatment_count
                FROM Treatment, Therapist
                WHERE tr_theraid = t_theraid
                AND tr_startdate >= @startDate
                AND tr_startdate <= @endDate
                GROUP BY tr_theraid, t_theraname
                ORDER BY treatment_count DESC;",
                startDate,
                endDate);
        }

        public List<string> GetOpenTreatmentCasesByTherapist()
        {
            return RunStringQuery(@"
                SELECT tr_theraid, t_theraname, COUNT(*) AS treatment_count
                FROM Treatment, Therapist
                WHERE tr_theraid = t_theraid
                AND tr_enddate IS NULL
                GROUP BY tr_theraid, t_theraname
                ORDER BY treatment_count DESC;");
        }

        public List<string> GetAverageSessionsPerTherapist()
        {
            return RunStringQuery(@"
                SELECT AVG(session_count) AS avg_sess_per_thera
                FROM (
                    SELECT COUNT(*) AS session_count
                    FROM PatientSession
                    GROUP BY ps_theraid
                ) t1;");
        }

        public List<string> GetAverageSkillsPerTherapist()
        {
            return RunStringQuery(@"
                SELECT AVG(num_skills) AS avg_num_skills
                FROM (
                    SELECT COUNT(*) AS num_skills
                    FROM TherapistSkills
                    GROUP BY ts_theraid
                ) t1;");
        }

        public List<string> GetTreatmentCodeUsage()
        {
            return RunStringQuery(@"
                SELECT tr_treatcode, COUNT(*) AS usage_count
                FROM Treatment
                GROUP BY tr_treatcode
                ORDER BY usage_count DESC;");
        }

        public List<string> GetTherapistsPerTreatment()
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT ps_treatid,
                        COUNT(DISTINCT ps_theraid) AS num_therapists,
                        COUNT(ps_theraid) AS num_sessions
                    FROM PatientSession
                    GROUP BY ps_treatid
                    ORDER BY num_therapists DESC;";

                using var command = new MySqlCommand(sql, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string treatId = reader["ps_treatid"].ToString() ?? "";
                    string therapistCount = reader["num_therapists"].ToString() ?? "";
                    string sessionCount = reader["num_sessions"].ToString() ?? "";

                    results.Add(
                        $"Treatment ID: {treatId} | Therapists Used: {therapistCount} | Total Sessions: {sessionCount}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public List<string> GetAverageTherapistsPerTreatment()
        {
            return RunStringQuery(@"
                SELECT AVG(therapist_count) AS avg_therapists_per_treatment
                FROM (
                    SELECT ps_treatid, COUNT(DISTINCT ps_theraid) AS therapist_count
                    FROM PatientSession
                    GROUP BY ps_treatid
                    HAVING COUNT(DISTINCT ps_theraid) > 1
                ) t1;");
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

        public List<string> GetPatientsWithOutstandingBalances()
        {
            return RunStringQuery(@"
                SELECT p_patid,
                    p_patname,
                    SUM(a_baldue - a_amtcolld) AS balance_due
                FROM Patient, Accounting
                WHERE p_patid = a_payer
                GROUP BY p_patid, p_patname
                HAVING SUM(a_baldue - a_amtcolld) > 0
                ORDER BY balance_due DESC;");
        }

        public List<string> GetPatientsWithMostSessions()
        {
            return RunStringQuery(@"
                SELECT p_patid,
                    p_patname,
                    COUNT(*) AS num_sessions
                FROM Patient, PatientSession
                WHERE p_patid = ps_patid
                GROUP BY p_patid, p_patname
                ORDER BY num_sessions DESC;");
        }

        public List<string> GetPatientsCurrentlyInTreatment()
        {
            return RunStringQuery(@"
                SELECT DISTINCT p_patid,
                                p_patname
                FROM Patient, Treatment
                WHERE p_patid = tr_patid
                AND tr_enddate IS NULL
                ORDER BY p_patname;");
        }

        public List<string> GetPatientsSeenByMultipleTherapists()
        {
            return RunStringQuery(@"
                SELECT p_patid,
                    p_patname,
                    COUNT(DISTINCT ps_theraid) AS therapist_count
                FROM Patient, PatientSession
                WHERE p_patid = ps_patid
                GROUP BY p_patid, p_patname
                HAVING COUNT(DISTINCT ps_theraid) > 1
                ORDER BY therapist_count DESC;");
        }

        public List<string> GetPatientsWithNoSessions()
        {
            return RunStringQuery(@"
                SELECT p_patid,
                    p_patname
                FROM Patient
                WHERE p_patid NOT IN (
                    SELECT ps_patid
                    FROM PatientSession
                )
                ORDER BY p_patname;");
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
        public List<string> GetLifetimeUnpaidBalanceByPatient()
        {
            return RunStringQuery(@"
                WITH balance AS (
                    SELECT
                        a.a_payer,
                        SUM(a.a_baldue) AS total_due,
                        SUM(a.a_amtcolld) AS total_collected,
                        SUM(a.a_baldue) - SUM(a.a_amtcolld) AS outstanding_balance
                    FROM Accounting a
                    GROUP BY a.a_payer
                )
                SELECT
                    p.p_patid,
                    p.p_patname,
                    p.p_insname,
                    p.p_inspol,
                    b.outstanding_balance,
                    CASE
                        WHEN LENGTH(b.a_payer) = 8 THEN 'SELF_PAY'
                        ELSE 'INSURANCE'
                    END AS payer_type
                FROM Patient p, balance b
                WHERE p.p_patid = b.a_payer
                AND b.outstanding_balance > 0
                ORDER BY b.outstanding_balance DESC;");
        }
        public List<string> GetLifetimeUnpaidBalances()
        {
            List<string> results = new List<string>();

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
                    decimal balDue = reader["bal_due"] != DBNull.Value ? Convert.ToDecimal(reader["bal_due"]) : 0;
                    decimal amtColld = reader["amt_colld"] != DBNull.Value ? Convert.ToDecimal(reader["amt_colld"]) : 0;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public List<string> GetYearEndUnpaidBalances(int year)
        {
            List<string> results = new List<string>();

            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = startDate.AddYears(1);

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
                    AND ps_sessdate < @endDate
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
                    decimal balDue = reader["balance_due"] != DBNull.Value ? Convert.ToDecimal(reader["balance_due"]) : 0;
                    decimal amtColld = reader["amount_collected"] != DBNull.Value ? Convert.ToDecimal(reader["amount_collected"]) : 0;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public List<string> GetMonthEndUnpaidBalances(int year, int month)
        {
            List<string> results = new List<string>();

            DateTime startDate = new DateTime(year, month, 1);
            DateTime endDate = startDate.AddMonths(1);

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
                    AND ps_sessdate < @endDate
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
                    decimal balDue = reader["balance_due"] != DBNull.Value ? Convert.ToDecimal(reader["balance_due"]) : 0;
                    decimal amtColld = reader["amount_collected"] != DBNull.Value ? Convert.ToDecimal(reader["amount_collected"]) : 0;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
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

        public List<string> GetAveragePaymentDelay()
        {
            return RunStringQuery(@"
                SELECT AVG(tx_delay) AS avg_payment_delay_days
                FROM (
                    SELECT DATEDIFF(MAX(a_txdate), ps_sessdate) AS tx_delay
                    FROM PatientSession, Accounting
                    WHERE ps_sessid = a_sessid
                    GROUP BY ps_sessid, ps_sessdate
                    HAVING SUM(a_baldue) = SUM(a_amtcolld)
                ) t1;");
        }

        public List<string> GetAverageSessionPayoffDelay()
        {
            return RunStringQuery(@"
                SELECT AVG(payoff_delay) AS avg_payoff_delay_days
                FROM (
                    SELECT ps_sessid,
                        DATEDIFF(MAX(a_txdate), MIN(a_txdate)) AS payoff_delay
                    FROM PatientSession, Accounting
                    WHERE ps_sessid = a_sessid
                    GROUP BY ps_sessid
                    HAVING SUM(a_baldue) - SUM(a_amtcolld) = 0
                ) t1;");
        }

        public List<string> GetRevenueCustomDateRange(DateTime startDate, DateTime endDate)
        {
            return RunStringQuery(@"
                SELECT SUM(a_amtcolld) AS total_collected
                FROM Accounting
                WHERE a_txdate >= @startDate
                AND a_txdate <= @endDate;",
                startDate,
                endDate);
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

        // CHECK HERE
        public List<string> GetPeakMonthsForTreatment()
        {
            return RunStringQuery(@"
                SELECT DATE_FORMAT(tr_startdate, '%b %Y') AS month,
                    COUNT(*) AS treatments_started
                FROM Treatment
                GROUP BY DATE_FORMAT(tr_startdate, '%b %Y'),
                        YEAR(tr_startdate),
                        MONTH(tr_startdate)
                ORDER BY treatments_started DESC,
                        YEAR(tr_startdate),
                        MONTH(tr_startdate);");
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

        // Checking Patient/Session methods
        public bool PatientExists(string patientId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Patient
                    WHERE p_patid = @patientId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@patientId", patientId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetSessionsForPatient(string patientId)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT ps_sessid, ps_sessdate, ps_theraid, ps_treatcode
                    FROM PatientSession
                    WHERE ps_patid = @patientId
                    ORDER BY ps_sessdate DESC;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@patientId", patientId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string sessionId = reader["ps_sessid"].ToString() ?? "";
                    string sessionDate = Convert.ToDateTime(reader["ps_sessdate"]).ToString("yyyy-MM-dd");
                    string therapistId = reader["ps_theraid"].ToString() ?? "";
                    string treatCode = reader["ps_treatcode"].ToString() ?? "";

                    results.Add($"Session ID: {sessionId} | Date: {sessionDate} | Therapist: {therapistId} | Treat Code: {treatCode}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public bool SessionExists(string sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM PatientSession
                    WHERE ps_sessid = @sessionId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        // Checking Therapist/Treatment methods
        public bool TherapistExists(string therapistId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Therapist
                    WHERE t_theraid = @therapistId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@therapistId", therapistId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool TreatmentCodeExists(string treatCode)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM SkillTreatcodeMap
                    WHERE st_treatcode = @treatCode;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@treatCode", treatCode);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetTreatmentsForPatient(string patientId)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT tr_treatid, tr_theraid, tr_patid, tr_startdate, tr_enddate, tr_treatcode
                    FROM Treatment
                    WHERE tr_patid = @patientId
                    ORDER BY tr_startdate DESC;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@patientId", patientId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string treatId = reader["tr_treatid"].ToString() ?? "";
                    string therapistId = reader["tr_theraid"].ToString() ?? "";
                    string startDate = Convert.ToDateTime(reader["tr_startdate"]).ToString("yyyy-MM-dd");
                    string endDate = reader["tr_enddate"] == DBNull.Value
                        ? "Ongoing"
                        : Convert.ToDateTime(reader["tr_enddate"]).ToString("yyyy-MM-dd");
                    string treatCode = reader["tr_treatcode"].ToString() ?? "";

                    results.Add($"Treatment ID: {treatId} | Therapist: {therapistId} | Start: {startDate} | End: {endDate} | Treat Code: {treatCode}");
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public bool TreatmentMatchesSessionInfo(
            int treatmentId,
            string patientId,
            string therapistId,
            string treatCode,
            DateTime sessionDate,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT tr_startdate, tr_enddate
                    FROM Treatment
                    WHERE tr_treatid = @treatmentId
                    AND tr_patid = @patientId
                    AND tr_theraid = @therapistId
                    AND tr_treatcode = @treatCode
                    LIMIT 1;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@treatmentId", treatmentId);
                command.Parameters.AddWithValue("@patientId", patientId);
                command.Parameters.AddWithValue("@therapistId", therapistId);
                command.Parameters.AddWithValue("@treatCode", treatCode);

                using var reader = command.ExecuteReader();

                if (!reader.Read())
                {
                    message = "No matching treatment found for that patient, therapist, and treatment code.";
                    return false;
                }

                DateTime startDate = Convert.ToDateTime(reader["tr_startdate"]);
                DateTime? endDate = reader["tr_enddate"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(reader["tr_enddate"]);

                if (sessionDate < startDate)
                {
                    message = "Session date cannot be before the treatment start date.";
                    return false;
                }

                if (endDate != null && sessionDate > endDate.Value)
                {
                    message = "Session date cannot be after the treatment end date.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public bool TherapistCanPerformTreatmentCode(string therapistId, string treatCode)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM TherapistSkills, SkillTreatcodeMap
                    WHERE ts_skillid = st_skillid
                    AND ts_theraid = @therapistId
                    AND st_treatcode = @treatCode;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@therapistId", therapistId);
                command.Parameters.AddWithValue("@treatCode", treatCode);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool SessionIdExists(string sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM PatientSession
                    WHERE ps_sessid = @sessionId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
            catch
            {
                return false;
            }
        }

        public bool InsertPatientSession(
            string sessionId,
            DateTime sessionDate,
            string patientId,
            string sessionNotes,
            string therapistId,
            string treatCode,
            int treatmentId,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    INSERT INTO PatientSession
                        (ps_sessid, ps_sessdate, ps_patid, ps_sessnotes, ps_theraid, ps_treatcode, ps_treatid)
                    VALUES
                        (@sessionId, @sessionDate, @patientId, @sessionNotes, @therapistId, @treatCode, @treatmentId);";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@sessionDate", sessionDate);
                command.Parameters.AddWithValue("@patientId", patientId);
                command.Parameters.AddWithValue("@sessionNotes",
                    string.IsNullOrWhiteSpace(sessionNotes) ? DBNull.Value : sessionNotes);
                command.Parameters.AddWithValue("@therapistId", therapistId);
                command.Parameters.AddWithValue("@treatCode", treatCode);
                command.Parameters.AddWithValue("@treatmentId", treatmentId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Session added successfully.";
                    return true;
                }

                message = "Session was not added.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        // Accounting methods
        public bool InsertAccountingTransaction(
            string transactionId,
            string sessionId,
            DateTime transactionDate,
            decimal balanceDue,
            decimal amountCollected,
            string payer,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    INSERT INTO Accounting
                        (a_txid, a_sessid, a_txdate, a_baldue, a_amtcolld, a_payer)
                    VALUES
                        (@transactionId, @sessionId, @transactionDate, @balanceDue, @amountCollected, @payer);";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@transactionId", transactionId);
                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@transactionDate", transactionDate);
                command.Parameters.AddWithValue("@balanceDue", balanceDue);
                command.Parameters.AddWithValue("@amountCollected", amountCollected);
                command.Parameters.AddWithValue("@payer", payer);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Accounting transaction added successfully.";
                    return true;
                }

                message = "Accounting transaction was not added.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public List<string> GetUnpaidTransactionsByPayer(string payer)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT a_txid, a_sessid, a_txdate, a_baldue, a_amtcolld, a_payer
                    FROM Accounting
                    WHERE a_payer = @payer
                    AND a_baldue > 0
                    ORDER BY a_txdate;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@payer", payer);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string txId = reader["a_txid"].ToString() ?? "";
                    string sessId = reader["a_sessid"].ToString() ?? "";
                    string txDate = Convert.ToDateTime(reader["a_txdate"]).ToString("yyyy-MM-dd");
                    decimal balDue = Convert.ToDecimal(reader["a_baldue"]);
                    decimal amtColld = Convert.ToDecimal(reader["a_amtcolld"]);
                    string payerValue = reader["a_payer"].ToString() ?? "";

                    results.Add(
                        $"Transaction ID: {txId} | Session ID: {sessId} | Date: {txDate} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Payer: {payerValue}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public bool AccountingTransactionExistsForPayerWithBalance(
            string transactionId,
            string payer,
            out decimal currentBalance,
            out string message)
        {
            currentBalance = 0;
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT a_baldue
                    FROM Accounting
                    WHERE a_txid = @transactionId
                    AND a_payer = @payer
                    AND a_baldue > 0
                    LIMIT 1;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@transactionId", transactionId);
                command.Parameters.AddWithValue("@payer", payer);

                object? result = command.ExecuteScalar();

                if (result == null)
                {
                    message = "No matching unpaid transaction found for that payer.";
                    return false;
                }

                currentBalance = Convert.ToDecimal(result);
                return true;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public bool ApplyAccountingPayment(
            string transactionId,
            string payer,
            decimal paymentAmount,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    UPDATE Accounting
                    SET a_baldue = a_baldue - @paymentAmount,
                        a_amtcolld = a_amtcolld + @paymentAmount
                    WHERE a_txid = @transactionId
                    AND a_payer = @payer
                    AND a_baldue >= @paymentAmount;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@paymentAmount", paymentAmount);
                command.Parameters.AddWithValue("@transactionId", transactionId);
                command.Parameters.AddWithValue("@payer", payer);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Payment applied successfully.";
                    return true;
                }

                message = "Payment was not applied. Check that the transaction exists and the payment does not exceed the balance due.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public bool InsertTreatment(
            string therapistId,
            string patientId,
            DateTime startDate,
            DateTime? endDate,
            string treatCode,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    INSERT INTO Treatment
                        (tr_theraid, tr_patid, tr_startdate, tr_enddate, tr_treatcode)
                    VALUES
                        (@therapistId, @patientId, @startDate, @endDate, @treatCode);";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@therapistId", therapistId);
                command.Parameters.AddWithValue("@patientId", patientId);
                command.Parameters.AddWithValue("@startDate", startDate);
                command.Parameters.AddWithValue("@endDate", endDate.HasValue ? endDate.Value : DBNull.Value);
                command.Parameters.AddWithValue("@treatCode", treatCode);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Treatment added successfully.";
                    return true;
                }

                message = "Treatment was not added.";
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
