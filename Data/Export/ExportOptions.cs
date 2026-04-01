namespace ClubTreasury.Data.Export;

public record ExportOptions(DateTime Begin, DateTime End, string Filename, int CashRegisterId);

public record PdfExportOptions(
    DateTime Begin,
    DateTime End,
    string Filename,
    int CashRegisterId,
    string CashRegisterName,
    string? TreasurerName) : ExportOptions(Begin, End, Filename, CashRegisterId);