using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class JournalDbContext : DbContext
{
    public DbSet<JournalEntry> JournalEntries { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<EntryTag> EntryTags { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<EntryTag>()
            .HasKey(et => new { et.JournalEntryId, et.TagId });

        modelBuilder.Entity<EntryTag>()
            .HasOne(et => et.JournalEntry)
            .WithMany(j => j.EntryTags)
            .HasForeignKey(et => et.JournalEntryId);

        modelBuilder.Entity<EntryTag>()
            .HasOne(et => et.Tag)
            .WithMany(t => t.EntryTags)
            .HasForeignKey(et => et.TagId);
        
        // Index on EntryDate to enforce one entry per day: application must ensure uniqueness per date.
        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => j.EntryDate)
            .IsUnique(false); // We'll enforce uniqueness at app level by comparing Date parts.
            
        // Configure Tag entity
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();
    }
    
    private readonly string _dbPath;

    public JournalDbContext()
    {
        var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _dbPath = Path.Combine(folder, "MoodJournal.db");
        Console.WriteLine("Database Path: " + _dbPath);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }
}