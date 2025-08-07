using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 代码片段仓储接口 - 遵循开闭原则和接口隔离原则
/// </summary>
public interface ICodeSnippetRepository
{
    Task<CodeSnippet?> GetByIdAsync(Guid id);
    Task<PaginatedResult<CodeSnippet>> GetPagedAsync(SnippetFilter filter);
    Task<CodeSnippet> CreateAsync(CodeSnippet snippet);
    Task<CodeSnippet> UpdateAsync(CodeSnippet snippet);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<CodeSnippet>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<CodeSnippet>> GetByTagAsync(string tag);
}