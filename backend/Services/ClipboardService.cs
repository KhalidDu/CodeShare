using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 剪贴板历史服务实现 - 遵循迪米特法则
/// </summary>
public class ClipboardService : IClipboardService
{
    private readonly IClipboardHistoryRepository _clipboardRepository;
    private readonly ICodeSnippetRepository _snippetRepository;

    public ClipboardService(
        IClipboardHistoryRepository clipboardRepository,
        ICodeSnippetRepository snippetRepository)
    {
        _clipboardRepository = clipboardRepository ?? throw new ArgumentNullException(nameof(clipboardRepository));
        _snippetRepository = snippetRepository ?? throw new ArgumentNullException(nameof(snippetRepository));
    }

    /// <summary>
    /// 记录代码片段复制操作
    /// </summary>
    public async Task<ClipboardHistoryDto> RecordCopyAsync(Guid userId, Guid snippetId)
    {
        // 验证代码片段是否存在
        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        if (snippet == null)
        {
            throw new ArgumentException($"代码片段 {snippetId} 不存在", nameof(snippetId));
        }

        var history = new ClipboardHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SnippetId = snippetId,
            CopiedAt = DateTime.UtcNow
        };

        var createdHistory = await _clipboardRepository.CreateAsync(history);

        // 更新代码片段的复制计数
        snippet.CopyCount++;
        await _snippetRepository.UpdateAsync(snippet);

        return MapToDto(createdHistory);
    }

    /// <summary>
    /// 获取用户的剪贴板历史
    /// </summary>
    public async Task<IEnumerable<ClipboardHistoryDto>> GetUserClipboardHistoryAsync(Guid userId, int limit = 50)
    {
        if (limit <= 0 || limit > 100)
        {
            limit = 50; // 默认限制为50条记录
        }

        var histories = await _clipboardRepository.GetByUserIdAsync(userId, limit);
        return histories.Select(MapToDto);
    }

    /// <summary>
    /// 清空用户的剪贴板历史
    /// </summary>
    public async Task<bool> ClearUserClipboardHistoryAsync(Guid userId)
    {
        return await _clipboardRepository.DeleteByUserIdAsync(userId);
    }

    /// <summary>
    /// 删除过期的剪贴板历史记录
    /// </summary>
    public async Task<int> CleanupExpiredHistoryAsync(int daysToKeep = 30)
    {
        if (daysToKeep <= 0)
        {
            daysToKeep = 30; // 默认保留30天
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
        return await _clipboardRepository.DeleteExpiredAsync(cutoffDate);
    }

    /// <summary>
    /// 将实体映射为DTO
    /// </summary>
    private static ClipboardHistoryDto MapToDto(ClipboardHistory history)
    {
        return new ClipboardHistoryDto
        {
            Id = history.Id,
            UserId = history.UserId,
            SnippetId = history.SnippetId,
            CopiedAt = history.CopiedAt
        };
    }
}