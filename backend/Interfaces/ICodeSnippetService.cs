using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 代码片段业务服务接口 - 遵循接口隔离原则
/// </summary>
public interface ICodeSnippetService
{
    Task<PaginatedResult<CodeSnippetDto>> GetSnippetsAsync(SnippetFilterDto filter);
    Task<CodeSnippetDto?> GetSnippetAsync(Guid id);
    Task<CodeSnippetDto> CreateSnippetAsync(CreateSnippetDto snippet);
    Task<CodeSnippetDto> UpdateSnippetAsync(Guid id, UpdateSnippetDto snippet);
    Task<bool> DeleteSnippetAsync(Guid id);
}