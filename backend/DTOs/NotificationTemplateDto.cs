using System.ComponentModel.DataAnnotations;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.DTOs;

/// <summary>
/// 通知模板DTO - 用于返回模板详情
/// </summary>
public class NotificationTemplateDto
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 标题模板
    /// </summary>
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 内容模板
    /// </summary>
    public string? ContentTemplate { get; set; }

    /// <summary>
    /// 消息模板
    /// </summary>
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// 模板变量
    /// </summary>
    public List<NotificationTemplateVariableDto> Variables { get; set; } = new();

    /// <summary>
    /// 默认数据
    /// </summary>
    public string? DefaultDataJson { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;

    /// <summary>
    /// 过期时间（小时）
    /// </summary>
    public int? ExpiresAfterHours { get; set; }

    /// <summary>
    /// 是否为系统模板
    /// </summary>
    public bool IsSystemTemplate { get; set; } = false;

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 使用次数
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// 最后使用时间
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>
    /// 创建者信息
    /// </summary>
    public UserInfoDto? Creator { get; set; }

    /// <summary>
    /// 是否可编辑
    /// </summary>
    public bool CanEdit { get; set; }

    /// <summary>
    /// 是否可删除
    /// </summary>
    public bool CanDelete { get; set; }
}

/// <summary>
/// 通知模板变量DTO
/// </summary>
public class NotificationTemplateVariableDto
{
    /// <summary>
    /// 变量名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 变量类型
    /// </summary>
    public NotificationTemplateVariableType Type { get; set; } = NotificationTemplateVariableType.String;

    /// <summary>
    /// 默认值
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// 验证规则
    /// </summary>
    public string? ValidationRule { get; set; }

    /// <summary>
    /// 示例值
    /// </summary>
    public string? Example { get; set; }
}

