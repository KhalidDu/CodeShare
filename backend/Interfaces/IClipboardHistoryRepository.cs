using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 剪贴板历史仓储接口 - 遵循迪米特法则
/// </summary>
public interface IClipboardHistoryRepository
{
    /// <summary>
    /// 根据ID获取剪贴板历史记录
    /// </summary>
    /// <param name="id">记录ID</param>
    /// <returns>剪贴板历史记录</returns>
    Task<ClipboardHistory?> GetByIdAsync(Guid id);

    /// <summary>
    /// 获取用户的剪贴板历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="limit">返回记录数限制</param>
    /// <returns>剪贴板历史列表</returns>
    Task<IEnumerable<ClipboardHistory>> GetByUserIdAsync(Guid userId, int limit = 50);

    /// <summary>
    /// 创建剪贴板历史记录
    /// </summary>
    /// <param name="history">剪贴板历史实体</param>
    /// <returns>创建的记录</returns>
    Task<ClipboardHistory> CreateAsync(ClipboardHistory history);

    /// <summary>
    /// 删除用户的所有剪贴板历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteByUserIdAsync(Guid userId);

    /// <summary>
    /// 删除过期的剪贴板历史记录
    /// </summary>
    /// <param name="cutoffDate">截止日期</param>
    /// <returns>删除的记录数</returns>
    Task<int> DeleteExpiredAsync(DateTime cutoffDate);

    /// <summary>
    /// 获取代码片段的复制统计
    /// </summary>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>复制次数</returns>
    Task<int> GetCopyCountAsync(Guid snippetId);

    /// <summary>
    /// 获取用户的剪贴板历史记录数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>历史记录数量</returns>
    Task<int> GetUserHistoryCountAsync(Guid userId);

    /// <summary>
    /// 删除用户最旧的剪贴板历史记录
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="countToDelete">要删除的记录数</param>
    /// <returns>实际删除的记录数</returns>
    Task<int> DeleteOldestUserRecordsAsync(Guid userId, int countToDelete);

    /// <summary>
    /// 批量获取多个代码片段的复制统计
    /// </summary>
    /// <param name="snippetIds">代码片段ID列表</param>
    /// <returns>代码片段ID和复制次数的字典</returns>
    Task<Dictionary<Guid, int>> GetCopyCountsBatchAsync(IEnumerable<Guid> snippetIds);
}