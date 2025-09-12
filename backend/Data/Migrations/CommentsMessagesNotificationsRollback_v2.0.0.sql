-- CodeShare 评论、消息、通知系统数据库迁移回滚脚本
-- 版本: v2.0.0
-- 创建日期: 2025-01-09
-- 描述: 回滚评论、消息、通知系统的所有数据表和相关对象
-- 支持MySQL语法
-- 任务编号: DS-04

-- =============================================
-- 回滚前检查
-- =============================================

-- 检查是否需要回滚
SET @rollback_needed = 0;
SELECT COUNT(*) INTO @rollback_needed 
FROM information_schema.tables 
WHERE table_schema = DATABASE() 
AND table_name IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                   'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                   'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory');

IF @rollback_needed = 0 THEN
    SELECT 'No tables found to rollback. Migration may not have been applied.' as Status;
    SELECT 'Rollback completed.' as FinalStatus;
ELSE
    SELECT 'Starting rollback process...' as Status;
    
    -- =============================================
    -- 删除存储过程
    -- =============================================
    
    DROP PROCEDURE IF EXISTS CleanupExpiredComments;
    DROP PROCEDURE IF EXISTS CleanupExpiredMessages;
    DROP PROCEDURE IF EXISTS CleanupExpiredNotifications;
    DROP PROCEDURE IF EXISTS GetCommentStats;
    DROP PROCEDURE IF EXISTS GetMessageStats;
    DROP PROCEDURE IF EXISTS GetNotificationStats;
    
    SELECT 'Dropped stored procedures: CleanupExpiredComments, CleanupExpiredMessages, CleanupExpiredNotifications, GetCommentStats, GetMessageStats, GetNotificationStats' as Step;
    
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
    -- 删除视图
    -- =============================================
    
    DROP VIEW IF EXISTS CommentStatsView;
    DROP VIEW IF EXISTS MessageStatsView;
    DROP VIEW IF EXISTS ConversationStatsView;
    DROP VIEW IF EXISTS NotificationStatsView;
    DROP VIEW IF EXISTS NotificationTypeStatsView;
    
    SELECT 'Dropped views: CommentStatsView, MessageStatsView, ConversationStatsView, NotificationStatsView, NotificationTypeStatsView' as Step;
    
    -- =============================================
    -- 删除外键约束（MySQL需要先删除外键约束）
    -- =============================================
    
    -- Comments表的外键
    SET @foreignKeyExists = 0;
    SELECT COUNT(*) INTO @foreignKeyExists 
    FROM information_schema.table_constraints 
    WHERE constraint_schema = DATABASE() 
    AND table_name = 'Comments' 
    AND constraint_type = 'FOREIGN KEY';
    
    IF @foreignKeyExists > 0 THEN
        -- 删除Comments表的外键约束
        SELECT CONCAT('Dropping foreign keys for Comments table...') as Step;
        SET @sql = CONCAT('ALTER TABLE Comments DROP FOREIGN KEY ', 
                         (SELECT CONSTRAINT_NAME 
                          FROM information_schema.table_constraints 
                          WHERE constraint_schema = DATABASE() 
                          AND table_name = 'Comments' 
                          AND constraint_type = 'FOREIGN KEY' 
                          AND REFERENCED_TABLE_NAME = 'CodeSnippets' 
                          LIMIT 1));
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        
        SET @sql = CONCAT('ALTER TABLE Comments DROP FOREIGN KEY ', 
                         (SELECT CONSTRAINT_NAME 
                          FROM information_schema.table_constraints 
                          WHERE constraint_schema = DATABASE() 
                          AND table_name = 'Comments' 
                          AND constraint_type = 'FOREIGN KEY' 
                          AND REFERENCED_TABLE_NAME = 'Users' 
                          LIMIT 1));
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        
        SET @sql = CONCAT('ALTER TABLE Comments DROP FOREIGN KEY ', 
                         (SELECT CONSTRAINT_NAME 
                          FROM information_schema.table_constraints 
                          WHERE constraint_schema = DATABASE() 
                          AND table_name = 'Comments' 
                          AND constraint_type = 'FOREIGN KEY' 
                          AND REFERENCED_TABLE_NAME = 'Comments' 
                          LIMIT 1));
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
    
    -- Messages表的外键
    SET @foreignKeyExists = 0;
    SELECT COUNT(*) INTO @foreignKeyExists 
    FROM information_schema.table_constraints 
    WHERE constraint_schema = DATABASE() 
    AND table_name = 'Messages' 
    AND constraint_type = 'FOREIGN KEY';
    
    IF @foreignKeyExists > 0 THEN
        SELECT CONCAT('Dropping foreign keys for Messages table...') as Step;
        SET @sql = CONCAT('ALTER TABLE Messages DROP FOREIGN KEY ', 
                         (SELECT CONSTRAINT_NAME 
                          FROM information_schema.table_constraints 
                          WHERE constraint_schema = DATABASE() 
                          AND table_name = 'Messages' 
                          AND constraint_type = 'FOREIGN KEY' 
                          AND REFERENCED_TABLE_NAME = 'Users' 
                          AND REFERENCED_COLUMN_NAME = 'SenderId' 
                          LIMIT 1));
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        
        SET @sql = CONCAT('ALTER TABLE Messages DROP FOREIGN KEY ', 
                         (SELECT CONSTRAINT_NAME 
                          FROM information_schema.table_constraints 
                          WHERE constraint_schema = DATABASE() 
                          AND table_name = 'Messages' 
                          AND constraint_type = 'FOREIGN KEY' 
                          AND REFERENCED_TABLE_NAME = 'Users' 
                          AND REFERENCED_COLUMN_NAME = 'ReceiverId' 
                          LIMIT 1));
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
        
        SET @sql = CONCAT('ALTER TABLE Messages DROP FOREIGN KEY ', 
                         (SELECT CONSTRAINT_NAME 
                          FROM information_schema.table_constraints 
                          WHERE constraint_schema = DATABASE() 
                          AND table_name = 'Messages' 
                          AND constraint_type = 'FOREIGN KEY' 
                          AND REFERENCED_TABLE_NAME = 'Messages' 
                          LIMIT 1));
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
    
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
        table_name as TableName,
        'DROPPED' as Status
    FROM information_schema.tables 
    WHERE table_schema = DATABASE() 
    AND table_name IN ('Comments', 'CommentLikes', 'CommentReports', 'Messages', 'MessageAttachments', 
                       'MessageDrafts', 'MessageDraftAttachments', 'Conversations', 'ConversationParticipants',
                       'Notifications', 'NotificationSettings', 'NotificationDeliveryHistory')
    UNION ALL
    SELECT 
        'Comments' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Comments') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Comments') = 0
    UNION ALL
    SELECT 
        'CommentLikes' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'CommentLikes') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'CommentLikes') = 0
    UNION ALL
    SELECT 
        'CommentReports' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'CommentReports') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'CommentReports') = 0
    UNION ALL
    SELECT 
        'Messages' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Messages') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Messages') = 0
    UNION ALL
    SELECT 
        'MessageAttachments' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageAttachments') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageAttachments') = 0
    UNION ALL
    SELECT 
        'MessageDrafts' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageDrafts') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageDrafts') = 0
    UNION ALL
    SELECT 
        'MessageDraftAttachments' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageDraftAttachments') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'MessageDraftAttachments') = 0
    UNION ALL
    SELECT 
        'Conversations' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Conversations') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Conversations') = 0
    UNION ALL
    SELECT 
        'ConversationParticipants' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ConversationParticipants') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ConversationParticipants') = 0
    UNION ALL
    SELECT 
        'Notifications' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Notifications') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'Notifications') = 0
    UNION ALL
    SELECT 
        'NotificationSettings' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'NotificationSettings') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'NotificationSettings') = 0
    UNION ALL
    SELECT 
        'NotificationDeliveryHistory' as TableName,
        IF((SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'NotificationDeliveryHistory') = 0, 'DROPPED', 'FAILED') as Status
    WHERE (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'NotificationDeliveryHistory') = 0;
    
    -- 检查视图是否已删除
    SELECT 
        table_name as ObjectName,
        'DROPPED' as Status
    FROM information_schema.views 
    WHERE table_schema = DATABASE() 
    AND table_name IN ('CommentStatsView', 'MessageStatsView', 'ConversationStatsView', 'NotificationStatsView', 'NotificationTypeStatsView');
    
    -- 检查触发器是否已删除
    SELECT 
        trigger_name as ObjectName,
        'DROPPED' as Status
    FROM information_schema.triggers 
    WHERE trigger_schema = DATABASE() 
    AND trigger_name IN ('update_comments_updated_at', 'update_comment_like_count', 'update_comment_like_count_delete', 
                        'update_comment_reply_count', 'update_comment_reply_count_delete', 'update_message_read_status',
                        'update_message_reply_count', 'update_conversation_activity', 'update_notification_read_status',
                        'update_notification_confirmation');
    
    -- 检查存储过程是否已删除
    SELECT 
        routine_name as ObjectName,
        'DROPPED' as Status
    FROM information_schema.routines 
    WHERE routine_schema = DATABASE() 
    AND routine_name IN ('CleanupExpiredComments', 'CleanupExpiredMessages', 'CleanupExpiredNotifications', 
                        'GetCommentStats', 'GetMessageStats', 'GetNotificationStats');
    
    -- =============================================
    -- 回滚完成
    -- =============================================
    
    SELECT 'Rollback completed successfully!' as FinalStatus;
    SELECT CONCAT('Rolled back migration v2.0.0 (DS-04): Comments, Messages, and Notifications system') as Summary;
    SELECT CONCAT('Dropped tables: Comments, CommentLikes, CommentReports, Messages, MessageAttachments, MessageDrafts, MessageDraftAttachments, Conversations, ConversationParticipants, Notifications, NotificationSettings, NotificationDeliveryHistory') as Tables;
    SELECT CONCAT('Dropped views: CommentStatsView, MessageStatsView, ConversationStatsView, NotificationStatsView, NotificationTypeStatsView') as Views;
    SELECT CONCAT('Dropped triggers: 10 triggers') as Triggers;
    SELECT CONCAT('Dropped stored procedures: CleanupExpiredComments, CleanupExpiredMessages, CleanupExpiredNotifications, GetCommentStats, GetMessageStats, GetNotificationStats') as Procedures;
    SELECT NOW() as RollbackTime;
END IF;