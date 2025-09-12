-- CodeShare 评论、消息、通知系统数据库迁移脚本
-- 版本: v2.0.0
-- 创建日期: 2025-01-09
-- 描述: 为评论、消息、通知系统添加必要的数据表和初始化数据
-- 支持SQLite语法（用于开发环境）
-- 任务编号: DS-04

-- =============================================
-- 执行前检查
-- =============================================

-- 检查是否已经存在Comments表
CREATE TABLE IF NOT EXISTS MigrationCheck (
    id INTEGER PRIMARY KEY,
    table_name TEXT NOT NULL,
    check_time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- =============================================
-- 评论系统数据表
-- =============================================

-- 创建Comments表 - 存储评论信息
CREATE TABLE IF NOT EXISTS Comments (
    Id TEXT PRIMARY KEY,                         -- UUID
    Content TEXT NOT NULL,                       -- 评论内容
    SnippetId TEXT NOT NULL,                     -- 关联的代码片段ID
    UserId TEXT NOT NULL,                        -- 评论者用户ID
    ParentId TEXT,                               -- 父评论ID（用于回复）
    ParentPath TEXT,                             -- 父评论路径（用于层级查询）
    Depth INTEGER NOT NULL DEFAULT 0,            -- 评论深度
    LikeCount INTEGER NOT NULL DEFAULT 0,        -- 点赞数量
    ReplyCount INTEGER NOT NULL DEFAULT 0,       -- 回复数量
    Status INTEGER NOT NULL DEFAULT 0,           -- 评论状态 (0:正常, 1:已删除, 2:已隐藏, 3:待审核)
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    DeletedAt TEXT,                              -- 删除时间（软删除）
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentId) REFERENCES Comments(Id) ON DELETE CASCADE
);

-- 创建CommentLikes表 - 存储评论点赞记录
CREATE TABLE IF NOT EXISTS CommentLikes (
    Id TEXT PRIMARY KEY,                         -- UUID
    CommentId TEXT NOT NULL,                     -- 评论ID
    UserId TEXT NOT NULL,                        -- 用户ID
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    UNIQUE (CommentId, UserId)  -- 一个用户只能给同一个评论点赞一次
);

-- 创建CommentReports表 - 存储评论举报记录
CREATE TABLE IF NOT EXISTS CommentReports (
    Id TEXT PRIMARY KEY,                         -- UUID
    CommentId TEXT NOT NULL,                     -- 评论ID
    UserId TEXT NOT NULL,                        -- 举报者用户ID
    Reason INTEGER NOT NULL,                     -- 举报原因 (0:垃圾信息, 1:不当内容, 2:侮辱性言论, 3:仇恨言论, 4:虚假信息, 5:侵权内容, 99:其他)
    Description TEXT,                            -- 举报描述
    Status INTEGER NOT NULL DEFAULT 0,           -- 举报状态 (0:待处理, 1:已处理, 2:已驳回, 3:调查中)
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    HandledAt TEXT,                              -- 处理时间
    HandledBy TEXT,                              -- 处理者用户ID
    Resolution TEXT,                             -- 处理结果说明
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (HandledBy) REFERENCES Users(Id) ON DELETE SET NULL
);

-- =============================================
-- 消息系统数据表
-- =============================================

-- 创建Messages表 - 存储用户消息
CREATE TABLE IF NOT EXISTS Messages (
    Id TEXT PRIMARY KEY,                         -- UUID
    SenderId TEXT NOT NULL,                      -- 发送者用户ID
    ReceiverId TEXT NOT NULL,                    -- 接收者用户ID
    Subject TEXT,                                -- 消息主题
    Content TEXT NOT NULL,                       -- 消息内容
    MessageType INTEGER NOT NULL DEFAULT 0,       -- 消息类型 (0:用户消息, 1:系统消息, 2:通知消息, 3:广播消息, 4:自动回复)
    Status INTEGER NOT NULL DEFAULT 1,           -- 消息状态 (0:草稿, 1:已发送, 2:已送达, 3:已读, 4:已回复, 5:已转发, 6:已删除, 7:发送失败, 8:已过期)
    Priority INTEGER NOT NULL DEFAULT 1,          -- 优先级 (0:低, 1:普通, 2:高, 3:紧急)
    ParentId TEXT,                               -- 父消息ID（用于回复）
    ConversationId TEXT,                         -- 会话ID（用于消息分组）
    IsRead INTEGER NOT NULL DEFAULT 0,           -- 是否已读
    ReadAt TEXT,                                 -- 阅读时间
    Tag TEXT,                                    -- 消息标签
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    DeletedAt TEXT,                              -- 删除时间（软删除）
    SenderDeletedAt TEXT,                        -- 发送者删除时间
    ReceiverDeletedAt TEXT,                      -- 接收者删除时间
    ExpiresAt TEXT,                              -- 过期时间
    SenderIp TEXT,                               -- 发送者IP地址
    SenderUserAgent TEXT,                        -- 发送者User-Agent
    
    FOREIGN KEY (SenderId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentId) REFERENCES Messages(Id) ON DELETE CASCADE
);

