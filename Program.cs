using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using MySqlX.XDevAPI;

namespace EnterpriseSystemApp
{
    class Program
    {
        static AccountManager accountManager = new AccountManager();
        // DatabaseManager is initialized but not yet used in the application flow
        static DatabaseManager databaseManager = new DatabaseManager();

        static void Main(string[] args)
        {
            RunApplication();
        }

        static void RunApplication()
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                Console.WriteLine("================================");
                Console.WriteLine("        MyPatient System");
                Console.WriteLine("================================");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Create Account");
                Console.WriteLine("3. Exit");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        CreateAccount();
                        break;
                    case "3":
                        running = false;
                        Console.WriteLine("Exiting application...");
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void CreateAccount()
        {
            Console.Clear();
            Console.WriteLine("========== Create Account ==========");
            Console.Write("Enter a username: ");
            string username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Enter a password: ");
            string password = Console.ReadLine() ?? "";

            Console.Write("Enter admin key for admin, or 0 for base account: ");
            string roleInput = Console.ReadLine()?.Trim() ?? "";

            bool created = accountManager.CreateAccount(username, password, roleInput, out string message);

            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void Login()
        {
            Console.Clear();
            Console.WriteLine("============= Login =============");
            Console.Write("Username: ");
            string username = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Password: ");
            string password = Console.ReadLine() ?? "";

            Account? account = accountManager.ValidateLogin(username, password);

            if (account != null)
            {
                Console.WriteLine($"Login successful. Role: {account.Role}");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                MainMenu(account);
            }
            else
            {
                Console.WriteLine("Invalid username or password.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }
        }

        static void MainMenu(Account account)
        {
            bool loggedIn = true;

            while (loggedIn)
            {
                Console.Clear();
                Console.WriteLine("======================================");
                Console.WriteLine($" Welcome, {account.Username}");
                Console.WriteLine($" Role: {account.Role}");
                Console.WriteLine("======================================");
                Console.WriteLine("1. Search The Database");
                Console.WriteLine("2. Update The Database");
                Console.WriteLine("3. Logout");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        SearchDatabase();
                        break;
                    case "2":
                        UpdateDatabase(account);
                        break;
                    case "3":
                        loggedIn = false;
                        Console.WriteLine("Logging out...");
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void SearchDatabase()
        {
            bool inSearchMenu = true;

            while (inSearchMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Search The Database =======");
                Console.WriteLine("0. Return To Main Menu");
                Console.WriteLine("1. Test Database Connection");
                Console.WriteLine("2. Search Therapists");
                Console.WriteLine("3. Search Patients");
                Console.WriteLine("4. Unpaid Balance Reports");
                Console.WriteLine("5. Payments Menu");
                Console.WriteLine("6. Treatment Reports");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inSearchMenu = false;
                        break;
                    case "1":
                        TestDatabaseConnection();
                        break;
                    case "2":
                        TherapistSearchMenu();
                        break;
                    case "3":
                        PatientSearchMenu();
                        break;
                    case "4":
                        UnpaidBalanceMenu();
                        break;
                    case "5":
                        PaymentsMenu();
                        break;
                    case "6":
                        TreatmentMenu();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void TherapistSearchMenu()
        {
            bool inTherapistMenu = true;

            while (inTherapistMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Therapist Search Menu =======");
                Console.WriteLine("0. Return To Search Menu");
                Console.WriteLine("1. Search Therapists By Name");
                Console.WriteLine("2. Number of Treatments Per Therapist");
                Console.WriteLine("3. Open/Incomplete Treatment Cases By Therapist");
                Console.WriteLine("4. Average Sessions Per Therapist");
                Console.WriteLine("5. Average Number Of Skills Per Therapist");
                Console.WriteLine("6. Treatment Code Usage Count");
                Console.WriteLine("7. Number Of Therapists Per Treatment");
                Console.WriteLine("8. Average Number Of Therapists Per Treatment");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inTherapistMenu = false;
                        break;
                    case "1":
                        SearchTherapistsByName();
                        break;
                    case "2":
                        ShowTreatmentCountPerTherapist();
                        break;
                    case "3":
                        ShowOpenTreatmentCasesByTherapist();
                        break;
                    case "4":
                        ShowAverageSessionsPerTherapist();
                        break;
                    case "5":
                        ShowAverageSkillsPerTherapist();
                        break;
                    case "6":
                        ShowTreatmentCodeUsage();
                        break;
                    case "7":
                        ShowTherapistsPerTreatment();
                        break;
                    case "8":
                        ShowAverageTherapistsPerTreatment();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void PatientSearchMenu()
        {
            bool inPatientMenu = true;

            while (inPatientMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Patient Search Menu =======");
                Console.WriteLine("0. Return To Search Menu");
                Console.WriteLine("1. Search Patients By Name");
                Console.WriteLine("2. Patients With Outstanding Balances");
                Console.WriteLine("3. Patients With Most Sessions");
                Console.WriteLine("4. Patients Currently In Treatment");
                Console.WriteLine("5. Patients Seen By Multiple Therapists");
                Console.WriteLine("6. Patients With No Sessions");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inPatientMenu = false;
                        break;
                    case "1":
                        SearchPatientsByName();
                        break;
                    case "2":
                        ShowPatientsWithOutstandingBalances();
                        break;
                    case "3":
                        ShowPatientsWithMostSessions();
                        break;
                    case "4":
                        ShowPatientsCurrentlyInTreatment();
                        break;
                    case "5":
                        ShowPatientsSeenByMultipleTherapists();
                        break;
                    case "6":
                        ShowPatientsWithNoSessions();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void UpdateDatabase(Account account)
        {
            if (account.Role != "admin")
            {
                Console.Clear();
                Console.WriteLine("======= Update The Database =======");
                Console.WriteLine("Access denied. Only admin users can update the database.");
                Console.WriteLine("Press Enter to return to the main menu...");
                Console.ReadLine();
                return;
            }

            bool inUpdateMenu = true;

            while (inUpdateMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Update The Database =======");
                Console.WriteLine("0. Return To Main Menu");
                Console.WriteLine("1. Patient");
                Console.WriteLine("2. Session");
                Console.WriteLine("3. Accounting");
                Console.WriteLine("4. Treatment");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inUpdateMenu = false;
                        break;
                    case "1":
                        PatientUpdateMenu();
                        break;
                    case "2":
                        SessionUpdateMenu();
                        break;
                    case "3":
                        AccountingUpdateMenu();
                        break;
                    case "4":
                        TreatmentUpdateMenu();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void PatientUpdateMenu()
        {
            bool inPatientMenu = true;

            while (inPatientMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Patient Update Menu =======");
                Console.WriteLine("0. Return To Update Menu");
                Console.WriteLine("1. Add New Patient");
                Console.WriteLine("2. Delete Patient");
                Console.WriteLine("3. Update Patient Information");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inPatientMenu = false;
                        break;         
                    case "1":
                        AddNewPatient();
                        break;
                    case "2":
                        DeletePatient();
                        break;
                    case "3":
                        UpdatePatientInfo();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void SessionUpdateMenu()
        {
            bool inSessionMenu = true;

            while (inSessionMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Session Update Menu =======");
                Console.WriteLine("0. Return To Update Menu");
                Console.WriteLine("1. Add New Session");
                Console.WriteLine("2. Delete Session");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inSessionMenu = false;
                        break;
                    case "1":
                        AddNewSession();
                        break;
                    case "2":
                        Console.WriteLine("Delete Session functionality will be added later.");
                        Console.WriteLine("Press Enter to continue...");
                        Console.ReadLine();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

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
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void TreatmentUpdateMenu()
        {
            bool inTreatmentMenu = true;

            while (inTreatmentMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Treatment Update Menu =======");
                Console.WriteLine("1. Add New Treatment");
                Console.WriteLine("0. Return To Update Menu");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inTreatmentMenu = false;
                        break;
                    case "1":
                        AddNewTreatment();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        // Methods for database search options
        static void TestDatabaseConnection()
        {
            Console.Clear();
            Console.WriteLine("======= Test Database Connection =======");

            bool connected = databaseManager.TestConnection(out string message);
            Console.WriteLine(message);

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        // Therapist search options
        static void SearchTherapistsByName()
        {
            Console.Clear();
            Console.WriteLine("======= Search Therapists By Name =======");
            Console.Write("Enter therapist name or part of a name: ");
            string name = Console.ReadLine()?.Trim() ?? "";

            var results = databaseManager.SearchTherapistsByName(name);

            Console.WriteLine();

            if (results.Count == 0)
            {
                Console.WriteLine("No matching therapists found.");
            }
            else
            {
                foreach (string row in results)
                {
                    Console.WriteLine(row);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void ShowTreatmentCountPerTherapist()
        {
            Console.Clear();
            Console.WriteLine("======= Number Of Treatments Per Therapist =======");

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

            DisplaySearchResults(databaseManager.GetTreatmentCountPerTherapist(startDate, endDate));
        }

        static void ShowOpenTreatmentCasesByTherapist()
        {
            Console.Clear();
            Console.WriteLine("======= Open/Incomplete Treatment Cases By Therapist =======");
            DisplaySearchResults(databaseManager.GetOpenTreatmentCasesByTherapist());
        }

        static void ShowAverageSessionsPerTherapist()
        {
            Console.Clear();
            Console.WriteLine("======= Average Sessions Per Therapist =======");
            DisplaySearchResults(databaseManager.GetAverageSessionsPerTherapist());
        }

        static void ShowAverageSkillsPerTherapist()
        {
            Console.Clear();
            Console.WriteLine("======= Average Number Of Skills Per Therapist =======");
            DisplaySearchResults(databaseManager.GetAverageSkillsPerTherapist());
        }

        static void ShowTreatmentCodeUsage()
        {
            Console.Clear();
            Console.WriteLine("======= Treatment Code Usage Count =======");
            DisplaySearchResults(databaseManager.GetTreatmentCodeUsage());
        }

        static void ShowTherapistsPerTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Number Of Therapists Per Treatment =======");
            DisplaySearchResults(databaseManager.GetTherapistsPerTreatment());
        }

        static void ShowAverageTherapistsPerTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Average Number Of Therapists Per Treatment =======");
            DisplaySearchResults(databaseManager.GetAverageTherapistsPerTreatment());
        }

        // Patient search options
        static void SearchPatientsByName()
        {
            Console.Clear();
            Console.WriteLine("======= Search Patients By Name =======");

            Console.Write("Enter patient name or part of name: ");
            string patientName = Console.ReadLine()?.Trim() ?? "";

            var results = databaseManager.SearchPatientsByName(patientName);

            Console.WriteLine();

            if (results.Count == 0)
            {
                Console.WriteLine("No matching patients found.");
            }
            else
            {
                foreach (string row in results)
                {
                    Console.WriteLine(row);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void ShowPatientsWithOutstandingBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Patients With Outstanding Balances =======");
            DisplaySearchResults(databaseManager.GetPatientsWithOutstandingBalances());
        }

        static void ShowPatientsWithMostSessions()
        {
            Console.Clear();
            Console.WriteLine("======= Patients With Most Sessions =======");
            DisplaySearchResults(databaseManager.GetPatientsWithMostSessions());
        }

        static void ShowPatientsCurrentlyInTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Patients Currently In Treatment =======");
            DisplaySearchResults(databaseManager.GetPatientsCurrentlyInTreatment());
        }

        static void ShowPatientsSeenByMultipleTherapists()
        {
            Console.Clear();
            Console.WriteLine("======= Patients Seen By Multiple Therapists =======");
            DisplaySearchResults(databaseManager.GetPatientsSeenByMultipleTherapists());
        }

        static void ShowPatientsWithNoSessions()
        {
            Console.Clear();
            Console.WriteLine("======= Patients With No Sessions =======");
            DisplaySearchResults(databaseManager.GetPatientsWithNoSessions());
        }

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

            var results =
                databaseManager.GetCustomRangeUnpaidBalances(
                    startDate,
                    endDate);

            DisplayUnpaidBalanceResults(results);
        }

        static void ShowLifetimeUnpaidBalanceByPatient()
        {
            Console.Clear();
            Console.WriteLine("======= Lifetime Unpaid Balance By Patient =======");

            var results = databaseManager.GetLifetimeUnpaidBalanceByPatient();
            DisplaySearchResults(results);
        }

        static void DisplayUnpaidBalanceResults(List<UnpaidBalanceResult> results)
        {
            Console.WriteLine();

            if (results.Count == 0)
            {
                Console.WriteLine("No unpaid balance records found.");
            }
            else
            {
                foreach (var result in results)
                {
                    if (result.SessionId.StartsWith("Database error:"))
                    {
                        Console.WriteLine(result.SessionId);
                    }
                    else
                    {
                        decimal unpaidAmount = result.BalanceDue - result.AmountCollected;

                        Console.WriteLine($"Session ID: {result.SessionId}");
                        Console.WriteLine($"  Balance Due:      {result.BalanceDue:C}");
                        Console.WriteLine($"  Amount Collected: {result.AmountCollected:C}");
                        Console.WriteLine($"  Unpaid Amount:    {unpaidAmount:C}");
                        Console.WriteLine();
                    }
                }
            }

            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        // Methods for payments search options
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

        static void DisplaySearchResults(List<string> results)
        {
            Console.WriteLine();

            int rowCount = results.Count;

            if (rowCount == 0)
            {
                Console.WriteLine("No records found.");
                Console.WriteLine();
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Total Rows Found: {rowCount}");
            Console.WriteLine();

            int rowsToShow;

            if (rowCount < 10)
            {
                rowsToShow = rowCount;
            }
            else
            {
                Console.WriteLine("Choose display amount:");
                Console.WriteLine("1. Top 10");
                Console.WriteLine("2. Top 100");
                Console.WriteLine("3. Show All");
                Console.Write("Selection: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        rowsToShow = Math.Min(10, rowCount);
                        break;

                    case "2":
                        rowsToShow = Math.Min(100, rowCount);
                        break;

                    case "3":
                        rowsToShow = rowCount;
                        break;

                    default:
                        rowsToShow = Math.Min(10, rowCount);
                        break;
                }

                Console.WriteLine();
            }

            for (int i = 0; i < rowsToShow; i++)
            {
                Console.WriteLine(results[i]);
            }

            Console.WriteLine();
            Console.WriteLine($"Displayed Rows: {rowsToShow}");
            Console.WriteLine($"Total Rows Available: {rowCount}");

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        // Methods for treatment reports search options
        static void TreatmentMenu()
        {
            bool inTreatmentMenu = true;

            while (inTreatmentMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Treatment Reports =======");
                Console.WriteLine("0. Return To Search Menu");
                Console.WriteLine("1. Number Of Treatments Per Patient");
                Console.WriteLine("2. Number Of On-Going Treatments");
                Console.WriteLine("3. Average Number Of Treatments Per Patient");
                Console.WriteLine("4. Peak Months For Treatment");
                Console.WriteLine("5. Patients Who Never Pursued Treatment");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "0":
                        inTreatmentMenu = false;
                        break;
                    case "1":
                        ShowTreatmentCountPerPatient();
                        break;
                    case "2":
                        ShowOngoingTreatments();
                        break;
                    case "3":
                        ShowAverageTreatmentsPerPatient();
                        break;
                    case "4":
                        ShowPeakMonthsForTreatment();
                        break;
                    case "5":
                        ShowPatientsNeverPursuedTreatment();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void ShowTreatmentCountPerPatient()
        {
            Console.Clear();
            Console.WriteLine("======= Number Of Treatments Per Patient =======");
            DisplaySearchResults(databaseManager.GetTreatmentCountPerPatient());
        }

        static void ShowOngoingTreatments()
        {
            Console.Clear();
            Console.WriteLine("======= On-Going Treatments =======");
            DisplaySearchResults(databaseManager.GetOngoingTreatments());
        }

        static void ShowAverageTreatmentsPerPatient()
        {
            Console.Clear();
            Console.WriteLine("======= Average Treatments Per Patient =======");
            DisplaySearchResults(databaseManager.GetAverageTreatmentsPerPatient());
        }

        static void ShowPeakMonthsForTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Peak Months For Treatment =======");
            DisplaySearchResults(databaseManager.GetPeakMonthsForTreatment());
        }

        static void ShowPatientsNeverPursuedTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Patients Who Never Pursued Treatment =======");
            DisplaySearchResults(databaseManager.GetPatientsNeverPursuedTreatment());
        }

        // Method for database update option
        static void AddNewPatient()
        {
            Console.Clear();
            Console.WriteLine("======= Add New Patient =======");

            Console.Write("Patient ID: ");
            string patientId = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Patient Name: ");
            string patientName = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Date of Birth (YYYY-MM-DD): ");
            string dobInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(dobInput, out DateTime dob))
            {
                Console.WriteLine("Invalid date format.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Insurance Name (leave blank if none): ");
            string insuranceName = Console.ReadLine()?.Trim() ?? "";

            Console.Write("Insurance Policy Number (leave blank if none): ");
            string insurancePolicy = Console.ReadLine()?.Trim() ?? "";

            bool success = databaseManager.InsertPatient(
                patientId,
                patientName,
                dob,
                insuranceName,
                insurancePolicy,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void DeletePatient()
        {
            Console.Clear();
            Console.WriteLine("======= Delete Patient =======");

            Console.Write("Enter Patient ID to delete: ");
            string patientId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(patientId))
            {
                Console.WriteLine("Patient ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.Write($"Are you sure you want to delete patient {patientId}? (Y/N): ");
            string confirm = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (confirm != "Y")
            {
                Console.WriteLine("Delete cancelled.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool success = databaseManager.DeletePatient(patientId, out string message);

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void UpdatePatientInfo()
        {
            Console.Clear();
            Console.WriteLine("======= Update Patient Information =======");

            Console.Write("Enter Patient ID to update: ");
            string patientId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(patientId))
            {
                Console.WriteLine("Patient ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();

            string currentInfo = databaseManager.GetSinglePatientById(patientId);

            if (string.IsNullOrWhiteSpace(currentInfo))
            {
                Console.WriteLine("No patient found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (currentInfo.StartsWith("Database error:"))
            {
                Console.WriteLine(currentInfo);
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Current Patient Information:");
            Console.WriteLine(currentInfo);
            Console.WriteLine();
            Console.WriteLine("Patient ID and Date of Birth cannot be changed.");
            Console.WriteLine();

            Console.Write("New Patient Name: ");
            string newName = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Patient name cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("New Insurance Name (leave blank if none): ");
            string newInsuranceName = Console.ReadLine()?.Trim() ?? "";

            Console.Write("New Insurance Policy Number (leave blank if none): ");
            string newInsurancePolicy = Console.ReadLine()?.Trim() ?? "";

            bool success = databaseManager.UpdatePatientInfo(
                patientId,
                newName,
                newInsuranceName,
                newInsurancePolicy,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        // Update Session Methods
        static void AddNewSession()
        {
            Console.Clear();
            Console.WriteLine("======= Add New Session =======");

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
            Console.WriteLine("Treatments for this patient:");
            var treatments = databaseManager.GetTreatmentsForPatient(patientId);

            if (treatments.Count == 0)
            {
                Console.WriteLine("No treatments found for this patient. A session must be tied to an existing treatment.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            foreach (string treatment in treatments)
            {
                Console.WriteLine(treatment);
            }

            Console.WriteLine();

            Console.Write("Session ID: ");
            string sessionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                Console.WriteLine("Session ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (sessionId.Length > 7)
            {
                Console.WriteLine("Session ID cannot be longer than 7 characters.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (databaseManager.SessionIdExists(sessionId))
            {
                Console.WriteLine("A session with that ID already exists.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Session Date (YYYY-MM-DD): ");
            string dateInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(dateInput, out DateTime sessionDate))
            {
                Console.WriteLine("Invalid date format.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Therapist ID: ");
            string therapistId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(therapistId))
            {
                Console.WriteLine("Therapist ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.TherapistExists(therapistId))
            {
                Console.WriteLine("No therapist found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Treatment Code: ");
            string treatCode = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(treatCode))
            {
                Console.WriteLine("Treatment code cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.TreatmentCodeExists(treatCode))
            {
                Console.WriteLine("That treatment code does not exist.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.TherapistCanPerformTreatmentCode(therapistId, treatCode))
            {
                Console.WriteLine("This therapist does not have a skill that supports this treatment code.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Treatment ID: ");
            string treatmentIdInput = Console.ReadLine()?.Trim() ?? "";

            if (!int.TryParse(treatmentIdInput, out int treatmentId))
            {
                Console.WriteLine("Invalid treatment ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.TreatmentMatchesSessionInfo(
                    treatmentId,
                    patientId,
                    therapistId,
                    treatCode,
                    sessionDate,
                    out string validationMessage))
            {
                Console.WriteLine(validationMessage);
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Session Notes (optional): ");
            string sessionNotes = Console.ReadLine()?.Trim() ?? "";

            bool success = databaseManager.InsertPatientSession(
                sessionId,
                sessionDate,
                patientId,
                sessionNotes,
                therapistId,
                treatCode,
                treatmentId,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        // Update Accounting Methods
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

        static void AddNewTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Add New Treatment =======");

            Console.Write("Patient ID: ");
            string patientId = Console.ReadLine()?.Trim() ?? "";

            if (!databaseManager.PatientExists(patientId))
            {
                Console.WriteLine("No patient found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Therapist ID: ");
            string therapistId = Console.ReadLine()?.Trim() ?? "";

            if (!databaseManager.TherapistExists(therapistId))
            {
                Console.WriteLine("No therapist found with that ID.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Treatment Code: ");
            string treatCode = Console.ReadLine()?.Trim() ?? "";

            if (!databaseManager.TreatmentCodeExists(treatCode))
            {
                Console.WriteLine("That treatment code does not exist.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (!databaseManager.TherapistCanPerformTreatmentCode(therapistId, treatCode))
            {
                Console.WriteLine("This therapist does not have a skill that supports this treatment code.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Start Date (YYYY-MM-DD): ");
            string startInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(startInput, out DateTime startDate))
            {
                Console.WriteLine("Invalid start date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("End Date (YYYY-MM-DD), or leave blank if ongoing: ");
            string endInput = Console.ReadLine()?.Trim() ?? "";

            DateTime? endDate = null;

            if (!string.IsNullOrWhiteSpace(endInput))
            {
                if (!DateTime.TryParse(endInput, out DateTime parsedEndDate))
                {
                    Console.WriteLine("Invalid end date.");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    return;
                }

                if (parsedEndDate < startDate)
                {
                    Console.WriteLine("End date cannot be before start date.");
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    return;
                }

                endDate = parsedEndDate;
            }

            bool success = databaseManager.InsertTreatment(
                therapistId,
                patientId,
                startDate,
                endDate,
                treatCode,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}
