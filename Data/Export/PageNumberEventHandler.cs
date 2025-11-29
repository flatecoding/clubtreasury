using iText.Kernel.Events;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace TTCCashRegister.Data.Export;

public class PageNumberEventHandler : IPageNumberEventHandler
{
    private readonly PdfFont _font;
    private readonly int _fontSize;
    private readonly PdfFormXObject _placeholder;
    private readonly float _pageCounterBottomMargin;
    private readonly float _placeholderWidth;
    private const string Page = "Seite";

    public PageNumberEventHandler(PdfFont font, int fontSize = 9, float footerPageCounterBottomMargin = 20f, float placeholderWidth = 50f)
    {
        _font = font;
        _fontSize = fontSize;
        _pageCounterBottomMargin = footerPageCounterBottomMargin;
        _placeholderWidth = placeholderWidth;
        _placeholder = new PdfFormXObject(new Rectangle(0, 0, _placeholderWidth, _fontSize + 4));
    }

    public void HandleEvent(Event currentEvent)
    {
        var docEvent = (PdfDocumentEvent)currentEvent;
        var pdf = docEvent.GetDocument();
        var page = docEvent.GetPage();
        var pageNumber = pdf.GetPageNumber(page);

        var pageSize = page.GetPageSize();
        var pageWidth = pageSize.GetWidth();
        
        var currentPage = $"{Page} {pageNumber} / ";
        var currentPageTextWidth = _font.GetWidth(currentPage, _fontSize);
        var totalPageCounterTextWidth = currentPageTextWidth + _placeholderWidth;
        var textStartPosition = (pageWidth - totalPageCounterTextWidth) / 2f;
        
        var pdfCanvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdf);
        var canvas = new Canvas(pdfCanvas, pageSize);
        
        var paragraph = new Paragraph(currentPage)
            .SetFont(_font)
            .SetFontSize(_fontSize)
            .SetMargin(0)
            .SetMultipliedLeading(1);

        canvas.ShowTextAligned(paragraph, textStartPosition, _pageCounterBottomMargin, TextAlignment.LEFT);
        
        pdfCanvas.AddXObjectAt(_placeholder, textStartPosition + currentPageTextWidth, _pageCounterBottomMargin);

        canvas.Close();
    }
    
    public void WriteTotal(PdfDocument pdf)
    {
        var totalPages = pdf.GetNumberOfPages();

        // In das FormXObject schreiben
        var canvas = new Canvas(_placeholder, pdf);
        var paragraph = new Paragraph(totalPages.ToString())
            .SetFont(_font)
            .SetFontSize(_fontSize)
            .SetMargin(0);

        canvas.ShowTextAligned(paragraph, 0, 0, TextAlignment.LEFT);
        canvas.Close();
    }
}