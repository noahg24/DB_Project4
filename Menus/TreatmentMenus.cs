using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
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

        // Treatment Update Menu
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

        static void AddNewTreatment()
        {
            Console.Clear();
            Console.WriteLine("======= Add New Treatment And Initial Session =======");

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

            Console.Write("Treatment Start Date (YYYY-MM-DD): ");
            string startInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(startInput, out DateTime startDate))
            {
                Console.WriteLine("Invalid start date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Treatment End Date (YYYY-MM-DD), or leave blank if ongoing: ");
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

            Console.WriteLine();
            Console.WriteLine("Now enter information for the initial session tied to this treatment.");

            string sessionId = databaseManager.GetNextPatientSessionId();
            Console.WriteLine($"Generated Session ID: {sessionId}");

            Console.Write("Session Date (YYYY-MM-DD): ");
            string sessionDateInput = Console.ReadLine()?.Trim() ?? "";

            if (!DateTime.TryParse(sessionDateInput, out DateTime sessionDate))
            {
                Console.WriteLine("Invalid session date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (sessionDate < startDate)
            {
                Console.WriteLine("Session date cannot be before treatment start date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            if (endDate.HasValue && sessionDate > endDate.Value)
            {
                Console.WriteLine("Session date cannot be after treatment end date.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            Console.Write("Session Notes (optional): ");
            string sessionNotes = Console.ReadLine()?.Trim() ?? "";

            bool success = databaseManager.InsertTreatmentWithInitialSession(
                therapistId,
                patientId,
                startDate,
                endDate,
                treatCode,
                sessionId,
                sessionDate,
                sessionNotes,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}