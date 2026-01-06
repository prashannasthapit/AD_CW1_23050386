namespace Application.Models;

/// <summary>
/// Model for creating a tag
/// </summary>
public class TagInputModel
{
    public string Name { get; set; } = string.Empty;
    public bool IsPrebuilt { get; set; } = false;
}

/// <summary>
/// Model for displaying a tag
/// </summary>
public class TagDisplayModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsPrebuilt { get; set; }
}

