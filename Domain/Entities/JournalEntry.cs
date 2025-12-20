using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class JournalEntry
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();

    // The date this entry represents (one entry per calendar day)
    [Required]
    public DateTime EntryDate { get; set; }

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public string Title { get; set; } = string.Empty;

    // Content can be Markdown or HTML/Rich text. Use IsMarkdown to determine storage/renderer.
    public string Content { get; set; } = string.Empty;
    public bool IsMarkdown { get; set; } = true;
    
    // Primary mood required
    public required Mood PrimaryMood { get; set; }

    // Up to 2 secondary moods — stored as comma-separated enum names for simplicity.
    // You can migrate this to a proper join table if you want richer queries.
    public string? SecondaryMoods { get; set; }

    // Category (optional)
    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
    
    // Tags many-to-many via EntryTag
    public ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
    
    [NotMapped]
    public int WordCount => string.IsNullOrWhiteSpace(Content) ? 0 : Content.Split(new[] {' ', '\n', '\r'}, StringSplitOptions.RemoveEmptyEntries).Length;
}