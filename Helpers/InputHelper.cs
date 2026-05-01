using System;

namespace EnterpriseSystemApp
{
    partial class Program
    {
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

        static bool IsSafeUserInput(string input, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Input cannot be blank.";
                return false;
            }

            string lowered = input.ToLower();

            string[] blockedPatterns =
            {
                "--",
                ";",
                "/*",
                "*/",
                " xp_",
                " drop ",
                " delete ",
                " insert ",
                " update ",
                " alter ",
                " create ",
                " truncate ",
                " union ",
                " select ",
                " exec ",
                " execute "
            };

            foreach (string pattern in blockedPatterns)
            {
                if (lowered.Contains(pattern))
                {
                    message = "Invalid input. Possible SQL injection attempt detected.";
                    return false;
                }
            }

            return true;
        }

        static bool ValidateAndPrint(string input)
        {
            if (!IsSafeUserInput(input, out string message))
            {
                Console.WriteLine(message);
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                return false;
            }
            return true;
        }

        static bool IsSafeTextInput(string input, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(input))
            {
                message = "Input cannot be blank.";
                return false;
            }

            string[] blockedPatterns =
            {
                "--",
                ";",
                "/*",
                "*/",
                "'",
                "\"",
                "="
            };

            foreach (string pattern in blockedPatterns)
            {
                if (input.Contains(pattern))
                {
                    message = "Invalid characters detected in input.";
                    return false;
                }
            }

            return true;
        }
    }
}