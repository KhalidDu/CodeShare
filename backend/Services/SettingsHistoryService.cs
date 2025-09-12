using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.DTOs;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Services;

/// <summary>
/// 设置变更历史服务实现 - 遵循单一职责原则
/// </summary>
public class SettingsHistoryService : ISettingsHistoryService
{
    private readonly ISystemSettingsRepository _settingsRepository;
    private readonly ILogger<SettingsHistoryService> _logger;

    public SettingsHistoryService(
        ISystemSettingsRepository settingsRepository,
        ILogger<SettingsHistoryService> logger)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SettingsHistoryDto> CreateChangeHistoryAsync(CreateSettingsHistoryRequest request)
    {
        try
        {
            var history = await _settingsRepository.RecordChangeHistoryAsync(request);
            var dto = MapToDto(history);
            
            _logger.LogInformation("设置变更历史已记录: {SettingType} - {SettingKey}", 
                request.SettingType, request.SettingKey);
            
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录设置变更历史失败");
            throw;
        }
    }

    public async Task<SettingsHistoryDto?> GetChangeHistoryByIdAsync(Guid id)
    {
        try
        {
            var history = await _settingsRepository.GetHistoryByIdAsync(id);
            return history != null ? MapToDto(history) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置变更历史失败: {Id}", id);
            throw;
        }
    }

    public async Task<SettingsHistoryResponse> GetChangeHistoryAsync(SettingsHistoryRequest request)
    {
        try
        {
            var historyItems = await _settingsRepository.GetChangeHistoryAsync(request);
            var statistics = await _settingsRepository.GetChangeHistoryStatisticsAsync();
            
            var totalCount = await GetTotalCountAsync(request);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            
            return new SettingsHistoryResponse
            {
                Items = historyItems.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages,
                Statistics = statistics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置变更历史列表失败");
            throw;
        }
    }

    public async Task<SettingsHistoryStatistics> GetChangeHistoryStatisticsAsync()
    {
        try
        {
            return await _settingsRepository.GetChangeHistoryStatisticsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置变更历史统计失败");
            throw;
        }
    }

    public async Task<List<SettingsHistoryDto>> GetRecentChangesAsync(int count = 10)
    {
        try
        {
            var request = new SettingsHistoryRequest
            {
                PageNumber = 1,
                PageSize = count,
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };
            
            var historyItems = await _settingsRepository.GetChangeHistoryAsync(request);
            return historyItems.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取最近设置变更失败");
            throw;
        }
    }

    public async Task<List<SettingsHistoryDto>> GetChangesBySettingTypeAsync(string settingType, int count = 50)
    {
        try
        {
            var request = new SettingsHistoryRequest
            {
                SettingType = settingType,
                PageNumber = 1,
                PageSize = count,
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };
            
            var historyItems = await _settingsRepository.GetChangeHistoryAsync(request);
            return historyItems.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设置类型变更历史失败: {SettingType}", settingType);
            throw;
        }
    }

    public async Task<List<SettingsHistoryDto>> GetChangesByUserAsync(string username, int count = 50)
    {
        try
        {
            var request = new SettingsHistoryRequest
            {
                ChangedBy = username,
                PageNumber = 1,
                PageSize = count,
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };
            
            var historyItems = await _settingsRepository.GetChangeHistoryAsync(request);
            return historyItems.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取用户设置变更历史失败: {Username}", username);
            throw;
        }
    }

    public async Task<List<SettingsHistoryDto>> GetImportantChangesAsync(int count = 20)
    {
        try
        {
            var request = new SettingsHistoryRequest
            {
                IsImportant = true,
                PageNumber = 1,
                PageSize = count,
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };
            
            var historyItems = await _settingsRepository.GetChangeHistoryAsync(request);
            return historyItems.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取重要设置变更失败");
            throw;
        }
    }

    public async Task<List<SettingsHistoryDto>> GetFailedChangesAsync(int count = 20)
    {
        try
        {
            var request = new SettingsHistoryRequest
            {
                Status = "Failed",
                PageNumber = 1,
                PageSize = count,
                SortBy = "CreatedAt",
                SortDirection = "desc"
            };
            
            var historyItems = await _settingsRepository.GetChangeHistoryAsync(request);
            return historyItems.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取失败设置变更失败");
            throw;
        }
    }

    public async Task<bool> DeleteChangeHistoryAsync(Guid id)
    {
        try
        {
            var result = await _settingsRepository.DeleteChangeHistoryAsync(id);
            if (result)
            {
                _logger.LogInformation("设置变更历史已删除: {Id}", id);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除设置变更历史失败: {Id}", id);
            throw;
        }
    }

    public async Task<int> BatchDeleteChangeHistoryAsync(List<Guid> ids)
    {
        try
        {
            var count = await _settingsRepository.BatchDeleteChangeHistoryAsync(ids);
            _logger.LogInformation("批量删除设置变更历史: {Count} 条记录", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除设置变更历史失败");
            throw;
        }
    }

    public async Task<int> CleanExpiredHistoryAsync(DateTime cutoffDate)
    {
        try
        {
            var count = await _settingsRepository.CleanExpiredChangeHistoryAsync(cutoffDate);
            _logger.LogInformation("清理过期设置变更历史: {Count} 条记录", count);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理过期设置变更历史失败");
            throw;
        }
    }

    // 私有辅助方法
    private static SettingsHistoryDto MapToDto(SettingsHistory entity)
    {
        return new SettingsHistoryDto
        {
            Id = entity.Id,
            CreatedAt = entity.CreatedAt,
            SettingType = entity.SettingType,
            SettingKey = entity.SettingKey,
            OldValue = entity.OldValue,
            NewValue = entity.NewValue,
            ChangedBy = entity.ChangedBy,
            ChangedById = entity.ChangedById,
            ChangeReason = entity.ChangeReason,
            ChangeCategory = entity.ChangeCategory,
            ClientIp = entity.ClientIp,
            UserAgent = entity.UserAgent,
            IsImportant = entity.IsImportant,
            Status = entity.Status,
            ErrorMessage = entity.ErrorMessage,
            Metadata = entity.Metadata
        };
    }

    private async Task<int> GetTotalCountAsync(SettingsHistoryRequest request)
    {
        // 这里可以实现获取总记录数的逻辑
        // 由于当前仓储接口没有直接提供总数查询，我们可以通过查询所有记录来计算
        try
        {
            var allRequest = new SettingsHistoryRequest
            {
                SettingType = request.SettingType,
                ChangeCategory = request.ChangeCategory,
                ChangedBy = request.ChangedBy,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                IsImportant = request.IsImportant,
                Status = request.Status,
                SettingKey = request.SettingKey,
                PageNumber = 1,
                PageSize = int.MaxValue // 获取所有记录
            };
            
            var allItems = await _settingsRepository.GetChangeHistoryAsync(allRequest);
            return allItems.Count;
        }
        catch
        {
            return 0;
        }
    }
}