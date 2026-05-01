using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
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
                Console.WriteLine("7. View Insurance Network");
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
                    case "7":
                        ShowAllInsuranceNetworkProviders();
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
                Console.WriteLine("5. Insurance Network");
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
                    case "5":
                        InsuranceNetworkUpdateMenu();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }
    }
}