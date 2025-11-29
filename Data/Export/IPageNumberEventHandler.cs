using iText.Kernel.Events;
using iText.Kernel.Pdf;

namespace TTCCashRegister.Data.Export;

public interface IPageNumberEventHandler : IEventHandler
{
    void HandleEvent(Event currentEvent);
    void WriteTotal(PdfDocument pdf);
}