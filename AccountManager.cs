using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EnterpriseSystemApp
{
    public class AccountManager
    {
        private const string AccountFile = "accounts.txt";
        private const string AdminKey = "987123";
        private List<Account> accounts = new List<Account>();

        public AccountManager()
        {
            LoadAccounts();
        }

        public bool CreateAccount(string username, string password, string roleInput, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                message = "Username and password cannot be empty.";
                return false;
            }

            if (accounts.Any(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                message = "That username already exists.";
                return false;
            }

            if (!IsValidPassword(password, out string passwordError))
            {
                message = passwordError;
                return false;
            }

            string role;

            if (roleInput == AdminKey)
            {
                role = "admin";
            }
            else if (roleInput == "0")
            {
                role = "base";
            }
            else
            {
                message = "Invalid role key. Enter admin key for admin or 0 for base.";
                return false;
            }

            accounts.Add(new Account(username, password, role));
            SaveAccounts();

            message = $"Account created successfully as {role}.";
            return true;
        }

        public Account? ValidateLogin(string username, string password)
        {
            return accounts.FirstOrDefault(a =>
                a.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                a.Password == password);
        }

        private bool IsValidPassword(string password, out string errorMessage)
        {
            errorMessage = "";

            if (password.Length < 8)
            {
                errorMessage = "Password must be at least 8 characters long.";
                return false;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            if (!hasUpper)
            {
                errorMessage = "Password must contain at least one uppercase letter.";
                return false;
            }

            if (!hasLower)
            {
                errorMessage = "Password must contain at least one lowercase letter.";
                return false;
            }

            if (!hasDigit)
            {
                errorMessage = "Password must contain at least one digit.";
                return false;
            }

            if (!hasSpecial)
            {
                errorMessage = "Password must contain at least one special character.";
                return false;
            }

            return true;
        }

        private void LoadAccounts()
        {
            accounts.Clear();

            if (!File.Exists(AccountFile))
            {
                return;
            }

            string[] lines = File.ReadAllLines(AccountFile);

            foreach (string line in lines)
            {
                string[] parts = line.Split('|');

                if (parts.Length == 3)
                {
                    accounts.Add(new Account(parts[0], parts[1], parts[2]));
                }
            }
        }

        private void SaveAccounts()
        {
            List<string> lines = new List<string>();

            foreach (Account account in accounts)
            {
                lines.Add($"{account.Username}|{account.Password}|{account.Role}");
            }

            File.WriteAllLines(AccountFile, lines);
        }
    }
}
