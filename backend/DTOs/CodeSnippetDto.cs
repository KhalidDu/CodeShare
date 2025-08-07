using System.ComponentModel.DataAnnotations;

namespace CodeSnippetManager.Api.DTOs;

public class CodeSnippetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public Guid CreatedBy { get; set; }
    public string CreatorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsPublic { get; set; }
    public int ViewCount { get; set; }
    public int CopyCount { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}

public class CreateSnippetDto
{
    [Required(ErrorMessage = "标题不能为空")]
    [StringLength(200, ErrorMessage = "标题长度不能超过200个字符")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "代码内容不能为空")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "编程语言不能为空")]
    [StringLength(50, ErrorMessage = "编程语言长度不能超过50个字符")]
    public string Language { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public List<string> Tags { get; set; } = new();
}

public class UpdateSnippetDto
{
    [StringLength(200, ErrorMessage = "标题长度不能超过200个字符")]
    public string? Title { get; set; }

    [StringLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    public string? Code { get; set; }

    [StringLength(50, ErrorMessage = "编程语言长度不能超过50个字符")]
    public string? Language { get; set; }

    public bool? IsPublic { get; set; }

    public List<string>? Tags { get; set; }
}