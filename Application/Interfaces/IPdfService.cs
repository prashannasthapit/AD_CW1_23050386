namespace Application.Interfaces;

public interface IPdfService
{
    Task<string> GeneratePdfAsync(string html, string filePath);
}

