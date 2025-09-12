using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeSnippetManager.Api.Models;

/// <summary>
/// 消息会话实体 - 遵循单一职责原则，只负责会话数据
/// </summary>
public class MessageConversation
{
    /// <summary>
    /// 会话唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 会话标题
    /// </summary>
    [StringLength(200, ErrorMessage = "标题长度不能超过200个字符")]
    public string? Title { get; set; }

    /// <summary>
    /// 会话描述
    /// </summary>
    [StringLength(1000, ErrorMessage = "描述长度不能超过1000个字符")]
    public string? Description { get; set; }

    /// <summary>
    /// 创建者用户ID
    /// </summary>
    [Required(ErrorMessage = "创建者ID不能为空")]
    public Guid CreatorId { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 更新时间
    /// </summary>
    [Required]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后消息ID
    /// </summary>
    public Guid? LastMessageId { get; set; }

    /// <summary>
    /// 最后消息时间
    /// </summary>
    public DateTime? LastMessageTime { get; set; }

    /// <summary>
    /// 是否已删除（软删除）
    /// </summary>
    [Required]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// 参与者数量
    /// </summary>
    [Required]
    public int ParticipantCount { get; set; } = 0;

    /// <summary>
    /// 创建者用户信息
    /// </summary>
    [ForeignKey("CreatorId")]
    public virtual User? Creator { get; set; }

    /// <summary>
    /// 最后消息信息
    /// </summary>
    [ForeignKey("LastMessageId")]
    public virtual Message? LastMessage { get; set; }

    /// <summary>
    /// 会话参与者列表
    /// </summary>
    public virtual ICollection<MessageConversationParticipant>? Participants { get; set; }

    /// <summary>
    /// 会话消息列表
    /// </summary>
    public virtual ICollection<Message>? Messages { get; set; }
}

/// <summary>
/// 消息会话参与者实体
/// </summary>
public class MessageConversationParticipant
{
    /// <summary>
    /// 参与者关系唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 会话ID
    /// </summary>
    [Required(ErrorMessage = "会话ID不能为空")]
    public Guid ConversationId { get; set; }

    /// <summary>
    /// 参与者用户ID
    /// </summary>
    [Required(ErrorMessage = "参与者ID不能为空")]
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// 加入时间
    /// </summary>
    [Required]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 离开时间
    /// </summary>
    public DateTime? LeftAt { get; set; }

    /// <summary>
    /// 是否活跃
    /// </summary>
    [Required]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// 会话信息
    /// </summary>
    [ForeignKey("ConversationId")]
    public virtual MessageConversation? Conversation { get; set; }

    /// <summary>
    /// 参与者用户信息
    /// </summary>
    [ForeignKey("ParticipantId")]
    public virtual User? Participant { get; set; }
}