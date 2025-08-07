using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 版本管理服务接口 - 遵循开闭原则
/// </summary>
public interface IVersionManagementService
{
    /// <summary>
    /// 创建代码片段的初始版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>创建的版本</returns>
    Task<SnippetVersionDto> CreateInitialVersionAsync(Guid snippetId);

    /// <summary>
    /// 创建新版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="changeDescription">变更描述</param>
    /// <returns>创建的版本</returns>
    Task<SnippetVersionDto> CreateVersionAsync(Guid snippetId, string changeDescription);

    /// <summary>
    /// 获取代码片段的版本历史
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>版本历史列表</returns>
    Task<IEnumerable<SnippetVersionDto>> GetVersionHistoryAsync(Guid snippetId);

    /// <summary>
    /// 获取特定版本详情
    /// </summary>
    /// <param name="versionId">版本ID</param>
    /// <returns>版本详情</returns>
    Task<SnippetVersionDto?> GetVersionAsync(Guid versionId);

    /// <summary>
    /// 恢复到指定版本
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <param name="versionId">要恢复的版本ID</param>
    /// <returns>恢复后的代码片段</returns>
    Task<bool> RestoreVersionAsync(Guid snippetId, Guid versionId);
}