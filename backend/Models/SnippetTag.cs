namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 代码片段标签关联实体
/// </summary>
public class SnippetTag
{
    public Guid SnippetId { get; set; }
    public Guid TagId { get; set; }
}