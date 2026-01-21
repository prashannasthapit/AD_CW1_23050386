namespace Maui;

using PdfSharpCore.Fonts;
using System.IO;

public class MacFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        // Map common fonts to system font path
        string fontPath = faceName switch
        {
            "Arial" => "/System/Library/Fonts/SFNS.ttf",   // default system font
            "Times New Roman" => "/System/Library/Fonts/SFNS.ttf",
            _ => "/System/Library/Fonts/SFNS.ttf"
        };

        return File.Exists(fontPath) ? File.ReadAllBytes(fontPath) : null;
    }

    public string DefaultFontName { get; }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // Return a key — must match one of the fonts in GetFont
        return new FontResolverInfo("Arial");
    }
}
