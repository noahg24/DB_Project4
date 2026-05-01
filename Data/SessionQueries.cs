using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
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
    }
}