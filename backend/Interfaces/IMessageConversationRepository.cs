using CodeSnippetManager.Api.Models;
using CodeSnippetManager.Api.DTOs;

namespace CodeSnippetManager.Api.Interfaces;

/// <summary>
/// 消息会话仓储接口 - 定义消息会话数据访问操作
/// </summary>
public interface IMessageConversationRepository
{
    /// <summary>
    /// 创建新会话
    /// </summary>
    /// <param name="conversation">会话实体</param>
    /// <returns>创建后的会话实体</returns>
    Task<MessageConversation> CreateAsync(MessageConversation conversation);

    /// <summary>
    /// 根据ID获取会话
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <returns>会话实体或null</returns>
    Task<MessageConversation?> GetByIdAsync(Guid id);

    /// <summary>
    /// 根据参与者ID获取会话
    /// </summary>
    /// <param name="participantIds">参与者ID列表</param>
    /// <returns>会话实体列表</returns>
    Task<IEnumerable<MessageConversation>> GetByParticipantsAsync(IEnumerable<Guid> participantIds);

    /// <summary>
    /// 获取用户参与的所有会话
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>会话列表</returns>
    Task<(IEnumerable<MessageConversation> Conversations, int TotalCount)> GetByUserAsync(Guid userId, int page = 1, int pageSize = 20);

    /// <summary>
    /// 更新会话
    /// </summary>
    /// <param name="conversation">会话实体</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateAsync(MessageConversation conversation);

    /// <summary>
    /// 删除会话
    /// </summary>
    /// <param name="id">会话ID</param>
    /// <returns>是否成功</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// 添加参与者到会话
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="participantIds">参与者ID列表</param>
    /// <returns>是否成功</returns>
    Task<bool> AddParticipantsAsync(Guid conversationId, IEnumerable<Guid> participantIds);

    /// <summary>
    /// 从会话中移除参与者
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="participantId">参与者ID</param>
    /// <returns>是否成功</returns>
    Task<bool> RemoveParticipantAsync(Guid conversationId, Guid participantId);

    /// <summary>
    /// 更新会话最后消息信息
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="lastMessageId">最后消息ID</param>
    /// <param name="lastMessageTime">最后消息时间</param>
    /// <returns>是否成功</returns>
    Task<bool> UpdateLastMessageAsync(Guid conversationId, Guid lastMessageId, DateTime lastMessageTime);

    /// <summary>
    /// 获取会话的消息数量
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <returns>消息数量</returns>
    Task<int> GetMessageCountAsync(Guid conversationId);

    /// <summary>
    /// 获取会话的未读消息数量
    /// </summary>
    /// <param name="conversationId">会话ID</param>
    /// <param name="userId">用户ID</param>
    /// <returns>未读消息数量</returns>
    Task<int> GetUnreadMessageCountAsync(Guid conversationId, Guid userId);
}