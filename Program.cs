using System;
using System.Collections.Generic;

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
                Console.WriteLine("======================================");
                Console.WriteLine("        Patient Portal System");
                Console.WriteLine("======================================");
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
                Console.WriteLine("1. Test Database Connection");
                Console.WriteLine("2. Search Therapists By Name");
                Console.WriteLine("3. Unpaid Balance Reports");
                Console.WriteLine("4. Return To Main Menu");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        TestDatabaseConnection();
                        break;
                    case "2":
                        SearchTherapistsByName();
                        break;
                    case "3":
                        UnpaidBalanceMenu();
                        break;
                    case "4":
                        inSearchMenu = false;
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
            Console.Clear();
            Console.WriteLine("======= Update The Database =======");

            if (account.Role != "admin")
            {
                Console.WriteLine("Access denied. Only admin users can update the database.");
            }
            else
            {
                Console.WriteLine("Update functionality will be added later.");
            }

            Console.WriteLine("Press Enter to return to the main menu...");
            Console.ReadLine();
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

        static void UnpaidBalanceMenu()
        {
            bool inUnpaidMenu = true;

            while (inUnpaidMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Unpaid Balance Reports =======");
                Console.WriteLine("1. Lifetime Unpaid Balance");
                Console.WriteLine("2. Year-End Unpaid Balance");
                Console.WriteLine("3. Month-End Unpaid Balance");
                Console.WriteLine("4. Return To Search Menu");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
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
                        inUnpaidMenu = false;
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
            Console.WriteLine("======= Lifetime Unpaid Balance =======");

            var results = databaseManager.GetLifetimeUnpaidBalances();
            DisplayUnpaidBalanceResults(results);
        }

        static void ShowYearEndUnpaidBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Year-End Unpaid Balance =======");
            Console.Write("Enter year (example: 2026): ");

            string input = Console.ReadLine() ?? "";

            if (!int.TryParse(input, out int year))
            {
                Console.WriteLine("Invalid year.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            var results = databaseManager.GetYearEndUnpaidBalances(year);
            DisplayUnpaidBalanceResults(results);
        }

        static void ShowMonthEndUnpaidBalances()
        {
            Console.Clear();
            Console.WriteLine("======= Month-End Unpaid Balance =======");
            Console.Write("Enter year (example: 2026): ");
            string yearInput = Console.ReadLine() ?? "";

            Console.Write("Enter month (1-12): ");
            string monthInput = Console.ReadLine() ?? "";

            if (!int.TryParse(yearInput, out int year) || !int.TryParse(monthInput, out int month) || month < 1 || month > 12)
            {
                Console.WriteLine("Invalid year or month.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            var results = databaseManager.GetMonthEndUnpaidBalances(year, month);
            DisplayUnpaidBalanceResults(results);
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
    }
}
