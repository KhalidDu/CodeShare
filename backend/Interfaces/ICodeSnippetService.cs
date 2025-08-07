using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 代码片段业务服务接口 - 遵循接口隔离原则
/// </summary>
public interface ICodeSnippetService
{
    Task<PaginatedResult<CodeSnippetDto>> GetSnippetsAsync(SnippetFilterDto filter, Guid? currentUserId = null);
    Task<CodeSnippetDto?> GetSnippetAsync(Guid id, Guid? currentUserId = null);
    Task<CodeSnippetDto> CreateSnippetAsync(CreateSnippetDto snippet, Guid currentUserId);
    Task<CodeSnippetDto> UpdateSnippetAsync(Guid id, UpdateSnippetDto snippet, Guid currentUserId);
    Task<bool> DeleteSnippetAsync(Guid id, Guid currentUserId);
    Task<IEnumerable<CodeSnippetDto>> GetUserSnippetsAsync(Guid userId, Guid? currentUserId = null);
    Task<bool> IncrementViewCountAsync(Guid id, Guid? currentUserId = null);
    Task<bool> IncrementCopyCountAsync(Guid id, Guid? currentUserId = null);
}