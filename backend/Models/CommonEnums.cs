namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 趋势方向枚举
/// </summary>
public enum TrendDirection
{
    /// <summary>
    /// 上升
    /// </summary>
    Up = 0,

    /// <summary>
    /// 下降
    /// </summary>
    Down = 1,

    /// <summary>
    /// 平稳
    /// </summary>
    Stable = 2
}

/// <summary>
/// 导出格式枚举
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// JSON格式
    /// </summary>
    Json = 0,

    /// <summary>
    /// CSV格式
    /// </summary>
    Csv = 1,

    /// <summary>
    /// Excel格式
    /// </summary>
    Excel = 2,

    /// <summary>
    /// XML格式
    /// </summary>
    Xml = 3,

    /// <summary>
    /// PDF格式
    /// </summary>
    Pdf = 4
}