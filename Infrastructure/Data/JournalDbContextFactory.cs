using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Data;

public class JournalDbContextFactory
    : IDesignTimeDbContextFactory<JournalDbContext>
{
    public JournalDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var dbDir = Path.Combine(basePath, "Data", "SqliteDb");
        Directory.CreateDirectory(dbDir); // IMPORTANT

        var dbPath = Path.Combine(dbDir, "MoodJournal.db");

        var options = new DbContextOptionsBuilder<JournalDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new JournalDbContext(options);
    }
}
