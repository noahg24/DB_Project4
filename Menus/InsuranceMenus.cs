using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
        static void InsuranceNetworkUpdateMenu()
        {
            bool inInsuranceMenu = true;

            while (inInsuranceMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Insurance Network Update Menu =======");
                Console.WriteLine("1. Add Insurance Provider");
                Console.WriteLine("2. Delete Insurance Provider");
                Console.WriteLine("0. Return To Update Menu");
                Console.Write("Select an option: ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddInsuranceProvider();
                        break;

                    case "2":
                        DeleteInsuranceProvider();
                        break;

                    case "0":
                        inInsuranceMenu = false;
                        break;

                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        static void AddInsuranceProvider()
        {
            Console.Clear();
            Console.WriteLine("======= Add Insurance Provider =======");

            Console.Write("Insurance Provider Name: ");
            string insuranceName = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(insuranceName))
            {
                Console.WriteLine("Insurance provider name cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool success = databaseManager.InsertInsuranceProvider(
                insuranceName,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }

        static void DeleteInsuranceProvider()
        {
            Console.Clear();
            Console.WriteLine("======= Delete Insurance Provider =======");

            Console.Write("Insurance Provider Name to delete: ");
            string insuranceName = Console.ReadLine()?.Trim() ?? "";
            if (!ValidateAndPrint(insuranceName)) return;

            if (string.IsNullOrWhiteSpace(insuranceName))
            {
                Console.WriteLine("Insurance provider name cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine();
            Console.Write($"Are you sure you want to delete '{insuranceName}' from InsuranceNetwork? (Y/N): ");
            string confirm = Console.ReadLine()?.Trim().ToUpper() ?? "";

            if (confirm != "Y")
            {
                Console.WriteLine("Delete cancelled.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            bool success = databaseManager.DeleteInsuranceProvider(
                insuranceName,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}