/// <summary>
/// 通知模板创建请求DTO
/// </summary>
public class NotificationTemplateCreateDto
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [Required(ErrorMessage = "模板名称不能为空")]
    [StringLength(100, ErrorMessage = "模板名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 模板描述
    /// </summary>
    [StringLength(500, ErrorMessage = "模板描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    [Required(ErrorMessage = "通知类型不能为空")]
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    [Required(ErrorMessage = "通知渠道不能为空")]
    public NotificationChannel Channel { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// 标题模板
    /// </summary>
    [Required(ErrorMessage = "标题模板不能为空")]
    [StringLength(200, ErrorMessage = "标题模板长度不能超过200个字符")]
    public string TitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// 内容模板
    /// </summary>
    [StringLength(2000, ErrorMessage = "内容模板长度不能超过2000个字符")]
    public string? ContentTemplate { get; set; }

    /// <summary>
    /// 消息模板
    /// </summary>
    [StringLength(500, ErrorMessage = "消息模板长度不能超过500个字符")]
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// 模板变量
    /// </summary>
    public List<NotificationTemplateVariableCreateDto> Variables { get; set; } = new();

    /// <summary>
    /// 默认数据
    /// </summary>
    [StringLength(2000, ErrorMessage = "默认数据长度不能超过2000个字符")]
    public string? DefaultDataJson { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [StringLength(50, ErrorMessage = "图标长度不能超过50个字符")]
    public string? Icon { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    [StringLength(20, ErrorMessage = "颜色长度不能超过20个字符")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool RequiresConfirmation { get; set; } = false;

    /// <summary>
    /// 过期时间（小时）
    /// </summary>
    [Range(1, 8760, ErrorMessage = "过期时间必须在1-8760小时之间")]
    public int? ExpiresAfterHours { get; set; }

    /// <summary>
    /// 是否为系统模板
    /// </summary>
    public bool IsSystemTemplate { get; set; } = false;

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 通知模板更新请求DTO
/// </summary>
public class NotificationTemplateUpdateDto
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [StringLength(100, ErrorMessage = "模板名称长度不能超过100个字符")]
    public string? Name { get; set; }

    /// <summary>
    /// 模板描述
    /// </summary>
    [StringLength(500, ErrorMessage = "模板描述长度不能超过500个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 通知类型
    /// </summary>
    public NotificationType? Type { get; set; }

    /// <summary>
    /// 通知渠道
    /// </summary>
    public NotificationChannel? Channel { get; set; }

    /// <summary>
    /// 优先级
    /// </summary>
    public NotificationPriority? Priority { get; set; }

    /// <summary>
    /// 标题模板
    /// </summary>
    [StringLength(200, ErrorMessage = "标题模板长度不能超过200个字符")]
    public string? TitleTemplate { get; set; }

    /// <summary>
    /// 内容模板
    /// </summary>
    [StringLength(2000, ErrorMessage = "内容模板长度不能超过2000个字符")]
    public string? ContentTemplate { get; set; }

    /// <summary>
    /// 消息模板
    /// </summary>
    [StringLength(500, ErrorMessage = "消息模板长度不能超过500个字符")]
    public string? MessageTemplate { get; set; }

    /// <summary>
    /// 模板变量
    /// </summary>
    public List<NotificationTemplateVariableCreateDto>? Variables { get; set; }

    /// <summary>
    /// 默认数据
    /// </summary>
    [StringLength(2000, ErrorMessage = "默认数据长度不能超过2000个字符")]
    public string? DefaultDataJson { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    [StringLength(50, ErrorMessage = "图标长度不能超过50个字符")]
    public string? Icon { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    [StringLength(20, ErrorMessage = "颜色长度不能超过20个字符")]
    public string? Color { get; set; }

    /// <summary>
    /// 是否需要确认
    /// </summary>
    public bool? RequiresConfirmation { get; set; }

    /// <summary>
    /// 过期时间（小时）
    /// </summary>
    [Range(1, 8760, ErrorMessage = "过期时间必须在1-8760小时之间")]
    public int? ExpiresAfterHours { get; set; }

    /// <summary>
    /// 是否为系统模板
    /// </summary>
    public bool? IsSystemTemplate { get; set; }

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// 通知模板变量创建请求DTO
/// </summary>
public class NotificationTemplateVariableCreateDto
{
    /// <summary>
    /// 变量名
    /// </summary>
    [Required(ErrorMessage = "变量名不能为空")]
    [StringLength(50, ErrorMessage = "变量名长度不能超过50个字符")]
    [RegularExpression(@"^[a-zA-Z_][a-zA-Z0-9_]*$", ErrorMessage = "变量名只能包含字母、数字和下划线，且必须以字母或下划线开头")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 变量描述
    /// </summary>
    [StringLength(200, ErrorMessage = "变量描述长度不能超过200个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 变量类型
    /// </summary>
    public NotificationTemplateVariableType Type { get; set; } = NotificationTemplateVariableType.String;

    /// <summary>
    /// 默认值
    /// </summary>
    [StringLength(500, ErrorMessage = "默认值长度不能超过500个字符")]
    public string? DefaultValue { get; set; }

    /// <summary>
    /// 是否必填
    /// </summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>
    /// 验证规则
    /// </summary>
    [StringLength(200, ErrorMessage = "验证规则长度不能超过200个字符")]
    public string? ValidationRule { get; set; }

    /// <summary>
    /// 示例值
    /// </summary>
    [StringLength(200, ErrorMessage = "示例值长度不能超过200个字符")]
    public string? Example { get; set; }
}

/// <summary>
/// 通知模板渲染请求DTO
/// </summary>
public class NotificationTemplateRenderDto
{
    /// <summary>
    /// 模板ID
    /// </summary>
    [Required(ErrorMessage = "模板ID不能为空")]
    public Guid TemplateId { get; set; }

    /// <summary>
    /// 模板数据
    /// </summary>
    [Required(ErrorMessage = "模板数据不能为空")]
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// 接收用户ID
    /// </summary>
    [Required(ErrorMessage = "接收用户ID不能为空")]
    public Guid UserId { get; set; }

    /// <summary>
    /// 是否立即发送
    /// </summary>
    public bool SendImmediately { get; set; } = false;

    /// <summary>
    /// 计划发送时间
    /// </summary>
    public DateTime? ScheduledToSendAt { get; set; }
}

/// <summary>
/// 通知模板渲染结果DTO
/// </summary>
public class NotificationTemplateRenderResultDto
{
    /// <summary>
    /// 渲染是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 渲染后的标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 渲染后的内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 渲染后的消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 渲染后的数据
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// 错误信息
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 渲染时间
    /// </summary>
    public DateTime RenderedAt { get; set; }

    /// <summary>
    /// 渲染耗时
    /// </summary>
    public TimeSpan RenderingDuration { get; set; }

    /// <summary>
    /// 生成的通知
    /// </summary>
    public NotificationDto? Notification { get; set; }
}

/// <summary>
/// 通知模板变量类型枚举
/// </summary>
public enum NotificationTemplateVariableType
{
    /// <summary>
    /// 字符串类型
    /// </summary>
    String = 0,

    /// <summary>
    /// 整数类型
    /// </summary>
    Integer = 1,

    /// <summary>
    /// 浮点数类型
    /// </summary>
    Float = 2,

    /// <summary>
    /// 布尔类型
    /// </summary>
    Boolean = 3,

    /// <summary>
    /// 日期时间类型
    /// </summary>
    DateTime = 4,

    /// <summary>
    /// 日期类型
    /// </summary>
    Date = 5,

    /// <summary>
    /// 时间类型
    /// </summary>
    Time = 6,

    /// <summary>
    /// 对象类型
    /// </summary>
    Object = 7,

    /// <summary>
    /// 数组类型
    /// </summary>
    Array = 8,

    /// <summary>
    /// 枚举类型
    /// </summary>
    Enum = 9,

    /// <summary>
    /// 邮箱类型
    /// </summary>
    Email = 10,

    /// <summary>
    /// URL类型
    /// </summary>
    Url = 11,

    /// <summary>
    /// 颜色类型
    /// </summary>
    Color = 12
}

/// <summary>
/// 通知模板列表DTO
/// </summary>
public class NotificationTemplateListDto
{
    /// <summary>
    /// 模板列表
    /// </summary>
    public List<NotificationTemplateDto> Templates { get; set; } = new();

    /// <summary>
    /// 总数量
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 是否有下一页
    /// </summary>
    public bool HasNext { get; set; }

    /// <summary>
    /// 是否有上一页
    /// </summary>
    public bool HasPrevious { get; set; }
}