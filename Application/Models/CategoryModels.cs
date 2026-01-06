namespace Application.Models;

/// <summary>
/// Model for creating/updating a category
/// </summary>
public class CategoryInputModel
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Model for displaying a category
/// </summary>
public class CategoryDisplayModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

