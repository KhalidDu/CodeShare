using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 代码片段版本仓储接口 - 遵循开闭原则
/// </summary>
public interface ISnippetVersionRepository
{
    /// <summary>
    /// 根据ID获取版本
    /// </summary>
    /// <param name="id">版本ID</param>
    /// <returns>版本实体</returns>
    Task<SnippetVersion?> GetByIdAsync(Guid id);

    /// <summary>
    /// 获取代码片段的所有版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>版本列表</returns>
    Task<IEnumerable<SnippetVersion>> GetBySnippetIdAsync(Guid snippetId);

    /// <summary>
    /// 获取代码片段的最新版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>最新版本</returns>
    Task<SnippetVersion?> GetLatestVersionAsync(Guid snippetId);

    /// <summary>
    /// 创建新版本
    /// </summary>
    /// <param name="version">版本实体</param>
    /// <returns>创建的版本</returns>
    Task<SnippetVersion> CreateAsync(SnippetVersion version);

    /// <summary>
    /// 获取下一个版本号
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>下一个版本号</returns>
    Task<int> GetNextVersionNumberAsync(Guid snippetId);

    /// <summary>
    /// 删除代码片段的所有版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteBySnippetIdAsync(Guid snippetId);

    /// <summary>
    /// 在事务中创建新版本
    /// </summary>
    /// <param name="version">版本实体</param>
    /// <param name="transaction">数据库事务</param>
    /// <returns>创建的版本</returns>
    Task<SnippetVersion> CreateWithTransactionAsync(SnippetVersion version, System.Data.IDbTransaction transaction);
}