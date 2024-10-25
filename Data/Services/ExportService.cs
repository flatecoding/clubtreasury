using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;

namespace TTCCashRegister.Data.Services
{
    public class ExportService
    {
        private readonly CashDataContext _context;

        public ExportService(CashDataContext context)
        {
            _context = context;
        }

        public async Task<string> ExportTransactionsToCsv(DateTime begin, DateTime end)
        {
            // IEnumerable<Transaction> transactions = new List<Transaction>();
            var transactions = await _context.Transactions
                 .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                 .ToListAsync();

            var csv = new StringBuilder();
            foreach (var transaction in transactions)
            {
                csv.AppendLine($"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
            }

            return csv.ToString();
        }

        public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string Destination)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                .ToListAsync();

            try
            {
                var writer = new PdfWriter(Destination);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf, PageSize.A4.Rotate());
                document.SetMargins(20, 20, 20, 20);
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var table = new Table(5);
                table.SetWidth(UnitValue.CreatePercentValue(100));

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
                    table.AddCell(new Cell().Add(new Paragraph(transaction.Sum.ToString()).SetFont(font)));
                    table.AddCell(new Cell().Add(new Paragraph(transaction.AccountMovement.ToString()).SetFont(font)));
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
    }
}