-- 创建MessageAttachments表 - 存储消息附件
CREATE TABLE IF NOT EXISTS MessageAttachments (
    Id TEXT PRIMARY KEY,                         -- UUID
    MessageId TEXT NOT NULL,                     -- 关联的消息ID
    FileName TEXT NOT NULL,                       -- 附件文件名
    OriginalFileName TEXT NOT NULL,              -- 附件原始文件名
    FileSize INTEGER NOT NULL,                    -- 文件大小（字节）
    ContentType TEXT,                             -- 文件类型
    FileExtension TEXT,                           -- 文件扩展名
    FilePath TEXT NOT NULL,                       -- 文件存储路径
    FileUrl TEXT,                                -- 文件访问URL
    AttachmentType INTEGER NOT NULL,              -- 附件类型 (0:图片, 1:文档, 2:视频, 3:音频, 4:压缩文件, 5:代码文件, 99:其他)
    AttachmentStatus INTEGER NOT NULL DEFAULT 0, -- 附件状态 (0:活跃, 1:上传中, 2:上传失败, 3:已删除, 4:已过期, 5:病毒检测中, 6:检测到病毒)
    FileHash TEXT,                               -- 文件哈希值
    ThumbnailPath TEXT,                           -- 缩略图路径
    UploadProgress INTEGER NOT NULL DEFAULT 0,   -- 上传进度（0-100）
    DownloadCount INTEGER NOT NULL DEFAULT 0,     -- 下载次数
    UploadedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 上传时间
    LastDownloadedAt TEXT,                       -- 最后下载时间
    DeletedAt TEXT,                              -- 删除时间
    
    FOREIGN KEY (MessageId) REFERENCES Messages(Id) ON DELETE CASCADE
);

-- 创建MessageDrafts表 - 存储消息草稿
CREATE TABLE IF NOT EXISTS MessageDrafts (
    Id TEXT PRIMARY KEY,                         -- UUID
    AuthorId TEXT NOT NULL,                      -- 草稿作者用户ID
    ReceiverId TEXT,                             -- 收件人用户ID
    Subject TEXT,                                -- 消息主题
    Content TEXT NOT NULL,                       -- 消息内容
    MessageType INTEGER NOT NULL DEFAULT 0,       -- 消息类型
    Priority INTEGER NOT NULL DEFAULT 1,          -- 优先级
    ParentId TEXT,                               -- 父消息ID（用于回复草稿）
    ConversationId TEXT,                         -- 会话ID
    Tag TEXT,                                    -- 消息标签
    Status INTEGER NOT NULL DEFAULT 0,           -- 草稿状态 (0:草稿, 1:已发送, 2:已取消, 3:已过期)
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    LastAutoSavedAt TEXT,                        -- 最后自动保存时间
    ScheduledToSendAt TEXT,                      -- 计划发送时间
    ExpiresAt TEXT,                              -- 过期时间
    IsScheduled INTEGER NOT NULL DEFAULT 0,      -- 是否定时发送
    AutoSaveInterval INTEGER NOT NULL DEFAULT 120, -- 自动保存间隔（秒）
    Notes TEXT,                                  -- 草稿备注
    
    FOREIGN KEY (AuthorId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ReceiverId) REFERENCES Users(Id) ON DELETE SET NULL,
    FOREIGN KEY (ParentId) REFERENCES Messages(Id) ON DELETE SET NULL
);

-- 创建MessageDraftAttachments表 - 存储草稿附件
CREATE TABLE IF NOT EXISTS MessageDraftAttachments (
    Id TEXT PRIMARY KEY,                         -- UUID
    DraftId TEXT NOT NULL,                       -- 关联的草稿ID
    FileName TEXT NOT NULL,                       -- 附件文件名
    OriginalFileName TEXT NOT NULL,              -- 附件原始文件名
    FileSize INTEGER NOT NULL,                    -- 文件大小（字节）
    ContentType TEXT,                             -- 文件类型
    FileExtension TEXT,                           -- 文件扩展名
    FilePath TEXT NOT NULL,                       -- 文件存储路径
    FileUrl TEXT,                                -- 文件访问URL
    AttachmentType INTEGER NOT NULL,              -- 附件类型
    AttachmentStatus INTEGER NOT NULL DEFAULT 0, -- 附件状态
    UploadProgress INTEGER NOT NULL DEFAULT 0,   -- 上传进度（0-100）
    UploadedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 上传时间
    DeletedAt TEXT,                              -- 删除时间
    
    FOREIGN KEY (DraftId) REFERENCES MessageDrafts(Id) ON DELETE CASCADE
);

-- 创建Conversations表 - 存储会话信息
CREATE TABLE IF NOT EXISTS Conversations (
    Id TEXT PRIMARY KEY,                         -- UUID
    Subject TEXT NOT NULL,                       -- 会话主题
    CreatedBy TEXT NOT NULL,                     -- 创建者用户ID
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    IsArchived INTEGER NOT NULL DEFAULT 0,       -- 是否已归档
    IsPinned INTEGER NOT NULL DEFAULT 0,          -- 是否已置顶
    IsMuted INTEGER NOT NULL DEFAULT 0,           -- 是否已静音
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE
);

