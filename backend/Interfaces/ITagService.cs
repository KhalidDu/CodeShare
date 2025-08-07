using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 标签业务服务接口 - 遵循迪米特法则
/// </summary>
public interface ITagService
{
    Task<IEnumerable<TagDto>> GetAllTagsAsync();
    Task<TagDto?> GetTagAsync(Guid id);
    Task<TagDto> CreateTagAsync(CreateTagDto tag);
    Task<TagDto> UpdateTagAsync(Guid id, UpdateTagDto tag);
    Task<bool> DeleteTagAsync(Guid id);
    Task<IEnumerable<TagDto>> GetTagsBySnippetIdAsync(Guid snippetId);
}