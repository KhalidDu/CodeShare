namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 用户实体 - 遵循单一职责原则，只负责用户数据
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}

public enum UserRole
{
    Viewer = 0,
    Editor = 1,
    Admin = 2
}