-- 创建ConversationParticipants表 - 存储会话参与者
CREATE TABLE IF NOT EXISTS ConversationParticipants (
    Id TEXT PRIMARY KEY,                         -- UUID
    ConversationId TEXT NOT NULL,                 -- 会话ID
    UserId TEXT NOT NULL,                        -- 用户ID
    Role TEXT NOT NULL DEFAULT 'Member',         -- 角色
    JoinedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 加入时间
    LastReadAt TEXT,                             -- 最后阅读时间
    UnreadCount INTEGER NOT NULL DEFAULT 0,      -- 未读消息数
    IsMuted INTEGER NOT NULL DEFAULT 0,           -- 是否静音
    
    FOREIGN KEY (ConversationId) REFERENCES Conversations(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    UNIQUE (ConversationId, UserId)  -- 一个用户在一个会话中只能有一条记录
);

-- =============================================
-- 通知系统数据表
-- =============================================

-- 创建Notifications表 - 存储通知信息
CREATE TABLE IF NOT EXISTS Notifications (
    Id TEXT PRIMARY KEY,                         -- UUID
    UserId TEXT NOT NULL,                        -- 接收通知的用户ID
    Type INTEGER NOT NULL,                       -- 通知类型 (0:评论, 1:回复, 2:消息, 3:系统, 4:点赞, 5:分享, 6:关注, 7:提及, 8:标签, 9:安全, 10:账户, 11:更新, 12:维护, 13:公告, 99:自定义)
    Title TEXT NOT NULL,                         -- 通知标题
    Content TEXT,                                -- 通知内容
    Message TEXT,                                 -- 通知消息
    Priority INTEGER NOT NULL DEFAULT 1,         -- 优先级 (0:低, 1:普通, 2:高, 3:紧急, 4:系统紧急)
    Status INTEGER NOT NULL DEFAULT 5,           -- 通知状态 (0:待发送, 1:发送中, 2:已发送, 3:已送达, 4:已读, 5:未读, 6:已确认, 7:发送失败, 8:已过期, 9:已取消, 10:已归档)
    RelatedEntityType INTEGER,                    -- 相关实体类型 (0:代码片段, 1:评论, 2:消息, 3:用户, 4:标签, 5:分享, 6:系统, 99:其他)
    RelatedEntityId TEXT,                        -- 相关实体ID
    TriggeredByUserId TEXT,                      -- 触发通知的用户ID
    Action INTEGER,                               -- 操作类型 (0:创建, 1:更新, 2:删除, 3:点赞, 4:分享, 5:关注, 6:提及, 7:评论, 8:回复, 9:报告, 10:审核, 99:其他)
    Channel INTEGER NOT NULL DEFAULT 0,          -- 通知渠道 (0:应用内, 1:邮件, 2:推送, 3:桌面, 4:短信, 5:Webhook, 6:Slack, 7:微信, 8:钉钉, 9:企业微信)
    IsRead INTEGER NOT NULL DEFAULT 0,           -- 是否已读
    ReadAt TEXT,                                 -- 阅读时间
    DeliveredAt TEXT,                            -- 送达时间
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    ExpiresAt TEXT,                              -- 过期时间
    ScheduledToSendAt TEXT,                      -- 计划发送时间
    SendCount INTEGER NOT NULL DEFAULT 0,         -- 发送次数
    LastSentAt TEXT,                             -- 最后发送时间
    ErrorMessage TEXT,                            -- 错误信息
    DataJson TEXT,                               -- 通知数据（JSON格式）
    Tag TEXT,                                    -- 通知标签
    Icon TEXT,                                    -- 通知图标
    Color TEXT,                                   -- 通知颜色
    RequiresConfirmation INTEGER NOT NULL DEFAULT 0, -- 是否需要确认
    ConfirmedAt TEXT,                            -- 确认时间
    IsArchived INTEGER NOT NULL DEFAULT 0,        -- 是否已归档
    ArchivedAt TEXT,                              -- 归档时间
    IsDeleted INTEGER NOT NULL DEFAULT 0,        -- 是否已删除
    DeletedAt TEXT,                               -- 删除时间
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (TriggeredByUserId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- 创建NotificationSettings表 - 存储用户通知设置
CREATE TABLE IF NOT EXISTS NotificationSettings (
    Id TEXT PRIMARY KEY,                         -- UUID
    UserId TEXT NOT NULL,                        -- 用户ID
    NotificationType INTEGER,                    -- 通知类型（NULL表示全局设置）
    EnableInApp INTEGER NOT NULL DEFAULT 1,      -- 启用应用内通知
    EnableEmail INTEGER NOT NULL DEFAULT 1,       -- 启用邮件通知
    EnablePush INTEGER NOT NULL DEFAULT 1,        -- 启用推送通知
    EnableDesktop INTEGER NOT NULL DEFAULT 1,     -- 启用桌面通知
    EnableSound INTEGER NOT NULL DEFAULT 1,       -- 启用声音提醒
    Frequency INTEGER NOT NULL DEFAULT 0,        -- 通知频率 (0:立即, 1:每小时, 2:每日, 3:每周, 4:每月, 5:从不)
    QuietHoursStart TEXT,                         -- 免打扰开始时间
    QuietHoursEnd TEXT,                           -- 免打扰结束时间
    EnableQuietHours INTEGER NOT NULL DEFAULT 0,  -- 是否启用免打扰
    EmailFrequency INTEGER NOT NULL DEFAULT 0,    -- 邮件通知频率 (0:立即, 1:每小时汇总, 2:每日汇总, 3:每周汇总, 4:从不)
    BatchIntervalMinutes INTEGER NOT NULL DEFAULT 30, -- 批量通知间隔（分钟）
    EnableBatching INTEGER NOT NULL DEFAULT 0,    -- 是否启用批量通知
    Language TEXT NOT NULL DEFAULT 'zh-CN',      -- 通知语言
    TimeZone TEXT NOT NULL DEFAULT 'Asia/Shanghai', -- 时区
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    LastUsedAt TEXT,                             -- 最后使用时间
    IsDefault INTEGER NOT NULL DEFAULT 0,        -- 是否为默认设置
    Name TEXT,                                    -- 设置名称
    Description TEXT,                             -- 设置描述
    IsActive INTEGER NOT NULL DEFAULT 1,         -- 是否激活
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);

-- 创建NotificationDeliveryHistory表 - 存储通知发送历史
CREATE TABLE IF NOT EXISTS NotificationDeliveryHistory (
    Id TEXT PRIMARY KEY,                         -- UUID
    NotificationId TEXT NOT NULL,                -- 关联的通知ID
    Channel INTEGER NOT NULL,                     -- 发送渠道
    Status INTEGER NOT NULL,                      -- 发送状态 (0:待发送, 1:发送中, 2:已发送, 3:已送达, 4:已读, 5:发送失败, 6:已跳过, 7:已取消, 8:已过期)
    SentAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 发送时间
    DeliveredAt TEXT,                            -- 送达时间
    ReadAt TEXT,                                 -- 阅读时间
    ErrorMessage TEXT,                            -- 错误信息
    RetryCount INTEGER NOT NULL DEFAULT 0,        -- 重试次数
    LastRetryAt TEXT,                             -- 最后重试时间
    RecipientAddress TEXT,                        -- 接收地址（邮箱地址、设备令牌等）
    Provider TEXT,                                -- 发送服务提供商
    Cost REAL,                                    -- 发送成本
    MetadataJson TEXT,                            -- 发送元数据（JSON格式）
    
    FOREIGN KEY (NotificationId) REFERENCES Notifications(Id) ON DELETE CASCADE
);

-- =============================================
-- 创建索引
-- =============================================

-- Comments表索引
CREATE INDEX IF NOT EXISTS idx_comments_snippet_id ON Comments(SnippetId);
CREATE INDEX IF NOT EXISTS idx_comments_user_id ON Comments(UserId);
CREATE INDEX IF NOT EXISTS idx_comments_parent_id ON Comments(ParentId);
CREATE INDEX IF NOT EXISTS idx_comments_parent_path ON Comments(ParentPath);
CREATE INDEX IF NOT EXISTS idx_comments_depth ON Comments(Depth);
CREATE INDEX IF NOT EXISTS idx_comments_status ON Comments(Status);
CREATE INDEX IF NOT EXISTS idx_comments_created_at ON Comments(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comments_updated_at ON Comments(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_comments_deleted_at ON Comments(DeletedAt);
CREATE INDEX IF NOT EXISTS idx_comments_like_count ON Comments(LikeCount);
CREATE INDEX IF NOT EXISTS idx_comments_reply_count ON Comments(ReplyCount);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_comments_snippet_status ON Comments(SnippetId, Status);
CREATE INDEX IF NOT EXISTS idx_comments_snippet_created ON Comments(SnippetId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comments_parent_path_created ON Comments(ParentPath, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comments_user_created ON Comments(UserId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comments_status_created ON Comments(Status, CreatedAt);

-- CommentLikes表索引
CREATE INDEX IF NOT EXISTS idx_comment_likes_comment_id ON CommentLikes(CommentId);
CREATE INDEX IF NOT EXISTS idx_comment_likes_user_id ON CommentLikes(UserId);
CREATE INDEX IF NOT EXISTS idx_comment_likes_created_at ON CommentLikes(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comment_likes_comment_created ON CommentLikes(CommentId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comment_likes_user_created ON CommentLikes(UserId, CreatedAt);

-- CommentReports表索引
CREATE INDEX IF NOT EXISTS idx_comment_reports_comment_id ON CommentReports(CommentId);
CREATE INDEX IF NOT EXISTS idx_comment_reports_user_id ON CommentReports(UserId);
CREATE INDEX IF NOT EXISTS idx_comment_reports_reason ON CommentReports(Reason);
CREATE INDEX IF NOT EXISTS idx_comment_reports_status ON CommentReports(Status);
CREATE INDEX IF NOT EXISTS idx_comment_reports_created_at ON CommentReports(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comment_reports_handled_at ON CommentReports(HandledAt);
CREATE INDEX IF NOT EXISTS idx_comment_reports_handled_by ON CommentReports(HandledBy);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_comment_reports_comment_status ON CommentReports(CommentId, Status);
CREATE INDEX IF NOT EXISTS idx_comment_reports_status_created ON CommentReports(Status, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comment_reports_reason_status ON CommentReports(Reason, Status);
CREATE INDEX IF NOT EXISTS idx_comment_reports_user_created ON CommentReports(UserId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_comment_reports_handled_created ON CommentReports(HandledBy, HandledAt);

-- Messages表索引
CREATE INDEX IF NOT EXISTS idx_messages_sender_id ON Messages(SenderId);
CREATE INDEX IF NOT EXISTS idx_messages_receiver_id ON Messages(ReceiverId);
CREATE INDEX IF NOT EXISTS idx_messages_message_type ON Messages(MessageType);
CREATE INDEX IF NOT EXISTS idx_messages_status ON Messages(Status);
CREATE INDEX IF NOT EXISTS idx_messages_priority ON Messages(Priority);
CREATE INDEX IF NOT EXISTS idx_messages_parent_id ON Messages(ParentId);
CREATE INDEX IF NOT EXISTS idx_messages_conversation_id ON Messages(ConversationId);
CREATE INDEX IF NOT EXISTS idx_messages_is_read ON Messages(IsRead);
CREATE INDEX IF NOT EXISTS idx_messages_read_at ON Messages(ReadAt);
CREATE INDEX IF NOT EXISTS idx_messages_tag ON Messages(Tag);
CREATE INDEX IF NOT EXISTS idx_messages_created_at ON Messages(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_updated_at ON Messages(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_deleted_at ON Messages(DeletedAt);
CREATE INDEX IF NOT EXISTS idx_messages_expires_at ON Messages(ExpiresAt);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_messages_sender_receiver ON Messages(SenderId, ReceiverId);
CREATE INDEX IF NOT EXISTS idx_messages_conversation_created ON Messages(ConversationId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_status_created ON Messages(Status, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_priority_created ON Messages(Priority, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_sender_created ON Messages(SenderId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_receiver_created ON Messages(ReceiverId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_messages_unread_messages ON Messages(ReceiverId, IsRead, Status);

-- MessageAttachments表索引
CREATE INDEX IF NOT EXISTS idx_message_attachments_message_id ON MessageAttachments(MessageId);
CREATE INDEX IF NOT EXISTS idx_message_attachments_file_name ON MessageAttachments(FileName);
CREATE INDEX IF NOT EXISTS idx_message_attachments_original_file_name ON MessageAttachments(OriginalFileName);
CREATE INDEX IF NOT EXISTS idx_message_attachments_content_type ON MessageAttachments(ContentType);
CREATE INDEX IF NOT EXISTS idx_message_attachments_attachment_type ON MessageAttachments(AttachmentType);
CREATE INDEX IF NOT EXISTS idx_message_attachments_attachment_status ON MessageAttachments(AttachmentStatus);
CREATE INDEX IF NOT EXISTS idx_message_attachments_file_hash ON MessageAttachments(FileHash);
CREATE INDEX IF NOT EXISTS idx_message_attachments_uploaded_at ON MessageAttachments(UploadedAt);
CREATE INDEX IF NOT EXISTS idx_message_attachments_download_count ON MessageAttachments(DownloadCount);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_message_attachments_message_type ON MessageAttachments(MessageId, AttachmentType);
CREATE INDEX IF NOT EXISTS idx_message_attachments_message_status ON MessageAttachments(MessageId, AttachmentStatus);
CREATE INDEX IF NOT EXISTS idx_message_attachments_type_status ON MessageAttachments(AttachmentType, AttachmentStatus);
CREATE INDEX IF NOT EXISTS idx_message_attachments_upload_created ON MessageAttachments(UploadedAt, AttachmentStatus);

-- MessageDrafts表索引
CREATE INDEX IF NOT EXISTS idx_message_drafts_author_id ON MessageDrafts(AuthorId);
CREATE INDEX IF NOT EXISTS idx_message_drafts_receiver_id ON MessageDrafts(ReceiverId);
CREATE INDEX IF NOT EXISTS idx_message_drafts_message_type ON MessageDrafts(MessageType);
CREATE INDEX IF NOT EXISTS idx_message_drafts_priority ON MessageDrafts(Priority);
CREATE INDEX IF NOT EXISTS idx_message_drafts_parent_id ON MessageDrafts(ParentId);
CREATE INDEX IF NOT EXISTS idx_message_drafts_conversation_id ON MessageDrafts(ConversationId);
CREATE INDEX IF NOT EXISTS idx_message_drafts_status ON MessageDrafts(Status);
CREATE INDEX IF NOT EXISTS idx_message_drafts_created_at ON MessageDrafts(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_updated_at ON MessageDrafts(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_scheduled_to_send_at ON MessageDrafts(ScheduledToSendAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_expires_at ON MessageDrafts(ExpiresAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_is_scheduled ON MessageDrafts(IsScheduled);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_message_drafts_author_status ON MessageDrafts(AuthorId, Status);
CREATE INDEX IF NOT EXISTS idx_message_drafts_author_created ON MessageDrafts(AuthorId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_status_created ON MessageDrafts(Status, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_scheduled_created ON MessageDrafts(ScheduledToSendAt, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_message_drafts_author_scheduled ON MessageDrafts(AuthorId, IsScheduled);

-- MessageDraftAttachments表索引
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_draft_id ON MessageDraftAttachments(DraftId);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_file_name ON MessageDraftAttachments(FileName);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_original_file_name ON MessageDraftAttachments(OriginalFileName);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_content_type ON MessageDraftAttachments(ContentType);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_attachment_type ON MessageDraftAttachments(AttachmentType);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_attachment_status ON MessageDraftAttachments(AttachmentStatus);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_uploaded_at ON MessageDraftAttachments(UploadedAt);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_draft_type ON MessageDraftAttachments(DraftId, AttachmentType);
CREATE INDEX IF NOT EXISTS idx_message_draft_attachments_draft_status ON MessageDraftAttachments(DraftId, AttachmentStatus);

-- Conversations表索引
CREATE INDEX IF NOT EXISTS idx_conversations_created_by ON Conversations(CreatedBy);
CREATE INDEX IF NOT EXISTS idx_conversations_created_at ON Conversations(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_conversations_updated_at ON Conversations(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_conversations_is_archived ON Conversations(IsArchived);
CREATE INDEX IF NOT EXISTS idx_conversations_is_pinned ON Conversations(IsPinned);
CREATE INDEX IF NOT EXISTS idx_conversations_is_muted ON Conversations(IsMuted);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_conversations_archived_created ON Conversations(IsArchived, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_conversations_pinned_created ON Conversations(IsPinned, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_conversations_muted_created ON Conversations(IsMuted, CreatedAt);

-- ConversationParticipants表索引
CREATE INDEX IF NOT EXISTS idx_conversation_participants_conversation_id ON ConversationParticipants(ConversationId);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_user_id ON ConversationParticipants(UserId);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_role ON ConversationParticipants(Role);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_joined_at ON ConversationParticipants(JoinedAt);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_last_read_at ON ConversationParticipants(LastReadAt);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_unread_count ON ConversationParticipants(UnreadCount);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_is_muted ON ConversationParticipants(IsMuted);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_conversation_participants_conversation_user ON ConversationParticipants(ConversationId, UserId);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_user_unread ON ConversationParticipants(UserId, UnreadCount);
CREATE INDEX IF NOT EXISTS idx_conversation_participants_conversation_joined ON ConversationParticipants(ConversationId, JoinedAt);

-- Notifications表索引
CREATE INDEX IF NOT EXISTS idx_notifications_user_id ON Notifications(UserId);
CREATE INDEX IF NOT EXISTS idx_notifications_type ON Notifications(Type);
CREATE INDEX IF NOT EXISTS idx_notifications_priority ON Notifications(Priority);
CREATE INDEX IF NOT EXISTS idx_notifications_status ON Notifications(Status);
CREATE INDEX IF NOT EXISTS idx_notifications_related_entity ON Notifications(RelatedEntityType, RelatedEntityId);
CREATE INDEX IF NOT EXISTS idx_notifications_triggered_by ON Notifications(TriggeredByUserId);
CREATE INDEX IF NOT EXISTS idx_notifications_action ON Notifications(Action);
CREATE INDEX IF NOT EXISTS idx_notifications_channel ON Notifications(Channel);
CREATE INDEX IF NOT EXISTS idx_notifications_is_read ON Notifications(IsRead);
CREATE INDEX IF NOT EXISTS idx_notifications_read_at ON Notifications(ReadAt);
CREATE INDEX IF NOT EXISTS idx_notifications_delivered_at ON Notifications(DeliveredAt);
CREATE INDEX IF NOT EXISTS idx_notifications_created_at ON Notifications(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_updated_at ON Notifications(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_expires_at ON Notifications(ExpiresAt);
CREATE INDEX IF NOT EXISTS idx_notifications_scheduled_to_send_at ON Notifications(ScheduledToSendAt);
CREATE INDEX IF NOT EXISTS idx_notifications_tag ON Notifications(Tag);
CREATE INDEX IF NOT EXISTS idx_notifications_is_archived ON Notifications(IsArchived);
CREATE INDEX IF NOT EXISTS idx_notifications_is_deleted ON Notifications(IsDeleted);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_notifications_user_status ON Notifications(UserId, Status);
CREATE INDEX IF NOT EXISTS idx_notifications_user_type ON Notifications(UserId, Type);
CREATE INDEX IF NOT EXISTS idx_notifications_user_created ON Notifications(UserId, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_status_created ON Notifications(Status, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_priority_created ON Notifications(Priority, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_type_created ON Notifications(Type, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notifications_unread_notifications ON Notifications(UserId, IsRead, Status);
CREATE INDEX IF NOT EXISTS idx_notifications_pending_notifications ON Notifications(Status, ScheduledToSendAt);
CREATE INDEX IF NOT EXISTS idx_notifications_expiring_notifications ON Notifications(ExpiresAt, Status);

-- NotificationSettings表索引
CREATE INDEX IF NOT EXISTS idx_notification_settings_user_id ON NotificationSettings(UserId);
CREATE INDEX IF NOT EXISTS idx_notification_settings_notification_type ON NotificationSettings(NotificationType);
CREATE INDEX IF NOT EXISTS idx_notification_settings_frequency ON NotificationSettings(Frequency);
CREATE INDEX IF NOT EXISTS idx_notification_settings_email_frequency ON NotificationSettings(EmailFrequency);
CREATE INDEX IF NOT EXISTS idx_notification_settings_language ON NotificationSettings(Language);
CREATE INDEX IF NOT EXISTS idx_notification_settings_time_zone ON NotificationSettings(TimeZone);
CREATE INDEX IF NOT EXISTS idx_notification_settings_created_at ON NotificationSettings(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_notification_settings_updated_at ON NotificationSettings(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_notification_settings_is_default ON NotificationSettings(IsDefault);
CREATE INDEX IF NOT EXISTS idx_notification_settings_is_active ON NotificationSettings(IsActive);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_notification_settings_user_type ON NotificationSettings(UserId, NotificationType);
CREATE INDEX IF NOT EXISTS idx_notification_settings_user_active ON NotificationSettings(UserId, IsActive);
CREATE INDEX IF NOT EXISTS idx_notification_settings_type_active ON NotificationSettings(NotificationType, IsActive);
CREATE INDEX IF NOT EXISTS idx_notification_settings_frequency_active ON NotificationSettings(Frequency, IsActive);

-- NotificationDeliveryHistory表索引
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_notification_id ON NotificationDeliveryHistory(NotificationId);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_channel ON NotificationDeliveryHistory(Channel);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_status ON NotificationDeliveryHistory(Status);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_sent_at ON NotificationDeliveryHistory(SentAt);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_delivered_at ON NotificationDeliveryHistory(DeliveredAt);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_read_at ON NotificationDeliveryHistory(ReadAt);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_retry_count ON NotificationDeliveryHistory(RetryCount);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_last_retry_at ON NotificationDeliveryHistory(LastRetryAt);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_recipient_address ON NotificationDeliveryHistory(RecipientAddress);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_provider ON NotificationDeliveryHistory(Provider);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_cost ON NotificationDeliveryHistory(Cost);

-- 复合索引
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_notification_channel ON NotificationDeliveryHistory(NotificationId, Channel);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_notification_status ON NotificationDeliveryHistory(NotificationId, Status);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_channel_status ON NotificationDeliveryHistory(Channel, Status);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_status_sent ON NotificationDeliveryHistory(Status, SentAt);
CREATE INDEX IF NOT EXISTS idx_notification_delivery_history_provider_status ON NotificationDeliveryHistory(Provider, Status);

-- =============================================
-- 创建触发器
-- =============================================

-- 更新Comments表UpdatedAt触发器
CREATE TRIGGER IF NOT EXISTS update_comments_updated_at 
AFTER UPDATE ON Comments
FOR EACH ROW
BEGIN
    UPDATE Comments SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
END;

-- 自动更新LikeCount触发器
CREATE TRIGGER IF NOT EXISTS update_comment_like_count
AFTER INSERT ON CommentLikes
FOR EACH ROW
BEGIN
    UPDATE Comments SET LikeCount = LikeCount + 1 WHERE Id = NEW.CommentId;
END;

CREATE TRIGGER IF NOT EXISTS update_comment_like_count_delete
AFTER DELETE ON CommentLikes
FOR EACH ROW
BEGIN
    UPDATE Comments SET LikeCount = LikeCount - 1 WHERE Id = OLD.CommentId;
END;

-- 自动更新ReplyCount触发器
CREATE TRIGGER IF NOT EXISTS update_comment_reply_count
AFTER INSERT ON Comments
FOR EACH ROW
BEGIN
    UPDATE Comments SET ReplyCount = ReplyCount + 1 WHERE Id = NEW.ParentId;
END;

CREATE TRIGGER IF NOT EXISTS update_comment_reply_count_delete
AFTER DELETE ON Comments
FOR EACH ROW
BEGIN
    UPDATE Comments SET ReplyCount = ReplyCount - 1 WHERE Id = OLD.ParentId;
END;

-- 自动更新消息的阅读时间和状态触发器
CREATE TRIGGER IF NOT EXISTS update_message_read_status
AFTER UPDATE ON Messages
FOR EACH ROW
BEGIN
    IF NEW.IsRead = 1 AND OLD.IsRead = 0 AND NEW.ReadAt IS NULL THEN
        UPDATE Messages SET ReadAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END;

-- 自动更新消息的回复计数触发器
CREATE TRIGGER IF NOT EXISTS update_message_reply_count
AFTER INSERT ON Messages
FOR EACH ROW
BEGIN
    IF NEW.ParentId IS NOT NULL THEN
        UPDATE Messages SET Status = 4 WHERE Id = NEW.ParentId; -- 设置状态为已回复
    END IF;
END;

-- 自动更新会话的最后活动时间触发器
CREATE TRIGGER IF NOT EXISTS update_conversation_activity
AFTER INSERT ON Messages
FOR EACH ROW
BEGIN
    IF NEW.ConversationId IS NOT NULL THEN
        UPDATE Conversations SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.ConversationId;
    END IF;
END;

-- 自动更新通知的阅读时间和状态触发器
CREATE TRIGGER IF NOT EXISTS update_notification_read_status
AFTER UPDATE ON Notifications
FOR EACH ROW
BEGIN
    IF NEW.IsRead = 1 AND OLD.IsRead = 0 AND NEW.ReadAt IS NULL THEN
        UPDATE Notifications SET ReadAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END;

-- 自动更新通知的确认时间触发器
CREATE TRIGGER IF NOT EXISTS update_notification_confirmation
AFTER UPDATE ON Notifications
FOR EACH ROW
BEGIN
    IF NEW.Status = 6 AND OLD.Status != 6 AND NEW.ConfirmedAt IS NULL THEN
        UPDATE Notifications SET ConfirmedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END IF;
END;

-- =============================================
-- 创建视图
-- =============================================

-- 创建评论统计视图
CREATE VIEW IF NOT EXISTS CommentStatsView AS
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
CREATE VIEW IF NOT EXISTS MessageStatsView AS
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
CREATE VIEW IF NOT EXISTS ConversationStatsView AS
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
CREATE VIEW IF NOT EXISTS NotificationStatsView AS
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
CREATE VIEW IF NOT EXISTS NotificationTypeStatsView AS
SELECT 
    Type,
    COUNT(*) as Count,
    COUNT(CASE WHEN IsRead = 0 THEN 1 END) as UnreadCount,
    COUNT(CASE WHEN Status = 7 THEN 1 END) as FailedCount,
    COUNT(CASE WHEN IsArchived = 1 THEN 1 END) as ArchivedCount,
    AVG(CAST((julianday(COALESCE(ReadAt, CreatedAt)) - julianday(CreatedAt)) * 24 * 60 AS INTEGER)) as AverageReadTimeMinutes,
    AVG(SendCount) as AverageSendCount,
    MAX(CreatedAt) as LastCreatedAt,
    MIN(CreatedAt) as FirstCreatedAt
FROM Notifications 
GROUP BY Type;

-- =============================================
-- 初始化数据
-- =============================================

-- 插入默认通知设置（为所有现有用户）
INSERT INTO NotificationSettings (Id, UserId, NotificationType, EnableInApp, EnableEmail, EnablePush, EnableDesktop, EnableSound, Frequency, EmailFrequency, BatchIntervalMinutes, Language, TimeZone, IsDefault, IsActive, CreatedAt, UpdatedAt)
SELECT 
    lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(6))),
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
    datetime('now'),
    datetime('now')
FROM Users u
WHERE u.Id NOT IN (SELECT UserId FROM NotificationSettings WHERE NotificationType IS NULL)
AND u.IsDeleted = 0;

-- =============================================
-- 验证脚本执行结果
-- =============================================

-- 显示迁移完成信息
SELECT 'CodeShare Comments, Messages and Notifications Database Migration (SQLite) completed successfully!' as Status;
SELECT datetime('now') as MigrationTime;

-- 显示表创建状态
SELECT 
    'Comments' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Comments') as IsCreated
UNION ALL
SELECT 
    'CommentLikes' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='CommentLikes') as IsCreated
UNION ALL
SELECT 
    'CommentReports' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='CommentReports') as IsCreated
UNION ALL
SELECT 
    'Messages' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Messages') as IsCreated
UNION ALL
SELECT 
    'MessageAttachments' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MessageAttachments') as IsCreated
UNION ALL
SELECT 
    'MessageDrafts' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MessageDrafts') as IsCreated
UNION ALL
SELECT 
    'MessageDraftAttachments' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MessageDraftAttachments') as IsCreated
UNION ALL
SELECT 
    'Conversations' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Conversations') as IsCreated
UNION ALL
SELECT 
    'ConversationParticipants' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ConversationParticipants') as IsCreated
UNION ALL
SELECT 
    'Notifications' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Notifications') as IsCreated
UNION ALL
SELECT 
    'NotificationSettings' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='NotificationSettings') as IsCreated
UNION ALL
SELECT 
    'NotificationDeliveryHistory' as TableName,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='NotificationDeliveryHistory') as IsCreated;

-- 显示索引创建状态
SELECT 
    'Total Tables' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                   'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                   'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory')) as Count
UNION ALL
SELECT 
    'Total Indexes' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND tbl_name IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                   'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                   'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory')) as Count
UNION ALL
SELECT 
    'Total Triggers' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND tbl_name IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                   'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                   'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory')) as Count
UNION ALL
SELECT 
    'Total Views' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='view' AND name IN ('CommentStatsView', 'MessageStatsView', 'ConversationStatsView', 'NotificationStatsView', 'NotificationTypeStatsView')) as Count;

-- 显示默认数据插入状态
SELECT 
    'NotificationSettings' as TableName,
    COUNT(*) as RecordCount
FROM NotificationSettings;

-- =============================================
-- 迁移完成
-- =============================================

SELECT 'Database migration completed successfully!' as FinalStatus;
SELECT 'Created/updated tables: Comments, CommentLikes, CommentReports, Messages, MessageAttachments, MessageDrafts, MessageDraftAttachments, Conversations, ConversationParticipants, Notifications, NotificationSettings, NotificationDeliveryHistory' as Tables;
SELECT 'Created views: CommentStatsView, MessageStatsView, ConversationStatsView, NotificationStatsView, NotificationTypeStatsView' as Views;
SELECT 'Created triggers: update_comments_updated_at, update_comment_like_count, update_comment_reply_count, update_message_read_status, update_conversation_activity, update_notification_read_status, update_notification_confirmation' as Triggers;
SELECT 'Inserted default notification settings for existing users' as Data;
SELECT 'Migration version: v2.0.0, Task: DS-04, Database: SQLite' as Version;