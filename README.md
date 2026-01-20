# Club Cash Register

Club Cash Register is a web-based financial management system for non-profit clubs.  
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

- **Framework:** .NET 9 (ASP.NET Core Blazor WebApp)
- **UI Framework:** MudBlazor
- **Database:** MariaDB / MySQL  
  (using EF Core + Pomelo.EntityFrameworkCore.MySql)
  **PDF Generation:** QuestPDF Community Edition
- **Excel Export:** EPPlus (Polyform NonCommercial 1.0.0)
- **Excel Import:** ExcelDataReader
- **Logging:** Serilog
- **Dependency Injection:** Standard .NET DI + MySqlConnector DI extensions

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

1. Clone the repository
2. Open in Visual Studio / Rider
3. Restore NuGet packages
4. Configure **MariaDB connection**
    - via *appsettings.json* or *User Secrets*
5. (Optional) Apply database migrations
   ```bash
   dotnet ef database update
