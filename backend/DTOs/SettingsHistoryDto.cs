using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 设置变更历史DTO
/// </summary>
public class SettingsHistoryDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SettingType { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public Guid? ChangedById { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string ChangeCategory { get; set; } = "Other";
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsImportant { get; set; } = false;
    public string Status { get; set; } = "Success";
    public string ErrorMessage { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
    
    /// <summary>
    /// 设置类型显示名称
    /// </summary>
    public string SettingTypeDisplayName => SettingsHistoryEnums.GetSettingTypeDisplayName(
        Enum.Parse<SettingsHistoryEnums.SettingType>(SettingType));
    
    /// <summary>
    /// 变更分类显示名称
    /// </summary>
    public string ChangeCategoryDisplayName => SettingsHistoryEnums.GetChangeCategoryDisplayName(
        Enum.Parse<SettingsHistoryEnums.ChangeCategory>(ChangeCategory));
    
    /// <summary>
    /// 变更状态显示名称
    /// </summary>
    public string StatusDisplayName => SettingsHistoryEnums.GetChangeStatusDisplayName(
        Enum.Parse<SettingsHistoryEnums.ChangeStatus>(Status));
}

/// <summary>
/// 设置历史查询请求DTO
/// </summary>
public class SettingsHistoryRequest
{
    /// <summary>
    /// 设置类型筛选
    /// </summary>
    public string? SettingType { get; set; }
    
    /// <summary>
    /// 变更分类筛选
    /// </summary>
    public string? ChangeCategory { get; set; }
    
    /// <summary>
    /// 操作人筛选
    /// </summary>
    public string? ChangedBy { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 是否只显示重要变更
    /// </summary>
    public bool? IsImportant { get; set; }
    
    /// <summary>
    /// 状态筛选
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// 设置键名筛选
    /// </summary>
    public string? SettingKey { get; set; }
    
    /// <summary>
    /// 页码
    /// </summary>
    public int PageNumber { get; set; } = 1;
    
    /// <summary>
    /// 页面大小
    /// </summary>
    public int PageSize { get; set; } = 20;
    
    /// <summary>
    /// 排序字段
    /// </summary>
    public string SortBy { get; set; } = "CreatedAt";
    
    /// <summary>
    /// 排序方向
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// 设置历史查询响应DTO
/// </summary>
public class SettingsHistoryResponse
{
    public List<SettingsHistoryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage { get; set; }
    public bool HasNextPage { get; set; }
    
    /// <summary>
    /// 统计信息
    /// </summary>
    public SettingsHistoryStatistics Statistics { get; set; } = new();
}

/// <summary>
/// 设置历史统计信息DTO
/// </summary>
public class SettingsHistoryStatistics
{
    /// <summary>
    /// 总变更次数
    /// </summary>
    public int TotalChanges { get; set; }
    
    /// <summary>
    /// 今日变更次数
    /// </summary>
    public int TodayChanges { get; set; }
    
    /// <summary>
    /// 本周变更次数
    /// </summary>
    public int ThisWeekChanges { get; set; }
    
    /// <summary>
    /// 本月变更次数
    /// </summary>
    public int ThisMonthChanges { get; set; }
    
    /// <summary>
    /// 重要变更次数
    /// </summary>
    public int ImportantChanges { get; set; }
    
    /// <summary>
    /// 失败变更次数
    /// </summary>
    public int FailedChanges { get; set; }
    
    /// <summary>
    /// 按设置类型统计
    /// </summary>
    public Dictionary<string, int> ChangesBySettingType { get; set; } = new();
    
    /// <summary>
    /// 按变更分类统计
    /// </summary>
    public Dictionary<string, int> ChangesByCategory { get; set; } = new();
    
    /// <summary>
    /// 按操作人统计
    /// </summary>
    public Dictionary<string, int> ChangesByUser { get; set; } = new();
    
    /// <summary>
    /// 最近变更时间
    /// </summary>
    public DateTime? LastChangeTime { get; set; }
    
    /// <summary>
    /// 最活跃的操作人
    /// </summary>
    public string? MostActiveUser { get; set; }
    
    /// <summary>
    /// 最常变更的设置类型
    /// </summary>
    public string? MostChangedSettingType { get; set; }
}

/// <summary>
/// 设置历史导出请求DTO
/// </summary>
public class SettingsHistoryExportRequest
{
    /// <summary>
    /// 导出格式：json, csv, excel
    /// </summary>
    public string Format { get; set; } = "json";
    
    /// <summary>
    /// 时间筛选条件
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 时间筛选条件
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 设置类型筛选
    /// </summary>
    public string? SettingType { get; set; }
    
    /// <summary>
    /// 变更分类筛选
    /// </summary>
    public string? ChangeCategory { get; set; }
    
    /// <summary>
    /// 操作人筛选
    /// </summary>
    public string? ChangedBy { get; set; }
    
    /// <summary>
    /// 是否只包含重要变更
    /// </summary>
    public bool? IsImportant { get; set; }
    
    /// <summary>
    /// 状态筛选
    /// </summary>
    public string? Status { get; set; }
    
    /// <summary>
    /// 是否包含统计信息
    /// </summary>
    public bool IncludeStatistics { get; set; } = true;
    
    /// <summary>
    /// 是否包含详细变更信息
    /// </summary>
    public bool IncludeDetailedChanges { get; set; } = true;
}

/// <summary>
/// 设置历史导出响应DTO
/// </summary>
public class SettingsHistoryExportResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime ExportedAt { get; set; }
    public int RecordCount { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// 创建设置变更记录请求DTO
/// </summary>
public class CreateSettingsHistoryRequest
{
    public string SettingType { get; set; } = string.Empty;
    public string SettingKey { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
    public string ChangedBy { get; set; } = string.Empty;
    public Guid? ChangedById { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public string ChangeCategory { get; set; } = "Other";
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public bool IsImportant { get; set; } = false;
    public string Status { get; set; } = "Success";
    public string ErrorMessage { get; set; } = string.Empty;
    public string Metadata { get; set; } = string.Empty;
}

/// <summary>
/// 批量删除设置历史请求DTO
/// </summary>
public class BatchDeleteSettingsHistoryRequest
{
    /// <summary>
    /// 要删除的历史记录ID列表
    /// </summary>
    public List<Guid> HistoryIds { get; set; } = new();
}

/// <summary>
/// 导入设置请求DTO
/// </summary>
public class ImportSettingsRequest
{
    /// <summary>
    /// JSON格式的设置数据
    /// </summary>
    public string JsonData { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作人
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;
}

/// <summary>
/// 验证导入数据请求DTO
/// </summary>
public class ValidateImportDataRequest
{
    /// <summary>
    /// JSON格式的设置数据
    /// </summary>
    public string JsonData { get; set; } = string.Empty;
}

/// <summary>
/// 从备份恢复请求DTO
/// </summary>
public class RestoreFromBackupRequest
{
    /// <summary>
    /// 备份数据
    /// </summary>
    public string BackupData { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作人
    /// </summary>
    public string UpdatedBy { get; set; } = string.Empty;
}