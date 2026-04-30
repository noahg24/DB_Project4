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

        // Therapist methods

        // Unpaid balance methods

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

        public List<string> GetSessionsByPatientId(string patientId)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT ps_sessid, ps_sessdate, ps_patid, ps_sessnotes, ps_theraid, ps_treatcode, ps_treatid
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
                    string patient = reader["ps_patid"].ToString() ?? "";
                    string notes = reader["ps_sessnotes"] == DBNull.Value ? "None" : reader["ps_sessnotes"].ToString() ?? "";
                    string therapist = reader["ps_theraid"].ToString() ?? "";
                    string treatCode = reader["ps_treatcode"].ToString() ?? "";
                    string treatId = reader["ps_treatid"].ToString() ?? "";

                    results.Add(
                        $"Session ID: {sessionId} | Date: {sessionDate} | Patient ID: {patient} | Notes: {notes} | Therapist: {therapist} | Treat Code: {treatCode} | Treatment ID: {treatId}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
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

        public string GetSingleSessionById(string sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT ps_sessid, ps_sessdate, ps_patid, ps_sessnotes, ps_theraid, ps_treatcode, ps_treatid
                    FROM PatientSession
                    WHERE ps_sessid = @sessionId
                    LIMIT 1;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string id = reader["ps_sessid"].ToString() ?? "";
                    string date = Convert.ToDateTime(reader["ps_sessdate"]).ToString("yyyy-MM-dd");
                    string patientId = reader["ps_patid"].ToString() ?? "";
                    string notes = reader["ps_sessnotes"] == DBNull.Value ? "None" : reader["ps_sessnotes"].ToString() ?? "";
                    string therapistId = reader["ps_theraid"].ToString() ?? "";
                    string treatCode = reader["ps_treatcode"].ToString() ?? "";
                    string treatId = reader["ps_treatid"].ToString() ?? "";

                    return $"Session ID: {id} | Date: {date} | Patient ID: {patientId} | Notes: {notes} | Therapist: {therapistId} | Treat Code: {treatCode} | Treatment ID: {treatId}";
                }

                return "";
            }
            catch (Exception ex)
            {
                return $"Database error: {ex.Message}";
            }
        }

        public bool UpdateSessionNotes(string sessionId, string newNotes, out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    UPDATE PatientSession
                    SET ps_sessnotes = @newNotes
                    WHERE ps_sessid = @sessionId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@newNotes",
                    string.IsNullOrWhiteSpace(newNotes) ? DBNull.Value : newNotes);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Session notes updated successfully.";
                    return true;
                }

                message = "No session found with that ID.";
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

        public List<string> GetAllSessionsForPatient(string patientId)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT ps_sessid, ps_sessdate, ps_theraid, ps_treatcode, ps_treatid
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
                    string treatId = reader["ps_treatid"].ToString() ?? "";

                    results.Add(
                        $"Session ID: {sessionId} | Date: {sessionDate} | Therapist: {therapistId} | Treat Code: {treatCode} | Treatment ID: {treatId}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public bool SessionBelongsToPatient(string sessionId, string patientId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM PatientSession
                    WHERE ps_sessid = @sessionId
                    AND ps_patid = @patientId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);
                command.Parameters.AddWithValue("@patientId", patientId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAccountingTransactionsForSession(string sessionId)
        {
            List<string> results = new List<string>();

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT a_txid, a_sessid, a_txdate, a_baldue, a_amtcolld, a_payer
                    FROM Accounting
                    WHERE a_sessid = @sessionId
                    ORDER BY a_txdate DESC;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string txId = reader["a_txid"].ToString() ?? "";
                    string sessId = reader["a_sessid"].ToString() ?? "";
                    string txDate = Convert.ToDateTime(reader["a_txdate"]).ToString("yyyy-MM-dd");
                    decimal balDue = Convert.ToDecimal(reader["a_baldue"]);
                    decimal amtColld = Convert.ToDecimal(reader["a_amtcolld"]);
                    string payer = reader["a_payer"].ToString() ?? "";

                    results.Add(
                        $"Transaction ID: {txId} | Session ID: {sessId} | Date: {txDate} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Payer: {payer}"
                    );
                }
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public bool AccountingTransactionBelongsToSession(string transactionId, string sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT COUNT(*)
                    FROM Accounting
                    WHERE a_txid = @transactionId
                    AND a_sessid = @sessionId;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@transactionId", transactionId);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch
            {
                return false;
            }
        }

        public string GetSingleAccountingTransaction(string transactionId, string sessionId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT a_txid, a_sessid, a_txdate, a_baldue, a_amtcolld, a_payer
                    FROM Accounting
                    WHERE a_txid = @transactionId
                    AND a_sessid = @sessionId
                    LIMIT 1;";

                using var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@transactionId", transactionId);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    string txId = reader["a_txid"].ToString() ?? "";
                    string sessId = reader["a_sessid"].ToString() ?? "";
                    string txDate = Convert.ToDateTime(reader["a_txdate"]).ToString("yyyy-MM-dd");
                    decimal balDue = Convert.ToDecimal(reader["a_baldue"]);
                    decimal amtColld = Convert.ToDecimal(reader["a_amtcolld"]);
                    string payer = reader["a_payer"].ToString() ?? "";

                    return $"Transaction ID: {txId} | Session ID: {sessId} | Date: {txDate} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Payer: {payer}";
                }

                return "";
            }
            catch (Exception ex)
            {
                return $"Database error: {ex.Message}";
            }
        }

        public bool UpdateAccountingTransaction(
            string transactionId,
            string sessionId,
            decimal newBalanceDue,
            decimal newAmountCollected,
            string newPayer,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    UPDATE Accounting
                    SET a_baldue = @newBalanceDue,
                        a_amtcolld = @newAmountCollected,
                        a_payer = @newPayer
                    WHERE a_txid = @transactionId
                    AND a_sessid = @sessionId;";

                using var command = new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@newBalanceDue", newBalanceDue);
                command.Parameters.AddWithValue("@newAmountCollected", newAmountCollected);
                command.Parameters.AddWithValue("@newPayer", newPayer);
                command.Parameters.AddWithValue("@transactionId", transactionId);
                command.Parameters.AddWithValue("@sessionId", sessionId);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected == 1)
                {
                    message = "Accounting transaction updated successfully.";
                    return true;
                }

                message = "Accounting transaction was not updated.";
                return false;
            }
            catch (Exception ex)
            {
                message = $"Database error: {ex.Message}";
                return false;
            }
        }

        public string GetNextPatientSessionId()
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                string sql = @"
                    SELECT MAX(CAST(ps_sessid AS UNSIGNED))
                    FROM PatientSession;";

                using var command = new MySqlCommand(sql, connection);

                object? result = command.ExecuteScalar();

                int nextId = 1;

                if (result != null && result != DBNull.Value)
                {
                    nextId = Convert.ToInt32(result) + 1;
                }

                return nextId.ToString("D7");   // 7 digits padded with zeros
            }
            catch
            {
                return "0000001";
            }
        }

        // Insert Treatment methods
        public bool InsertTreatmentWithInitialSession(
            string therapistId,
            string patientId,
            DateTime startDate,
            DateTime? endDate,
            string treatCode,
            string sessionId,
            DateTime sessionDate,
            string sessionNotes,
            out string message)
        {
            message = "";

            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                using var transaction = connection.BeginTransaction();

                try
                {
                    string treatmentSql = @"
                        INSERT INTO Treatment
                            (tr_theraid, tr_patid, tr_startdate, tr_enddate, tr_treatcode)
                        VALUES
                            (@therapistId, @patientId, @startDate, @endDate, @treatCode);";

                    using var treatmentCommand = new MySqlCommand(treatmentSql, connection, transaction);

                    treatmentCommand.Parameters.AddWithValue("@therapistId", therapistId);
                    treatmentCommand.Parameters.AddWithValue("@patientId", patientId);
                    treatmentCommand.Parameters.AddWithValue("@startDate", startDate);
                    treatmentCommand.Parameters.AddWithValue("@endDate", endDate.HasValue ? endDate.Value : DBNull.Value);
                    treatmentCommand.Parameters.AddWithValue("@treatCode", treatCode);

                    treatmentCommand.ExecuteNonQuery();

                    int newTreatmentId = (int)treatmentCommand.LastInsertedId;

                    string sessionSql = @"
                        INSERT INTO PatientSession
                            (ps_sessid, ps_sessdate, ps_patid, ps_sessnotes, ps_theraid, ps_treatcode, ps_treatid)
                        VALUES
                            (@sessionId, @sessionDate, @patientId, @sessionNotes, @therapistId, @treatCode, @treatmentId);";

                    using var sessionCommand = new MySqlCommand(sessionSql, connection, transaction);

                    sessionCommand.Parameters.AddWithValue("@sessionId", sessionId);
                    sessionCommand.Parameters.AddWithValue("@sessionDate", sessionDate);
                    sessionCommand.Parameters.AddWithValue("@patientId", patientId);
                    sessionCommand.Parameters.AddWithValue("@sessionNotes",
                        string.IsNullOrWhiteSpace(sessionNotes) ? DBNull.Value : sessionNotes);
                    sessionCommand.Parameters.AddWithValue("@therapistId", therapistId);
                    sessionCommand.Parameters.AddWithValue("@treatCode", treatCode);
                    sessionCommand.Parameters.AddWithValue("@treatmentId", newTreatmentId);

                    sessionCommand.ExecuteNonQuery();

                    transaction.Commit();

                    message = $"Treatment and initial session added successfully. Treatment ID: {newTreatmentId}, Session ID: {sessionId}";
                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    message = $"Database error. Changes rolled back: {ex.Message}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                message = $"Database connection error: {ex.Message}";
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

        // Insurance Network methods
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
