using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
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

        public List<string> GetPatientsWithInNetworkInsurance()
        {
            return RunStringQuery(@"
                SELECT p_patid,
                    p_patname,
                    p_insname,
                    p_inspol
                FROM Patient, InsuranceNetwork
                WHERE p_insname = i_insname
                ORDER BY p_patname;");
        }
    }
}