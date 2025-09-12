-- CodeShare 系统设置功能数据库回滚脚本
-- 版本: v1.0.0
-- 创建日期: 2025-01-01
-- 描述: 回滚系统设置功能的数据库更改

-- =============================================
-- 执行前检查
-- =============================================

-- 显示警告信息
SELECT 'WARNING: This script will drop all system settings related tables and data!' as Warning;
SELECT 'Make sure you have backed up your data before proceeding.' as Caution;
SELECT 'Press Ctrl+C to cancel or wait 5 seconds to continue...' as Countdown;
-- SELECT SLEEP(5) as Waiting;

-- =============================================
-- 删除触发器
-- =============================================

DROP TRIGGER IF EXISTS update_last_accessed_at;

-- =============================================
-- 删除存储过程
-- =============================================

DROP PROCEDURE IF EXISTS CleanupExpiredShareTokens;
DROP PROCEDURE IF EXISTS GetShareStats;

-- =============================================
-- 删除视图
-- =============================================

DROP VIEW IF EXISTS ShareStatsView;

-- =============================================
-- 删除索引
-- =============================================

-- SystemSettings表索引
DROP INDEX IF EXISTS idx_system_settings_created_at ON SystemSettings;
DROP INDEX IF EXISTS idx_system_settings_updated_at ON SystemSettings;
DROP INDEX IF EXISTS idx_system_settings_updated_by ON SystemSettings;
DROP INDEX IF EXISTS idx_system_settings_json_site ON SystemSettings;
DROP INDEX IF EXISTS idx_system_settings_json_security ON SystemSettings;
DROP INDEX IF EXISTS idx_system_settings_json_feature ON SystemSettings;
DROP INDEX IF EXISTS idx_system_settings_json_email ON SystemSettings;

-- SettingsHistory表索引
DROP INDEX IF EXISTS idx_settings_history_created_at ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_setting_type ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_setting_key ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_changed_by ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_change_category ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_is_important ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_status ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_client_ip ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_type_created ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_category_created ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_user_created ON SettingsHistory;
DROP INDEX IF EXISTS idx_settings_history_important_created ON SettingsHistory;

-- ShareTokens表索引
DROP INDEX IF EXISTS idx_share_tokens_token ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_snippet ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_created_by ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_expires_at ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_is_active ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_created_at ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_permission ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_active_expires ON ShareTokens;
DROP INDEX IF EXISTS idx_share_tokens_snippet_active ON ShareTokens;

-- ShareAccessLogs表索引
DROP INDEX IF EXISTS idx_share_access_logs_share_token ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_access_time ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_ip_address ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_access_type ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_session_id ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_token_time ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_time_type ON ShareAccessLogs;
DROP INDEX IF EXISTS idx_share_access_logs_ip_session ON ShareAccessLogs;

-- =============================================
-- 删除表
-- =============================================

-- 删除ShareAccessLogs表（先删除因为有外键依赖）
DROP TABLE IF EXISTS ShareAccessLogs;

-- 删除ShareTokens表
DROP TABLE IF EXISTS ShareTokens;

-- 删除SettingsHistory表
DROP TABLE IF EXISTS SettingsHistory;

-- 删除SystemSettings表
DROP TABLE IF EXISTS SystemSettings;

-- =============================================
-- 验证回滚结果
-- =============================================

-- 显示回滚完成信息
SELECT 'CodeShare System Settings Database Rollback completed successfully!' as Status;
SELECT NOW() as RollbackTime;

-- 显示表删除状态
SELECT 
    'SystemSettings' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'SystemSettings') as Exists
UNION ALL
SELECT 
    'SettingsHistory' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'SettingsHistory') as Exists
UNION ALL
SELECT 
    'ShareTokens' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ShareTokens') as Exists
UNION ALL
SELECT 
    'ShareAccessLogs' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ShareAccessLogs') as Exists;

-- 显示视图删除状态
SELECT 
    'ShareStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'ShareStatsView') as Exists;

-- 显示存储过程删除状态
SELECT 
    routine_name as ProcedureName,
    (SELECT COUNT(*) FROM information_schema.routines WHERE routine_schema = DATABASE() AND routine_name = 'CleanupExpiredShareTokens') as Exists
FROM information_schema.routines 
WHERE routine_schema = DATABASE() 
AND routine_name IN ('CleanupExpiredShareTokens', 'GetShareStats');

-- 显示触发器删除状态
SELECT 
    trigger_name as TriggerName,
    (SELECT COUNT(*) FROM information_schema.triggers WHERE trigger_schema = DATABASE() AND trigger_name = 'update_last_accessed_at') as Exists
FROM information_schema.triggers 
WHERE trigger_schema = DATABASE() 
AND trigger_name = 'update_last_accessed_at';

-- =============================================
-- 回滚完成
-- =============================================

SELECT 'Database rollback completed successfully!' as FinalStatus;
SELECT 'Dropped tables: SystemSettings, SettingsHistory, ShareTokens, ShareAccessLogs' as Tables;
SELECT 'Dropped views: ShareStatsView' as Views;
SELECT 'Dropped stored procedures: CleanupExpiredShareTokens, GetShareStats' as Procedures;
SELECT 'Dropped triggers: update_last_accessed_at' as Triggers;
SELECT 'All system settings related data has been removed from the database.' as Note;