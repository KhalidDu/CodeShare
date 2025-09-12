using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 消息服务接口 - 遵循开闭原则和接口隔离原则
/// 定义完整的消息发送、接收、附件管理、草稿管理和会话管理业务逻辑
/// </summary>
public interface IMessageService
{
    // 消息发送和接收操作
    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="request">发送消息请求</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>发送后的消息DTO</returns>
    Task<MessageDto> SendMessageAsync(CreateMessageDto request, Guid senderId);

    /// <summary>
    /// 批量发送消息
    /// </summary>
    /// <param name="requests">发送消息请求列表</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>发送结果列表</returns>
    Task<IEnumerable<MessageDto>> SendMultipleMessagesAsync(IEnumerable<CreateMessageDto> requests, Guid senderId);

    /// <summary>
    /// 回复消息
    /// </summary>
    /// <param name="parentMessageId">父消息ID</param>
    /// <param name="request">回复消息请求</param>
    /// <param name="replyerId">回复者ID</param>
    /// <returns>回复消息DTO</returns>
    Task<MessageDto> ReplyMessageAsync(Guid parentMessageId, CreateMessageDto request, Guid replyerId);

    /// <summary>
    /// 转发消息
    /// </summary>
    /// <param name="originalMessageId">原消息ID</param>
    /// <param name="request">转发消息请求</param>
    /// <param name="forwarderId">转发者ID</param>
    /// <returns>转发后的消息DTO</returns>
    Task<MessageDto> ForwardMessageAsync(Guid originalMessageId, CreateMessageDto request, Guid forwarderId);

    /// <summary>
    /// 获取消息详情
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>消息DTO</returns>
    Task<MessageDto?> GetMessageAsync(Guid messageId, Guid currentUserId);

