# Club Treasury

Club Treasury is a web-based financial management system for non-profit clubs.
It allows club treasurers to record income and expenses, generate financial reports,
and manage all financial data in a structured and transparent way.

---

## ✨ Features

- **Income & Expense Management**  
  Record and organize all financial transactions of the club.

- **PDF & Excel Reporting**  
  Generate reports for meetings, auditors, or annual summaries.

- **Excel Import & Export**  
  Import financial data from Excel and export it when needed.

- **PDF Document Generation**  
  Built using the modern library **QuestPDF**.

- **User & Role Management**  
  Powered by **ASP.NET Identity**.

- **Logging**  
  Implemented using **Serilog**.

- **Modern UI**  
  Designed with **MudBlazor** and related extensions.

---

## 🛠️ Technical Details

- **Framework:** .NET 10 (ASP.NET Core Blazor WebApp)
- **UI Framework:** MudBlazor
- **Database:** PostgreSQL (using EF Core + Npgsql)
- **PDF Generation:** QuestPDF Community Edition
- **Excel Export:** EPPlus (Polyform NonCommercial 1.0.0)
- **Excel Import:** ExcelDataReader
- **Logging:** Serilog
- **Dependency Injection:** Standard .NET DI

---

## 🚨 License Information for External Libraries

- **QuestPDF Community Edition** is free to use under its custom open license  
  ✔ Allows open-source, private, and commercial use

- **EPPlus** uses the **Polyform NonCommercial** license  
  ✔ Allowed for non-commercial club use  
  ✘ Requires a commercial license if the software is sold, used commercially,
  or generates revenue in any form.<br>
  (This project is currently used exclusively for internal administration
of a non-profit organization and therefore qualifies as non-commercial use.)

More information can be found in:  
👉 **ThirdPartyLicenses.md**

---

## 📦 Installation & Setup

### Docker (Recommended)

For a full installation guide using Docker, see the [Installation Documentation](docs/index.md).

### Development Setup

**Prerequisites:** A local PostgreSQL installation is required. The configured database user must have permissions to create databases.

1. Clone the repository
2. Open in Visual Studio / Rider
3. Restore NuGet packages
4. Configure database credentials via **User Secrets**:
   ```bash
   dotnet user-secrets set "DbName" "ClubCash"
   dotnet user-secrets set "DbUser" "dev"
   dotnet user-secrets set "DbPassword" "YourPassword"
   ```
5. Configure the initial admin user via **User Secrets**:
   ```bash
   dotnet user-secrets set "ADMIN_USERNAME" "admin"
   dotnet user-secrets set "ADMIN_EMAIL" "admin@admin.de"
   dotnet user-secrets set "ADMIN_PASSWORD" "YourAdminPassword"
   ```
   The admin user is created automatically on first startup when no users exist.
6. Run the application — database migrations are applied automatically on startup
