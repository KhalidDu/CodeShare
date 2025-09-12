-- CodeShare 评论、消息、通知系统数据库迁移回滚脚本
-- 版本: v2.0.0
-- 创建日期: 2025-01-09
-- 描述: 回滚评论、消息、通知系统的所有数据表和相关对象
-- 支持SQLite语法（用于开发环境）
-- 任务编号: DS-04

-- =============================================
-- 回滚前检查
-- =============================================

-- 检查是否需要回滚
CREATE TABLE IF NOT EXISTS RollbackCheck (
    id INTEGER PRIMARY KEY,
    table_name TEXT NOT NULL,
    check_time TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 删除测试记录
DELETE FROM RollbackCheck WHERE table_name = 'CommentsMessagesNotifications_v2.0.0';

-- 插入检查记录
INSERT INTO RollbackCheck (table_name) VALUES ('CommentsMessagesNotifications_v2.0.0');

-- =============================================
-- 删除视图
-- =============================================

DROP VIEW IF EXISTS CommentStatsView;
DROP VIEW IF EXISTS MessageStatsView;
DROP VIEW IF EXISTS ConversationStatsView;
DROP VIEW IF EXISTS NotificationStatsView;
DROP VIEW IF EXISTS NotificationTypeStatsView;

SELECT 'Dropped views: CommentStatsView, MessageStatsView, ConversationStatsView, NotificationStatsView, NotificationTypeStatsView' as Step;

-- =============================================
-- 删除触发器
-- =============================================

DROP TRIGGER IF EXISTS update_comments_updated_at;
DROP TRIGGER IF EXISTS update_comment_like_count;
DROP TRIGGER IF EXISTS update_comment_like_count_delete;
DROP TRIGGER IF EXISTS update_comment_reply_count;
DROP TRIGGER IF EXISTS update_comment_reply_count_delete;
DROP TRIGGER IF EXISTS update_message_read_status;
DROP TRIGGER IF EXISTS update_message_reply_count;
DROP TRIGGER IF EXISTS update_conversation_activity;
DROP TRIGGER IF EXISTS update_notification_read_status;
DROP TRIGGER IF EXISTS update_notification_confirmation;

SELECT 'Dropped triggers: update_comments_updated_at, update_comment_like_count, update_comment_reply_count, update_message_read_status, update_conversation_activity, update_notification_read_status, update_notification_confirmation' as Step;

-- =============================================
-- 删除索引（SQLite会自动删除索引，但我们可以显式删除）
-- =============================================

-- Comments表索引
DROP INDEX IF EXISTS idx_comments_snippet_id;
DROP INDEX IF EXISTS idx_comments_user_id;
DROP INDEX IF EXISTS idx_comments_parent_id;
DROP INDEX IF EXISTS idx_comments_parent_path;
DROP INDEX IF EXISTS idx_comments_depth;
DROP INDEX IF EXISTS idx_comments_status;
DROP INDEX IF EXISTS idx_comments_created_at;
DROP INDEX IF EXISTS idx_comments_updated_at;
DROP INDEX IF EXISTS idx_comments_deleted_at;
DROP INDEX IF EXISTS idx_comments_like_count;
DROP INDEX IF EXISTS idx_comments_reply_count;
DROP INDEX IF EXISTS idx_comments_snippet_status;
DROP INDEX IF EXISTS idx_comments_snippet_created;
DROP INDEX IF EXISTS idx_comments_parent_path_created;
DROP INDEX IF EXISTS idx_comments_user_created;
DROP INDEX IF EXISTS idx_comments_status_created;

-- CommentLikes表索引
DROP INDEX IF EXISTS idx_comment_likes_comment_id;
DROP INDEX IF EXISTS idx_comment_likes_user_id;
DROP INDEX IF EXISTS idx_comment_likes_created_at;
DROP INDEX IF EXISTS idx_comment_likes_comment_created;
DROP INDEX IF EXISTS idx_comment_likes_user_created;

-- CommentReports表索引
DROP INDEX IF EXISTS idx_comment_reports_comment_id;
DROP INDEX IF EXISTS idx_comment_reports_user_id;
DROP INDEX IF EXISTS idx_comment_reports_reason;
DROP INDEX IF EXISTS idx_comment_reports_status;
DROP INDEX IF EXISTS idx_comment_reports_created_at;
DROP INDEX IF EXISTS idx_comment_reports_handled_at;
DROP INDEX IF EXISTS idx_comment_reports_handled_by;
DROP INDEX IF EXISTS idx_comment_reports_comment_status;
DROP INDEX IF EXISTS idx_comment_reports_status_created;
DROP INDEX IF EXISTS idx_comment_reports_reason_status;
DROP INDEX IF EXISTS idx_comment_reports_user_created;
DROP INDEX IF EXISTS idx_comment_reports_handled_created;

-- Messages表索引
DROP INDEX IF EXISTS idx_messages_sender_id;
DROP INDEX IF EXISTS idx_messages_receiver_id;
DROP INDEX IF EXISTS idx_messages_message_type;
DROP INDEX IF EXISTS idx_messages_status;
DROP INDEX IF EXISTS idx_messages_priority;
DROP INDEX IF EXISTS idx_messages_parent_id;
DROP INDEX IF EXISTS idx_messages_conversation_id;
DROP INDEX IF EXISTS idx_messages_is_read;
DROP INDEX IF EXISTS idx_messages_read_at;
DROP INDEX IF EXISTS idx_messages_tag;
DROP INDEX IF EXISTS idx_messages_created_at;
DROP INDEX IF EXISTS idx_messages_updated_at;
DROP INDEX IF EXISTS idx_messages_deleted_at;
DROP INDEX IF EXISTS idx_messages_expires_at;
DROP INDEX IF EXISTS idx_messages_sender_receiver;
DROP INDEX IF EXISTS idx_messages_conversation_created;
DROP INDEX IF EXISTS idx_messages_status_created;
DROP INDEX IF EXISTS idx_messages_priority_created;
DROP INDEX IF EXISTS idx_messages_sender_created;
DROP INDEX IF EXISTS idx_messages_receiver_created;
DROP INDEX IF EXISTS idx_messages_unread_messages;

-- MessageAttachments表索引
DROP INDEX IF EXISTS idx_message_attachments_message_id;
DROP INDEX IF EXISTS idx_message_attachments_file_name;
DROP INDEX IF EXISTS idx_message_attachments_original_file_name;
DROP INDEX IF EXISTS idx_message_attachments_content_type;
DROP INDEX IF EXISTS idx_message_attachments_attachment_type;
DROP INDEX IF EXISTS idx_message_attachments_attachment_status;
DROP INDEX IF EXISTS idx_message_attachments_file_hash;
DROP INDEX IF EXISTS idx_message_attachments_uploaded_at;
DROP INDEX IF EXISTS idx_message_attachments_download_count;
DROP INDEX IF EXISTS idx_message_attachments_message_type;
DROP INDEX IF EXISTS idx_message_attachments_message_status;
DROP INDEX IF EXISTS idx_message_attachments_type_status;
DROP INDEX IF EXISTS idx_message_attachments_upload_created;

-- MessageDrafts表索引
DROP INDEX IF EXISTS idx_message_drafts_author_id;
DROP INDEX IF EXISTS idx_message_drafts_receiver_id;
DROP INDEX IF EXISTS idx_message_drafts_message_type;
DROP INDEX IF EXISTS idx_message_drafts_priority;
DROP INDEX IF EXISTS idx_message_drafts_parent_id;
DROP INDEX IF EXISTS idx_message_drafts_conversation_id;
DROP INDEX IF EXISTS idx_message_drafts_status;
DROP INDEX IF EXISTS idx_message_drafts_created_at;
DROP INDEX IF EXISTS idx_message_drafts_updated_at;
DROP INDEX IF EXISTS idx_message_drafts_scheduled_to_send_at;
DROP INDEX IF EXISTS idx_message_drafts_expires_at;
DROP INDEX IF EXISTS idx_message_drafts_is_scheduled;
DROP INDEX IF EXISTS idx_message_drafts_author_status;
DROP INDEX IF EXISTS idx_message_drafts_author_created;
DROP INDEX IF EXISTS idx_message_drafts_status_created;
DROP INDEX IF EXISTS idx_message_drafts_scheduled_created;
DROP INDEX IF EXISTS idx_message_drafts_author_scheduled;

-- MessageDraftAttachments表索引
DROP INDEX IF EXISTS idx_message_draft_attachments_draft_id;
DROP INDEX IF EXISTS idx_message_draft_attachments_file_name;
DROP INDEX IF EXISTS idx_message_draft_attachments_original_file_name;
DROP INDEX IF EXISTS idx_message_draft_attachments_content_type;
DROP INDEX IF EXISTS idx_message_draft_attachments_attachment_type;
DROP INDEX IF EXISTS idx_message_draft_attachments_attachment_status;
DROP INDEX IF EXISTS idx_message_draft_attachments_uploaded_at;
DROP INDEX IF EXISTS idx_message_draft_attachments_draft_type;
DROP INDEX IF EXISTS idx_message_draft_attachments_draft_status;

-- Conversations表索引
DROP INDEX IF EXISTS idx_conversations_created_by;
DROP INDEX IF EXISTS idx_conversations_created_at;
DROP INDEX IF EXISTS idx_conversations_updated_at;
DROP INDEX IF EXISTS idx_conversations_is_archived;
DROP INDEX IF EXISTS idx_conversations_is_pinned;
DROP INDEX IF EXISTS idx_conversations_is_muted;
DROP INDEX IF EXISTS idx_conversations_archived_created;
DROP INDEX IF EXISTS idx_conversations_pinned_created;
DROP INDEX IF EXISTS idx_conversations_muted_created;

-- ConversationParticipants表索引
DROP INDEX IF EXISTS idx_conversation_participants_conversation_id;
DROP INDEX IF EXISTS idx_conversation_participants_user_id;
DROP INDEX IF EXISTS idx_conversation_participants_role;
DROP INDEX IF EXISTS idx_conversation_participants_joined_at;
DROP INDEX IF EXISTS idx_conversation_participants_last_read_at;
DROP INDEX IF EXISTS idx_conversation_participants_unread_count;
DROP INDEX IF EXISTS idx_conversation_participants_is_muted;
DROP INDEX IF EXISTS idx_conversation_participants_conversation_user;
DROP INDEX IF EXISTS idx_conversation_participants_user_unread;
DROP INDEX IF EXISTS idx_conversation_participants_conversation_joined;

-- Notifications表索引
DROP INDEX IF EXISTS idx_notifications_user_id;
DROP INDEX IF EXISTS idx_notifications_type;
DROP INDEX IF EXISTS idx_notifications_priority;
DROP INDEX IF EXISTS idx_notifications_status;
DROP INDEX IF EXISTS idx_notifications_related_entity;
DROP INDEX IF EXISTS idx_notifications_triggered_by;
DROP INDEX IF EXISTS idx_notifications_action;
DROP INDEX IF EXISTS idx_notifications_channel;
DROP INDEX IF EXISTS idx_notifications_is_read;
DROP INDEX IF EXISTS idx_notifications_read_at;
DROP INDEX IF EXISTS idx_notifications_delivered_at;
DROP INDEX IF EXISTS idx_notifications_created_at;
DROP INDEX IF EXISTS idx_notifications_updated_at;
DROP INDEX IF EXISTS idx_notifications_expires_at;
DROP INDEX IF EXISTS idx_notifications_scheduled_to_send_at;
DROP INDEX IF EXISTS idx_notifications_tag;
DROP INDEX IF EXISTS idx_notifications_is_archived;
DROP INDEX IF EXISTS idx_notifications_is_deleted;
DROP INDEX IF EXISTS idx_notifications_user_status;
DROP INDEX IF EXISTS idx_notifications_user_type;
DROP INDEX IF EXISTS idx_notifications_user_created;
DROP INDEX IF EXISTS idx_notifications_status_created;
DROP INDEX IF EXISTS idx_notifications_priority_created;
DROP INDEX IF EXISTS idx_notifications_type_created;
DROP INDEX IF EXISTS idx_notifications_unread_notifications;
DROP INDEX IF EXISTS idx_notifications_pending_notifications;
DROP INDEX IF EXISTS idx_notifications_expiring_notifications;

-- NotificationSettings表索引
DROP INDEX IF EXISTS idx_notification_settings_user_id;
DROP INDEX IF EXISTS idx_notification_settings_notification_type;
DROP INDEX IF EXISTS idx_notification_settings_frequency;
DROP INDEX IF EXISTS idx_notification_settings_email_frequency;
DROP INDEX IF EXISTS idx_notification_settings_language;
DROP INDEX IF EXISTS idx_notification_settings_time_zone;
DROP INDEX IF EXISTS idx_notification_settings_created_at;
DROP INDEX IF EXISTS idx_notification_settings_updated_at;
DROP INDEX IF EXISTS idx_notification_settings_is_default;
DROP INDEX IF EXISTS idx_notification_settings_is_active;
DROP INDEX IF EXISTS idx_notification_settings_user_type;
DROP INDEX IF EXISTS idx_notification_settings_user_active;
DROP INDEX IF EXISTS idx_notification_settings_type_active;
DROP INDEX IF EXISTS idx_notification_settings_frequency_active;

-- NotificationDeliveryHistory表索引
DROP INDEX IF EXISTS idx_notification_delivery_history_notification_id;
DROP INDEX IF EXISTS idx_notification_delivery_history_channel;
DROP INDEX IF EXISTS idx_notification_delivery_history_status;
DROP INDEX IF EXISTS idx_notification_delivery_history_sent_at;
DROP INDEX IF EXISTS idx_notification_delivery_history_delivered_at;
DROP INDEX IF EXISTS idx_notification_delivery_history_read_at;
DROP INDEX IF EXISTS idx_notification_delivery_history_retry_count;
DROP INDEX IF EXISTS idx_notification_delivery_history_last_retry_at;
DROP INDEX IF EXISTS idx_notification_delivery_history_recipient_address;
DROP INDEX IF EXISTS idx_notification_delivery_history_provider;
DROP INDEX IF EXISTS idx_notification_delivery_history_cost;
DROP INDEX IF EXISTS idx_notification_delivery_history_notification_channel;
DROP INDEX IF EXISTS idx_notification_delivery_history_notification_status;
DROP INDEX IF EXISTS idx_notification_delivery_history_channel_status;
DROP INDEX IF EXISTS idx_notification_delivery_history_status_sent;
DROP INDEX IF EXISTS idx_notification_delivery_history_provider_status;

SELECT 'Dropped all indexes' as Step;

-- =============================================
-- 删除数据表（按依赖关系反向删除）
-- =============================================

-- 通知系统表
DROP TABLE IF EXISTS NotificationDeliveryHistory;
SELECT 'Dropped table: NotificationDeliveryHistory' as Step;

DROP TABLE IF EXISTS NotificationSettings;
SELECT 'Dropped table: NotificationSettings' as Step;

DROP TABLE IF EXISTS Notifications;
SELECT 'Dropped table: Notifications' as Step;

-- 消息系统表
DROP TABLE IF EXISTS MessageDraftAttachments;
SELECT 'Dropped table: MessageDraftAttachments' as Step;

DROP TABLE IF EXISTS MessageDrafts;
SELECT 'Dropped table: MessageDrafts' as Step;

DROP TABLE IF EXISTS MessageAttachments;
SELECT 'Dropped table: MessageAttachments' as Step;

DROP TABLE IF EXISTS ConversationParticipants;
SELECT 'Dropped table: ConversationParticipants' as Step;

DROP TABLE IF EXISTS Conversations;
SELECT 'Dropped table: Conversations' as Step;

DROP TABLE IF EXISTS Messages;
SELECT 'Dropped table: Messages' as Step;

-- 评论系统表
DROP TABLE IF EXISTS CommentReports;
SELECT 'Dropped table: CommentReports' as Step;

DROP TABLE IF EXISTS CommentLikes;
SELECT 'Dropped table: CommentLikes' as Step;

DROP TABLE IF EXISTS Comments;
SELECT 'Dropped table: Comments' as Step;

-- =============================================
-- 验证回滚结果
-- =============================================

SELECT 'Verifying rollback results...' as Step;

-- 检查表是否已删除
SELECT 
    'Comments' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Comments') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'CommentLikes' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='CommentLikes') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'CommentReports' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='CommentReports') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'Messages' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Messages') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'MessageAttachments' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MessageAttachments') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'MessageDrafts' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MessageDrafts') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'MessageDraftAttachments' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='MessageDraftAttachments') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'Conversations' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Conversations') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'ConversationParticipants' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='ConversationParticipants') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'Notifications' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Notifications') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'NotificationSettings' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='NotificationSettings') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
