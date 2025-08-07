namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 标签实体
/// </summary>
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007bff";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}