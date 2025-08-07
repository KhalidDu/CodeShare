using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 批量获取复制统计请求数据传输对象
/// </summary>
public class BatchCopyStatsRequestDto
{
    /// <summary>
    /// 代码片段ID列表
    /// </summary>
    [Required(ErrorMessage = "代码片段ID列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要提供一个代码片段ID")]
    [MaxLength(100, ErrorMessage = "一次最多查询100个代码片段")]
    public IEnumerable<Guid> SnippetIds { get; set; } = new List<Guid>();
}