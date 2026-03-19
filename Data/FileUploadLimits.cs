namespace ClubTreasury.Data;

public static class FileUploadLimits
{
    public const long LogoMaxSize = 512 * 1024;           // 512 KB
    public const long ImportTextMaxSize = 5 * 1024 * 1024; // 5 MB
    public const long ImportExcelMaxSize = 10 * 1024 * 1024; // 10 MB
}