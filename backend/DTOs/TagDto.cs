using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007bff";
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTagDto
{
    /// <summary>
    /// 标签名称
    /// </summary>
    [Required(ErrorMessage = "标签名称不能为空")]
    [StringLength(50, ErrorMessage = "标签名称长度不能超过50个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 标签颜色（十六进制格式）
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "颜色格式无效，请使用十六进制格式（如：#007bff）")]
    public string Color { get; set; } = "#007bff";
}

public class UpdateTagDto
{
    /// <summary>
    /// 标签名称
    /// </summary>
    [StringLength(50, ErrorMessage = "标签名称长度不能超过50个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 标签颜色（十六进制格式）
    /// </summary>
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "颜色格式无效，请使用十六进制格式（如：#007bff）")]
    public string? Color { get; set; }
}

/// <summary>
/// 标签使用统计DTO
/// </summary>
public class TagUsageDto
{
    public Guid TagId { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string TagColor { get; set; } = "#007bff";
    public int UsageCount { get; set; }
    public DateTime CreatedAt { get; set; }
}