namespace ClubTreasury.Data.Export.Transaction;

public record PdfRenderOptions(
    DateTime Begin,
    DateTime End,
    string FilePath,
    string CashRegisterName,
    string? TreasurerName,
    byte[]? LogoData,
    string? LogoContentType);