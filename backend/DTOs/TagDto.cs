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
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007bff";
}

public class UpdateTagDto
{
    public string? Name { get; set; }
    public string? Color { get; set; }
}