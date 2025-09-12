using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 消息仓储接口 - 遵循开闭原则和接口隔离原则
/// 定义完整的消息CRUD操作、查询功能和会话管理
/// </summary>
public interface IMessageRepository
{
    // 基础 CRUD 操作
    /// <summary>
    /// 根据ID获取消息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取消息（包含发送者信息）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetByIdWithSenderAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取消息（包含发送者和接收者信息）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetByIdWithUsersAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取消息（包含附件信息）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetByIdWithAttachmentsAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取消息（包含回复消息）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetByIdWithRepliesAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取消息（完整信息，包含所有关联数据）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetByIdWithFullDetailsAsync(Guid id);
    
    /// <summary>
    /// 创建新消息
    /// </summary>
    /// <param name="message">消息实体</param>
    /// <returns>创建后的消息实体</returns>
    Task<Message> CreateAsync(Message message);
    
    /// <summary>
    /// 更新消息
    /// </summary>
    /// <param name="message">消息实体</param>
    /// <returns>更新后的消息实体</returns>
    Task<Message> UpdateAsync(Message message);
    
    /// <summary>
    /// 删除消息（软删除）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// 硬删除消息（从数据库中永久删除）
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> HardDeleteAsync(Guid id);
    
    /// <summary>
    /// 恢复已删除的消息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>是否恢复成功</returns>
    Task<bool> RestoreAsync(Guid id);
    
    // 分页查询
    /// <summary>
    /// 分页获取消息列表
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<Message>> GetPagedAsync(MessageFilterDto filter);
    
    /// <summary>
    /// 获取用户发送的消息列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<Message>> GetSentMessagesAsync(Guid userId, MessageFilterDto filter);
    
    /// <summary>
    /// 获取用户接收的消息列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<Message>> GetReceivedMessagesAsync(Guid userId, MessageFilterDto filter);
    
    /// <summary>
    /// 获取用户未读消息列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<Message>> GetUnreadMessagesAsync(Guid userId, MessageFilterDto filter);
    
    // 会话管理
    /// <summary>
    /// 获取用户的会话列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">会话筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<MessageConversationDto>> GetConversationsAsync(Guid userId, MessageConversationFilterDto filter);
    
    /// <summary>
    /// 获取会话的消息列表
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="filter">消息筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<Message>> GetConversationMessagesAsync(Guid conversationId, MessageFilterDto filter);
    
    /// <summary>
    /// 创建新会话
    /// </summary>
    /// <param name="participants">参与者用户ID列表</param>
    /// <param name="conversationType">会话类型</param>
    /// <param name="title">会话标题</param>
    /// <param name="description">会话描述</param>
    /// <returns>会话ID</returns>
    Task<Guid> CreateConversationAsync(IEnumerable<Guid> participants, ConversationType conversationType, string title, string? description = null);
    
    /// <summary>
    /// 更新会话信息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="title">会话标题</param>
    /// <param name="description">会话描述</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateConversationAsync(Guid conversationId, string title, string? description = null);
    
    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteConversationAsync(Guid conversationId, Guid userId);
    
