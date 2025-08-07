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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class UpdateSnippetDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Code { get; set; }
    public string? Language { get; set; }
    public bool? IsPublic { get; set; }
    public List<string>? Tags { get; set; }
}