using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
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

        public async Task<byte[]> ExportTransactionsToPdf(DateTime begin, DateTime end)
        {
            var transactions = await _context.Transactions
                .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                .ToListAsync();

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                foreach (var transaction in transactions)
                {
                    document.Add(new Paragraph($"Documentnummer: {transaction.Documentnumber}"));
                    document.Add(new Paragraph($"Description: {transaction.Description}"));
                    document.Add(new Paragraph($"Sum: {transaction.Sum}"));
                    document.Add(new Paragraph($"AccountMovement: {transaction.AccountMovement}"));
                    document.Add(new Paragraph(" "));
                }

                document.Close();
                return stream.ToArray();
            }
        }
    }
}