UNION ALL
SELECT 
    'NotificationDeliveryHistory' as TableName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='NotificationDeliveryHistory') = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status;

-- 检查视图是否已删除
SELECT 
    table_name as ObjectName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='view' AND name=table_name) = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
FROM (
    SELECT 'CommentStatsView' as table_name UNION ALL
    SELECT 'MessageStatsView' as table_name UNION ALL
    SELECT 'ConversationStatsView' as table_name UNION ALL
    SELECT 'NotificationStatsView' as table_name UNION ALL
    SELECT 'NotificationTypeStatsView' as table_name
);

-- 检查触发器是否已删除
SELECT 
    name as ObjectName,
    CASE WHEN (SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name=name) = 0 THEN 'DROPPED' ELSE 'FAILED' END as Status
FROM (
    SELECT 'update_comments_updated_at' as name UNION ALL
    SELECT 'update_comment_like_count' as name UNION ALL
    SELECT 'update_comment_like_count_delete' as name UNION ALL
    SELECT 'update_comment_reply_count' as name UNION ALL
    SELECT 'update_comment_reply_count_delete' as name UNION ALL
    SELECT 'update_message_read_status' as name UNION ALL
    SELECT 'update_message_reply_count' as name UNION ALL
    SELECT 'update_conversation_activity' as name UNION ALL
    SELECT 'update_notification_read_status' as name UNION ALL
    SELECT 'update_notification_confirmation' as name
);

