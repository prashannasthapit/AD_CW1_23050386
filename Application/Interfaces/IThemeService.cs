namespace Application.Interfaces;

public interface IThemeService
{
    string CurrentTheme { get; }
    event Action? OnThemeChanged;
    void SetTheme(string theme);
}

