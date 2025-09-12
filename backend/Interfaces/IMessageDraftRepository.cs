using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 消息草稿仓储接口 - 遵循开闭原则和接口隔离原则
/// 定义消息草稿的CRUD操作、查询功能和自动保存管理
/// </summary>
public interface IMessageDraftRepository
{
    // 基础 CRUD 操作
    /// <summary>
    /// 根据ID获取草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿实体或null</returns>
    Task<MessageDraft?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取草稿（包含作者信息）
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿实体或null</returns>
    Task<MessageDraft?> GetByIdWithAuthorAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取草稿（包含收件人信息）
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿实体或null</returns>
    Task<MessageDraft?> GetByIdWithReceiverAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取草稿（包含附件信息）
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿实体或null</returns>
    Task<MessageDraft?> GetByIdWithAttachmentsAsync(Guid id);
    
    /// <summary>
    /// 根据ID获取草稿（完整信息，包含所有关联数据）
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿实体或null</returns>
    Task<MessageDraft?> GetByIdWithFullDetailsAsync(Guid id);
    
    /// <summary>
    /// 创建新草稿
    /// </summary>
    /// <param name="draft">草稿实体</param>
    /// <returns>创建后的草稿实体</returns>
    Task<MessageDraft> CreateAsync(MessageDraft draft);
    
    /// <summary>
    /// 更新草稿
    /// </summary>
    /// <param name="draft">草稿实体</param>
    /// <returns>更新后的草稿实体</returns>
    Task<MessageDraft> UpdateAsync(MessageDraft draft);
    
