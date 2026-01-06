namespace Application.Models;

/// <summary>
/// Model for user login/registration input
/// </summary>
public class UserLoginModel
{
    public string Username { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
}

/// <summary>
/// Model for displaying user information
/// </summary>
public class UserDisplayModel
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Theme { get; set; } = "Light";
    public string CreatedAt { get; set; } = string.Empty;
}
