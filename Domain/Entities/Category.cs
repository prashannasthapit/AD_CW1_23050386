using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Category
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required] [Length(1,200)] public string Name { get; set; } = string.Empty;

    public ICollection<JournalEntry> Entries { get; set; } = new List<JournalEntry>();
}