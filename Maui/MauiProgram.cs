using Application.Interfaces;
using Application.Services;
using Infrastructure.Data;
using Microsoft.Extensions.Logging;
using PdfSharpCore.Fonts;

namespace Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        GlobalFontSettings.FontResolver = new MacFontResolver();
        
        // Add DbContext
        builder.Services.AddDbContext<JournalDbContext>();

        // Add Data Access
        builder.Services.AddScoped<IJournalDbAccess, JournalDbAccess>();

        // Add Services
        builder.Services.AddSingleton<IThemeService, ThemeService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<IJournalService, JournalService>();
        builder.Services.AddSingleton<IPdfService, PdfService>();
        
        var app = builder.Build();

        // Ensure database is created
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
        db.Database.EnsureCreated();

        return app;
    }
}