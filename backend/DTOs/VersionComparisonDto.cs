namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 版本比较结果数据传输对象
/// </summary>
public class VersionComparisonDto
{
    /// <summary>
    /// 源版本信息
    /// </summary>
    public SnippetVersionDto FromVersion { get; set; } = new();

    /// <summary>
    /// 目标版本信息
    /// </summary>
    public SnippetVersionDto ToVersion { get; set; } = new();

    /// <summary>
    /// 标题是否有变化
    /// </summary>
    public bool TitleChanged { get; set; }

    /// <summary>
    /// 描述是否有变化
    /// </summary>
    public bool DescriptionChanged { get; set; }

    /// <summary>
    /// 代码是否有变化
    /// </summary>
    public bool CodeChanged { get; set; }

    /// <summary>
    /// 语言是否有变化
    /// </summary>
    public bool LanguageChanged { get; set; }

    /// <summary>
    /// 代码差异详情 (简单的行级差异)
    /// </summary>
    public List<CodeDiffLine> CodeDifferences { get; set; } = new();
}

/// <summary>
/// 代码差异行
/// </summary>
public class CodeDiffLine
{
    /// <summary>
    /// 行号
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// 差异类型：Added, Removed, Modified, Unchanged
    /// </summary>
    public string DiffType { get; set; } = string.Empty;

    /// <summary>
    /// 源版本的行内容
    /// </summary>
    public string? FromContent { get; set; }

    /// <summary>
    /// 目标版本的行内容
    /// </summary>
    public string? ToContent { get; set; }
}