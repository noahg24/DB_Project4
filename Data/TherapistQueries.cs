using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
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
    }
}