# MyPatient

MyPatient is a C# console-based database application for managing patients, therapists, treatments, sessions, accounting records, and insurance network information.

The application connects to a MySQL database and runs manually written SQL queries through C#. Was originally created and ran on a Windows machine, but should be compatible with any OS that supports .NET and MySQL.

---

## Requirements

Before running the project, make sure you have:

- MySQL Server installed
- MySQL Workbench installed
- .NET SDK installed
- The project source code folder
- The SQL schema/data files for Patient Portal

To check if .NET is installed, open Command Prompt and run:

```bash```
dotnet --version

## Database Setup
1. Open MySQL Workbench
Open MySQL Workbench and connect to your local MySQL server.

2. Create the database
Run:
CREATE DATABASE patient_portal;
USE patient_portal;
If the database already exists, just run:
USE patient_portal;

3. Run the schema script
Open the project schema file, usually named something like:
schema.sql
Run the full script in MySQL Workbench.
This will create the required tables, such as:
Patient
Therapist
Treatment
PatientSession
Accounting
InsuranceNetwork
ListSkills
TherapistSkills
SkillTreatcodeMap

4. Import data
Import the CSV files into the matching MySQL tables.
Recommended import order:
1. InsuranceNetwork
2. Therapist
3. Patient
4. ListSkills
5. TherapistSkills
6. SkillTreatcodeMap
7. Treatment
8. PatientSession
9. Accounting
This order helps avoid foreign key errors.
In MySQL Workbench:
Right-click the table
Select Table Data Import Wizard
Choose the matching CSV file
Map columns carefully
Run the import
After importing, verify each table:
SELECT COUNT(*) FROM Patient;
SELECT COUNT(*) FROM Therapist;
SELECT COUNT(*) FROM Treatment;
SELECT COUNT(*) FROM PatientSession;
SELECT COUNT(*) FROM Accounting;

## C# Project Setup
1. Open Command Prompt
Open Command Prompt.

2. Navigate to the project folder
Use cd to move into the folder containing the .csproj file.
Example:
cd "C:\Users\noahg\DBproj4\ProgramFolder"
Use quotes if the path contains spaces.

3. Confirm the project file exists
Run:
dir
You should see a file ending in:
.csproj
Example:
Final Program.csproj

4. Install the MySQL connector package
Run:
dotnet add package MySqlConnector
This allows the C# program to connect to MySQL.

## Configure Database Connection
Open DatabaseManager.cs.
Find the connection string:
connectionString = "Server=localhost;Database=patient_portal;User ID=root;Password=YOUR_PASSWORD;";
Replace YOUR_PASSWORD with your MySQL root password.
Example:
connectionString = "Server=localhost;Database=patient_portal;User ID=root;Password=mypassword;";
Make sure the database name is:
patient_portal

If your database is named differently (like my_patient), change that name in the connectionString as well

## Running the Program
From the project folder, run:
dotnet run
The program should launch the Patient Portal console menu.

## Login and Accounts
The application supports account creation and login.
Accounts are saved locally in:
accounts.txt
During account creation, users choose either:
987123 = admin
0 = base user
Admin users can access update features.
Base users can search the database but cannot update it.

## Common Commands
Run the project
dotnet run
Build the project
dotnet build
Clean the project
dotnet clean
Restore packages
dotnet restore

## Common Issues
Error: Couldn't find a project to run
You are probably not in the folder containing the .csproj file.
Run:
dir *.csproj
If nothing appears, navigate to the correct folder.

Error: Access denied for MySQL user
Check the username and password in DatabaseManager.cs.

Error: Unknown database patient_portal
Make sure you created the database:
CREATE DATABASE patient_portal;

Error: Table does not exist
Make sure you ran the schema script in the patient_portal database:
USE patient_portal;

Error: Foreign key constraint fails during import
Import the parent tables first.
Recommended order:
InsuranceNetwork
Therapist
Patient
ListSkills
TherapistSkills
SkillTreatcodeMap
Treatment
PatientSession
Accounting
