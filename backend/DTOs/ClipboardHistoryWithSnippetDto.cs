namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 包含代码片段信息的剪贴板历史数据传输对象
/// </summary>
public class ClipboardHistoryWithSnippetDto
{
    /// <summary>
    /// 历史记录ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }

    /// <summary>
    /// 复制时间
    /// </summary>
    public DateTime CopiedAt { get; set; }

    /// <summary>
    /// 代码片段信息（用于显示）
    /// </summary>
    public CodeSnippetDto? Snippet { get; set; }
}