using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 代码片段仓储接口 - 遵循开闭原则和接口隔离原则
/// </summary>
public interface ICodeSnippetRepository
{
    Task<CodeSnippet?> GetByIdAsync(Guid id);
    Task<CodeSnippet?> GetByIdWithTagsAsync(Guid id);
    Task<PaginatedResult<CodeSnippet>> GetPagedAsync(SnippetFilter filter);
    Task<CodeSnippet> CreateAsync(CodeSnippet snippet);
    Task<CodeSnippet> UpdateAsync(CodeSnippet snippet);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<CodeSnippet>> GetByTagAsync(string tag);
    Task<bool> AddTagToSnippetAsync(Guid snippetId, Guid tagId);
    Task<bool> RemoveTagFromSnippetAsync(Guid snippetId, Guid tagId);
    Task<bool> UpdateSnippetTagsAsync(Guid snippetId, IEnumerable<Guid> tagIds);
    Task<bool> IncrementViewCountAsync(Guid id);
    Task<bool> IncrementCopyCountAsync(Guid id);
}