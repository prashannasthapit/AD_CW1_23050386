using Application.Interfaces;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace Application.Services;

public class PdfService : IPdfService
{
    public Task<string> GeneratePdfAsync(string html, string filePath)
    {
        var pdf = new PdfDocument();

        // ✅ Configure PDF page size / margins
        var config = new PdfGenerateConfig
        {
            PageSize = PdfSharpCore.PageSize.A4,
            MarginLeft = 20,
            MarginRight = 20,
            MarginTop = 20,
            MarginBottom = 20
        };

        PdfGenerator.AddPdfPages(
            pdf, // PDF document
            html, // HTML content
            config, // configuration
            null, // CssData (optional)
            null, // Stylesheet load event
            null // Image load event
        );
        
        pdf.Save(filePath);

        Console.WriteLine($"Generated pdf file: {filePath}");

        return Task.FromResult(filePath);
    }
}