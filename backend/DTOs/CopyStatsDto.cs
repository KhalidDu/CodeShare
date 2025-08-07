namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 复制统计数据传输对象
/// </summary>
public class CopyStatsDto
{
    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }

    /// <summary>
    /// 总复制次数
    /// </summary>
    public int TotalCopyCount { get; set; }

    /// <summary>
    /// 查看次数
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// 最后复制时间（可选）
    /// </summary>
    public DateTime? LastCopiedAt { get; set; }
}