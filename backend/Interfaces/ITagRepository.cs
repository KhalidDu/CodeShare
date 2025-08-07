using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 标签仓储接口 - 遵循里氏替换原则
/// </summary>
public interface ITagRepository
{
    Task<Tag?> GetByIdAsync(Guid id);
    Task<Tag?> GetByNameAsync(string name);
    Task<IEnumerable<Tag>> GetAllAsync();
    Task<Tag> CreateAsync(Tag tag);
    Task<Tag> UpdateAsync(Tag tag);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Tag>> GetTagsBySnippetIdAsync(Guid snippetId);
}