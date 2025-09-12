-- CodeShare项目评论和通知系统数据库迁移脚本 v1.0.0
-- MySQL版本 - 支持完整的评论、消息、通知系统功能

-- =============================================
-- 评论系统表
-- =============================================

-- 创建Comments表 - 评论表，支持嵌套回复
CREATE TABLE Comments (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    Content TEXT NOT NULL,                      -- 评论内容
    SnippetId CHAR(36) NOT NULL,                -- 关联的代码片段ID
    UserId CHAR(36) NOT NULL,                   -- 评论者用户ID
    ParentId CHAR(36) NULL,                     -- 父评论ID（用于回复）
    ParentPath VARCHAR(1000) NULL,              -- 父评论路径（用于层级查询）
    Depth INT NOT NULL DEFAULT 0,               -- 评论深度
    LikeCount INT NOT NULL DEFAULT 0,           -- 点赞数量
    ReplyCount INT NOT NULL DEFAULT 0,          -- 回复数量
    Status TINYINT NOT NULL DEFAULT 0,          -- 评论状态 (0:正常, 1:已删除, 2:已隐藏, 3:待审核)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    DeletedAt DATETIME NULL,                     -- 删除时间（软删除）
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_comments_snippet_id (SnippetId),
    INDEX idx_comments_user_id (UserId),
    INDEX idx_comments_parent_id (ParentId),
    INDEX idx_comments_parent_path (ParentPath(255)),
    INDEX idx_comments_depth (Depth),
    INDEX idx_comments_status (Status),
    INDEX idx_comments_created_at (CreatedAt),
    INDEX idx_comments_updated_at (UpdatedAt),
    INDEX idx_comments_deleted_at (DeletedAt),
    INDEX idx_comments_like_count (LikeCount),
    INDEX idx_comments_reply_count (ReplyCount),
    
    -- 复合索引
    INDEX idx_comments_snippet_status (SnippetId, Status),
    INDEX idx_comments_snippet_created (SnippetId, CreatedAt),
    INDEX idx_comments_parent_path_created (ParentPath(255), CreatedAt),
    INDEX idx_comments_user_created (UserId, CreatedAt),
    INDEX idx_comments_status_created (Status, CreatedAt)
);

-- 创建CommentLikes表 - 评论点赞表
CREATE TABLE CommentLikes (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CommentId CHAR(36) NOT NULL,                -- 评论ID
    UserId CHAR(36) NOT NULL,                   -- 用户ID
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 唯一约束：一个用户只能给同一个评论点赞一次
    UNIQUE KEY uk_comment_likes_comment_user (CommentId, UserId),
    
    -- 索引优化
    INDEX idx_comment_likes_comment_id (CommentId),
    INDEX idx_comment_likes_user_id (UserId),
    INDEX idx_comment_likes_created_at (CreatedAt),
    
    -- 复合索引
    INDEX idx_comment_likes_comment_created (CommentId, CreatedAt),
    INDEX idx_comment_likes_user_created (UserId, CreatedAt)
);

