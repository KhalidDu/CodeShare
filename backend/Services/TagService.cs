using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 标签业务服务实现 - 遵循迪米特法则
/// </summary>
public class TagService : ITagService
{
    private readonly ITagRepository _repository;

    public TagService(ITagRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        var tags = await _repository.GetAllAsync();
        return tags.Select(MapToDto);
    }

    public async Task<TagDto?> GetTagAsync(Guid id)
    {
        var tag = await _repository.GetByIdAsync(id);
        return tag != null ? MapToDto(tag) : null;
    }

    public async Task<TagDto> CreateTagAsync(CreateTagDto tag)
    {
        var entity = new Tag
        {
            Id = Guid.NewGuid(),
            Name = tag.Name,
            Color = tag.Color,
            CreatedBy = Guid.NewGuid(), // TODO: Get from current user context
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.CreateAsync(entity);
        return MapToDto(created);
    }

    public async Task<TagDto> UpdateTagAsync(Guid id, UpdateTagDto tag)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new ArgumentException("Tag not found");

        if (!string.IsNullOrEmpty(tag.Name))
            existing.Name = tag.Name;
        if (!string.IsNullOrEmpty(tag.Color))
            existing.Color = tag.Color;

        var updated = await _repository.UpdateAsync(existing);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteTagAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<TagDto>> GetTagsBySnippetIdAsync(Guid snippetId)
    {
        var tags = await _repository.GetTagsBySnippetIdAsync(snippetId);
        return tags.Select(MapToDto);
    }

    private static TagDto MapToDto(Tag tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Color = tag.Color,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt
        };
    }
}