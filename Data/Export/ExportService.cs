using System.Text;
using Microsoft.EntityFrameworkCore;
using TTCCashRegister.Data.Export.Budget;
using TTCCashRegister.Data.Export.Transaction;
using TTCCashRegister.Data.Mapper;
using TTCCashRegister.Data.Transaction;
using Path = System.IO.Path;

namespace TTCCashRegister.Data.Export
{
    public class ExportService :IExportService
    {
        private readonly CashDataContext _context;
        private readonly ILogger<ExportService> _logger;
        private readonly IBudgetMapper _budgetMapper;
        private readonly ICsvBudgetWriter _csvWriter;
        private readonly IExcelBudgetWriter _excelWriter;
        private readonly string _exportPath;
        private readonly IPdfTransactionRenderer _transactionPdfRenderer;
        private const string SelectedFolder = "Export";
        private const string CsvHeader = "Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung";

        public ExportService(CashDataContext context, ILogger<ExportService> logger, 
            IBudgetMapper budgetMapper, ICsvBudgetWriter csvWriter, 
            IExcelBudgetWriter excelWriter, IPdfTransactionRenderer pdfTransactionRenderer,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _budgetMapper = budgetMapper;
            _csvWriter = csvWriter;
            _excelWriter = excelWriter;
            _transactionPdfRenderer = pdfTransactionRenderer;
    
            var exportBasePath = configuration["ExportSettings:ExportPath"] ?? "Exports";
            
            if (!Path.IsPathRooted(exportBasePath))
            {
                var projectDirectory = AppContext.BaseDirectory;
                var binDirectory = Directory.GetParent(projectDirectory)?.Parent?.Parent?.FullName;
                var basePath = binDirectory ?? projectDirectory;
                exportBasePath = Path.Combine(basePath, exportBasePath);
            }
    
            _exportPath = Path.Combine(exportBasePath, SelectedFolder);
            Directory.CreateDirectory(_exportPath);
            _logger.LogInformation("Export path configured: {ExportPath}", _exportPath);
        }

        private async Task<List<TransactionModel>> GetTransactionsInDateRange(DateTime begin, DateTime end)
        {
            return await _context.Transactions
                .Where(t => t.Date >= DateOnly.FromDateTime(begin) && t.Date <= DateOnly.FromDateTime(end))
                .Select(t => new TransactionModel
                {
                    Date = t.Date,
                    Documentnumber = t.Documentnumber,
                    Description = t.Description,
                    Sum = t.Sum,
                    AccountMovement = t.AccountMovement
                }).ToListAsync();
        }

        private async Task<List<TransactionModel>> GetBudgetByDateRange(DateTime begin, DateTime end)
        {
            var beginDateOnly = DateOnly.FromDateTime(begin);
            var endDateOnly   = DateOnly.FromDateTime(end);

            return await _context.Transactions
                .AsSplitQuery()
                .Include(t => t.Allocation).ThenInclude(a => a.CostCenter)
                .Include(t => t.Allocation).ThenInclude(a => a.Category)
                .Include(t => t.Allocation).ThenInclude(a => a.ItemDetail)
                .Include(t => t.TransactionDetails).ThenInclude(st => st.Person)
                .Where(t => t.Date >= beginDateOnly && t.Date <= endDateOnly)
                .ToListAsync();
        }


        public async Task<bool> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Beginning export transactions to csv");
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                    _logger.LogInformation("Export directory created {path}", _exportPath);
                }

                var transactions = await GetTransactionsInDateRange(begin, end);
                transactions = transactions.OrderBy(t => t.Documentnumber).ToList();

                var csv = new StringBuilder();
                foreach (var transaction in transactions)
                {
                    csv.AppendLine(
                        $"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
                }

                if (string.IsNullOrWhiteSpace(csv.ToString()))
                {
                    _logger.LogError("CSV file for transactions could not be created");
                    return false;
                }

                await using var sw = new StreamWriter(Path.Combine(_exportPath, filename));
                await sw.WriteLineAsync(CsvHeader);
                await sw.WriteAsync(csv);
                _logger.LogInformation("Export transactions to csv completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting transactions to csv");
                return false;
            }
        }
        
        
        public async Task<bool> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename)
        {
            try
            {
                var transactions = await GetTransactionsInDateRange(begin, end);

                var filePath = Path.Combine(_exportPath, filename);

                await _transactionPdfRenderer.RenderTransactionPdfExportAsync(transactions, begin, end, filePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF export failed");
                return false;
            }
        }
        
        
        public async Task<bool> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Export budget to csv started");

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);

                var transactions = await GetBudgetByDateRange(begin, end);
                var flat = _budgetMapper.BuildFlatEntries(transactions);
                var grouped = _budgetMapper.BuildBudgetHierarchy(flat);

                var filePath = Path.Combine(_exportPath, filename);
                await _csvWriter.WriteAsync(filePath, grouped);
                _logger.LogInformation("Export budget to csv completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget csv");
                return false;
            }
        }
        
        public async Task<bool> ExportBudgetToExcelWithCharts(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Export budget to Excel started");

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);

                var transactions = await GetBudgetByDateRange(begin, end);
                var flatEntries = _budgetMapper.BuildFlatEntries(transactions);
                var grouped = _budgetMapper.BuildBudgetHierarchy(flatEntries);

                var filePath = Path.Combine(_exportPath, filename);
                await _excelWriter.WriteAsync(filePath, grouped, begin, end);

                _logger.LogInformation("Export budget to Excel completed");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export budget to Excel: Error creating Excel file");
                return false;
            }
        }
        
        public async Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end)
        {
            var filename = $"Budget_{begin:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
            var success = await ExportBudgetToExcelWithCharts(begin, end, filename);

            if (!success) return Array.Empty<byte>();
            var filePath = Path.Combine(_exportPath, filename);
            return await File.ReadAllBytesAsync(filePath);
        }
    }
}


