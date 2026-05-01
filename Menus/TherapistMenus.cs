using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
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
    }
}