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

/// <summary>
/// 标签使用统计信息
/// </summary>
public class TagUsageStatistic
{
    public Guid TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string TagColor { get; set; } = "#007bff";
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}