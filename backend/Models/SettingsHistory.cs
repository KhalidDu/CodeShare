namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 设置变更历史实体 - 遵循单一职责原则，负责设置变更记录的追踪
/// </summary>
public class SettingsHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 设置类型：Site, Security, Feature, Email
    /// </summary>
    public string SettingType { get; set; } = string.Empty;
    
    /// <summary>
    /// 设置键名
    /// </summary>
    public string SettingKey { get; set; } = string.Empty;
    
    /// <summary>
    /// 变更前的值
    /// </summary>
    public string OldValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 变更后的值
    /// </summary>
    public string NewValue { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作人用户名
    /// </summary>
    public string ChangedBy { get; set; } = string.Empty;
    
    /// <summary>
    /// 操作人ID
    /// </summary>
    public Guid? ChangedById { get; set; }
    
    /// <summary>
    /// 变更原因
    /// </summary>
    public string ChangeReason { get; set; } = string.Empty;
    
    /// <summary>
    /// 变更分类：System, User, Security, Performance, Other
    /// </summary>
    public string ChangeCategory { get; set; } = "Other";
    
    /// <summary>
    /// 客户端IP地址
    /// </summary>
    public string ClientIp { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户代理
    /// </summary>
    public string UserAgent { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否为重要变更
    /// </summary>
    public bool IsImportant { get; set; } = false;
    
    /// <summary>
    /// 变更状态：Success, Failed, Pending
    /// </summary>
    public string Status { get; set; } = "Success";
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
    
    /// <summary>
    /// 额外的元数据（JSON格式）
    /// </summary>
    public string Metadata { get; set; } = string.Empty;
}

/// <summary>
/// 设置变更历史枚举定义
/// </summary>
public static class SettingsHistoryEnums
{
    /// <summary>
    /// 设置类型枚举
    /// </summary>
    public enum SettingType
    {
        Site = 0,
        Security = 1,
        Feature = 2,
        Email = 3
    }
    
    /// <summary>
    /// 变更分类枚举
    /// </summary>
    public enum ChangeCategory
    {
        System = 0,
        User = 1,
        Security = 2,
        Performance = 3,
        Other = 4
    }
    
    /// <summary>
    /// 变更状态枚举
    /// </summary>
    public enum ChangeStatus
    {
        Success = 0,
        Failed = 1,
        Pending = 2
    }
    
    /// <summary>
    /// 获取设置类型显示名称
    /// </summary>
    public static string GetSettingTypeDisplayName(SettingType type)
    {
        return type switch
        {
            SettingType.Site => "站点设置",
            SettingType.Security => "安全设置",
            SettingType.Feature => "功能设置",
            SettingType.Email => "邮件设置",
            _ => "未知设置"
        };
    }
    
    /// <summary>
    /// 获取变更分类显示名称
    /// </summary>
    public static string GetChangeCategoryDisplayName(ChangeCategory category)
    {
        return category switch
        {
            ChangeCategory.System => "系统变更",
            ChangeCategory.User => "用户变更",
            ChangeCategory.Security => "安全变更",
            ChangeCategory.Performance => "性能变更",
            ChangeCategory.Other => "其他变更",
            _ => "未知分类"
        };
    }
    
    /// <summary>
    /// 获取变更状态显示名称
    /// </summary>
    public static string GetChangeStatusDisplayName(ChangeStatus status)
    {
        return status switch
        {
            ChangeStatus.Success => "成功",
            ChangeStatus.Failed => "失败",
            ChangeStatus.Pending => "待处理",
            _ => "未知状态"
        };
    }
}