using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class User
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required] [MaxLength(100)] public string Username { get; set; } = "Default";

    /// <summary>
    /// Hashed PIN or password for app access
    /// </summary>
    [MaxLength(255)] public string? Pin { get; set; }
    
    /// <summary>
    /// User's preferred theme: "Light" or "Dark"
    /// </summary>
    [MaxLength(20)] public string Theme { get; set; } = "Light";
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}