# Club cash register

The club cash register project implement desktop application designed to manage the finances of a non-profit club. It allows tracking of all income and expenses in a clear and organized way.

---

## Features

- **Income & Expense Management:** Track all financial transactions of the club, including income and expenses.
- **Export Reports:** Generate reports as PDF or Excel files for bookkeeping and auditing purposes.
- **Easy Data Handling:** Import and export financial data for future reference or club meetings.
- **Background Database:** Stores all financial data in a MariaDB database for reliable and persistent storage.
- **Third-Party Libraries:** Uses well-known libraries like AutoMapper, EPPlus, iText, and Serilog for efficient data handling, PDF/Excel generation, and logging.

---

## Technical Details

- **Database:** MariaDB (used in the background to store all transactions, categories, and reports)
- **ORM / Mapping:** AutoMapper 12.0.1 for object-to-object mapping
- **PDF Generation:** iText 5.x / 7.x
- **Excel Export:** EPPlus 5.x (non-commercial license)
- **Logging:** Serilog (Apache License 2.0)
- **Platform:** .NET (Core or Framework depending on your setup)

> **Note:** Ensure MariaDB is installed and configured. Connection details should be set in the application's configuration file.

---

## Usage

1. Open the application.
2. Add income or expense items with relevant details (date, amount, category, description).
3. View all entries in a summary table.
4. Export data as PDF or Excel for audits or reporting.

---

## Installation

- Clone this repository.
- Open the project in Visual Studio (or your preferred .NET IDE).
- Restore NuGet packages.
- Configure MariaDB connection in the app configuration.
- Build and run the application.

---

## Third-Party Libraries

This project uses the following third-party libraries:

- **AutoMapper 12.0.1** (MIT License)
- **EPPlus 5.x** (Polyform Noncommercial License 1.0.0 – for internal non-commercial use)
- **iText 5.x / 7.x** (AGPL)
- **Serilog** (Apache License 2.0)

For details, see [ThirdPartyLicenses.md](ThirdPartyLicenes.md).

---

## Notes

- This project is intended for private use within the club.
- EPPlus usage is limited to non-commercial purposes.
- If the project is made public, licensing of third-party components, particularly EPPlus and iText, must be reviewed to comply with open-source li
