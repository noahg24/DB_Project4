using System;
using System.Collections.Generic;
using MySqlConnector;

namespace EnterpriseSystemApp
{
    public partial class DatabaseManager
    {
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
    }
}