using iText.Kernel.Events;
using iText.Kernel.Pdf;

namespace TTCCashRegister.Data.Export;

public interface IPageNumberEventHandler : IEventHandler
{
    void WriteTotal(PdfDocument pdf);
}