using iText.Kernel.Colors;

namespace TTCCashRegister.Data.Export;

public static class PdfColors
{
    public static readonly DeviceRgb RowColorEven = new DeviceRgb(250, 250, 250);
    public static readonly DeviceRgb RowColorOdd = new DeviceRgb(220,220, 220);
    public static readonly DeviceRgb PositivSum = new DeviceRgb(0, 150, 0);
    public static readonly DeviceRgb NegativSum = new DeviceRgb(225, 0, 0);
    public static readonly DeviceRgb HeaderColor = new DeviceRgb(221, 235, 247);
}