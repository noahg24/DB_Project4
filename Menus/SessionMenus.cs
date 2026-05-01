using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
        static void SessionUpdateMenu()
        {
            bool inSessionMenu = true;

            while (inSessionMenu)
            {
                Console.Clear();
                Console.WriteLine("======= Session Update Menu =======");
                Console.WriteLine("0. Return To Update Menu");
                Console.WriteLine("1. Add New Session");
                Console.WriteLine("2. Update Session Notes");
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
                        UpdateSessionNotes();
                        break;
                    default:
                        Console.WriteLine("Invalid option. Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

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

        static void UpdateSessionNotes()
        {
            Console.Clear();
            Console.WriteLine("======= Update Session Notes =======");

            Console.Write("Enter Session ID: ");
            string sessionId = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                Console.WriteLine("Session ID cannot be blank.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return;
            }

            string currentInfo = databaseManager.GetSingleSessionById(sessionId);

            if (string.IsNullOrWhiteSpace(currentInfo))
            {
                Console.WriteLine("No session found with that ID.");
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

            Console.WriteLine();
            Console.WriteLine("Current Session Information:");
            Console.WriteLine(currentInfo);
            Console.WriteLine();

            Console.Write("Enter new session notes, or leave blank to clear notes: ");
            string newNotes = Console.ReadLine()?.Trim() ?? "";

            bool success = databaseManager.UpdateSessionNotes(
                sessionId,
                newNotes,
                out string message
            );

            Console.WriteLine();
            Console.WriteLine(message);

            if (success)
            {
                Console.WriteLine();
                Console.WriteLine("Updated Session Information:");
                Console.WriteLine(databaseManager.GetSingleSessionById(sessionId));
            }

            Console.WriteLine();
            Console.WriteLine("Press Enter to continue...");
            Console.ReadLine();
        }
    }
}