    /// <summary>
    /// 添加会话参与者
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userIds">要添加的用户ID列表</param>
    /// <returns>是否添加成功</returns>
    Task<bool> AddConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds);
    
    /// <summary>
    /// 移除会话参与者
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userIds">要移除的用户ID列表</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveConversationParticipantsAsync(Guid conversationId, IEnumerable<Guid> userIds);
    
    // 回复管理
    /// <summary>
    /// 获取消息的回复列表
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<Message>> GetRepliesAsync(Guid messageId, MessageFilterDto filter);
    
    /// <summary>
    /// 获取父消息链
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>消息链列表</returns>
    Task<IEnumerable<Message>> GetParentChainAsync(Guid messageId);
    
    /// <summary>
    /// 获取根消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <returns>根消息实体</returns>
    Task<Message?> GetRootMessageAsync(Guid messageId);
    
    // 搜索和筛选
    /// <summary>
    /// 搜索消息
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>搜索结果</returns>
    Task<IEnumerable<Message>> SearchMessagesAsync(string searchTerm, MessageFilterDto filter);
    
    /// <summary>
    /// 获取指定状态的消息列表
    /// </summary>
    /// <param name="status">消息状态</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>消息列表</returns>
    Task<IEnumerable<Message>> GetMessagesByStatusAsync(MessageStatus status, MessageFilterDto filter);
    
    /// <summary>
    /// 获取指定类型的消息列表
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>消息列表</returns>
    Task<IEnumerable<Message>> GetMessagesByTypeAsync(MessageType messageType, MessageFilterDto filter);
    
    /// <summary>
    /// 获取指定优先级的消息列表
    /// </summary>
    /// <param name="priority">消息优先级</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>消息列表</returns>
    Task<IEnumerable<Message>> GetMessagesByPriorityAsync(MessagePriority priority, MessageFilterDto filter);
    
    /// <summary>
    /// 获取指定标签的消息列表
    /// </summary>
    /// <param name="tag">标签</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>消息列表</returns>
    Task<IEnumerable<Message>> GetMessagesByTagAsync(string tag, MessageFilterDto filter);
    
    /// <summary>
    /// 获取日期范围内的消息列表
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>消息列表</returns>
    Task<IEnumerable<Message>> GetMessagesByDateRangeAsync(DateTime startDate, DateTime endDate, MessageFilterDto filter);
    
    // 状态管理
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
    /// 标记会话中的所有消息为已读
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>标记成功的消息数量</returns>
    Task<int> MarkConversationAsReadAsync(Guid conversationId, Guid userId);
    
    /// <summary>
    /// 更新消息状态
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateMessageStatusAsync(Guid messageId, MessageStatus status);
    
    /// <summary>
    /// 批量更新消息状态
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <param name="status">新状态</param>
    /// <returns>更新成功的消息数量</returns>
    Task<int> UpdateMultipleMessageStatusAsync(IEnumerable<Guid> messageIds, MessageStatus status);
    
    // 统计信息
    /// <summary>
    /// 获取用户消息统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">统计筛选参数</param>
    /// <returns>统计信息</returns>
    Task<MessageStatsDto> GetUserMessageStatsAsync(Guid userId, MessageStatsFilterDto filter);
    
    /// <summary>
    /// 获取多个用户的消息统计信息
    /// </summary>
    /// <param name="userIds">用户ID列表</param>
    /// <param name="filter">统计筛选参数</param>
    /// <returns>用户统计信息列表</returns>
    Task<IEnumerable<UserMessageStatsDto>> GetUsersMessageStatsAsync(IEnumerable<Guid> userIds, MessageStatsFilterDto filter);
    
    /// <summary>
    /// 获取会话统计信息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <returns>会话统计信息</returns>
    Task<MessageStatsDto> GetConversationStatsAsync(Guid conversationId);
    
    /// <summary>
    /// 获取未读消息数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>未读消息数量</returns>
    Task<int> GetUnreadMessageCountAsync(Guid userId);
    
    /// <summary>
    /// 获取会话未读消息数量
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>未读消息数量</returns>
    Task<int> GetConversationUnreadCountAsync(Guid conversationId, Guid userId);
    
    /// <summary>
    /// 获取消息总数
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>消息总数</returns>
    Task<int> GetMessageCountAsync(MessageFilterDto filter);
    
    // 权限和验证
    /// <summary>
    /// 检查用户是否有权限查看消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserViewMessageAsync(Guid messageId, Guid userId);
    
    /// <summary>
    /// 检查用户是否有权限编辑消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserEditMessageAsync(Guid messageId, Guid userId);
    
    /// <summary>
    /// 检查用户是否有权限删除消息
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserDeleteMessageAsync(Guid messageId, Guid userId);
    
    /// <summary>
    /// 检查用户是否参与会话
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否参与</returns>
    Task<bool> IsUserInConversationAsync(Guid conversationId, Guid userId);
    
    // 高级查询
    /// <summary>
    /// 获取最新消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">消息数量</param>
    /// <returns>最新消息列表</returns>
    Task<IEnumerable<Message>> GetLatestMessagesAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取高优先级未读消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">消息数量</param>
    /// <returns>高优先级未读消息列表</returns>
    Task<IEnumerable<Message>> GetHighPriorityUnreadMessagesAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取即将过期的消息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="hours">小时数</param>
    /// <returns>即将过期的消息列表</returns>
    Task<IEnumerable<Message>> GetExpiringMessagesAsync(Guid userId, int hours = 24);
    
    /// <summary>
    /// 获取已过期的消息
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>已过期的消息列表</returns>
    Task<IEnumerable<Message>> GetExpiredMessagesAsync(MessageFilterDto filter);
    
    // 缓存相关
    /// <summary>
    /// 从缓存获取消息
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>消息实体或null</returns>
    Task<Message?> GetFromCacheAsync(Guid id);
    
    /// <summary>
    /// 设置消息缓存
    /// </summary>
    /// <param name="message">消息实体</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetCacheAsync(Message message, TimeSpan? expiration = null);
    
    /// <summary>
    /// 移除消息缓存
    /// </summary>
    /// <param name="id">消息ID</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveCacheAsync(Guid id);
    
    /// <summary>
    /// 批量移除消息缓存
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <returns>移除成功的消息数量</returns>
    Task<int> RemoveMultipleCacheAsync(IEnumerable<Guid> messageIds);
    
    // 批量操作
    /// <summary>
    /// 批量插入消息
    /// </summary>
    /// <param name="messages">消息列表</param>
    /// <returns>插入成功的消息数量</returns>
    Task<int> BulkInsertAsync(IEnumerable<Message> messages);
    
    /// <summary>
    /// 批量更新消息
    /// </summary>
    /// <param name="messages">消息列表</param>
    /// <returns>更新成功的消息数量</returns>
    Task<int> BulkUpdateAsync(IEnumerable<Message> messages);
    
    /// <summary>
    /// 批量删除消息
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <returns>删除成功的消息数量</returns>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> messageIds);
    
    /// <summary>
    /// 批量软删除消息
    /// </summary>
    /// <param name="messageIds">消息ID列表</param>
    /// <param name="userId">操作用户ID</param>
    /// <returns>删除成功的消息数量</returns>
    Task<int> BulkSoftDeleteAsync(IEnumerable<Guid> messageIds, Guid userId);
}