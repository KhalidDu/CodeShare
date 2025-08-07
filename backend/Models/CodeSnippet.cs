namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 代码片段实体 - 遵循单一职责原则，只负责代码片段数据
/// </summary>
public class CodeSnippet
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublic { get; set; }
    public int ViewCount { get; set; }
    public int CopyCount { get; set; }
    
    // 导航属性 - 用于 Multi-mapping 查询结果
    public string CreatorName { get; set; } = string.Empty;
    public List<Tag> Tags { get; set; } = new();
}