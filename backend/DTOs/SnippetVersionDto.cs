namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 代码片段版本数据传输对象
/// </summary>
public class SnippetVersionDto
{
    /// <summary>
    /// 版本ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 代码片段ID
    /// </summary>
    public Guid SnippetId { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public int VersionNumber { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 代码内容
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 编程语言
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 变更描述
    /// </summary>
    public string? ChangeDescription { get; set; }
}