namespace TTCCashRegister.Data.Export;

public sealed class ExportPathProvider : IExportPathProvider
{
    public string ExportPath { get; }

    public ExportPathProvider(
        IConfiguration configuration,
        ILogger<ExportPathProvider> logger)
    {
        var exportBasePath = configuration["ExportSettings:ExportPath"] ?? "Exports";

        if (!Path.IsPathRooted(exportBasePath))
        {
            var projectDirectory = AppContext.BaseDirectory;
            var binDirectory = Directory.GetParent(projectDirectory)?.Parent?.Parent?.FullName;
            var basePath = binDirectory ?? projectDirectory;
            exportBasePath = Path.Combine(basePath, exportBasePath);
        }

        Directory.CreateDirectory(exportBasePath);
        ExportPath = exportBasePath;

        logger.LogInformation("Configured export path: {ExportPath}", ExportPath);
    }
}