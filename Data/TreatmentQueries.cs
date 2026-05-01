using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
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
    }
}