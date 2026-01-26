using System.Text;
using Microsoft.Extensions.Localization;
using TTCCashRegister.Data.Export.Budget;
using TTCCashRegister.Data.Export.Transaction;
using TTCCashRegister.Data.Mapper;
using TTCCashRegister.Data.OperationResult;
using TTCCashRegister.Data.Transaction;
using Path = System.IO.Path;

namespace TTCCashRegister.Data.Export
{
    public class ExportService : IExportService
    {
        private readonly ITransactionService _transactionService;
        private readonly ILogger<ExportService> _logger;
        private readonly IBudgetMapper _budgetMapper;
        private readonly ICsvBudgetWriter _csvWriter;
        private readonly IExcelBudgetWriter _excelWriter;
        private readonly string _exportPath;
        private readonly IPdfTransactionRenderer _transactionPdfRenderer;
        private readonly IOperationResultFactory _operationResultFactory;
        private readonly IStringLocalizer<Translation> _localizer;
        private const string CsvHeader = "Belegnr.;Beschreibung;Rechnungsbetrag;Kontobewegung";

        public ExportService(ITransactionService transactionService, ILogger<ExportService> logger, 
            IBudgetMapper budgetMapper, ICsvBudgetWriter csvWriter, 
            IExcelBudgetWriter excelWriter, IPdfTransactionRenderer pdfTransactionRenderer,
            IOperationResultFactory operationResultFactory,
            IStringLocalizer<Translation> localizer,
            IConfiguration configuration)
        {
            _transactionService = transactionService;
            _logger = logger;
            _budgetMapper = budgetMapper;
            _csvWriter = csvWriter;
            _excelWriter = excelWriter;
            _transactionPdfRenderer = pdfTransactionRenderer;
            _operationResultFactory = operationResultFactory;
            _localizer = localizer;
    
            var exportBasePath = configuration["ExportSettings:ExportPath"] ?? "Exports";
            
            if (!Path.IsPathRooted(exportBasePath))
            {
                var projectDirectory = AppContext.BaseDirectory;
                var binDirectory = Directory.GetParent(projectDirectory)?.Parent?.Parent?.FullName;
                var basePath = binDirectory ?? projectDirectory;
                exportBasePath = Path.Combine(basePath, exportBasePath);
            }

            _exportPath = exportBasePath;
            Directory.CreateDirectory(_exportPath);
            _logger.LogInformation("Configured export path: {ExportPath}", _exportPath);
        }

        public async Task<IOperationResult> ExportTransactionsToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Beginning export transactions to csv");
                if (!Directory.Exists(_exportPath))
                {
                    Directory.CreateDirectory(_exportPath);
                    _logger.LogInformation("Export directory created {path}", _exportPath);
                }

                var transactions = await _transactionService.GetTransactionsForExport(begin, end);

                var csv = new StringBuilder();
                foreach (var transaction in transactions.OrderBy(t => t.Documentnumber))
                {
                    csv.AppendLine(
                        $"{transaction.Documentnumber};{transaction.Description};{transaction.Sum};{transaction.AccountMovement}");
                }

                if (string.IsNullOrWhiteSpace(csv.ToString()))
                {
                    _logger.LogError("CSV file for transactions could not be created");
                    return _operationResultFactory.ExportFailed($"{_localizer["NoData"]}");
                }

                await using var sw = new StreamWriter(Path.Combine(_exportPath, filename));
                await sw.WriteLineAsync(CsvHeader);
                await sw.WriteAsync(csv);
                _logger.LogInformation("Export transactions to csv completed");
                
                return _operationResultFactory.ExportSuccessful(filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting transactions to csv");
                return _operationResultFactory.ExportFailed($"{_localizer["Exception"]}");
            }
        }

        
        public async Task<IOperationResult> ExportTransactionsToPdf(DateTime begin, DateTime end, string filename, 
            CancellationToken cancellationToken)
        {
            try
            {
                var transactions = await _transactionService.GetTransactionsForBudgetExport(begin, end);
                var filePath = Path.Combine(_exportPath, filename);

                await _transactionPdfRenderer.RenderTransactionPdfExportAsync(transactions, begin, end, filePath, cancellationToken);

                return _operationResultFactory.ExportSuccessful(filename);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("PDF export canceled → {File}", filename);
                return _operationResultFactory.Canceled();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF export failed");
                return _operationResultFactory.ExportFailed($"{_localizer["Exception"]}");
            }
        }
        
        
       public async Task<IOperationResult> ExportBudgetToCsv(DateTime begin, DateTime end, string filename)
        {
            try
            {
                _logger.LogInformation("Export budget to csv started");

                if (!Directory.Exists(_exportPath))
                    Directory.CreateDirectory(_exportPath);

                var transactions = await _transactionService.GetTransactionsForBudgetExport(begin, end);
                var flat = _budgetMapper.BuildFlatEntries(transactions);
                var grouped = _budgetMapper.BuildBudgetHierarchy(flat);

                var filePath = Path.Combine(_exportPath, filename);
                await _csvWriter.WriteAsync(filePath, grouped);
                _logger.LogInformation("Export budget to csv completed");
                
                return _operationResultFactory.ExportSuccessful(filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating budget csv");
                return _operationResultFactory.ExportFailed(ex.Message);
            }
        }

        public async Task<IOperationResult> ExportBudgetToExcel(DateTime begin, DateTime end, string filename)
        {
            try
            {
                Directory.CreateDirectory(_exportPath);

                var transactions = await _transactionService.GetTransactionsForBudgetExport(begin, end);
                var flatEntries = _budgetMapper.BuildFlatEntries(transactions);
                var grouped = _budgetMapper.BuildBudgetHierarchy(flatEntries);

                var filePath = Path.Combine(_exportPath, filename);
                await _excelWriter.WriteAsync(filePath, grouped, begin, end);

                _logger.LogInformation("Export budget to Excel completed");
                return _operationResultFactory.ExportSuccessful(filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export budget to Excel: Error creating Excel file");
                return _operationResultFactory.ExportFailed(ex.Message);
            }
        }
        
        public async Task<byte[]> ExportBudgetToExcelBytes(DateTime begin, DateTime end)
        {
            var filename = $"Budget_{begin:yyyyMMdd}_{end:yyyyMMdd}.xlsx";
            var result = await ExportBudgetToExcel(begin, end, filename);

            if (result.Status != OperationResultStatus.Success) 
                return Array.Empty<byte>();
            
            var filePath = Path.Combine(_exportPath, filename);
            return await File.ReadAllBytesAsync(filePath);
        }
    }
}


