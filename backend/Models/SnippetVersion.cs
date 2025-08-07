namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 代码片段版本实体
/// </summary>
public class SnippetVersion
{
    public Guid Id { get; set; }
    public Guid SnippetId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ChangeDescription { get; set; } = string.Empty;
}