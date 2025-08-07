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
    private readonly IConfiguration _configuration;

    // 默认的历史记录限制数量
    private const int DefaultMaxHistoryCount = 100;

    public ClipboardService(
        IClipboardHistoryRepository clipboardRepository,
        ICodeSnippetRepository snippetRepository,
        IConfiguration configuration)
    {
        _clipboardRepository = clipboardRepository ?? throw new ArgumentNullException(nameof(clipboardRepository));
        _snippetRepository = snippetRepository ?? throw new ArgumentNullException(nameof(snippetRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 记录代码片段复制操作 - 包含历史记录限制逻辑
    /// 注意：此方法只记录历史，不更新复制计数（复制计数由 CodeSnippetService 处理）
    /// </summary>
    public async Task<ClipboardHistoryDto> RecordCopyAsync(Guid userId, Guid snippetId)
    {
        // 验证代码片段是否存在
        var snippet = await _snippetRepository.GetByIdAsync(snippetId);
        if (snippet == null)
        {
            throw new ArgumentException($"代码片段 {snippetId} 不存在", nameof(snippetId));
        }

        // 获取历史记录限制配置
        var maxHistoryCount = _configuration.GetValue<int>("ClipboardSettings:MaxHistoryCount", DefaultMaxHistoryCount);

        // 检查用户当前的历史记录数量
        var currentCount = await _clipboardRepository.GetUserHistoryCountAsync(userId);

        // 如果达到限制，删除最旧的记录以腾出空间
        if (currentCount >= maxHistoryCount)
        {
            var recordsToDelete = currentCount - maxHistoryCount + 1;
            await _clipboardRepository.DeleteOldestUserRecordsAsync(userId, recordsToDelete);
        }

        var history = new ClipboardHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            SnippetId = snippetId,
            CopiedAt = DateTime.UtcNow
        };

        var createdHistory = await _clipboardRepository.CreateAsync(history);

        // 注意：复制计数的更新由 CodeSnippetService.IncrementCopyCountAsync 处理
        // 这里不重复更新，避免双重计数

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
    /// 获取用户的剪贴板历史记录数量
    /// </summary>
    public async Task<int> GetUserHistoryCountAsync(Guid userId)
    {
        return await _clipboardRepository.GetUserHistoryCountAsync(userId);
    }

    /// <summary>
    /// 批量获取代码片段的复制统计 - 使用批量操作优化性能
    /// </summary>
    public async Task<Dictionary<Guid, int>> GetCopyCountsBatchAsync(IEnumerable<Guid> snippetIds)
    {
        if (snippetIds == null || !snippetIds.Any())
        {
            return new Dictionary<Guid, int>();
        }

        return await _clipboardRepository.GetCopyCountsBatchAsync(snippetIds);
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