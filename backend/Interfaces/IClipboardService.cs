using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 剪贴板历史服务接口 - 遵循迪米特法则
/// </summary>
public interface IClipboardService
{
    /// <summary>
    /// 记录代码片段复制操作
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="snippetId">代码片段ID</param>
    /// <returns>复制记录</returns>
    Task<ClipboardHistoryDto> RecordCopyAsync(Guid userId, Guid snippetId);

    /// <summary>
    /// 获取用户的剪贴板历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="limit">返回记录数限制</param>
    /// <returns>剪贴板历史列表</returns>
    Task<IEnumerable<ClipboardHistoryDto>> GetUserClipboardHistoryAsync(Guid userId, int limit = 50);

    /// <summary>
    /// 清空用户的剪贴板历史
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否成功</returns>
    Task<bool> ClearUserClipboardHistoryAsync(Guid userId);

    /// <summary>
    /// 删除过期的剪贴板历史记录
    /// </summary>
    /// <param name="daysToKeep">保留天数</param>
    /// <returns>删除的记录数</returns>
    Task<int> CleanupExpiredHistoryAsync(int daysToKeep = 30);
}