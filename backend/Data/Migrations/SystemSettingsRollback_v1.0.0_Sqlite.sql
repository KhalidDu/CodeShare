-- CodeShare 系统设置功能数据库回滚脚本 (SQLite版本)
-- 版本: v1.0.0
-- 创建日期: 2025-01-01
-- 描述: 回滚系统设置功能的数据库更改 (SQLite版本)

-- =============================================
-- 执行前检查
-- =============================================

-- 显示警告信息
SELECT 'WARNING: This script will drop all system settings related tables and data!' as Warning;
SELECT 'Make sure you have backed up your data before proceeding.' as Caution;
SELECT 'SQLite rollback starting...' as Status;

-- =============================================
-- 删除触发器
-- =============================================

DROP TRIGGER IF EXISTS update_last_accessed_at;

-- =============================================
-- 删除视图
-- =============================================

DROP VIEW IF EXISTS ShareStatsView;

-- =============================================
-- 删除索引
-- =============================================

-- SystemSettings表索引
DROP INDEX IF EXISTS idx_system_settings_created_at;
DROP INDEX IF EXISTS idx_system_settings_updated_at;
DROP INDEX IF EXISTS idx_system_settings_updated_by;
DROP INDEX IF EXISTS idx_system_settings_json_site;
DROP INDEX IF EXISTS idx_system_settings_json_security;
DROP INDEX IF EXISTS idx_system_settings_json_feature;
DROP INDEX IF EXISTS idx_system_settings_json_email;

-- SettingsHistory表索引
DROP INDEX IF EXISTS idx_settings_history_created_at;
DROP INDEX IF EXISTS idx_settings_history_setting_type;
DROP INDEX IF EXISTS idx_settings_history_setting_key;
DROP INDEX IF EXISTS idx_settings_history_changed_by;
DROP INDEX IF EXISTS idx_settings_history_change_category;
DROP INDEX IF EXISTS idx_settings_history_is_important;
DROP INDEX IF EXISTS idx_settings_history_status;
DROP INDEX IF EXISTS idx_settings_history_client_ip;
DROP INDEX IF EXISTS idx_settings_history_type_created;
DROP INDEX IF EXISTS idx_settings_history_category_created;
DROP INDEX IF EXISTS idx_settings_history_user_created;
DROP INDEX IF EXISTS idx_settings_history_important_created;

-- ShareTokens表索引
DROP INDEX IF EXISTS idx_share_tokens_token;
DROP INDEX IF EXISTS idx_share_tokens_snippet;
DROP INDEX IF EXISTS idx_share_tokens_created_by;
DROP INDEX IF EXISTS idx_share_tokens_expires_at;
DROP INDEX IF EXISTS idx_share_tokens_is_active;
DROP INDEX IF EXISTS idx_share_tokens_created_at;
DROP INDEX IF EXISTS idx_share_tokens_permission;
DROP INDEX IF EXISTS idx_share_tokens_active_expires;
DROP INDEX IF EXISTS idx_share_tokens_snippet_active;

-- ShareAccessLogs表索引
DROP INDEX IF EXISTS idx_share_access_logs_share_token;
DROP INDEX IF EXISTS idx_share_access_logs_access_time;
DROP INDEX IF EXISTS idx_share_access_logs_ip_address;
DROP INDEX IF EXISTS idx_share_access_logs_access_type;
DROP INDEX IF EXISTS idx_share_access_logs_session_id;
DROP INDEX IF EXISTS idx_share_access_logs_token_time;
DROP INDEX IF EXISTS idx_share_access_logs_time_type;
DROP INDEX IF EXISTS idx_share_access_logs_ip_session;

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
SELECT 'CodeShare System Settings Database Rollback (SQLite) completed successfully!' as Status;
SELECT datetime('now') as RollbackTime;

-- 显示表删除状态
SELECT 
    'SystemSettings' as TableName,
    COUNT(*) as Exists
FROM sqlite_master 
WHERE type='table' AND name='SystemSettings'

UNION ALL

SELECT 
    'SettingsHistory' as TableName,
    COUNT(*) as Exists
FROM sqlite_master 
WHERE type='table' AND name='SettingsHistory'

UNION ALL

SELECT 
    'ShareTokens' as TableName,
    COUNT(*) as Exists
FROM sqlite_master 
WHERE type='table' AND name='ShareTokens'

UNION ALL

SELECT 
    'ShareAccessLogs' as TableName,
    COUNT(*) as Exists
FROM sqlite_master 
WHERE type='table' AND name='ShareAccessLogs';

-- 显示视图删除状态
SELECT 
    'ShareStatsView' as ViewName,
    COUNT(*) as Exists
FROM sqlite_master 
WHERE type='view' AND name='ShareStatsView';

-- 显示触发器删除状态
SELECT 
    'update_last_accessed_at' as TriggerName,
    COUNT(*) as Exists
FROM sqlite_master 
WHERE type='trigger' AND name='update_last_accessed_at';

-- 显示索引删除状态
SELECT 
    'Index Cleanup' as Category,
    'Remaining Indexes' as Metric,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%' AND tbl_name IN ('SystemSettings', 'SettingsHistory', 'ShareTokens', 'ShareAccessLogs')) as Value;

-- 显示数据库统计信息
SELECT 
    'Database Statistics' as Category,
    'Total Tables' as Metric,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='table') as Value

UNION ALL

SELECT 
    'Database Statistics' as Category,
    'Total Indexes' as Metric,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name NOT LIKE 'sqlite_%') as Value

UNION ALL

SELECT 
    'Database Statistics' as Category,
    'Total Views' as Metric,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='view') as Value

UNION ALL

SELECT 
    'Database Statistics' as Category,
    'Total Triggers' as Metric,
    (SELECT COUNT(*) FROM sqlite_master WHERE type='trigger') as Value;

-- =============================================
-- 回滚完成
-- =============================================

SELECT 'SQLite database rollback completed successfully!' as FinalStatus;
SELECT 'Dropped tables: SystemSettings, SettingsHistory, ShareTokens, ShareAccessLogs' as Tables;
SELECT 'Dropped views: ShareStatsView' as Views;
SELECT 'Dropped triggers: update_last_accessed_at' as Triggers;
SELECT 'All system settings related data has been removed from the SQLite database.' as Note;

-- 显示版本信息
SELECT 'CodeShare System Settings v1.0.0' as Version;
SELECT 'SQLite Database Rollback' as RollbackType;