using Application.Interfaces;

namespace Application.Services;

public class ThemeService : IThemeService
{
    private string _currentTheme = "light";
    
    public string CurrentTheme => _currentTheme;
    
    public event Action? OnThemeChanged;

    public void SetTheme(string theme)
    {
        if (_currentTheme != theme)
        {
            _currentTheme = theme;
            OnThemeChanged?.Invoke();
        }
    }
}