-- 创建CommentReports表 - 评论举报表
CREATE TABLE CommentReports (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CommentId CHAR(36) NOT NULL,                -- 评论ID
    UserId CHAR(36) NOT NULL,                   -- 举报者用户ID
    Reason TINYINT NOT NULL,                     -- 举报原因
    Description TEXT NULL,                       -- 举报描述
    Status TINYINT NOT NULL DEFAULT 0,          -- 举报状态 (0:待处理, 1:已处理, 2:已驳回, 3:调查中)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    HandledAt DATETIME NULL,                     -- 处理时间
    HandledBy CHAR(36) NULL,                    -- 处理者用户ID
    Resolution TEXT NULL,                        -- 处理结果说明
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (HandledBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    -- 索引优化
    INDEX idx_comment_reports_comment_id (CommentId),
    INDEX idx_comment_reports_user_id (UserId),
    INDEX idx_comment_reports_reason (Reason),
    INDEX idx_comment_reports_status (Status),
    INDEX idx_comment_reports_created_at (CreatedAt),
    INDEX idx_comment_reports_handled_at (HandledAt),
    INDEX idx_comment_reports_handled_by (HandledBy),
    
    -- 复合索引
    INDEX idx_comment_reports_comment_status (CommentId, Status),
    INDEX idx_comment_reports_status_created (Status, CreatedAt),
    INDEX idx_comment_reports_reason_status (Reason, Status),
    INDEX idx_comment_reports_user_created (UserId, CreatedAt),
    INDEX idx_comment_reports_handled_created (HandledBy, HandledAt)
);

-- =============================================
-- 消息系统表
-- =============================================

-- 创建Messages表 - 消息表，支持用户间通信
CREATE TABLE Messages (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    SenderId CHAR(36) NOT NULL,                 -- 发送者用户ID
    ReceiverId CHAR(36) NOT NULL,               -- 接收者用户ID
    Subject VARCHAR(200) NULL,                  -- 消息主题
    Content TEXT NOT NULL,                      -- 消息内容
    MessageType TINYINT NOT NULL DEFAULT 0,      -- 消息类型 (0:用户消息, 1:系统消息, 2:通知消息, 3:广播消息, 4:自动回复)
    Status TINYINT NOT NULL DEFAULT 1,          -- 消息状态 (0:草稿, 1:已发送, 2:已送达, 3:已读, 4:已回复, 5:已转发, 6:已删除, 7:发送失败, 8:已过期)
    Priority TINYINT NOT NULL DEFAULT 1,        -- 优先级 (0:低, 1:普通, 2:高, 3:紧急)
    ParentId CHAR(36) NULL,                     -- 父消息ID（用于回复）
    ConversationId CHAR(36) NULL,               -- 会话ID（用于消息分组）
    IsRead BOOLEAN NOT NULL DEFAULT 0,         -- 是否已读
    ReadAt DATETIME NULL,                       -- 阅读时间
    Tag VARCHAR(100) NULL,                      -- 消息标签
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    DeletedAt DATETIME NULL,                     -- 删除时间（软删除）
    SenderDeletedAt DATETIME NULL,              -- 发送者删除时间
    ReceiverDeletedAt DATETIME NULL,            -- 接收者删除时间
    ExpiresAt DATETIME NULL,                    -- 过期时间
    SenderIp VARCHAR(45) NULL,                   -- 发送者IP地址
    SenderUserAgent VARCHAR(500) NULL,           -- 发送者User-Agent
    
    FOREIGN KEY (SenderId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentId) REFERENCES Messages(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_messages_sender_id (SenderId),
    INDEX idx_messages_receiver_id (ReceiverId),
    INDEX idx_messages_message_type (MessageType),
    INDEX idx_messages_status (Status),
    INDEX idx_messages_priority (Priority),
    INDEX idx_messages_parent_id (ParentId),
    INDEX idx_messages_conversation_id (ConversationId),
    INDEX idx_messages_is_read (IsRead),
    INDEX idx_messages_read_at (ReadAt),
    INDEX idx_messages_tag (Tag),
    INDEX idx_messages_created_at (CreatedAt),
    INDEX idx_messages_updated_at (UpdatedAt),
    INDEX idx_messages_deleted_at (DeletedAt),
    INDEX idx_messages_expires_at (ExpiresAt),
    
    -- 复合索引
    INDEX idx_messages_sender_receiver (SenderId, ReceiverId),
    INDEX idx_messages_conversation_created (ConversationId, CreatedAt),
    INDEX idx_messages_status_created (Status, CreatedAt),
    INDEX idx_messages_priority_created (Priority, CreatedAt),
    INDEX idx_messages_sender_created (SenderId, CreatedAt),
    INDEX idx_messages_receiver_created (ReceiverId, CreatedAt),
    INDEX idx_messages_unread_messages (ReceiverId, IsRead, Status)
);

-- 创建MessageAttachments表 - 消息附件表
CREATE TABLE MessageAttachments (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    MessageId CHAR(36) NOT NULL,                 -- 关联的消息ID
    FileName VARCHAR(255) NOT NULL,             -- 附件文件名
    OriginalFileName VARCHAR(255) NOT NULL,      -- 附件原始文件名
    FileSize BIGINT NOT NULL,                    -- 文件大小（字节）
    ContentType VARCHAR(100) NULL,              -- 文件类型
    FileExtension VARCHAR(10) NULL,             -- 文件扩展名
    FilePath VARCHAR(500) NOT NULL,             -- 文件存储路径
    FileUrl VARCHAR(1000) NULL,                 -- 文件访问URL
    AttachmentType TINYINT NOT NULL,             -- 附件类型 (0:图片, 1:文档, 2:视频, 3:音频, 4:压缩文件, 5:代码文件, 99:其他)
    AttachmentStatus TINYINT NOT NULL DEFAULT 0, -- 附件状态 (0:活跃, 1:上传中, 2:上传失败, 3:已删除, 4:已过期, 5:病毒检测中, 6:检测到病毒)
    FileHash VARCHAR(64) NULL,                  -- 文件哈希值
    ThumbnailPath VARCHAR(500) NULL,            -- 缩略图路径
    UploadProgress INT NOT NULL DEFAULT 0,      -- 上传进度（0-100）
    DownloadCount INT NOT NULL DEFAULT 0,        -- 下载次数
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 上传时间
    LastDownloadedAt DATETIME NULL,             -- 最后下载时间
    DeletedAt DATETIME NULL,                    -- 删除时间
    
    FOREIGN KEY (MessageId) REFERENCES Messages(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_message_attachments_message_id (MessageId),
    INDEX idx_message_attachments_file_name (FileName),
    INDEX idx_message_attachments_original_file_name (OriginalFileName),
    INDEX idx_message_attachments_content_type (ContentType),
    INDEX idx_message_attachments_attachment_type (AttachmentType),
    INDEX idx_message_attachments_attachment_status (AttachmentStatus),
    INDEX idx_message_attachments_file_hash (FileHash),
    INDEX idx_message_attachments_uploaded_at (UploadedAt),
    INDEX idx_message_attachments_download_count (DownloadCount),
    
    -- 复合索引
    INDEX idx_message_attachments_message_type (MessageId, AttachmentType),
    INDEX idx_message_attachments_message_status (MessageId, AttachmentStatus),
    INDEX idx_message_attachments_type_status (AttachmentType, AttachmentStatus),
    INDEX idx_message_attachments_upload_created (UploadedAt, AttachmentStatus)
);

-- 创建MessageDrafts表 - 消息草稿表
CREATE TABLE MessageDrafts (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    AuthorId CHAR(36) NOT NULL,                 -- 草稿作者用户ID
    ReceiverId CHAR(36) NULL,                   -- 收件人用户ID
    Subject VARCHAR(200) NULL,                  -- 消息主题
    Content TEXT NOT NULL,                      -- 消息内容
    MessageType TINYINT NOT NULL DEFAULT 0,      -- 消息类型
    Priority TINYINT NOT NULL DEFAULT 1,        -- 优先级
    ParentId CHAR(36) NULL,                     -- 父消息ID（用于回复草稿）
    ConversationId CHAR(36) NULL,                -- 会话ID
    Tag VARCHAR(100) NULL,                      -- 消息标签
    Status TINYINT NOT NULL DEFAULT 0,          -- 草稿状态 (0:草稿, 1:已发送, 2:已取消, 3:已过期)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    LastAutoSavedAt DATETIME NULL,              -- 最后自动保存时间
    ScheduledToSendAt DATETIME NULL,            -- 计划发送时间
    ExpiresAt DATETIME NULL,                    -- 过期时间
    IsScheduled BOOLEAN NOT NULL DEFAULT 0,     -- 是否定时发送
    AutoSaveInterval INT NOT NULL DEFAULT 120,   -- 自动保存间隔（秒）
    Notes TEXT NULL,                             -- 草稿备注
    
    FOREIGN KEY (AuthorId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ParentId) REFERENCES Messages(Id) ON DELETE SET NULL,
    
    -- 索引优化
    INDEX idx_message_drafts_author_id (AuthorId),
    INDEX idx_message_drafts_receiver_id (ReceiverId),
    INDEX idx_message_drafts_message_type (MessageType),
    INDEX idx_message_drafts_priority (Priority),
    INDEX idx_message_drafts_parent_id (ParentId),
    INDEX idx_message_drafts_conversation_id (ConversationId),
    INDEX idx_message_drafts_status (Status),
    INDEX idx_message_drafts_created_at (CreatedAt),
    INDEX idx_message_drafts_updated_at (UpdatedAt),
    INDEX idx_message_drafts_scheduled_to_send_at (ScheduledToSendAt),
    INDEX idx_message_drafts_expires_at (ExpiresAt),
    INDEX idx_message_drafts_is_scheduled (IsScheduled),
    
    -- 复合索引
    INDEX idx_message_drafts_author_status (AuthorId, Status),
    INDEX idx_message_drafts_author_created (AuthorId, CreatedAt),
    INDEX idx_message_drafts_status_created (Status, CreatedAt),
    INDEX idx_message_drafts_scheduled_created (ScheduledToSendAt, CreatedAt),
    INDEX idx_message_drafts_author_scheduled (AuthorId, IsScheduled)
);

-- 创建MessageDraftAttachments表 - 草稿附件表
CREATE TABLE MessageDraftAttachments (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    DraftId CHAR(36) NOT NULL,                  -- 关联的草稿ID
    FileName VARCHAR(255) NOT NULL,             -- 附件文件名
    OriginalFileName VARCHAR(255) NOT NULL,      -- 附件原始文件名
    FileSize BIGINT NOT NULL,                    -- 文件大小（字节）
    ContentType VARCHAR(100) NULL,              -- 文件类型
    FileExtension VARCHAR(10) NULL,             -- 文件扩展名
    FilePath VARCHAR(500) NOT NULL,             -- 文件存储路径
    FileUrl VARCHAR(1000) NULL,                 -- 文件访问URL
    AttachmentType TINYINT NOT NULL,             -- 附件类型
    AttachmentStatus TINYINT NOT NULL DEFAULT 0, -- 附件状态
    UploadProgress INT NOT NULL DEFAULT 0,      -- 上传进度（0-100）
    UploadedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 上传时间
    DeletedAt DATETIME NULL,                    -- 删除时间
    
    FOREIGN KEY (DraftId) REFERENCES MessageDrafts(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_message_draft_attachments_draft_id (DraftId),
    INDEX idx_message_draft_attachments_file_name (FileName),
    INDEX idx_message_draft_attachments_original_file_name (OriginalFileName),
    INDEX idx_message_draft_attachments_content_type (ContentType),
    INDEX idx_message_draft_attachments_attachment_type (AttachmentType),
    INDEX idx_message_draft_attachments_attachment_status (AttachmentStatus),
    INDEX idx_message_draft_attachments_uploaded_at (UploadedAt),
    
    -- 复合索引
    INDEX idx_message_draft_attachments_draft_type (DraftId, AttachmentType),
    INDEX idx_message_draft_attachments_draft_status (DraftId, AttachmentStatus)
);

-- 创建Conversations表 - 会话表
CREATE TABLE Conversations (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    Subject VARCHAR(200) NOT NULL,              -- 会话主题
    CreatedBy CHAR(36) NOT NULL,                -- 创建者用户ID
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    IsArchived BOOLEAN NOT NULL DEFAULT 0,      -- 是否已归档
    IsPinned BOOLEAN NOT NULL DEFAULT 0,         -- 是否已置顶
    IsMuted BOOLEAN NOT NULL DEFAULT 0,          -- 是否已静音
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_conversations_created_by (CreatedBy),
    INDEX idx_conversations_created_at (CreatedAt),
    INDEX idx_conversations_updated_at (UpdatedAt),
    INDEX idx_conversations_is_archived (IsArchived),
    INDEX idx_conversations_is_pinned (IsPinned),
    INDEX idx_conversations_is_muted (IsMuted),
    
    -- 复合索引
    INDEX idx_conversations_archived_created (IsArchived, CreatedAt),
    INDEX idx_conversations_pinned_created (IsPinned, CreatedAt),
    INDEX idx_conversations_muted_created (IsMuted, CreatedAt)
);

-- 创建ConversationParticipants表 - 会话参与者表
CREATE TABLE ConversationParticipants (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    ConversationId CHAR(36) NOT NULL,            -- 会话ID
    UserId CHAR(36) NOT NULL,                   -- 用户ID
    Role VARCHAR(50) NOT NULL DEFAULT 'Member', -- 角色
    JoinedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 加入时间
    LastReadAt DATETIME NULL,                   -- 最后阅读时间
    UnreadCount INT NOT NULL DEFAULT 0,         -- 未读消息数
    IsMuted BOOLEAN NOT NULL DEFAULT 0,         -- 是否静音
    
    FOREIGN KEY (ConversationId) REFERENCES Conversations(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 唯一约束：一个用户在一个会话中只能有一条记录
    UNIQUE KEY uk_conversation_participants_conversation_user (ConversationId, UserId),
    
    -- 索引优化
    INDEX idx_conversation_participants_conversation_id (ConversationId),
    INDEX idx_conversation_participants_user_id (UserId),
    INDEX idx_conversation_participants_role (Role),
    INDEX idx_conversation_participants_joined_at (JoinedAt),
    INDEX idx_conversation_participants_last_read_at (LastReadAt),
    INDEX idx_conversation_participants_unread_count (UnreadCount),
    INDEX idx_conversation_participants_is_muted (IsMuted),
    
    -- 复合索引
    INDEX idx_conversation_participants_conversation_user (ConversationId, UserId),
    INDEX idx_conversation_participants_user_unread (UserId, UnreadCount),
    INDEX idx_conversation_participants_conversation_joined (ConversationId, JoinedAt)
);

-- =============================================
-- 通知系统表
-- =============================================

-- 创建Notifications表 - 通知表
CREATE TABLE Notifications (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    UserId CHAR(36) NOT NULL,                   -- 接收通知的用户ID
    Type TINYINT NOT NULL,                      -- 通知类型 (0:评论, 1:回复, 2:消息, 3:系统, 4:点赞, 5:分享, 6:关注, 7:提及, 8:标签, 9:安全, 10:账户, 11:更新, 12:维护, 13:公告, 99:自定义)
    Title VARCHAR(200) NOT NULL,                -- 通知标题
    Content TEXT NULL,                          -- 通知内容
    Message VARCHAR(500) NULL,                  -- 通知消息
    Priority TINYINT NOT NULL DEFAULT 1,        -- 优先级 (0:低, 1:普通, 2:高, 3:紧急, 4:系统紧急)
    Status TINYINT NOT NULL DEFAULT 5,          -- 通知状态 (0:待发送, 1:发送中, 2:已发送, 3:已送达, 4:已读, 5:未读, 6:已确认, 7:发送失败, 8:已过期, 9:已取消, 10:已归档)
    RelatedEntityType TINYINT NULL,             -- 相关实体类型 (0:代码片段, 1:评论, 2:消息, 3:用户, 4:标签, 5:分享, 6:系统, 99:其他)
    RelatedEntityId VARCHAR(36) NULL,           -- 相关实体ID
    TriggeredByUserId CHAR(36) NULL,             -- 触发通知的用户ID
    Action TINYINT NULL,                        -- 操作类型 (0:创建, 1:更新, 2:删除, 3:点赞, 4:分享, 5:关注, 6:提及, 7:评论, 8:回复, 9:报告, 10:审核, 99:其他)
    Channel TINYINT NOT NULL DEFAULT 0,         -- 通知渠道 (0:应用内, 1:邮件, 2:推送, 3:桌面, 4:短信, 5:Webhook, 6:Slack, 7:微信, 8:钉钉, 9:企业微信)
    IsRead BOOLEAN NOT NULL DEFAULT 0,         -- 是否已读
    ReadAt DATETIME NULL,                       -- 阅读时间
    DeliveredAt DATETIME NULL,                  -- 送达时间
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    ExpiresAt DATETIME NULL,                    -- 过期时间
    ScheduledToSendAt DATETIME NULL,            -- 计划发送时间
    SendCount INT NOT NULL DEFAULT 0,            -- 发送次数
    LastSentAt DATETIME NULL,                    -- 最后发送时间
    ErrorMessage TEXT NULL,                      -- 错误信息
    DataJson TEXT NULL,                         -- 通知数据（JSON格式）
    Tag VARCHAR(100) NULL,                      -- 通知标签
    Icon VARCHAR(50) NULL,                      -- 通知图标
    Color VARCHAR(20) NULL,                     -- 通知颜色
    RequiresConfirmation BOOLEAN NOT NULL DEFAULT 0, -- 是否需要确认
    ConfirmedAt DATETIME NULL,                  -- 确认时间
    IsArchived BOOLEAN NOT NULL DEFAULT 0,      -- 是否已归档
    ArchivedAt DATETIME NULL,                    -- 归档时间
    IsDeleted BOOLEAN NOT NULL DEFAULT 0,      -- 是否已删除
    DeletedAt DATETIME NULL,                    -- 删除时间
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (TriggeredByUserId) REFERENCES Users(Id) ON DELETE SET NULL,
    
    -- 索引优化
    INDEX idx_notifications_user_id (UserId),
    INDEX idx_notifications_type (Type),
    INDEX idx_notifications_priority (Priority),
    INDEX idx_notifications_status (Status),
    INDEX idx_notifications_related_entity (RelatedEntityType, RelatedEntityId),
    INDEX idx_notifications_triggered_by (TriggeredByUserId),
    INDEX idx_notifications_action (Action),
    INDEX idx_notifications_channel (Channel),
    INDEX idx_notifications_is_read (IsRead),
    INDEX idx_notifications_read_at (ReadAt),
    INDEX idx_notifications_delivered_at (DeliveredAt),
    INDEX idx_notifications_created_at (CreatedAt),
    INDEX idx_notifications_updated_at (UpdatedAt),
    INDEX idx_notifications_expires_at (ExpiresAt),
    INDEX idx_notifications_scheduled_to_send_at (ScheduledToSendAt),
    INDEX idx_notifications_tag (Tag),
    INDEX idx_notifications_is_archived (IsArchived),
    INDEX idx_notifications_is_deleted (IsDeleted),
    
    -- 复合索引
    INDEX idx_notifications_user_status (UserId, Status),
    INDEX idx_notifications_user_type (UserId, Type),
    INDEX idx_notifications_user_created (UserId, CreatedAt),
    INDEX idx_notifications_status_created (Status, CreatedAt),
    INDEX idx_notifications_priority_created (Priority, CreatedAt),
    INDEX idx_notifications_type_created (Type, CreatedAt),
    INDEX idx_notifications_unread_notifications (UserId, IsRead, Status),
    INDEX idx_notifications_pending_notifications (Status, ScheduledToSendAt),
    INDEX idx_notifications_expiring_notifications (ExpiresAt, Status)
);

-- 创建NotificationSettings表 - 通知设置表
CREATE TABLE NotificationSettings (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    UserId CHAR(36) NOT NULL,                   -- 用户ID
    NotificationType TINYINT NULL,               -- 通知类型（NULL表示全局设置）
    EnableInApp BOOLEAN NOT NULL DEFAULT 1,     -- 启用应用内通知
    EnableEmail BOOLEAN NOT NULL DEFAULT 1,      -- 启用邮件通知
    EnablePush BOOLEAN NOT NULL DEFAULT 1,       -- 启用推送通知
    EnableDesktop BOOLEAN NOT NULL DEFAULT 1,    -- 启用桌面通知
    EnableSound BOOLEAN NOT NULL DEFAULT 1,      -- 启用声音提醒
    Frequency TINYINT NOT NULL DEFAULT 0,       -- 通知频率 (0:立即, 1:每小时, 2:每日, 3:每周, 4:每月, 5:从不)
    QuietHoursStart TIME NULL,                  -- 免打扰开始时间
    QuietHoursEnd TIME NULL,                    -- 免打扰结束时间
    EnableQuietHours BOOLEAN NOT NULL DEFAULT 0, -- 是否启用免打扰
    EmailFrequency TINYINT NOT NULL DEFAULT 0,   -- 邮件通知频率 (0:立即, 1:每小时汇总, 2:每日汇总, 3:每周汇总, 4:从不)
    BatchIntervalMinutes INT NOT NULL DEFAULT 30, -- 批量通知间隔（分钟）
    EnableBatching BOOLEAN NOT NULL DEFAULT 0,   -- 是否启用批量通知
    Language VARCHAR(10) NOT NULL DEFAULT 'zh-CN', -- 通知语言
    TimeZone VARCHAR(50) NOT NULL DEFAULT 'Asia/Shanghai', -- 时区
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    LastUsedAt DATETIME NULL,                   -- 最后使用时间
    IsDefault BOOLEAN NOT NULL DEFAULT 0,       -- 是否为默认设置
    Name VARCHAR(100) NULL,                     -- 设置名称
    Description TEXT NULL,                       -- 设置描述
    IsActive BOOLEAN NOT NULL DEFAULT 1,        -- 是否激活
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_notification_settings_user_id (UserId),
    INDEX idx_notification_settings_notification_type (NotificationType),
    INDEX idx_notification_settings_frequency (Frequency),
    INDEX idx_notification_settings_email_frequency (EmailFrequency),
    INDEX idx_notification_settings_language (Language),
    INDEX idx_notification_settings_time_zone (TimeZone),
    INDEX idx_notification_settings_created_at (CreatedAt),
    INDEX idx_notification_settings_updated_at (UpdatedAt),
    INDEX idx_notification_settings_is_default (IsDefault),
    INDEX idx_notification_settings_is_active (IsActive),
    
    -- 复合索引
    INDEX idx_notification_settings_user_type (UserId, NotificationType),
    INDEX idx_notification_settings_user_active (UserId, IsActive),
    INDEX idx_notification_settings_type_active (NotificationType, IsActive),
    INDEX idx_notification_settings_frequency_active (Frequency, IsActive)
);

-- 创建NotificationDeliveryHistory表 - 通知送达记录表
CREATE TABLE NotificationDeliveryHistory (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    NotificationId CHAR(36) NOT NULL,            -- 关联的通知ID
    Channel TINYINT NOT NULL,                   -- 发送渠道
    Status TINYINT NOT NULL,                    -- 发送状态 (0:待发送, 1:发送中, 2:已发送, 3:已送达, 4:已读, 5:发送失败, 6:已跳过, 7:已取消, 8:已过期)
    SentAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 发送时间
    DeliveredAt DATETIME NULL,                  -- 送达时间
    ReadAt DATETIME NULL,                       -- 阅读时间
    ErrorMessage TEXT NULL,                     -- 错误信息
    RetryCount INT NOT NULL DEFAULT 0,           -- 重试次数
    LastRetryAt DATETIME NULL,                  -- 最后重试时间
    RecipientAddress VARCHAR(500) NULL,         -- 接收地址（邮箱地址、设备令牌等）
    Provider VARCHAR(50) NULL,                  -- 发送服务提供商
    Cost DECIMAL(10,4) NULL,                    -- 发送成本
    MetadataJson TEXT NULL,                      -- 发送元数据（JSON格式）
    
    FOREIGN KEY (NotificationId) REFERENCES Notifications(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_notification_delivery_history_notification_id (NotificationId),
    INDEX idx_notification_delivery_history_channel (Channel),
    INDEX idx_notification_delivery_history_status (Status),
    INDEX idx_notification_delivery_history_sent_at (SentAt),
    INDEX idx_notification_delivery_history_delivered_at (DeliveredAt),
    INDEX idx_notification_delivery_history_read_at (ReadAt),
    INDEX idx_notification_delivery_history_retry_count (RetryCount),
    INDEX idx_notification_delivery_history_last_retry_at (LastRetryAt),
    INDEX idx_notification_delivery_history_recipient_address (RecipientAddress),
    INDEX idx_notification_delivery_history_provider (Provider),
    INDEX idx_notification_delivery_history_cost (Cost),
    
    -- 复合索引
    INDEX idx_notification_delivery_history_notification_channel (NotificationId, Channel),
    INDEX idx_notification_delivery_history_notification_status (NotificationId, Status),
    INDEX idx_notification_delivery_history_channel_status (Channel, Status),
    INDEX idx_notification_delivery_history_status_sent (Status, SentAt),
    INDEX idx_notification_delivery_history_provider_status (Provider, Status)
);

-- =============================================
-- 额外系统表
-- =============================================

-- 创建SystemSettings表 - 系统设置表
CREATE TABLE SystemSettings (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    UpdatedBy VARCHAR(100) NOT NULL DEFAULT 'System',       -- 最后更新者
    SiteSettingsJson TEXT NOT NULL DEFAULT '{}',            -- 站点设置JSON数据
    SecuritySettingsJson TEXT NOT NULL DEFAULT '{}',       -- 安全设置JSON数据
    FeatureSettingsJson TEXT NOT NULL DEFAULT '{}',        -- 功能设置JSON数据
    EmailSettingsJson TEXT NOT NULL DEFAULT '{}',           -- 邮件设置JSON数据
    
    -- 索引优化
    INDEX idx_system_settings_created_at (CreatedAt),
    INDEX idx_system_settings_updated_at (UpdatedAt),
    INDEX idx_system_settings_updated_by (UpdatedBy)
);

-- 创建SettingsHistory表 - 设置历史记录表
CREATE TABLE SettingsHistory (
    Id CHAR(36) PRIMARY KEY,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    SettingType VARCHAR(50) NOT NULL,                     -- 设置类型
    SettingKey VARCHAR(200) NOT NULL,                      -- 设置键名
    OldValue TEXT,                                         -- 变更前的值
    NewValue TEXT,                                         -- 变更后的值
    ChangedBy VARCHAR(100) NOT NULL,                      -- 操作人
    ChangedById CHAR(36),                                 -- 操作人ID
    ChangeReason TEXT,                                     -- 变更原因
    ChangeCategory VARCHAR(50) NOT NULL DEFAULT 'Other',   -- 变更分类
    ClientIp VARCHAR(45),                                  -- 客户端IP
    UserAgent TEXT,                                        -- 用户代理
    IsImportant BOOLEAN NOT NULL DEFAULT 0,               -- 是否重要变更
    Status VARCHAR(20) NOT NULL DEFAULT 'Success',        -- 变更状态
    ErrorMessage TEXT,                                     -- 错误信息
    Metadata TEXT,                                         -- 额外元数据
    
    -- 索引优化
    INDEX idx_settings_history_created_at (CreatedAt),
    INDEX idx_settings_history_setting_type (SettingType),
    INDEX idx_settings_history_setting_key (SettingKey),
    INDEX idx_settings_history_changed_by (ChangedBy),
    INDEX idx_settings_history_change_category (ChangeCategory),
    INDEX idx_settings_history_is_important (IsImportant),
    INDEX idx_settings_history_status (Status),
    INDEX idx_settings_history_client_ip (ClientIp),
    
    -- 复合索引
    INDEX idx_settings_history_type_created (SettingType, CreatedAt),
    INDEX idx_settings_history_category_created (ChangeCategory, CreatedAt),
    INDEX idx_settings_history_user_created (ChangedBy, CreatedAt),
    INDEX idx_settings_history_important_created (IsImportant, CreatedAt)
);

-- =============================================
-- 视图创建
-- =============================================

-- 创建消息统计视图
CREATE VIEW MessageStatsView AS
SELECT 
    m.Id as MessageId,
    m.Subject,
    m.Content,
    m.MessageType,
    m.Status,
    m.Priority,
    m.SenderId,
    s.Username as SenderName,
    m.ReceiverId,
    r.Username as ReceiverName,
    m.IsRead,
    m.CreatedAt,
    m.ReadAt,
    COUNT(ma.Id) as AttachmentCount,
    SUM(ma.FileSize) as TotalAttachmentSize
FROM Messages m
LEFT JOIN MessageAttachments ma ON m.Id = ma.MessageId
LEFT JOIN Users s ON m.SenderId = s.Id
LEFT JOIN Users r ON m.ReceiverId = r.Id
GROUP BY m.Id, m.Subject, m.Content, m.MessageType, m.Status, m.Priority, 
         m.SenderId, s.Username, m.ReceiverId, r.Username, m.IsRead, m.CreatedAt, m.ReadAt;

-- 创建会话统计视图
CREATE VIEW ConversationStatsView AS
SELECT 
    c.Id as ConversationId,
    c.Subject,
    c.CreatedBy,
    u.Username as CreatorName,
    c.CreatedAt,
    c.UpdatedAt,
    c.IsArchived,
    c.IsPinned,
    c.IsMuted,
    COUNT(DISTINCT cp.UserId) as ParticipantCount,
    COUNT(m.Id) as MessageCount,
    COUNT(CASE WHEN m.IsRead = 0 THEN 1 END) as UnreadCount,
    MAX(m.CreatedAt) as LastMessageAt
FROM Conversations c
LEFT JOIN ConversationParticipants cp ON c.Id = cp.ConversationId
LEFT JOIN Messages m ON c.Id = m.ConversationId
LEFT JOIN Users u ON c.CreatedBy = u.Id
GROUP BY c.Id, c.Subject, c.CreatedBy, u.Username, c.CreatedAt, c.UpdatedAt, 
         c.IsArchived, c.IsPinned, c.IsMuted;

-- 创建通知统计视图
CREATE VIEW NotificationStatsView AS
SELECT 
    n.UserId,
    u.Username as UserName,
    COUNT(n.Id) as TotalNotifications,
    COUNT(CASE WHEN n.IsRead = 0 THEN 1 END) as UnreadCount,
    COUNT(CASE WHEN n.Status = 7 THEN 1 END) as FailedCount,
    COUNT(CASE WHEN n.IsArchived = 1 THEN 1 END) as ArchivedCount,
    COUNT(CASE WHEN n.IsDeleted = 1 THEN 1 END) as DeletedCount,
    COUNT(CASE WHEN n.Priority >= 3 THEN 1 END) as HighPriorityCount,
    COUNT(CASE WHEN n.Type = 3 THEN 1 END) as SystemNotificationCount,
    COUNT(CASE WHEN n.Type IN (0, 1, 4, 5, 6, 7) THEN 1 END) as UserInteractionCount,
    MAX(n.CreatedAt) as LastNotificationAt,
    MIN(n.CreatedAt) as FirstNotificationAt,
    AVG(n.SendCount) as AverageSendCount
FROM Notifications n
LEFT JOIN Users u ON n.UserId = u.Id
GROUP BY n.UserId, u.Username;

-- 创建通知类型统计视图
CREATE VIEW NotificationTypeStatsView AS
SELECT 
    Type,
    COUNT(*) as Count,
    COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadCount,
    COUNT(CASE WHEN Status = 7 THEN 1 END) as FailedCount,
    COUNT(CASE WHEN IsArchived = 1 THEN 1 END) as ArchivedCount,
    AVG(TIMESTAMPDIFF(MINUTE, CreatedAt, COALESCE(ReadAt, CreatedAt))) as AverageReadTimeMinutes,
    AVG(SendCount) as AverageSendCount,
    MAX(CreatedAt) as LastCreatedAt,
    MIN(CreatedAt) as FirstCreatedAt
FROM Notifications 
GROUP BY Type;

-- =============================================
-- 触发器创建
-- =============================================

-- 创建触发器：自动更新评论的最后修改时间
DELIMITER //
CREATE TRIGGER update_comments_updated_at 
AFTER UPDATE ON Comments
FOR EACH ROW
BEGIN
    IF NEW.Content != OLD.Content OR NEW.Status != OLD.Status THEN
        UPDATE Comments SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- 创建触发器：自动更新评论点赞数
DELIMITER //
CREATE TRIGGER update_comment_like_count_insert
AFTER INSERT ON CommentLikes
FOR EACH ROW
BEGIN
    UPDATE Comments SET LikeCount = LikeCount + 1 WHERE Id = NEW.CommentId;
END//
DELIMITER ;

DELIMITER //
CREATE TRIGGER update_comment_like_count_delete
AFTER DELETE ON CommentLikes
FOR EACH ROW
BEGIN
    UPDATE Comments SET LikeCount = LikeCount - 1 WHERE Id = OLD.CommentId;
END//
DELIMITER ;

-- 创建触发器：自动更新评论回复数
DELIMITER //
CREATE TRIGGER update_comment_reply_count_insert
AFTER INSERT ON Comments
FOR EACH ROW
BEGIN
    IF NEW.ParentId IS NOT NULL THEN
        UPDATE Comments SET ReplyCount = ReplyCount + 1 WHERE Id = NEW.ParentId;
    END IF;
END//
DELIMITER ;

DELIMITER //
CREATE TRIGGER update_comment_reply_count_delete
AFTER DELETE ON Comments
FOR EACH ROW
BEGIN
    IF OLD.ParentId IS NOT NULL THEN
        UPDATE Comments SET ReplyCount = ReplyCount - 1 WHERE Id = OLD.ParentId;
    END IF;
END//
DELIMITER ;

-- 创建触发器：自动更新消息的阅读时间和状态
DELIMITER //
CREATE TRIGGER update_message_read_status
AFTER UPDATE ON Messages
FOR EACH ROW
BEGIN
    IF NEW.IsRead = 1 AND OLD.IsRead = 0 AND NEW.ReadAt IS NULL THEN
        UPDATE Messages SET ReadAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- 创建触发器：自动更新消息的回复计数
DELIMITER //
CREATE TRIGGER update_message_reply_count
AFTER INSERT ON Messages
FOR EACH ROW
BEGIN
    IF NEW.ParentId IS NOT NULL THEN
        UPDATE Messages SET Status = 4 WHERE Id = NEW.ParentId; -- 设置状态为已回复
    END IF;
END//
DELIMITER ;

-- 创建触发器：自动更新会话的最后活动时间
DELIMITER //
CREATE TRIGGER update_conversation_activity
AFTER INSERT ON Messages
FOR EACH ROW
BEGIN
    IF NEW.ConversationId IS NOT NULL THEN
        UPDATE Conversations SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.ConversationId;
    END IF;
END//
DELIMITER ;

-- 创建触发器：自动更新通知的阅读时间和状态
DELIMITER //
CREATE TRIGGER update_notification_read_status
AFTER UPDATE ON Notifications
FOR EACH ROW
BEGIN
    IF NEW.IsRead = 1 AND OLD.IsRead = 0 AND NEW.ReadAt IS NULL THEN
        UPDATE Notifications SET ReadAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- 创建触发器：自动更新通知的确认时间
DELIMITER //
CREATE TRIGGER update_notification_confirmation
AFTER UPDATE ON Notifications
FOR EACH ROW
BEGIN
    IF NEW.Status = 6 AND OLD.Status != 6 AND NEW.ConfirmedAt IS NULL THEN
        UPDATE Notifications SET ConfirmedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- =============================================
-- 存储过程创建
-- =============================================

-- 创建存储过程：清理过期的通知
DELIMITER //
CREATE PROCEDURE CleanupExpiredNotifications()
BEGIN
    -- 将过期的通知标记为已过期
    UPDATE Notifications 
    SET Status = 8,  -- 已过期状态
        IsDeleted = 1,
        DeletedAt = CURRENT_TIMESTAMP
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND Status NOT IN (8, 9, 10);  -- 排除已过期、已取消、已归档的通知
    
    -- 清理已删除的通知（软删除超过30天）
    UPDATE Notifications 
    SET IsArchived = 1,
        ArchivedAt = CURRENT_TIMESTAMP
    WHERE IsDeleted = 1 
    AND DeletedAt < DATE_SUB(NOW(), INTERVAL 30 DAY);
    
    -- 返回影响的行数
    SELECT ROW_COUNT() as ExpiredNotificationsCount;
END//
DELIMITER ;

-- 创建存储过程：获取通知统计信息
DELIMITER //
CREATE PROCEDURE GetNotificationStats(IN pUserId CHAR(36))
BEGIN
    -- 基本统计信息
    SELECT 
        COUNT(*) as TotalCount,
        COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadCount,
        COUNT(CASE WHEN IsRead = 1 THEN 1 END) as ReadCount,
        COUNT(CASE WHEN Status = 7 THEN 1 END) as FailedCount,
        COUNT(CASE WHEN IsArchived = 1 THEN 1 END) as ArchivedCount,
        COUNT(CASE WHEN IsDeleted = 1 THEN 1 END) as DeletedCount,
        COUNT(CASE WHEN Priority >= 3 THEN 1 END) as HighPriorityCount,
        COUNT(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 7 DAY) THEN 1 END) as RecentCount,
        COUNT(CASE WHEN CreatedAt >= DATE_SUB(NOW(), INTERVAL 1 DAY) THEN 1 END) as TodayCount,
        MAX(CreatedAt) as LastNotificationAt
    FROM Notifications 
    WHERE UserId = pUserId;
    
    -- 按类型统计
    SELECT 
        Type,
        COUNT(*) as Count,
        COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadCount
    FROM Notifications 
    WHERE UserId = pUserId
    GROUP BY Type;
    
    -- 按优先级统计
    SELECT 
        Priority,
        COUNT(*) as Count,
        COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadCount
    FROM Notifications 
    WHERE UserId = pUserId
    GROUP BY Priority;
    
    -- 按状态统计
    SELECT 
        Status,
        COUNT(*) as Count
    FROM Notifications 
    WHERE UserId = pUserId
    GROUP BY Status;
END//
DELIMITER ;

-- 创建存储过程：清理过期的消息
DELIMITER //
CREATE PROCEDURE CleanupExpiredMessages()
BEGIN
    -- 将过期的消息标记为已过期
    UPDATE Messages 
    SET Status = 8  -- 已过期状态
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND Status NOT IN (6, 8);  -- 排除已删除和已过期的消息
    
    -- 清理过期的草稿
    UPDATE MessageDrafts 
    SET Status = 3  -- 已过期状态
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND Status = 0;  -- 只处理草稿状态的
    
    -- 返回影响的行数
    SELECT ROW_COUNT() as ExpiredMessagesCount;
END//
DELIMITER ;

-- 创建存储过程：获取消息统计信息
DELIMITER //
CREATE PROCEDURE GetMessageStats(IN pUserId CHAR(36))
BEGIN
    SELECT 
        COUNT(*) as TotalMessages,
        COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadMessages,
        COUNT(CASE WHEN SenderId = pUserId THEN 1 END) as SentMessages,
        COUNT(CASE WHEN ReceiverId = pUserId THEN 1 END) as ReceivedMessages,
        COUNT(CASE WHEN Status = 6 THEN 1 END) as DeletedMessages,
        COUNT(DISTINCT ConversationId) as ConversationCount,
        MAX(CreatedAt) as LastMessageAt
    FROM Messages 
    WHERE (SenderId = pUserId OR ReceiverId = pUserId)
    AND Status NOT IN (6, 8);  -- 排除已删除和已过期的消息
    
    -- 按消息类型统计
    SELECT 
        MessageType,
        COUNT(*) as Count
    FROM Messages 
    WHERE (SenderId = pUserId OR ReceiverId = pUserId)
    AND Status NOT IN (6, 8)
    GROUP BY MessageType;
    
    -- 按优先级统计
    SELECT 
        Priority,
        COUNT(*) as Count
    FROM Messages 
    WHERE (SenderId = pUserId OR ReceiverId = pUserId)
    AND Status NOT IN (6, 8)
    GROUP BY Priority;
END//
DELIMITER ;

-- =============================================
-- 验证脚本执行结果
-- =============================================

-- 显示创建结果
SELECT 'Comments and Notifications database schema migration completed successfully!' as Status;

-- 显示表结构信息
SELECT 
    TABLE_NAME as TableName,
    COUNT(*) as ColumnCount
FROM information_schema.columns 
WHERE TABLE_NAME IN (
    'Comments', 'CommentLikes', 'CommentReports',
    'Messages', 'MessageAttachments', 'MessageDrafts', 'MessageDraftAttachments',
    'Conversations', 'ConversationParticipants',
    'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory',
    'SystemSettings', 'SettingsHistory'
)
GROUP BY TABLE_NAME
ORDER BY TABLE_NAME;

-- 显示视图统计
SELECT 
    TABLE_NAME as ViewName,
    TABLE_COMMENT as Description
FROM information_schema.views 
WHERE TABLE_NAME IN ('MessageStatsView', 'ConversationStatsView', 'NotificationStatsView', 'NotificationTypeStatsView');

-- 显示触发器统计
SELECT 
    TRIGGER_NAME as TriggerName,
    EVENT_MANIPULATION as EventType,
    EVENT_OBJECT_TABLE as TableName,
    ACTION_STATEMENT as Action
FROM information_schema.triggers 
WHERE TRIGGER_NAME LIKE 'update_%'
ORDER BY EVENT_OBJECT_TABLE, TRIGGER_NAME;

-- 显示存储过程统计
SELECT 
    ROUTINE_NAME as ProcedureName,
    ROUTINE_TYPE as Type
FROM information_schema.routines 
WHERE ROUTINE_NAME IN ('CleanupExpiredNotifications', 'GetNotificationStats', 'CleanupExpiredMessages', 'GetMessageStats')
ORDER BY ROUTINE_NAME;

-- 完成提示
SELECT CONCAT(
    '数据库迁移脚本 v1.0.0 执行完成！\n',
    '创建了 ', COUNT(DISTINCT TABLE_NAME), ' 个数据表\n',
    '创建了 ', COUNT(DISTINCT TABLE_NAME), ' 个视图\n',
    '创建了 ', COUNT(DISTINCT TRIGGER_NAME), ' 个触发器\n',
    '创建了 ', COUNT(DISTINCT ROUTINE_NAME), ' 个存储过程\n'
) as MigrationSummary
FROM information_schema.tables 
WHERE TABLE_TYPE = 'BASE TABLE'
AND TABLE_NAME IN (
    'Comments', 'CommentLikes', 'CommentReports',
    'Messages', 'MessageAttachments', 'MessageDrafts', 'MessageDraftAttachments',
    'Conversations', 'ConversationParticipants',
    'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory',
    'SystemSettings', 'SettingsHistory'
);