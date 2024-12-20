using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TTCCashRegister.Data.Models;

namespace TTCCashRegister.Data.Services
{
    public class ExportService
    {
        private readonly CashDataContext _context;
        private readonly string _tempPath;
        private const string SelectedFolder = "Export";

        public ExportService(CashDataContext context)
        {
            _context = context;
            
            if (OperatingSystem.IsWindows())
            {
                _tempPath = System.IO.Path.GetTempPath();
            }
            else
            {
                _tempPath = Environment.GetEnvironmentVariable("TMPDIR") ?? "/tmp";
            }
        }

        private async Task<List<Transaction>> GetTransactionsInDateRange(DateTime begin, DateTime end)
        {
            return await _context.Transactions
                                 .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                                 .ToListAsync();
        }
        
        private async Task<List<Transaction>> GetBudgetByDateRange(DateTime begin, DateTime end)
        {
            return await _context.Transactions
                .Include(t => t.CostUnit)
                .Include(t => t.BasicUnit)
                .ThenInclude(bu => bu.CostUnitDetails)
                .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                .ToListAsync();
        }


        public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                var folderPath = System.IO.Path.Combine(_tempPath, SelectedFolder);
                var fullPath = System.IO.Path.Combine(folderPath, filename);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var transactions = await GetTransactionsInDateRange(begin, end);

                var csv = new StringBuilder();
                foreach (var transaction in transactions)
                {
                    csv.AppendLine($"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
                }

                if (string.IsNullOrWhiteSpace(csv.ToString()))
                {
                    Console.WriteLine("Keine Daten für den Export vorhanden.");
                    return false;
                }

                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    await sw.WriteLineAsync("Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung");
                    await sw.WriteAsync(csv);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
        {
            try
            {
                var folderPath = System.IO.Path.Combine(_tempPath, SelectedFolder);
                var fullPath = System.IO.Path.Combine(folderPath, filename);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var transactions = await GetTransactionsInDateRange(begin, end);
                var writer = new PdfWriter(fullPath);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4.Rotate());
                document.SetMargins(20, 30, 20, 30);
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var table = new Table(5);
                table.SetWidth(UnitValue.CreatePercentValue(100));

                // Header hinzufügen
                document.Add(new Paragraph("Kassenbuch TTC Hagen")
                    .SetFont(bold)
                    .SetFontSize(20)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph($"Zeitraum: {begin:dd.MM.yyyy} - {end:dd.MM.yyyy}")
                    .SetFont(font)
                    .SetFontSize(14)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("\n")); // Leerzeile für Abstand

                table.AddHeaderCell(new Cell().Add(new Paragraph("Datum").SetFont(bold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Belegnr.").SetFont(bold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Beschreibung.").SetFont(bold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Rechnungsbetrag").SetFont(bold)));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Kontobewegung").SetFont(bold)));
                
                foreach (var transaction in transactions)
                {
                    table.AddCell(new Cell().Add(new Paragraph(transaction.Date.ToString()).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(transaction.Documentnumber.ToString()).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(transaction.Description).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(transaction.Sum.ToString(CultureInfo.CurrentCulture)).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(transaction.AccountMovement.ToString(CultureInfo.CurrentCulture)).SetFont(font)));
                }
                document.Add(table);
                document.Close();
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }
        
        public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                var folderPath = System.IO.Path.Combine(_tempPath, SelectedFolder);
                var fullPath = System.IO.Path.Combine(folderPath, filename);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                var budget = await GetBudgetByDateRange(begin, end);

                var csv = new StringBuilder();
                foreach (var bu in budget)
                {
                    var unitDetails = bu.UnitDetails != null ? bu.UnitDetails.CostDetails : "N/A";
                    csv.AppendLine(
                        $"{bu.CostUnit.CostUnitName};{bu.BasicUnit.Name};{unitDetails};{bu.AccountMovement}");
                }

                if (string.IsNullOrWhiteSpace(csv.ToString()))
                {
                    Console.WriteLine("Keine Daten für den Export vorhanden.");
                    return false;
                }
                
                await using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    await sw.WriteLineAsync("Kostenstelle;Position;Details;Summe");
                    await sw.WriteAsync(csv);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }
    }
    
    
    
}

