using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
        static void DisplaySearchResults(List<string> results)
        {
            Console.WriteLine();

            if (results.Count == 0)
            {
                Console.WriteLine("No records found.");
                Console.WriteLine();
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            List<string> dataRows = new List<string>();
            List<string> summaryRows = new List<string>();

            foreach (string row in results)
            {
                if (row.StartsWith("TOTAL") ||
                    row.StartsWith("COMBINED TOTAL") ||
                    row.StartsWith("---"))
                {
                    summaryRows.Add(row);
                }
                else
                {
                    dataRows.Add(row);
                }
            }

            int rowCount = dataRows.Count;

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
                Console.WriteLine(dataRows[i]);
            }

            if (summaryRows.Count > 0)
            {
                Console.WriteLine();

                foreach (string row in summaryRows)
                {
                    Console.WriteLine(row);
                }
            }

            Console.WriteLine();
            Console.WriteLine($"Displayed Rows: {rowsToShow}");
            Console.WriteLine($"Total Rows Available: {rowCount}");

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
        
        static void ShowAllInsuranceNetworkProviders()
        {
            Console.Clear();
            Console.WriteLine("======= Insurance Network Providers =======");
            DisplaySearchResults(databaseManager.GetAllInsuranceNetworkProviders());
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