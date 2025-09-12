using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 消息附件仓储接口 - 遵循开闭原则和接口隔离原则
/// 定义消息附件的CRUD操作、查询功能和文件管理
/// </summary>
public interface IMessageAttachmentRepository
{
    // 基础 CRUD 操作
    /// <summary>
    /// 根据ID获取附件
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>附件实体或null</returns>
    Task<MessageAttachment?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取附件（包含消息信息）
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>附件实体或null</returns>
    Task<MessageAttachment?> GetByIdWithMessageAsync(Guid id);
    
    /// <summary>
    /// 创建新附件
    /// </summary>
    /// <param name="attachment">附件实体</param>
    /// <returns>创建后的附件实体</returns>
    Task<MessageAttachment> CreateAsync(MessageAttachment attachment);
    
    /// <summary>
    /// 更新附件
    /// </summary>
    /// <param name="attachment">附件实体</param>
    /// <returns>更新后的附件实体</returns>
    Task<MessageAttachment> UpdateAsync(MessageAttachment attachment);
    
    /// <summary>
    /// 删除附件（软删除）
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// 硬删除附件（从数据库中永久删除）
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> HardDeleteAsync(Guid id);
    
    /// <summary>
    /// 恢复已删除的附件
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>是否恢复成功</returns>
    Task<bool> RestoreAsync(Guid id);
    