    /// <summary>
    /// 获取用户的消息列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页消息列表</returns>
    Task<PaginatedResult<MessageDto>> GetUserMessagesAsync(Guid userId, MessageFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 获取用户发送的消息列表
    /// </summary>
    /// <param name="senderId">发送者ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页消息列表</returns>
    Task<PaginatedResult<MessageDto>> GetSentMessagesAsync(Guid senderId, MessageFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 获取用户接收的消息列表
    /// </summary>
    /// <param name="receiverId">接收者ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页消息列表</returns>
    Task<PaginatedResult<MessageDto>> GetReceivedMessagesAsync(Guid receiverId, MessageFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 获取用户未读消息列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页消息列表</returns>
    Task<PaginatedResult<MessageDto>> GetUnreadMessagesAsync(Guid userId, MessageFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 标记消息为已读
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkAsReadAsync(Guid messageId, Guid userId);

    /// <summary>
    /// 批量标记消息为已读
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <param name="userId">用户ID</param>
    /// <returns>标记成功的消息数量</returns>
    Task<int> MarkMultipleAsReadAsync(IEnumerable<Guid> messageIds, Guid userId);

    /// <summary>
    /// 更新消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="request">更新消息请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>更新后的消息DTO</returns>
    Task<MessageDto> UpdateMessageAsync(Guid messageId, UpdateMessageDto request, Guid currentUserId);

    /// <summary>
    /// 删除消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteMessageAsync(Guid messageId, Guid currentUserId);

    /// <summary>
    /// 批量删除消息
    /// </summary>
    /// <param name="request">批量操作请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>操作结果</returns>
    Task<BatchOperationResult> BatchDeleteMessagesAsync(BatchMessageOperationDto request, Guid currentUserId);

    /// <summary>
    /// 搜索消息
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>搜索结果</returns>
    Task<MessageSearchResultDto> SearchMessagesAsync(string searchTerm, MessageFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 获取用户消息统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">统计筛选条件</param>
    /// <returns>统计信息</returns>
    Task<MessageStatsDto> GetUserMessageStatsAsync(Guid userId, MessageStatsFilterDto filter);

    // 消息附件管理操作
    /// <summary>
    /// 上传消息附件
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="fileStream">文件流</param>
    /// <param name="fileName">文件名</param>
    /// <param name="contentType">文件类型</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>上传的附件DTO</returns>
    Task<MessageAttachmentDto> UploadAttachmentAsync(Guid messageId, Stream fileStream, string fileName, string contentType, Guid currentUserId);

    /// <summary>
    /// 批量上传消息附件
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="files">文件列表</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>上传的附件DTO列表</returns>
    Task<IEnumerable<MessageAttachmentDto>> UploadMultipleAttachmentsAsync(Guid messageId, IEnumerable<MessageFileUploadDto> files, Guid currentUserId);

    /// <summary>
    /// 下载消息附件
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>附件文件流</returns>
    Task<Stream> DownloadAttachmentAsync(Guid attachmentId, Guid currentUserId);

    /// <summary>
    /// 获取消息附件列表
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>附件DTO列表</returns>
    Task<IEnumerable<MessageAttachmentDto>> GetMessageAttachmentsAsync(Guid messageId, Guid currentUserId);

    /// <summary>
    /// 删除消息附件
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAttachmentAsync(Guid attachmentId, Guid currentUserId);

    /// <summary>
    /// 获取附件下载URL
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>下载URL</returns>
    Task<string> GetAttachmentDownloadUrlAsync(Guid attachmentId, Guid currentUserId, TimeSpan? expiration = null);

    /// <summary>
    /// 验证附件文件完整性
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="expectedHash">期望的文件哈希值</param>
    /// <returns>验证结果</returns>
    Task<bool> ValidateAttachmentIntegrityAsync(Guid attachmentId, string expectedHash);

    /// <summary>
    /// 获取附件统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>附件统计信息</returns>
    Task<MessageAttachmentStatsDto> GetUserAttachmentStatsAsync(Guid userId);

    // 消息草稿管理操作
    /// <summary>
    /// 创建消息草稿
    /// </summary>
    /// <param name="request">创建草稿请求</param>
    /// <param name="authorId">作者ID</param>
    /// <returns>创建的草稿DTO</returns>
    Task<MessageDraftDto> CreateDraftAsync(CreateMessageDraftDto request, Guid authorId);

    /// <summary>
    /// 获取消息草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>草稿DTO</returns>
    Task<MessageDraftDto?> GetDraftAsync(Guid draftId, Guid currentUserId);

    /// <summary>
    /// 获取用户的草稿列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页草稿列表</returns>
    Task<PaginatedResult<MessageDraftDto>> GetUserDraftsAsync(Guid userId, MessageDraftFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 更新消息草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="request">更新草稿请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>更新后的草稿DTO</returns>
    Task<MessageDraftDto> UpdateDraftAsync(Guid draftId, UpdateMessageDraftDto request, Guid currentUserId);

    /// <summary>
    /// 自动保存草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="content">草稿内容</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否自动保存成功</returns>
    Task<bool> AutoSaveDraftAsync(Guid draftId, string content, Guid currentUserId);

    /// <summary>
    /// 发送草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>发送后的消息DTO</returns>
    Task<MessageDto> SendDraftAsync(Guid draftId, Guid currentUserId);

    /// <summary>
    /// 删除消息草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteDraftAsync(Guid draftId, Guid currentUserId);

    /// <summary>
    /// 批量删除草稿
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>删除成功的草稿数量</returns>
    Task<int> BulkDeleteDraftsAsync(IEnumerable<Guid> draftIds, Guid currentUserId);

    /// <summary>
    /// 设置定时发送
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="scheduledTime">计划发送时间</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> ScheduleDraftSendAsync(Guid draftId, DateTime scheduledTime, Guid currentUserId);

    /// <summary>
    /// 取消定时发送
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否取消成功</returns>
    Task<bool> CancelScheduledSendAsync(Guid draftId, Guid currentUserId);

    /// <summary>
    /// 获取定时发送的草稿列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>定时发送的草稿列表</returns>
    Task<IEnumerable<MessageDraftDto>> GetScheduledDraftsAsync(Guid userId, Guid currentUserId);

    /// <summary>
    /// 上传草稿附件
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="fileStream">文件流</param>
    /// <param name="fileName">文件名</param>
    /// <param name="contentType">文件类型</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>上传的附件DTO</returns>
    Task<MessageDraftAttachmentDto> UploadDraftAttachmentAsync(Guid draftId, Stream fileStream, string fileName, string contentType, Guid currentUserId);

    /// <summary>
    /// 删除草稿附件
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteDraftAttachmentAsync(Guid attachmentId, Guid currentUserId);

    /// <summary>
    /// 清理过期草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>清理的草稿数量</returns>
    Task<int> CleanupExpiredDraftsAsync(Guid userId);

    // 会话管理操作
    /// <summary>
    /// 创建会话
    /// </summary>
    /// <param name="request">创建会话请求</param>
    /// <param name="creatorId">创建者ID</param>
    /// <returns>会话DTO</returns>
    Task<ConversationDto> CreateConversationAsync(CreateConversationDto request, Guid creatorId);

    /// <summary>
    /// 获取会话列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页会话列表</returns>
    Task<PaginatedResult<ConversationDto>> GetConversationsAsync(Guid userId, MessageConversationFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 获取会话详情
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>会话DTO</returns>
    Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid currentUserId);

    /// <summary>
    /// 获取会话消息列表
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="filter">筛选条件</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>分页消息列表</returns>
    Task<PaginatedResult<MessageDto>> GetConversationMessagesAsync(Guid conversationId, MessageFilterDto filter, Guid currentUserId);

    /// <summary>
    /// 发送会话消息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="request">发送消息请求</param>
    /// <param name="senderId">发送者ID</param>
    /// <returns>发送后的消息DTO</returns>
    Task<MessageDto> SendConversationMessageAsync(Guid conversationId, CreateMessageDto request, Guid senderId);

    /// <summary>
    /// 更新会话信息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="request">更新会话请求</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>更新后的会话DTO</returns>
    Task<ConversationDto> UpdateConversationAsync(Guid conversationId, UpdateConversationDto request, Guid currentUserId);

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteConversationAsync(Guid conversationId, Guid currentUserId);

    /// <summary>
    /// 添加会话参与者
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userIds">用户ID列表</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds, Guid currentUserId);

    /// <summary>
    /// 移除会话参与者
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userIds">用户ID列表</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds, Guid currentUserId);

    /// <summary>
    /// 设置会话置顶
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="isPinned">是否置顶</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetConversationPinnedAsync(Guid conversationId, bool isPinned, Guid currentUserId);

    /// <summary>
    /// 设置会话静音
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="isMuted">是否静音</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetConversationMutedAsync(Guid conversationId, bool isMuted, Guid currentUserId);

    /// <summary>
    /// 归档会话
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="isArchived">是否归档</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetConversationArchivedAsync(Guid conversationId, bool isArchived, Guid currentUserId);

    /// <summary>
    /// 标记会话为已读
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>标记成功的消息数量</returns>
    Task<int> MarkConversationAsReadAsync(Guid conversationId, Guid userId);

    /// <summary>
    /// 获取会话统计信息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <returns>会话统计信息</returns>
    Task<MessageStatsDto> GetConversationStatsAsync(Guid conversationId);

    /// <summary>
    /// 离开会话
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否离开成功</returns>
    Task<bool> LeaveConversationAsync(Guid conversationId, Guid userId);

    // 缓存和性能优化方法
    /// <summary>
    /// 清除消息缓存
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearMessageCacheAsync(Guid messageId);

    /// <summary>
    /// 清除用户消息缓存
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearUserMessageCacheAsync(Guid userId);

    /// <summary>
    /// 清除会话缓存
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <returns>是否清除成功</returns>
    Task<bool> ClearConversationCacheAsync(Guid conversationId);

    /// <summary>
    /// 预加载用户消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">消息数量</param>
    /// <returns>预加载结果</returns>
    Task<IEnumerable<MessageDto>> PreloadUserMessagesAsync(Guid userId, int count = 50);

    /// <summary>
    /// 预加载会话消息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="count">消息数量</param>
    /// <returns>预加载结果</returns>
    Task<IEnumerable<MessageDto>> PreloadConversationMessagesAsync(Guid conversationId, int count = 50);

    /// <summary>
    /// 批量获取消息
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>消息DTO列表</returns>
    Task<IEnumerable<MessageDto>> GetMessagesByIdsAsync(IEnumerable<Guid> messageIds, Guid currentUserId);

    /// <summary>
    /// 获取消息线程（包含回复）
    /// </summary>
    /// <param name="rootMessageId">根消息ID</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>消息线程</returns>
    Task<MessageThreadDto> GetMessageThreadAsync(Guid rootMessageId, Guid currentUserId);

    // 权限验证和异常处理
    /// <summary>
    /// 验证用户消息权限
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="permission">权限类型</param>
    /// <returns>是否有权限</returns>
    Task<bool> ValidateMessagePermissionAsync(Guid messageId, Guid userId, MessagePermission permission);

    /// <summary>
    /// 验证用户会话权限
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="permission">权限类型</param>
    /// <returns>是否有权限</returns>
    Task<bool> ValidateConversationPermissionAsync(Guid conversationId, Guid userId, ConversationPermission permission);

    /// <summary>
    /// 检查消息是否过期
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>是否过期</returns>
    Task<bool> IsMessageExpiredAsync(Guid messageId);

    /// <summary>
    /// 检查附件是否过期
    /// </summary>
    /// <param name="attachmentId">附件ID</param>
    /// <returns>是否过期</returns>
    Task<bool> IsAttachmentExpiredAsync(Guid attachmentId);

    /// <summary>
    /// 检查草稿是否过期
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否过期</returns>
    Task<bool> IsDraftExpiredAsync(Guid draftId);

    /// <summary>
    /// 处理过期消息
    /// </summary>
    /// <returns>处理的消息数量</returns>
    Task<int> ProcessExpiredMessagesAsync();

    /// <summary>
    /// 处理过期附件
    /// </summary>
    /// <returns>处理的附件数量</returns>
    Task<int> ProcessExpiredAttachmentsAsync();

    /// <summary>
    /// 处理过期草稿
    /// </summary>
    /// <returns>处理的草稿数量</returns>
    Task<int> ProcessExpiredDraftsAsync();

    // 导出功能
    /// <summary>
    /// 导出用户消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="options">导出选项</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>导出文件流</returns>
    Task<Stream> ExportUserMessagesAsync(Guid userId, MessageExportOptionsDto options, Guid currentUserId);

    /// <summary>
    /// 导出会话消息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="options">导出选项</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>导出文件流</returns>
    Task<Stream> ExportConversationMessagesAsync(Guid conversationId, MessageExportOptionsDto options, Guid currentUserId);

    /// <summary>
    /// 导出消息线程
    /// </summary>
    /// <param name="rootMessageId">根消息ID</param>
    /// <param name="options">导出选项</param>
    /// <param name="currentUserId">当前用户ID</param>
    /// <returns>导出文件流</returns>
    Task<Stream> ExportMessageThreadAsync(Guid rootMessageId, MessageExportOptionsDto options, Guid currentUserId);
}

/// <summary>
/// 创建会话请求DTO
/// </summary>
public class CreateConversationDto
{
    /// <summary>
    /// 会话主题
    /// </summary>
    [Required(ErrorMessage = "会话主题不能为空")]
    [StringLength(200, ErrorMessage = "会话主题长度不能超过200个字符")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 会话描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "会话描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 参与者用户ID列表
    /// </summary>
    [Required(ErrorMessage = "参与者列表不能为空")]
    [MinLength(1, ErrorMessage = "至少需要一个参与者")]
    public List<Guid> ParticipantIds { get; set; } = new();

    /// <summary>
    /// 会话类型
    /// </summary>
    public ConversationType ConversationType { get; set; } = ConversationType.Group;

    /// <summary>
    /// 初始消息内容
    /// </summary>
    [StringLength(2000, ErrorMessage = "初始消息长度不能超过2000个字符")]
    public string? InitialMessage { get; set; }

    /// <summary>
    /// 初始消息附件ID列表
    /// </summary>
    public List<Guid>? InitialMessageAttachmentIds { get; set; }
}

/// <summary>
/// 更新会话请求DTO
/// </summary>
public class UpdateConversationDto
{
    /// <summary>
    /// 会话主题
    /// </summary>
    [StringLength(200, ErrorMessage = "会话主题长度不能超过200个字符")]
    public string? Subject { get; set; }

    /// <summary>
    /// 会话描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "会话描述长度不能超过1000个字符")]
    public string? Description { get; set; }
}

/// <summary>
/// 消息文件上传DTO
/// </summary>
public class MessageFileUploadDto
{
    /// <summary>
    /// 文件流
    /// </summary>
    public Stream FileStream { get; set; } = null!;

    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }
}

/// <summary>
/// 批量操作结果DTO
/// </summary>
public class BatchOperationResult
{
    /// <summary>
    /// 成功数量
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数量
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 错误消息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccess => FailedCount == 0;
}

/// <summary>
/// 消息线程DTO
/// </summary>
public class MessageThreadDto
{
    /// <summary>
    /// 根消息
    /// </summary>
    public MessageDto RootMessage { get; set; } = null!;

    /// <summary>
    /// 回复消息列表
    /// </summary>
    public List<MessageDto> Replies { get; set; } = new();

    /// <summary>
    /// 总回复数
    /// </summary>
    public int TotalReplies { get; set; }

    /// <summary>
    /// 最后回复时间
    /// </summary>
    public DateTime? LastReplyAt { get; set; }
}

/// <summary>
/// 消息权限枚举
/// </summary>
public enum MessagePermission
{
    /// <summary>
    /// 查看权限
    /// </summary>
    View = 0,

    /// <summary>
    /// 编辑权限
    /// </summary>
    Edit = 1,

    /// <summary>
    /// 删除权限
    /// </summary>
    Delete = 2,

    /// <summary>
    /// 回复权限
    /// </summary>
    Reply = 3,

    /// <summary>
    /// 转发权限
    /// </summary>
    Forward = 4
}

/// <summary>
/// 会话权限枚举
/// </summary>
public enum ConversationPermission
{
    /// <summary>
    /// 查看权限
    /// </summary>
    View = 0,

    /// <summary>
    /// 发送消息权限
    /// </summary>
    SendMessage = 1,

    /// <summary>
    /// 编辑权限
    /// </summary>
    Edit = 2,

    /// <summary>
    /// 删除权限
    /// </summary>
    Delete = 3,

    /// <summary>
    /// 管理参与者权限
    /// </summary>
    ManageParticipants = 4,

    /// <summary>
    /// 管理权限
    /// </summary>
    Admin = 5
}

/// <summary>
/// 会话类型枚举
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// 私聊
    /// </summary>
    Private = 0,

    /// <summary>
    /// 群聊
    /// </summary>
    Group = 1,

    /// <summary>
    /// 系统会话
    /// </summary>
    System = 2
}