using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
        static void UnpaidBalanceMenu()
        {
            bool inUnpaidMenu = true;

            while (inUnpaidMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Unpaid Balance Reports =======");
                Console.WriteLine("0. Return To Search Menu");
                Console.WriteLine("1. Lifetime Unpaid Balance");
                Console.WriteLine("2. Year-End Unpaid Balance");
                Console.WriteLine("3. Month-End Unpaid Balance");
                Console.WriteLine("4. Custom Date Range");
                Console.WriteLine("5. Patients With Unpaid Balances");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inUnpaidMenu = false;
                        break;
                    case "1":
                        ShowLifetimeUnpaidBalances();
                        break;
                    case "2":
                        ShowYearEndUnpaidBalances();
                        break;
                    case "3":
                        ShowMonthEndUnpaidBalances();
                        break;
                    case "4":
                        ShowCustomRangeUnpaidBalances();
                        break;
                    case "5":
                        ShowLifetimeUnpaidBalanceByPatient();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void ShowLifetimeUnpaidBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Lifetime Unpaid Balances =======");

            DisplaySearchResults(databaseManager.GetLifetimeUnpaidBalances());
        }

        static void ShowYearEndUnpaidBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Year-End Unpaid Balances =======");

            Console.Write("Enter Year: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (!int.TryParse(input, out int year))
            {
                Console.WriteLine("Invalid year.");
                Console.ReadLine();
                return;
            }

            DisplaySearchResults(databaseManager.GetYearEndUnpaidBalances(year));
        }

        static void ShowMonthEndUnpaidBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Month-End Unpaid Balances =======");

            Console.Write("Enter Year: ");
            string yearInput = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter Month (1-12): ");
            string monthInput = Console.ReadLine()?.Trim() ?? "";

            if (!int.TryParse(yearInput, out int year) ||
                !int.TryParse(monthInput, out int month) ||
                month < 1 || month > 12)
            {
                Console.WriteLine("Invalid input.");
                Console.ReadLine();
                return;
            }

            DisplaySearchResults(databaseManager.GetMonthEndUnpaidBalances(year, month));
        }

        static void ShowCustomRangeUnpaidBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Custom Date Range Unpaid Balance =======");

            Console.Write("Enter Start Date (YYYY-MM-DD): ");
            string startInput = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter End Date (YYYY-MM-DD): ");
            string endInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(startInput, out DateTime startDate) ||
                !DateTime.TryParse(endInput, out DateTime endDate))
            {
                Console.WriteLine("Invalid date format.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (endDate < startDate)
            {
                Console.WriteLine("End date cannot be before start date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            DisplaySearchResults(databaseManager.GetCustomRangeUnpaidBalances(startDate, endDate));
        }

        static void ShowLifetimeUnpaidBalanceByPatient()
        {
            Console.Clear();
            Console.WriteLine("======= Lifetime Unpaid Balance By Patient =======");

            var results = databaseManager.GetLifetimeUnpaidBalanceByPatient();
            DisplaySearchResults(results);
        }

        static void PaymentsMenu()
        {
            bool inPaymentsMenu = true;

            while (inPaymentsMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Payments Menu =======");
                Console.WriteLine("0. Return To Search Menu");
                Console.WriteLine("1. Out-Of-Network Insurance Payments");
                Console.WriteLine("2. In-Network Insurance Payments");
                Console.WriteLine("3. Self-Pay Payments");
                Console.WriteLine("4. Average Payment Delay");
                Console.WriteLine("5. Average Session Payoff Delay");
                Console.WriteLine("6. Revenue - Custom Date Range");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inPaymentsMenu = false;
                        break;
                    case "1":
                        ShowOutOfNetworkInsurancePayments();
                        break;
                    case "2":
                        ShowInNetworkInsurancePayments();
                        break;
                    case "3":
                        ShowSelfPayPayments();
                        break;
                    case "4":
                        ShowAveragePaymentDelay();
                        break;
                    case "5":
                        ShowAverageSessionPayoffDelay();
                        break;
                    case "6":
                        ShowRevenueCustomDateRange();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void ShowOutOfNetworkInsurancePayments()
        {
            Console.Clear();
            Console.WriteLine("======= Out-Of-Network Insurance Payments =======");

            var results = databaseManager.GetOutOfNetworkInsurancePayments();
            DisplaySearchResults(results);
        }

        static void ShowInNetworkInsurancePayments()
        {
            Console.Clear();
            Console.WriteLine("======= In-Network Insurance Payments =======");

            var results = databaseManager.GetInNetworkInsurancePayments();
            DisplaySearchResults(results);
        }

        static void ShowSelfPayPayments()
        {
            Console.Clear();
            Console.WriteLine("======= Self-Pay Payments =======");

            var results = databaseManager.GetSelfPayPayments();
            DisplaySearchResults(results);
        }

        static void ShowAveragePaymentDelay()
        {
            Console.Clear();
            Console.WriteLine("======= Average Payment Delay =======");
            DisplaySearchResults(databaseManager.GetAveragePaymentDelay());
        }

        static void ShowAverageSessionPayoffDelay()
        {
            Console.Clear();
            Console.WriteLine("======= Average Session Payoff Delay =======");
            DisplaySearchResults(databaseManager.GetAverageSessionPayoffDelay());
        }

        static void ShowRevenueCustomDateRange()
        {
            Console.Clear();
            Console.WriteLine("======= Revenue - Custom Date Range =======");

            Console.Write("Enter Start Date (YYYY-MM-DD): ");
            string startInput = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter End Date (YYYY-MM-DD): ");
            string endInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(startInput, out DateTime startDate) ||
                !DateTime.TryParse(endInput, out DateTime endDate))
            {
                Console.WriteLine("Invalid date format.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (endDate < startDate)
            {
                Console.WriteLine("End date cannot be before start date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            DisplaySearchResults(
                databaseManager.GetRevenueCustomDateRange(
                    startDate,
                    endDate));
        }

        // Update Accounting
        static void AccountingUpdateMenu()
        {
            bool inAccountingMenu = true;

            while (inAccountingMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Accounting Update Menu =======");
                Console.WriteLine("0. Return To Update Menu");
                Console.WriteLine("1. Add New Accounting Transaction");
                Console.WriteLine("2. Update Accounting Transaction Payment");
                Console.WriteLine("3. Update Accounting Transaction Details");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inAccountingMenu = false;
                        break;
                    case "1":
                        AddNewAccountingTransaction();
                        break;
                    case "2":
                        UpdateAccountingTransactionPayment();
                        break;
                    case "3":
                        UpdateAccountingTransactionInfo();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void AddNewAccountingTransaction()
        {
            Console.Clear();
            Console.WriteLine("======= Add New Accounting Transaction =======");

            Console.Write("Enter Patient ID: ");
            string patientId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(patientId))
            {
                Console.WriteLine("Patient ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.PatientExists(patientId))
            {
                Console.WriteLine("No patient found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Sessions for this patient:");
            var sessions = databaseManager.GetSessionsForPatient(patientId);

            if (sessions.Count == 0)
            {
                Console.WriteLine("No sessions found for this patient.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            foreach (string session in sessions)
            {
                Console.WriteLine(session);
            }

            Console.WriteLine();
            Console.Write("Enter Session ID for this transaction: ");
            string sessionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                Console.WriteLine("Session ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.SessionExists(sessionId))
            {
                Console.WriteLine("No session found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Transaction ID: ");
            string transactionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Console.WriteLine("Transaction ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Transaction Date (YYYY-MM-DD): ");
            string dateInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(dateInput, out DateTime transactionDate))
            {
                Console.WriteLine("Invalid date format.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Balance Due: ");
            string balanceInput = Console.ReadLine()?.Trim() ?? "";

            if (!decimal.TryParse(balanceInput, out decimal balanceDue))
            {
                Console.WriteLine("Invalid balance due amount.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Amount Collected: ");
            string collectedInput = Console.ReadLine()?.Trim() ?? "";

            if (!decimal.TryParse(collectedInput, out decimal amountCollected))
            {
                Console.WriteLine("Invalid amount collected.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Payer insurance name or patient ID: ");
            string payer = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(payer))
            {
                Console.WriteLine("Payer cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool success = databaseManager.InsertAccountingTransaction(
                transactionId,
                sessionId,
                transactionDate,
                balanceDue,
                amountCollected,
                payer,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void UpdateAccountingTransactionPayment()
        {
            Console.Clear();
            Console.WriteLine("======= Update Accounting Transaction Payment =======");

            Console.Write("Enter payer name or patient ID: ");
            string payer = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(payer))
            {
                Console.WriteLine("Payer cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Unpaid transactions for this payer:");

            List<string> transactions = databaseManager.GetUnpaidTransactionsByPayer(payer);

            if (transactions.Count == 0)
            {
                Console.WriteLine("No unpaid transactions found for that payer.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            foreach (string transaction in transactions)
            {
                Console.WriteLine(transaction);
            }

            Console.WriteLine();
            Console.Write("Enter Transaction ID to update: ");
            string transactionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Console.WriteLine("Transaction ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool found = databaseManager.AccountingTransactionExistsForPayerWithBalance(
                transactionId,
                payer,
                out decimal currentBalance,
                out string lookupMessage
            );

            if (!found)
            {
                Console.WriteLine(lookupMessage);
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Current Balance Due: {currentBalance:C}");
            Console.Write("Enter payment amount: ");
            string paymentInput = Console.ReadLine()?.Trim() ?? "";

            if (!decimal.TryParse(paymentInput, out decimal paymentAmount))
            {
                Console.WriteLine("Invalid payment amount.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (paymentAmount <= 0)
            {
                Console.WriteLine("Payment amount must be greater than 0.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (paymentAmount > currentBalance)
            {
                Console.WriteLine("Payment amount cannot exceed current balance due.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool success = databaseManager.ApplyAccountingPayment(
                transactionId,
                payer,
                paymentAmount,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void UpdateAccountingTransactionInfo()
        {
            Console.Clear();
            Console.WriteLine("======= Update Accounting Transaction =======");

            Console.Write("Enter Patient ID: ");
            string patientId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(patientId))
            {
                Console.WriteLine("Patient ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.PatientExists(patientId))
            {
                Console.WriteLine("No patient found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Sessions for this patient:");

            List<string> sessions = databaseManager.GetAllSessionsForPatient(patientId);

            if (sessions.Count == 0)
            {
                Console.WriteLine("No sessions found for this patient.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            foreach (string session in sessions)
            {
                Console.WriteLine(session);
            }

            Console.WriteLine();
            Console.Write("Enter Session ID: ");
            string sessionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                Console.WriteLine("Session ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.SessionBelongsToPatient(sessionId, patientId))
            {
                Console.WriteLine("That session does not belong to the selected patient.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Accounting transactions for this session:");

            List<string> transactions = databaseManager.GetAccountingTransactionsForSession(sessionId);

            if (transactions.Count == 0)
            {
                Console.WriteLine("No accounting transactions found for this session.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            foreach (string transaction in transactions)
            {
                Console.WriteLine(transaction);
            }

            Console.WriteLine();
            Console.Write("Enter Transaction ID to update: ");
            string transactionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(transactionId))
            {
                Console.WriteLine("Transaction ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.AccountingTransactionBelongsToSession(transactionId, sessionId))
            {
                Console.WriteLine("That transaction does not belong to the selected session.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Current Transaction Information:");
            string currentInfo = databaseManager.GetSingleAccountingTransaction(transactionId, sessionId);

            if (currentInfo.StartsWith("Database error:"))
            {
                Console.WriteLine(currentInfo);
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine(currentInfo);
            Console.WriteLine();
            Console.WriteLine("Only Balance Due, Amount Collected, and Payer can be changed.");
            Console.WriteLine();

            Console.Write("New Balance Due: ");
            string balanceInput = Console.ReadLine()?.Trim() ?? "";

            if (!decimal.TryParse(balanceInput, out decimal newBalanceDue))
            {
                Console.WriteLine("Invalid balance due amount.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (newBalanceDue < 0)
            {
                Console.WriteLine("Balance due cannot be negative.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("New Amount Collected: ");
            string collectedInput = Console.ReadLine()?.Trim() ?? "";

            if (!decimal.TryParse(collectedInput, out decimal newAmountCollected))
            {
                Console.WriteLine("Invalid amount collected.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (newAmountCollected < 0)
            {
                Console.WriteLine("Amount collected cannot be negative.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("New Payer insurance name or patient ID: ");
            string newPayer = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(newPayer))
            {
                Console.WriteLine("Payer cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool success = databaseManager.UpdateAccountingTransaction(
                transactionId,
                sessionId,
                newBalanceDue,
                newAmountCollected,
                newPayer,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);

            if (success)
            {
                Console.WriteLine();
                Console.WriteLine("Updated Transaction Information:");
                Console.WriteLine(databaseManager.GetSingleAccountingTransaction(transactionId, sessionId));
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}