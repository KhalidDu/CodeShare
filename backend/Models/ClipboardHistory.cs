namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 剪贴板历史实体
/// </summary>
public class ClipboardHistory
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid SnippetId { get; set; }
    public DateTime CopiedAt { get; set; }
}