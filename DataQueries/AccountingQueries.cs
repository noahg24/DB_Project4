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
    }
}