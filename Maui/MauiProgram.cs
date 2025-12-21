using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });
        
        var appDataDir = FileSystem.AppDataDirectory;
        Directory.CreateDirectory(appDataDir);

        var dbPath = Path.Combine(appDataDir, "MoodJournal.db");
        Console.WriteLine($"DB is at: {dbPath}");

        builder.Services.AddDbContext<JournalDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}")
        );

        builder.Services.AddScoped<IJournalService, JournalService>();
        
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.PreventDuplicates = false;
            config.SnackbarConfiguration.NewestOnTop = true;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 3000;
        });

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<JournalDbContext>();
        db.Database.Migrate();
        
        // Ensure prebuilt tags exist
        var journalService = scope.ServiceProvider.GetRequiredService<IJournalService>();
        journalService.EnsurePrebuiltTagsAsync().GetAwaiter().GetResult();

        return app;
    }
}