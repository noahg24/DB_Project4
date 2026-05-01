using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
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
                Console.WriteLine("7. Patients with Insurance on the Insurance Network");
                Console.WriteLine("8. All Sessions For A Patient");
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
                    case "7":
                        ShowPatientsWithInNetworkInsurance();
                        break;
                    case "8":
                        ShowAllSessionsForPatient();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

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

        static void ShowPatientsWithInNetworkInsurance()
        {
            Console.Clear();
            Console.WriteLine("======= Patients With In-Network Insurance =======");
            DisplaySearchResults(databaseManager.GetPatientsWithInNetworkInsurance());
        }

        static void ShowAllSessionsForPatient()
        {
            Console.Clear();
            Console.WriteLine("======= View Sessions By Patient ID =======");

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

            DisplaySearchResults(databaseManager.GetSessionsByPatientId(patientId));
        }

        // Patient Update Menu
        static void PatientUpdateMenu()
        {
            bool inPatientMenu = true;

            while (inPatientMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Patient Update Menu =======");
                Console.WriteLine("0. Return To Update Menu");
                Console.WriteLine("1. Add New Patient");
                Console.WriteLine("2. Delete Patient (Not Recommended)");
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
    }
}