    /// <summary>
    /// 删除草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>是否删除成功</returns>
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// 批量删除草稿
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>删除成功的草稿数量</returns>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> draftIds);
    
    // 分页查询
    /// <summary>
    /// 分页获取草稿列表
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<MessageDraft>> GetPagedAsync(MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取用户的草稿列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>分页结果</returns>
    Task<PaginatedResult<MessageDraft>> GetByUserIdAsync(Guid userId, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取指定状态的草稿列表
    /// </summary>
    /// <param name="status">草稿状态</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByStatusAsync(DraftStatus status, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取指定类型的草稿列表
    /// </summary>
    /// <param name="messageType">消息类型</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByMessageTypeAsync(MessageType messageType, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取指定优先级的草稿列表
    /// </summary>
    /// <param name="priority">消息优先级</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByPriorityAsync(MessagePriority priority, MessageDraftFilterDto filter);
    
    // 会话相关查询
    /// <summary>
    /// 获取会话的草稿列表
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByConversationIdAsync(Guid conversationId, Guid userId, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取回复草稿列表
    /// </summary>
    /// <param name="parentId">父消息ID</param>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByParentIdAsync(Guid parentId, Guid userId, MessageDraftFilterDto filter);
    
    // 搜索和筛选
    /// <summary>
    /// 搜索草稿
    /// </summary>
    /// <param name="searchTerm">搜索关键词</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>搜索结果</returns>
    Task<IEnumerable<MessageDraft>> SearchDraftsAsync(string searchTerm, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取指定标签的草稿列表
    /// </summary>
    /// <param name="tag">标签</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByTagAsync(string tag, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取指定收件人的草稿列表
    /// </summary>
    /// <param name="receiverId">收件人ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByReceiverIdAsync(Guid receiverId, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取日期范围内的草稿列表
    /// </summary>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, MessageDraftFilterDto filter);
    
    // 定时发送管理
    /// <summary>
    /// 获取定时发送的草稿列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetScheduledDraftsAsync(Guid userId, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取即将发送的草稿列表（在指定时间内）
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="minutes">分钟数</param>
    /// <returns>草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetUpcomingScheduledDraftsAsync(Guid userId, int minutes = 60);
    
    /// <summary>
    /// 获取过期的定时发送草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>过期的草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetExpiredScheduledDraftsAsync(Guid userId);
    
    /// <summary>
    /// 取消定时发送
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否取消成功</returns>
    Task<bool> CancelScheduledSendAsync(Guid draftId);
    
    /// <summary>
    /// 批量取消定时发送
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>取消成功的草稿数量</returns>
    Task<int> CancelMultipleScheduledSendAsync(IEnumerable<Guid> draftIds);
    
    // 自动保存管理
    /// <summary>
    /// 更新自动保存时间
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateAutoSaveTimeAsync(Guid draftId);
    
    /// <summary>
    /// 获取需要自动保存的草稿列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="interval">自动保存间隔（秒）</param>
    /// <returns>需要自动保存的草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetDraftsNeedingAutoSaveAsync(Guid userId, int interval = 120);
    
    /// <summary>
    /// 获取最近自动保存的草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">草稿数量</param>
    /// <returns>最近自动保存的草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetRecentlyAutoSavedDraftsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 设置自动保存间隔
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="interval">自动保存间隔（秒）</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetAutoSaveIntervalAsync(Guid draftId, int interval);
    
    // 状态管理
    /// <summary>
    /// 更新草稿状态
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="status">新状态</param>
    /// <returns>是否更新成功</returns>
    Task<bool> UpdateDraftStatusAsync(Guid draftId, DraftStatus status);
    
    /// <summary>
    /// 批量更新草稿状态
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <param name="status">新状态</param>
    /// <returns>更新成功的草稿数量</returns>
    Task<int> UpdateMultipleDraftStatusAsync(IEnumerable<Guid> draftIds, DraftStatus status);
    
    /// <summary>
    /// 标记草稿为已发送
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkAsSentAsync(Guid draftId);
    
    /// <summary>
    /// 标记草稿为已取消
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkAsCancelledAsync(Guid draftId);
    
    /// <summary>
    /// 标记草稿为已过期
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否标记成功</returns>
    Task<bool> MarkAsExpiredAsync(Guid draftId);
    
    // 统计信息
    /// <summary>
    /// 获取草稿统计信息
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>统计信息</returns>
    Task<MessageStatsDto> GetDraftStatsAsync(Guid userId, MessageStatsFilterDto filter);
    
    /// <summary>
    /// 获取草稿总数
    /// </summary>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿总数</returns>
    Task<int> GetDraftCountAsync(MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取用户草稿数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="filter">筛选参数</param>
    /// <returns>草稿数量</returns>
    Task<int> GetUserDraftCountAsync(Guid userId, MessageDraftFilterDto filter);
    
    /// <summary>
    /// 获取定时发送草稿数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>定时发送草稿数量</returns>
    Task<int> GetScheduledDraftCountAsync(Guid userId);
    
    /// <summary>
    /// 获取过期草稿数量
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>过期草稿数量</returns>
    Task<int> GetExpiredDraftCountAsync(Guid userId);
    
    // 权限和验证
    /// <summary>
    /// 检查用户是否有权限查看草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserViewDraftAsync(Guid draftId, Guid userId);
    
    /// <summary>
    /// 检查用户是否有权限编辑草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserEditDraftAsync(Guid draftId, Guid userId);
    
    /// <summary>
    /// 检查用户是否有权限删除草稿
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>是否有权限</returns>
    Task<bool> CanUserDeleteDraftAsync(Guid draftId, Guid userId);
    
    /// <summary>
    /// 检查草稿是否过期
    /// </summary>
    /// <param name="draftId">草稿ID</param>
    /// <returns>是否过期</returns>
    Task<bool> IsDraftExpiredAsync(Guid draftId);
    
    // 高级查询
    /// <summary>
    /// 获取最新草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">草稿数量</param>
    /// <returns>最新草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetLatestDraftsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取最近编辑的草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">草稿数量</param>
    /// <returns>最近编辑的草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetRecentlyEditedDraftsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取高优先级草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="count">草稿数量</param>
    /// <returns>高优先级草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetHighPriorityDraftsAsync(Guid userId, int count = 10);
    
    /// <summary>
    /// 获取即将过期的草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="days">天数</param>
    /// <returns>即将过期的草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetExpiringDraftsAsync(Guid userId, int days = 7);
    
    /// <summary>
    /// 获取长时间未编辑的草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="days">天数</param>
    /// <returns>长时间未编辑的草稿列表</returns>
    Task<IEnumerable<MessageDraft>> GetStaleDraftsAsync(Guid userId, int days = 30);
    
    // 缓存相关
    /// <summary>
    /// 从缓存获取草稿
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>草稿实体或null</returns>
    Task<MessageDraft?> GetFromCacheAsync(Guid id);
    
    /// <summary>
    /// 设置草稿缓存
    /// </summary>
    /// <param name="draft">草稿实体</param>
    /// <param name="expiration">过期时间</param>
    /// <returns>是否设置成功</returns>
    Task<bool> SetCacheAsync(MessageDraft draft, TimeSpan? expiration = null);
    
    /// <summary>
    /// 移除草稿缓存
    /// </summary>
    /// <param name="id">草稿ID</param>
    /// <returns>是否移除成功</returns>
    Task<bool> RemoveCacheAsync(Guid id);
    
    /// <summary>
    /// 批量移除草稿缓存
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>移除成功的草稿数量</returns>
    Task<int> RemoveMultipleCacheAsync(IEnumerable<Guid> draftIds);
    
    // 批量操作
    /// <summary>
    /// 批量插入草稿
    /// </summary>
    /// <param name="drafts">草稿列表</param>
    /// <returns>插入成功的草稿数量</returns>
    Task<int> BulkInsertAsync(IEnumerable<MessageDraft> drafts);
    
    /// <summary>
    /// 批量更新草稿
    /// </summary>
    /// <param name="drafts">草稿列表</param>
    /// <returns>更新成功的草稿数量</returns>
    Task<int> BulkUpdateAsync(IEnumerable<MessageDraft> drafts);
    
    /// <summary>
    /// 批量删除草稿
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>删除成功的草稿数量</returns>
    Task<int> BulkDeleteAsync(IEnumerable<Guid> draftIds);
    
    /// <summary>
    /// 批量标记草稿为已发送
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>标记成功的草稿数量</returns>
    Task<int> BulkMarkAsSentAsync(IEnumerable<Guid> draftIds);
    
    /// <summary>
    /// 批量标记草稿为已取消
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>标记成功的草稿数量</returns>
    Task<int> BulkMarkAsCancelledAsync(IEnumerable<Guid> draftIds);
    
    /// <summary>
    /// 批量标记草稿为已过期
    /// </summary>
    /// <param name="draftIds">草稿ID列表</param>
    /// <returns>标记成功的草稿数量</returns>
    Task<int> BulkMarkAsExpiredAsync(IEnumerable<Guid> draftIds);
    
    // 草稿清理
    /// <summary>
    /// 清理过期草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>清理的草稿数量</returns>
    Task<int> CleanupExpiredDraftsAsync(Guid userId);
    
    /// <summary>
    /// 清理所有用户的过期草稿
    /// </summary>
    /// <returns>清理的草稿数量</returns>
    Task<int> CleanupAllExpiredDraftsAsync();
    
    /// <summary>
    /// 清理长时间未编辑的草稿
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="days">天数</param>
    /// <returns>清理的草稿数量</returns>
    Task<int> CleanupStaleDraftsAsync(Guid userId, int days = 90);
    
    /// <summary>
    /// 清理所有用户长时间未编辑的草稿
    /// </summary>
    /// <param name="days">天数</param>
    /// <returns>清理的草稿数量</returns>
    Task<int> CleanupAllStaleDraftsAsync(int days = 90);
}