-- 统计结果
SELECT 
    'Tables Remaining' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                   'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                   'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory')) as Count
UNION ALL
SELECT 
    'Views Remaining' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='view' AND name IN ('CommentStatsView', 'MessageStatsView', 'ConversationStatsView', 'NotificationStatsView', 'NotificationTypeStatsView')) as Count
UNION ALL
SELECT 
    'Triggers Remaining' as ObjectType,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='trigger' AND name IN ('update_comments_updated_at', 'update_comment_like_count', 'update_comment_reply_count', 'update_message_read_status', 'update_conversation_activity', 'update_notification_read_status', 'update_notification_confirmation')) as Count;

-- 清理检查表
DELETE FROM RollbackCheck WHERE table_name = 'CommentsMessagesNotifications_v2.0.0';

-- =============================================
-- 回滚完成
-- =============================================

SELECT 'Rollback completed successfully!' as FinalStatus;
SELECT 'Rolled back migration v2.0.0 (DS-04): Comments, Messages, and Notifications system' as Summary;
SELECT 'Dropped tables: Comments, CommentLikes, CommentReports, Messages, MessageAttachments, MessageDrafts, MessageDraftAttachments, Conversations, ConversationParticipants, Notifications, NotificationSettings, NotificationDeliveryHistory' as Tables;
SELECT 'Dropped views: CommentStatsView, MessageStatsView, ConversationStatsView, NotificationStatsView, NotificationTypeStatsView' as Views;
SELECT 'Dropped triggers: 10 triggers' as Triggers;
SELECT 'Dropped indexes: All indexes automatically dropped with tables' as Indexes;
SELECT 'Migration version: v2.0.0, Task: DS-04, Database: SQLite' as Version;
SELECT datetime('now') as RollbackTime;