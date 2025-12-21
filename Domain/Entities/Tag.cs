using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Tag
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required] [MaxLength(100)] public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this is a pre-built system tag vs user-created custom tag
    /// </summary>
    public bool IsPrebuilt { get; set; }
    
    public ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
    
    /// <summary>
    /// Pre-built tags as per requirements
    /// </summary>
    public static readonly string[] PrebuiltTags =
    [
        "Work", "Career", "Studies", "Family", "Friends", "Relationships",
        "Health", "Fitness", "Personal Growth", "Self-care", "Hobbies", "Travel", "Nature",
        "Finance", "Spirituality", "Birthday", "Holiday", "Vacation", "Celebration", "Exercise",
        "Reading", "Writing", "Cooking", "Meditation", "Yoga", "Music", "Shopping", "Parenting",
        "Projects", "Planning", "Reflection"
    ];
}