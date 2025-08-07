using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 代码片段业务服务实现 - 遵循依赖倒置原则
/// </summary>
public class CodeSnippetService : ICodeSnippetService
{
    private readonly ICodeSnippetRepository _repository;

    public CodeSnippetService(ICodeSnippetRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<PaginatedResult<CodeSnippetDto>> GetSnippetsAsync(SnippetFilterDto filter)
    {
        var domainFilter = new SnippetFilter
        {
            Search = filter.Search,
            Language = filter.Language,
            Tag = filter.Tag,
            Page = filter.Page,
            PageSize = filter.PageSize
        };

        var result = await _repository.GetPagedAsync(domainFilter);
        
        return new PaginatedResult<CodeSnippetDto>
        {
            Items = result.Items.Select(MapToDto),
            TotalCount = result.TotalCount,
            Page = result.Page,
            PageSize = result.PageSize
        };
    }

    public async Task<CodeSnippetDto?> GetSnippetAsync(Guid id)
    {
        var snippet = await _repository.GetByIdAsync(id);
        return snippet != null ? MapToDto(snippet) : null;
    }

    public async Task<CodeSnippetDto> CreateSnippetAsync(CreateSnippetDto snippet)
    {
        var entity = new CodeSnippet
        {
            Id = Guid.NewGuid(),
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = Guid.NewGuid(), // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublic = snippet.IsPublic,
            ViewCount = 0,
            CopyCount = 0
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<CodeSnippetDto> UpdateSnippetAsync(Guid id, UpdateSnippetDto snippet)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new ArgumentException("Code snippet not found");

        if (!string.IsNullOrEmpty(snippet.Title))
            existing.Title = snippet.Title;
        if (!string.IsNullOrEmpty(snippet.Description))
            existing.Description = snippet.Description;
        if (!string.IsNullOrEmpty(snippet.Code))
            existing.Code = snippet.Code;
        if (!string.IsNullOrEmpty(snippet.Language))
            existing.Language = snippet.Language;
        if (snippet.IsPublic.HasValue)
            existing.IsPublic = snippet.IsPublic.Value;

        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteSnippetAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static CodeSnippetDto MapToDto(CodeSnippet snippet)
    {
        return new CodeSnippetDto
        {
            Id = snippet.Id,
            Title = snippet.Title,
            Description = snippet.Description,
            Code = snippet.Code,
            Language = snippet.Language,
            CreatedBy = snippet.CreatedBy,
            CreatedAt = snippet.CreatedAt,
            UpdatedAt = snippet.UpdatedAt,
            IsPublic = snippet.IsPublic,
            ViewCount = snippet.ViewCount,
            CopyCount = snippet.CopyCount
        };
    }
}