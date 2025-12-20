using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Tag
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required] public string Name { get; set; } = string.Empty;
    
    public ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
}