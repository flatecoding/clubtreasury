namespace TTCCashRegister.Data.Source;

public static class UTools
{
    public static DateTime GetBeginDateTime()
    {
        return (DateTime.Today.Month is >= 1 && DateTime.Today.Month < 7)
            ? new DateTime(DateTime.Today.Year - 1, 7, 1)
            : new DateTime(DateTime.Today.Year, 7, 1);
    }
    
}