    // 分页查询
    /// <summary>
    /// 分页获取附件列表
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<MessageAttachment>> GetPagedAsync(MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取消息的附件列表
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByMessageIdAsync(Guid messageId, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取多个消息的附件列表
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByMessageIdsAsync(IEnumerable<Guid> messageIds, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取用户上传的附件列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<MessageAttachment>> GetByUserIdAsync(Guid userId, MessageAttachmentFilterDto filter);
    
    // 按类型查询
    /// <summary>
    /// 获取指定类型的附件列表
    /// </summary>
    /// <param name="attachmentType">附件类型</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByAttachmentTypeAsync(AttachmentType attachmentType, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取指定文件扩展名的附件列表
    /// </summary>
    /// <param name="fileExtension">文件扩展名</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByFileExtensionAsync(string fileExtension, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取指定Content-Type的附件列表
    /// </summary>
    /// <param name="contentType">Content-Type</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByContentTypeAsync(string contentType, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取指定状态的附件列表
    /// </summary>
    /// <param name="status">附件状态</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByStatusAsync(AttachmentStatus status, MessageAttachmentFilterDto filter);
    
    // 搜索和筛选
    /// <summary>
    /// 搜索附件
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>搜索结果</returns>
    Task<IEnumerable<MessageAttachment>> SearchAttachmentsAsync(string searchTerm, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取指定大小范围的附件列表
    /// </summary>
    /// <param name="minSize">最小文件大小</param>
    /// <param name="maxSize">最大文件大小</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByFileSizeRangeAsync(long minSize, long maxSize, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取指定日期范围内的附件列表
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, MessageAttachmentFilterDto filter);
    
    // 下载管理
    /// <summary>
    /// 增加附件下载次数
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="userId">下载用户ID</param>
    /// <returns>是否增加成功</returns>
    Task<bool> IncrementDownloadCountAsync(Guid attachmentId, Guid userId);
    
    /// <summary>
    /// 批量增加附件下载次数
    /// </summary>
    /// <param name="attachmentIds">附件ID列表</param>
    /// <param name="userId">下载用户ID</param>
    /// <returns>增加成功的附件数量</returns>
    Task<int> IncrementMultipleDownloadCountAsync(IEnumerable<Guid> attachmentIds, Guid userId);
    
    /// <summary>
    /// 获取附件下载历史
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>下载历史记录</returns>
    Task<IEnumerable<AttachmentDownloadHistory>> GetDownloadHistoryAsync(Guid attachmentId, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取用户下载的附件列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetDownloadedByUserAsync(Guid userId, MessageAttachmentFilterDto filter);
    
    // 上传进度管理
    /// <summary>
    /// 更新附件上传进度
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="progress">上传进度（0-100）</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateUploadProgressAsync(Guid attachmentId, int progress);
    
    /// <summary>
    /// 获取正在上传的附件列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>正在上传的附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetUploadingAttachmentsAsync(Guid userId, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取上传失败的附件列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>上传失败的附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetUploadFailedAttachmentsAsync(Guid userId, MessageAttachmentFilterDto filter);
    
    // 状态管理
    /// <summary>
    /// 更新附件状态
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateAttachmentStatusAsync(Guid attachmentId, AttachmentStatus status);
    
    /// <summary>
    /// 批量更新附件状态
    /// </summary>
    /// <param name="attachmentIds">附件ID列表</param>
    /// <param name="status">新状态</param>
    /// <returns>更新成功的附件数量</returns>
    Task<int> UpdateMultipleAttachmentStatusAsync(IEnumerable<Guid> attachmentIds, AttachmentStatus status);
    
    /// <summary>
    /// 设置附件为活跃状态
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetAttachmentActiveAsync(Guid attachmentId);
    
    /// <summary>
    /// 标记附件为病毒扫描中
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkAsVirusScanningAsync(Guid attachmentId);
    
    /// <summary>
    /// 标记附件为检测到病毒
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="virusInfo">病毒信息</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkAsVirusDetectedAsync(Guid attachmentId, string virusInfo);
    
    // 文件验证和完整性
    /// <summary>
    /// 验证附件文件完整性
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="expectedHash">期望的文件哈希值</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateFileIntegrityAsync(Guid attachmentId, string expectedHash);
    
    /// <summary>
    /// 更新附件文件哈希值
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="fileHash">文件哈希值</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateFileHashAsync(Guid attachmentId, string fileHash);
    
    /// <summary>
    /// 检查文件是否已存在（根据文件哈希）
    /// </summary>
    /// <param name="fileHash">文件哈希值</param>
    /// <returns>已存在的附件实体或null</returns>
    Task<MessageAttachment?> GetByFileHashAsync(string fileHash);
    
    // 统计信息
    /// <summary>
    /// 获取附件统计信息
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>统计信息</returns>
    Task<MessageAttachmentStatsDto> GetAttachmentStatsAsync(MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取用户附件统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>统计信息</returns>
    Task<MessageAttachmentStatsDto> GetUserAttachmentStatsAsync(Guid userId, MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取附件总数
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>附件总数</returns>
    Task<int> GetAttachmentCountAsync(MessageAttachmentFilterDto filter);
    
    /// <summary>
    /// 获取消息附件数量
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>附件数量</returns>
    Task<int> GetAttachmentCountByMessageIdAsync(Guid messageId);
    
    /// <summary>
    /// 获取用户附件总大小
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>总文件大小（字节）</returns>
    Task<long> GetUserTotalFileSizeAsync(Guid userId, MessageAttachmentFilterDto filter);
    
    // 权限和验证
    /// <summary>
    /// 检查用户是否有权限下载附件
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserDownloadAttachmentAsync(Guid attachmentId, Guid userId);
    
    /// <summary>
    /// 检查用户是否有权限删除附件
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserDeleteAttachmentAsync(Guid attachmentId, Guid userId);
    
    /// <summary>
    /// 检查附件是否过期
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <returns>是否过期</returns>
    Task<bool> IsAttachmentExpiredAsync(Guid attachmentId);
    
    // 高级查询
    /// <summary>
    /// 获取最新上传的附件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">附件数量</param>
    /// <returns>最新附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetLatestAttachmentsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取下载次数最多的附件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">附件数量</param>
    /// <returns>热门附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetMostDownloadedAttachmentsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取最大的附件文件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">附件数量</param>
    /// <returns>大文件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetLargestAttachmentsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取即将过期的附件
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="days">天数</param>
    /// <returns>即将过期的附件列表</returns>
    Task<IEnumerable<MessageAttachment>> GetExpiringAttachmentsAsync(Guid userId, int days = 7);
    
    // 缓存相关
    /// <summary>
    /// 从缓存获取附件
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>附件实体或null</returns>
    Task<MessageAttachment?> GetFromCacheAsync(Guid id);
    
    /// <summary>
    /// 设置附件缓存
    /// </summary>
    /// <param name="attachment">附件实体</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetCacheAsync(MessageAttachment attachment, TimeSpan? expiration = null);
    
    /// <summary>
    /// 移除附件缓存
    /// </summary>
    /// <param name="id">附件ID</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveCacheAsync(Guid id);
    
    /// <summary>
    /// 批量移除附件缓存
    /// </summary>
    /// <param name="attachmentIds">附件ID列表</param>
    /// <returns>移除成功的附件数量</returns>
    Task<int> RemoveMultipleCacheAsync(IEnumerable<Guid> attachmentIds);
    
    // 批量操作
    /// <summary>
    /// 批量插入附件
    /// </summary>
    /// <param name="attachments">附件列表</param>
    /// <returns>插入成功的附件数量</returns>
    Task<int> BulkInsertAsync(IEnumerable<MessageAttachment> attachments);
    
    /// <summary>
    /// 批量更新附件
    /// </summary>
    /// <param name="attachments">附件列表</param>
    /// <returns>更新成功的附件数量</returns>
    Task<int> BulkUpdateAsync(IEnumerable<MessageAttachment> attachments);
    
    /// <summary>
    /// 批量删除附件
    /// </summary>
    /// <param name="attachmentIds">附件ID列表</param>
    /// <returns>删除成功的附件数量</returns>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> attachmentIds);
    
    /// <summary>
    /// 批量软删除附件
    /// </summary>
    /// <param name="attachmentIds">附件ID列表</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>删除成功的附件数量</returns>
    Task<int> BulkSoftDeleteAsync(IEnumerable<Guid> attachmentIds, Guid userId);
}

/// <summary>
/// 附件下载历史记录
/// </summary>
public class AttachmentDownloadHistory
{
    /// <summary>
    /// 记录ID
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// 附件ID
    /// </summary>
    public Guid AttachmentId { get; set; }
    
    /// <summary>
    /// 下载用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 下载时间
    /// </summary>
    public DateTime DownloadedAt { get; set; }
    
    /// <summary>
    /// 下载IP地址
    /// </summary>
    public string? DownloadIp { get; set; }
    
    /// <summary>
    /// 下载User-Agent
    /// </summary>
    public string? DownloadUserAgent { get; set; }
}