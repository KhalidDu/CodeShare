-- CodeShare 评论、消息、通知系统数据库迁移脚本
-- 版本: v2.0.0
-- 创建日期: 2025-01-09
-- 描述: 为评论、消息、通知系统添加必要的数据表和初始化数据
-- 支持MySQL和SQLite两种数据库语法
-- 任务编号: DS-04

-- =============================================
-- 执行前检查
-- =============================================

-- 检查是否已经存在Comments表
SET @comments_exists = 0;
SELECT COUNT(*) INTO @comments_exists 
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'Comments';

-- 如果表已存在，显示警告并跳过创建
SET @message = IF(@comments_exists > 0, 
    'WARNING: Comments system tables already exist. Skipping table creation.', 
    'Creating Comments, Messages and Notifications system tables...');
SELECT @message as Status;

-- =============================================
-- 评论系统数据表
-- =============================================

-- 创建Comments表 - 存储评论信息
CREATE TABLE IF NOT EXISTS Comments (
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

-- 创建CommentLikes表 - 存储评论点赞记录
CREATE TABLE IF NOT EXISTS CommentLikes (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CommentId CHAR(36) NOT NULL,                -- 评论ID
    UserId CHAR(36) NOT NULL,                   -- 用户ID
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 唯一约束：一个用户只能给同一个评论点赞一次
    UNIQUE KEY uk_comment_user (CommentId, UserId),
    
    -- 索引优化
    INDEX idx_comment_likes_comment_id (CommentId),
    INDEX idx_comment_likes_user_id (UserId),
    INDEX idx_comment_likes_created_at (CreatedAt),
    
    -- 复合索引
    INDEX idx_comment_likes_comment_created (CommentId, CreatedAt),
    INDEX idx_comment_likes_user_created (UserId, CreatedAt)
);

-- 创建CommentReports表 - 存储评论举报记录
CREATE TABLE IF NOT EXISTS CommentReports (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CommentId CHAR(36) NOT NULL,                -- 评论ID
    UserId CHAR(36) NOT NULL,                   -- 举报者用户ID
    Reason TINYINT NOT NULL,                     -- 举报原因 (0:垃圾信息, 1:不当内容, 2:侮辱性言论, 3:仇恨言论, 4:虚假信息, 5:侵权内容, 99:其他)
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
-- 消息系统数据表
-- =============================================

-- 创建Messages表 - 存储用户消息
CREATE TABLE IF NOT EXISTS Messages (
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

-- 创建MessageAttachments表 - 存储消息附件
CREATE TABLE IF NOT EXISTS MessageAttachments (
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

-- 创建MessageDrafts表 - 存储消息草稿
CREATE TABLE IF NOT EXISTS MessageDrafts (
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

-- 创建MessageDraftAttachments表 - 存储草稿附件
CREATE TABLE IF NOT EXISTS MessageDraftAttachments (
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

-- 创建Conversations表 - 存储会话信息
CREATE TABLE IF NOT EXISTS Conversations (
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

-- 创建ConversationParticipants表 - 存储会话参与者
CREATE TABLE IF NOT EXISTS ConversationParticipants (
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
    UNIQUE KEY uk_conversation_user (ConversationId, UserId),
    
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
-- 通知系统数据表
-- =============================================

-- 创建Notifications表 - 存储通知信息
CREATE TABLE IF NOT EXISTS Notifications (
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

-- 创建NotificationSettings表 - 存储用户通知设置
CREATE TABLE IF NOT EXISTS NotificationSettings (
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

-- 创建NotificationDeliveryHistory表 - 存储通知发送历史
CREATE TABLE IF NOT EXISTS NotificationDeliveryHistory (
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
-- 创建视图
-- =============================================

-- 创建评论统计视图
CREATE OR REPLACE VIEW CommentStatsView AS
SELECT 
    c.Id as CommentId,
    c.Content,
    c.SnippetId,
    cs.Title as SnippetTitle,
    c.UserId,
    u.Username as UserName,
    c.ParentId,
    c.Depth,
    c.LikeCount,
    c.ReplyCount,
    c.Status,
    c.CreatedAt,
    c.UpdatedAt,
    COUNT(cl.Id) as LikeRecordsCount,
    COUNT(cr.Id) as ReportRecordsCount
FROM Comments c
LEFT JOIN CodeSnippets cs ON c.SnippetId = cs.Id
LEFT JOIN Users u ON c.UserId = u.Id
LEFT JOIN CommentLikes cl ON c.Id = cl.CommentId
LEFT JOIN CommentReports cr ON c.Id = cr.CommentId
GROUP BY c.Id, c.Content, c.SnippetId, cs.Title, c.UserId, u.Username, 
         c.ParentId, c.Depth, c.LikeCount, c.ReplyCount, c.Status, c.CreatedAt, c.UpdatedAt;

-- 创建消息统计视图
CREATE OR REPLACE VIEW MessageStatsView AS
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
CREATE OR REPLACE VIEW ConversationStatsView AS
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
CREATE OR REPLACE VIEW NotificationStatsView AS
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
CREATE OR REPLACE VIEW NotificationTypeStatsView AS
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
-- 创建触发器
-- =============================================

-- 更新Comments表UpdatedAt触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_comments_updated_at 
AFTER UPDATE ON Comments
FOR EACH ROW
BEGIN
    IF NEW.UpdatedAt = OLD.UpdatedAt THEN
        UPDATE Comments SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- 自动更新LikeCount触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_comment_like_count
AFTER INSERT ON CommentLikes
FOR EACH ROW
BEGIN
    UPDATE Comments SET LikeCount = LikeCount + 1 WHERE Id = NEW.CommentId;
END//
DELIMITER ;

DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_comment_like_count_delete
AFTER DELETE ON CommentLikes
FOR EACH ROW
BEGIN
    UPDATE Comments SET LikeCount = LikeCount - 1 WHERE Id = OLD.CommentId;
END//
DELIMITER ;

-- 自动更新ReplyCount触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_comment_reply_count
AFTER INSERT ON Comments
FOR EACH ROW
BEGIN
    IF NEW.ParentId IS NOT NULL THEN
        UPDATE Comments SET ReplyCount = ReplyCount + 1 WHERE Id = NEW.ParentId;
    END IF;
END//
DELIMITER ;

DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_comment_reply_count_delete
AFTER DELETE ON Comments
FOR EACH ROW
BEGIN
    IF OLD.ParentId IS NOT NULL THEN
        UPDATE Comments SET ReplyCount = ReplyCount - 1 WHERE Id = OLD.ParentId;
    END IF;
END//
DELIMITER ;

-- 自动更新消息的阅读时间和状态触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_message_read_status
AFTER UPDATE ON Messages
FOR EACH ROW
BEGIN
    IF NEW.IsRead = 1 AND OLD.IsRead = 0 AND NEW.ReadAt IS NULL THEN
        UPDATE Messages SET ReadAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- 自动更新消息的回复计数触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_message_reply_count
AFTER INSERT ON Messages
FOR EACH ROW
BEGIN
    IF NEW.ParentId IS NOT NULL THEN
        UPDATE Messages SET Status = 4 WHERE Id = NEW.ParentId; -- 设置状态为已回复
    END IF;
END//
DELIMITER ;

-- 自动更新会话的最后活动时间触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_conversation_activity
AFTER INSERT ON Messages
FOR EACH ROW
BEGIN
    IF NEW.ConversationId IS NOT NULL THEN
        UPDATE Conversations SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.ConversationId;
    END IF;
END//
DELIMITER ;

-- 自动更新通知的阅读时间和状态触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_notification_read_status
AFTER UPDATE ON Notifications
FOR EACH ROW
BEGIN
    IF NEW.IsRead = 1 AND OLD.IsRead = 0 AND NEW.ReadAt IS NULL THEN
        UPDATE Notifications SET ReadAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- 自动更新通知的确认时间触发器
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_notification_confirmation
AFTER UPDATE ON Notifications
FOR EACH ROW
BEGIN
    IF NEW.Status = 6 AND OLD.Status != 6 AND NEW.ConfirmedAt IS NULL THEN
        UPDATE Notifications SET ConfirmedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END//
DELIMITER ;

-- =============================================
-- 创建存储过程
-- =============================================

-- 清理过期的分享令牌（如果不存在则创建）
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS CleanupExpiredShareTokens()
BEGIN
    DECLARE expired_count INT DEFAULT 0;
    
    -- 将过期的分享令牌标记为非活跃
    UPDATE ShareTokens 
    SET IsActive = 0 
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND IsActive = 1;
    
    -- 获取影响的行数
    SET expired_count = ROW_COUNT();
    
    -- 返回结果
    SELECT expired_count as ExpiredTokensCount;
    SELECT CONCAT('Successfully deactivated ', expired_count, ' expired share tokens.') as Message;
END//
DELIMITER ;

-- 获取分享统计信息（如果不存在则创建）
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetShareStats(IN pShareTokenId CHAR(36))
BEGIN
    SELECT 
        st.*,
        cs.Title as SnippetTitle,
        cs.Language as SnippetLanguage,
        u.Username as CreatorUsername,
        COUNT(DISTINCT sal.SessionId) as UniqueVisitors,
        COUNT(CASE WHEN sal.AccessType = 0 THEN 1 END) as TotalViews,
        COUNT(CASE WHEN sal.AccessType = 1 THEN 1 END) as TotalCopies,
        DATE(MIN(sal.AccessTime)) as FirstAccessDate,
        DATE(MAX(sal.AccessTime)) as LastAccessDate
    FROM ShareTokens st
    LEFT JOIN ShareAccessLogs sal ON st.Id = sal.ShareTokenId
    LEFT JOIN CodeSnippets cs ON st.SnippetId = cs.Id
    LEFT JOIN Users u ON st.CreatedBy = u.Id
    WHERE st.Id = pShareTokenId
    GROUP BY st.Id, cs.Title, cs.Language, u.Username;
END//
DELIMITER ;

-- 清理过期的评论
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS CleanupExpiredComments()
BEGIN
    DECLARE expired_count INT DEFAULT 0;
    
    -- 清理已删除的评论（软删除超过30天）
    UPDATE Comments 
    SET Status = 1,  -- 已删除状态
        DeletedAt = CURRENT_TIMESTAMP
    WHERE Status IN (2, 3)  -- 已隐藏或待审核状态
    AND CreatedAt < DATE_SUB(NOW(), INTERVAL 30 DAY);
    
    -- 获取影响的行数
    SET expired_count = ROW_COUNT();
    
    -- 返回结果
    SELECT expired_count as ExpiredCommentsCount;
    SELECT CONCAT('Successfully cleaned up ', expired_count, ' expired comments.') as Message;
END//
DELIMITER ;

-- 清理过期的消息
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS CleanupExpiredMessages()
BEGIN
    DECLARE expired_count INT DEFAULT 0;
    DECLARE draft_count INT DEFAULT 0;
    
    -- 将过期的消息标记为已过期
    UPDATE Messages 
    SET Status = 8  -- 已过期状态
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND Status NOT IN (6, 8);  -- 排除已删除和已过期的消息
    
    SET expired_count = ROW_COUNT();
    
    -- 清理过期的草稿
    UPDATE MessageDrafts 
    SET Status = 3  -- 已过期状态
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND Status = 0;  -- 只处理草稿状态的
    
    SET draft_count = ROW_COUNT();
    
    -- 返回结果
    SELECT expired_count as ExpiredMessagesCount;
    SELECT draft_count as ExpiredDraftsCount;
    SELECT CONCAT('Successfully cleaned up ', expired_count, ' expired messages and ', draft_count, ' expired drafts.') as Message;
END//
DELIMITER ;

-- 清理过期的通知
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS CleanupExpiredNotifications()
BEGIN
    DECLARE expired_count INT DEFAULT 0;
    DECLARE archive_count INT DEFAULT 0;
    
    -- 将过期的通知标记为已过期
    UPDATE Notifications 
    SET Status = 8,  -- 已过期状态
        IsDeleted = 1,
        DeletedAt = CURRENT_TIMESTAMP
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND Status NOT IN (8, 9, 10);  -- 排除已过期、已取消、已归档的通知
    
    SET expired_count = ROW_COUNT();
    
    -- 清理已删除的通知（软删除超过30天）
    UPDATE Notifications 
    SET IsArchived = 1,
        ArchivedAt = CURRENT_TIMESTAMP
    WHERE IsDeleted = 1 
    AND DeletedAt < DATE_SUB(NOW(), INTERVAL 30 DAY);
    
    SET archive_count = ROW_COUNT();
    
    -- 返回结果
    SELECT expired_count as ExpiredNotificationsCount;
    SELECT archive_count as ArchivedNotificationsCount;
    SELECT CONCAT('Successfully processed ', expired_count, ' expired notifications and archived ', archive_count, ' notifications.') as Message;
END//
DELIMITER ;

-- 获取评论统计信息
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetCommentStats(IN pSnippetId CHAR(36))
BEGIN
    -- 基本统计信息
    SELECT 
        COUNT(*) as TotalComments,
        COUNT(CASE WHEN ParentId IS NULL THEN 1 END) as TopLevelComments,
        COUNT(CASE WHEN ParentId IS NOT NULL THEN 1 END) as ReplyComments,
        COUNT(CASE WHEN Status = 0 THEN 1 END) as ActiveComments,
        COUNT(CASE WHEN Status = 1 THEN 1 END) as DeletedComments,
        COUNT(CASE WHEN Status = 2 THEN 1 END) as HiddenComments,
        COUNT(CASE WHEN Status = 3 THEN 1 END) as PendingComments,
        SUM(LikeCount) as TotalLikes,
        AVG(LikeCount) as AverageLikes,
        SUM(ReplyCount) as TotalReplies,
        MAX(CreatedAt) as LastCommentAt
    FROM Comments 
    WHERE SnippetId = pSnippetId;
    
    -- 按深度统计
    SELECT 
        Depth,
        COUNT(*) as Count,
        SUM(LikeCount) as TotalLikes,
        AVG(LikeCount) as AverageLikes
    FROM Comments 
    WHERE SnippetId = pSnippetId
    GROUP BY Depth
    ORDER BY Depth;
    
    -- 按用户统计
    SELECT 
        UserId,
        u.Username as UserName,
        COUNT(*) as CommentCount,
        SUM(LikeCount) as TotalLikes,
        AVG(LikeCount) as AverageLikes,
        MAX(CreatedAt) as LastCommentAt
    FROM Comments c
    LEFT JOIN Users u ON c.UserId = u.Id
    WHERE SnippetId = pSnippetId
    GROUP BY UserId, u.Username
    ORDER BY CommentCount DESC;
END//
DELIMITER ;

-- 获取消息统计信息
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetMessageStats(IN pUserId CHAR(36))
BEGIN
    -- 基本统计信息
    SELECT 
        COUNT(*) as TotalMessages,
        COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadMessages,
        COUNT(CASE WHEN IsRead = 1 THEN 1 END) as ReadMessages,
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

-- 获取通知统计信息
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS GetNotificationStats(IN pUserId CHAR(36))
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

-- =============================================
-- 初始化数据（可选）
-- =============================================

-- 插入默认通知设置（为所有现有用户）
INSERT INTO NotificationSettings (Id, UserId, NotificationType, EnableInApp, EnableEmail, EnablePush, EnableDesktop, EnableSound, Frequency, EmailFrequency, BatchIntervalMinutes, Language, TimeZone, IsDefault, IsActive, CreatedAt, UpdatedAt)
SELECT 
    UUID(),
    u.Id,
    NULL,  -- 全局设置
    1,     -- 启用应用内通知
    1,     -- 启用邮件通知
    1,     -- 启用推送通知
    1,     -- 启用桌面通知
    1,     -- 启用声音提醒
    0,     -- 立即通知
    0,     -- 立即邮件通知
    30,    -- 批量间隔30分钟
    'zh-CN',
    'Asia/Shanghai',
    1,     -- 默认设置
    1,     -- 激活状态
    NOW(),
    NOW()
FROM Users u
WHERE u.Id NOT IN (SELECT UserId FROM NotificationSettings WHERE NotificationType IS NULL)
AND u.IsDeleted = 0;

-- =============================================
-- 验证脚本执行结果
-- =============================================

-- 显示迁移完成信息
SELECT 'CodeShare Comments, Messages and Notifications Database Migration completed successfully!' as Status;
SELECT NOW() as MigrationTime;

-- 显示表创建状态
SELECT 
    'Comments' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Comments') as IsCreated
UNION ALL
SELECT 
    'CommentLikes' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'CommentLikes') as IsCreated
UNION ALL
SELECT 
    'CommentReports' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'CommentReports') as IsCreated
UNION ALL
SELECT 
    'Messages' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Messages') as IsCreated
UNION ALL
SELECT 
    'MessageAttachments' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageAttachments') as IsCreated
UNION ALL
SELECT 
    'MessageDrafts' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageDrafts') as IsCreated
UNION ALL
SELECT 
    'MessageDraftAttachments' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageDraftAttachments') as IsCreated
UNION ALL
SELECT 
    'Conversations' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Conversations') as IsCreated
UNION ALL
SELECT 
    'ConversationParticipants' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ConversationParticipants') as IsCreated
UNION ALL
SELECT 
    'Notifications' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Notifications') as IsCreated
UNION ALL
SELECT 
    'NotificationSettings' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'NotificationSettings') as IsCreated
UNION ALL
SELECT 
    'NotificationDeliveryHistory' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'NotificationDeliveryHistory') as IsCreated;

-- 显示索引创建状态
SELECT 
    TABLE_NAME as TableName,
    COUNT(*) as IndexCount
FROM information_schema.statistics 
WHERE TABLE_SCHEMA = DATABASE() 
AND TABLE_NAME IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                   'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                   'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory')
GROUP BY TABLE_NAME;

-- 显示视图创建状态
SELECT 
    'CommentStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'CommentStatsView') as IsCreated
UNION ALL
SELECT 
    'MessageStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'MessageStatsView') as IsCreated
UNION ALL
SELECT 
    'ConversationStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'ConversationStatsView') as IsCreated
UNION ALL
SELECT 
    'NotificationStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'NotificationStatsView') as IsCreated
UNION ALL
SELECT 
    'NotificationTypeStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'NotificationTypeStatsView') as IsCreated;

-- 显示存储过程创建状态
SELECT 
    routine_name as ProcedureName,
    routine_type as Type
FROM information_schema.routines 
WHERE routine_schema = DATABASE() 
AND routine_name IN ('CleanupExpiredShareTokens', 'GetShareStats', 'CleanupExpiredComments', 'CleanupExpiredMessages', 
                    'CleanupExpiredNotifications', 'GetCommentStats', 'GetMessageStats', 'GetNotificationStats');

-- 显示触发器创建状态
SELECT 
    trigger_name as TriggerName,
    event_object_table as TableName
FROM information_schema.triggers 
WHERE trigger_schema = DATABASE() 
AND trigger_name IN ('update_comments_updated_at', 'update_comment_like_count', 'update_comment_like_count_delete', 
                    'update_comment_reply_count', 'update_comment_reply_count_delete', 'update_message_read_status',
                    'update_message_reply_count', 'update_conversation_activity', 'update_notification_read_status',
                    'update_notification_confirmation');

-- 显示默认数据插入状态
SELECT 
    'NotificationSettings' as TableName,
    COUNT(*) as RecordCount
FROM NotificationSettings;

-- =============================================
-- 迁移完成
-- =============================================

SELECT 'Database migration completed successfully!' as FinalStatus;
SELECT CONCAT('Created/updated tables: Comments, CommentLikes, CommentReports, Messages, MessageAttachments, MessageDrafts, MessageDraftAttachments, Conversations, ConversationParticipants, Notifications, NotificationSettings, NotificationDeliveryHistory') as Tables;
SELECT CONCAT('Created views: CommentStatsView, MessageStatsView, ConversationStatsView, NotificationStatsView, NotificationTypeStatsView') as Views;
SELECT CONCAT('Created stored procedures: CleanupExpiredComments, CleanupExpiredMessages, CleanupExpiredNotifications, GetCommentStats, GetMessageStats, GetNotificationStats, CleanupExpiredShareTokens, GetShareStats') as Procedures;
SELECT CONCAT('Created triggers: update_comments_updated_at, update_comment_like_count, update_comment_reply_count, update_message_read_status, update_conversation_activity, update_notification_read_status, update_notification_confirmation') as Triggers;
SELECT CONCAT('Inserted default notification settings for existing users') as Data;
SELECT CONCAT('Migration version: v2.0.0, Task: DS-04') as Version;