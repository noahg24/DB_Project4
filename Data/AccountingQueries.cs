using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
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
            decimal totalOutstanding = 0;

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

                    decimal outstanding = balDue - amtColld;
                    totalOutstanding += outstanding;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Unpaid: {outstanding:C}"
                    );
                }

                results.Add("--------------------------------------------------");
                results.Add($"TOTAL UNPAID BALANCE: {totalOutstanding:C}");
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
            decimal totalOutstanding = 0;

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

                    decimal outstanding = balDue - amtColld;
                    totalOutstanding += outstanding;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Unpaid: {outstanding:C}"
                    );
                }

                results.Add("--------------------------------------------------");
                results.Add($"TOTAL UNPAID BALANCE FOR {year}: {totalOutstanding:C}");
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
            decimal totalOutstanding = 0;

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

                    decimal outstanding = balDue - amtColld;
                    totalOutstanding += outstanding;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Unpaid: {outstanding:C}"
                    );
                }

                results.Add("--------------------------------------------------");
                results.Add($"TOTAL UNPAID BALANCE FOR {month}/{year}: {totalOutstanding:C}");
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }

        public List<string> GetCustomRangeUnpaidBalances(DateTime startDate, DateTime endDate)
        {
            List<string> results = new List<string>();
            decimal totalOutstanding = 0;

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
                    decimal balDue = reader["balance_due"] != DBNull.Value ? Convert.ToDecimal(reader["balance_due"]) : 0;
                    decimal amtColld = reader["amount_collected"] != DBNull.Value ? Convert.ToDecimal(reader["amount_collected"]) : 0;

                    decimal outstanding = balDue - amtColld;
                    totalOutstanding += outstanding;

                    results.Add(
                        $"Session ID: {sessionId} | Balance Due: {balDue:C} | Amount Collected: {amtColld:C} | Unpaid: {outstanding:C}"
                    );
                }

                results.Add("--------------------------------------------------");
                results.Add($"COMBINED TOTAL UNPAID BALANCE: {totalOutstanding:C}");
            }
            catch (Exception ex)
            {
                results.Add($"Database error: {ex.Message}");
            }

            return results;
        }
    }
}