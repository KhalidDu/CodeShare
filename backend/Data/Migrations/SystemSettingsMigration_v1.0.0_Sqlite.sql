-- CodeShare 系统设置功能数据库迁移脚本 (SQLite版本)
-- 版本: v1.0.0
-- 创建日期: 2025-01-01
-- 描述: 为系统设置功能添加必要的数据表和初始化数据 (SQLite版本)

-- =============================================
-- 启用外键约束
-- =============================================
PRAGMA foreign_keys = ON;

-- =============================================
-- 创建SystemSettings表
-- =============================================
CREATE TABLE IF NOT EXISTS SystemSettings (
    Id TEXT PRIMARY KEY,                        -- UUID主键
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间 (ISO8601格式)
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),  -- 更新时间 (ISO8601格式)
    UpdatedBy TEXT NOT NULL DEFAULT 'System',   -- 最后更新者
    SiteSettingsJson TEXT NOT NULL DEFAULT '{}', -- 站点设置JSON数据
    SecuritySettingsJson TEXT NOT NULL DEFAULT '{}', -- 安全设置JSON数据
    FeatureSettingsJson TEXT NOT NULL DEFAULT '{}', -- 功能设置JSON数据
    EmailSettingsJson TEXT NOT NULL DEFAULT '{}'  -- 邮件设置JSON数据
);

-- =============================================
-- 创建SettingsHistory表
-- =============================================
CREATE TABLE IF NOT EXISTS SettingsHistory (
    Id TEXT PRIMARY KEY,
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间 (ISO8601格式)
    SettingType TEXT NOT NULL,                  -- 设置类型
    SettingKey TEXT NOT NULL,                   -- 设置键名
    OldValue TEXT,                              -- 变更前的值
    NewValue TEXT,                              -- 变更后的值
    ChangedBy TEXT NOT NULL,                    -- 操作人
    ChangedById TEXT,                           -- 操作人ID
    ChangeReason TEXT,                          -- 变更原因
    ChangeCategory TEXT NOT NULL DEFAULT 'Other', -- 变更分类
    ClientIp TEXT,                              -- 客户端IP
    UserAgent TEXT,                             -- 用户代理
    IsImportant INTEGER NOT NULL DEFAULT 0,     -- 是否重要变更
    Status TEXT NOT NULL DEFAULT 'Success',     -- 变更状态
    ErrorMessage TEXT,                          -- 错误信息
    Metadata TEXT                               -- 额外元数据
);

-- =============================================
-- 创建ShareTokens表（用于代码片段分享功能）
-- =============================================
CREATE TABLE IF NOT EXISTS ShareTokens (
    Id TEXT PRIMARY KEY,                        -- UUID主键
    Token TEXT NOT NULL UNIQUE,                 -- 加密的分享令牌
    SnippetId TEXT NOT NULL,                    -- 关联的代码片段ID
    CreatedBy TEXT NOT NULL,                    -- 创建者用户ID
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),  -- 创建时间 (ISO8601格式)
    ExpiresAt TEXT,                             -- 过期时间
    Permission INTEGER NOT NULL DEFAULT 1,     -- 分享权限 (0:只读, 1:允许复制)
    IsActive INTEGER NOT NULL DEFAULT 1,        -- 是否激活
    ViewCount INTEGER NOT NULL DEFAULT 0,       -- 查看次数
    CopyCount INTEGER NOT NULL DEFAULT 0,       -- 复制次数
    LastAccessedAt TEXT,                        -- 最后访问时间
    UpdatedAt TEXT DEFAULT (datetime('now')),   -- 更新时间
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE
);

-- =============================================
-- 创建ShareAccessLogs表（用于分享访问统计）
-- =============================================
CREATE TABLE IF NOT EXISTS ShareAccessLogs (
    Id TEXT PRIMARY KEY,
    ShareTokenId TEXT NOT NULL,
    AccessTime TEXT NOT NULL,                   -- ISO8601格式
    IPAddress TEXT NOT NULL,
    UserAgent TEXT,
    AccessType INTEGER NOT NULL,                -- 访问类型 (0:查看, 1:复制)
    SessionId TEXT,                             -- 会话ID
    Referrer TEXT,                              -- 来源页面
    
    FOREIGN KEY (ShareTokenId) REFERENCES ShareTokens(Id) ON DELETE CASCADE
);

-- =============================================
-- 创建索引
-- =============================================

-- SystemSettings表索引
CREATE INDEX IF NOT EXISTS idx_system_settings_created_at ON SystemSettings(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_system_settings_updated_at ON SystemSettings(UpdatedAt);
CREATE INDEX IF NOT EXISTS idx_system_settings_updated_by ON SystemSettings(UpdatedBy);

-- SettingsHistory表索引
CREATE INDEX IF NOT EXISTS idx_settings_history_created_at ON SettingsHistory(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_settings_history_setting_type ON SettingsHistory(SettingType);
CREATE INDEX IF NOT EXISTS idx_settings_history_setting_key ON SettingsHistory(SettingKey);
CREATE INDEX IF NOT EXISTS idx_settings_history_changed_by ON SettingsHistory(ChangedBy);
CREATE INDEX IF NOT EXISTS idx_settings_history_change_category ON SettingsHistory(ChangeCategory);
CREATE INDEX IF NOT EXISTS idx_settings_history_is_important ON SettingsHistory(IsImportant);
CREATE INDEX IF NOT EXISTS idx_settings_history_status ON SettingsHistory(Status);
CREATE INDEX IF NOT EXISTS idx_settings_history_client_ip ON SettingsHistory(ClientIp);
CREATE INDEX IF NOT EXISTS idx_settings_history_type_created ON SettingsHistory(SettingType, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_settings_history_category_created ON SettingsHistory(ChangeCategory, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_settings_history_user_created ON SettingsHistory(ChangedBy, CreatedAt);
CREATE INDEX IF NOT EXISTS idx_settings_history_important_created ON SettingsHistory(IsImportant, CreatedAt);

-- ShareTokens表索引
CREATE INDEX IF NOT EXISTS idx_share_tokens_token ON ShareTokens(Token);
CREATE INDEX IF NOT EXISTS idx_share_tokens_snippet ON ShareTokens(SnippetId);
CREATE INDEX IF NOT EXISTS idx_share_tokens_created_by ON ShareTokens(CreatedBy);
CREATE INDEX IF NOT EXISTS idx_share_tokens_expires_at ON ShareTokens(ExpiresAt);
CREATE INDEX IF NOT EXISTS idx_share_tokens_is_active ON ShareTokens(IsActive);
CREATE INDEX IF NOT EXISTS idx_share_tokens_created_at ON ShareTokens(CreatedAt);
CREATE INDEX IF NOT EXISTS idx_share_tokens_permission ON ShareTokens(Permission);
CREATE INDEX IF NOT EXISTS idx_share_tokens_active_expires ON ShareTokens(IsActive, ExpiresAt);
CREATE INDEX IF NOT EXISTS idx_share_tokens_snippet_active ON ShareTokens(SnippetId, IsActive);

-- ShareAccessLogs表索引
CREATE INDEX IF NOT EXISTS idx_share_access_logs_share_token ON ShareAccessLogs(ShareTokenId);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_access_time ON ShareAccessLogs(AccessTime);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_ip_address ON ShareAccessLogs(IPAddress);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_access_type ON ShareAccessLogs(AccessType);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_session_id ON ShareAccessLogs(SessionId);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_token_time ON ShareAccessLogs(ShareTokenId, AccessTime);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_time_type ON ShareAccessLogs(AccessTime, AccessType);
CREATE INDEX IF NOT EXISTS idx_share_access_logs_ip_session ON ShareAccessLogs(IPAddress, SessionId);

-- =============================================
-- 创建视图
-- =============================================

-- 分享统计视图
CREATE VIEW IF NOT EXISTS ShareStatsView AS
SELECT 
    st.Id as ShareTokenId,
    st.Token,
    st.SnippetId,
    cs.Title as SnippetTitle,
    st.CreatedBy,
    u.Username as CreatorUsername,
    st.CreatedAt,
    st.ExpiresAt,
    st.Permission,
    st.IsActive,
    st.ViewCount,
    st.CopyCount,
    st.LastAccessedAt,
    COUNT(DISTINCT sal.SessionId) as UniqueVisitors,
    COUNT(CASE WHEN sal.AccessType = 0 THEN 1 END) as TotalViews,
    COUNT(CASE WHEN sal.AccessType = 1 THEN 1 END) as TotalCopies
FROM ShareTokens st
LEFT JOIN ShareAccessLogs sal ON st.Id = sal.ShareTokenId
LEFT JOIN CodeSnippets cs ON st.SnippetId = cs.Id
LEFT JOIN Users u ON st.CreatedBy = u.Id
GROUP BY st.Id, st.Token, st.SnippetId, cs.Title, st.CreatedBy, u.Username, 
         st.CreatedAt, st.ExpiresAt, st.Permission, st.IsActive, 
         st.ViewCount, st.CopyCount, st.LastAccessedAt;

-- =============================================
-- 创建触发器
-- =============================================

-- 自动更新分享令牌的最后访问时间和计数器
CREATE TRIGGER IF NOT EXISTS update_last_accessed_at
AFTER INSERT ON ShareAccessLogs
FOR EACH ROW
BEGIN
    UPDATE ShareTokens 
    SET LastAccessedAt = NEW.AccessTime,
        ViewCount = ViewCount + CASE WHEN NEW.AccessType = 0 THEN 1 ELSE 0 END,
        CopyCount = CopyCount + CASE WHEN NEW.AccessType = 1 THEN 1 ELSE 0 END
    WHERE Id = NEW.ShareTokenId;
END;

-- =============================================
-- 插入默认系统设置
-- =============================================

-- 生成UUID函数 (SQLite兼容)
CREATE TEMPORARY TABLE IF NOT EXISTS temp_uuid (id TEXT);
INSERT INTO temp_uuid (id) VALUES (lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(6))));

-- 插入默认系统设置记录
INSERT OR REPLACE INTO SystemSettings (Id, CreatedAt, UpdatedAt, UpdatedBy, SiteSettingsJson, SecuritySettingsJson, FeatureSettingsJson, EmailSettingsJson) 
SELECT 
    id,
    datetime('now'),
    datetime('now'),
    'System',
    '{"siteName":"CodeShare","siteDescription":"代码片段分享平台","siteKeywords":"代码,片段,分享,编程","logoUrl":"","faviconUrl":"","footerText":"© 2025 CodeShare","defaultLanguage":"zh-CN","theme":"light","maxUploadSize":10,"maxSnippetLength":50000,"enableComments":true,"enableRatings":true,"enableSharing":true,"enableClipboard":true,"enableSearch":true,"customCss":"","customJs":"","seoTitle":"","seoDescription":"","seoKeywords":""}',
    '{"minPasswordLength":8,"maxPasswordLength":128,"maxLoginAttempts":5,"accountLockoutDuration":15,"sessionTimeout":30,"enableRememberMe":true,"enable2FA":false,"enableSecurityLogging":true,"enablePasswordStrength":true,"requireEmailVerification":false,"allowedLoginAttempts":3,"lockoutDurationMinutes":15,"sessionTimeoutMinutes":30,"passwordMinLength":8,"passwordMaxLength":128,"passwordRequireUppercase":true,"passwordRequireLowercase":true,"passwordRequireNumbers":true,"passwordRequireSpecialChars":true,"enableAccountLockout":true,"maxFailedLoginAttempts":5,"lockoutDurationMinutes":15,"enableSessionTimeout":true,"sessionTimeoutMinutes":30,"enableRememberMe":true,"rememberMeDays":30,"enable2FA":false,"enableSecurityLogging":true,"logFailedLoginAttempts":true,"logSuccessfulLogins":true,"logPasswordChanges":true,"logProfileChanges":true,"enablePasswordStrengthMeter":true,"enablePasswordExpiration":false,"passwordExpirationDays":90,"enableSso":false,"ssoProviders":[],"enableCaptcha":false,"captchaType":"recaptcha","recaptchaSiteKey":"","recaptchaSecretKey":""}',
    '{"enableCodeSnippets":true,"enableSharing":true,"enableClipboard":true,"enableFileUpload":true,"enableSearch":true,"enableApi":true,"enablePublicSnippets":true,"enablePrivateSnippets":true,"enableCategories":true,"enableTags":true,"enableComments":true,"enableRatings":true,"enableFavorites":true,"enableReports":true,"enableAnalytics":true,"enableNotifications":true,"enableEmailNotifications":true,"enablePushNotifications":false,"enableWebhooks":false,"maxSnippetLength":50000,"maxUploadSize":10,"maxFileCount":5,"maxFileSize":5,"allowedFileTypes":[".txt",".json",".xml",".csv",".log",".md"],"enableSyntaxHighlighting":true,"enableLineNumbers":true,"enableCopyButton":true,"enableFullScreen":true,"enableThemeSwitching":true,"enableLanguageDetection":true,"enableAutoSave":true,"autoSaveInterval":30,"enableVersioning":true,"maxVersions":10,"enableExport":true,"exportFormats":["json","xml","csv","txt"],"enableImport":true,"importFormats":["json","xml","csv","txt"],"enableApiRateLimiting":true,"apiRateLimit":100,"apiRateLimitWindow":60,"enableApiAuthentication":true,"apiKeyExpiration":365,"enableWebhooks":false,"webhookEvents":[],"webhookUrl":"","webhookSecret":""}',
    '{"smtpHost":"","smtpPort":587,"smtpUsername":"","smtpPassword":"","enableSsl":true,"enableTls":false,"fromEmail":"noreply@codeshare.com","fromName":"CodeShare","enableEmailNotifications":true,"enableUserRegistrationEmail":true,"enablePasswordResetEmail":true,"enableWelcomeEmail":true,"enableNotificationEmail":true,"enableQueue":true,"queueMaxRetries":3,"queueRetryDelay":5,"enableBcc":false,"bccEmail":"","enableTracking":true,"trackingDomain":"","enableAnalytics":true,"analyticsProvider":"default","testEmail":""}'
FROM temp_uuid LIMIT 1;

-- 清理临时表
DROP TABLE temp_uuid;

-- =============================================
-- 创建SQLite特定的优化索引
-- =============================================

-- 为JSON字段创建虚拟列和索引（SQLite 3.31+支持）
CREATE INDEX IF NOT EXISTS idx_system_settings_json_site ON SystemSettings(SiteSettingsJson);
CREATE INDEX IF NOT EXISTS idx_system_settings_json_security ON SystemSettings(SecuritySettingsJson);
CREATE INDEX IF NOT EXISTS idx_system_settings_json_feature ON SystemSettings(FeatureSettingsJson);
CREATE INDEX IF NOT EXISTS idx_system_settings_json_email ON SystemSettings(EmailSettingsJson);

-- =============================================
-- 验证脚本执行结果
-- =============================================

-- 显示迁移完成信息
SELECT 'CodeShare System Settings Database Migration (SQLite) completed successfully!' as Status;
SELECT datetime('now') as MigrationTime;

-- 显示表创建状态
SELECT 
    'SystemSettings' as TableName,
    COUNT(*) as IsCreated
FROM sqlite_master 
WHERE type='table' AND name='SystemSettings'

UNION ALL

SELECT 
    'SettingsHistory' as TableName,
    COUNT(*) as IsCreated
FROM sqlite_master 
WHERE type='table' AND name='SettingsHistory'

UNION ALL

SELECT 
    'ShareTokens' as TableName,
    COUNT(*) as IsCreated
FROM sqlite_master 
WHERE type='table' AND name='ShareTokens'

UNION ALL

SELECT 
    'ShareAccessLogs' as TableName,
    COUNT(*) as IsCreated
FROM sqlite_master 
WHERE type='table' AND name='ShareAccessLogs';

-- 显示索引创建状态
SELECT 
    tbl_name as TableName,
    COUNT(*) as IndexCount
FROM sqlite_master 
WHERE type='index' 
AND tbl_name IN ('SystemSettings', 'SettingsHistory', 'ShareTokens', 'ShareAccessLogs')
AND name NOT LIKE 'sqlite_%'
GROUP BY tbl_name;

-- 显示视图创建状态
SELECT 
    name as ViewName,
    COUNT(*) as IsCreated
FROM sqlite_master 
WHERE type='view' AND name='ShareStatsView';

-- 显示触发器创建状态
SELECT 
    name as TriggerName,
    tbl_name as TableName
FROM sqlite_master 
WHERE type='trigger' AND name='update_last_accessed_at';

-- 显示默认数据插入状态
SELECT 
    'SystemSettings' as TableName,
    COUNT(*) as RecordCount
FROM SystemSettings;

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
-- 迁移完成
-- =============================================

SELECT 'SQLite database migration completed successfully!' as FinalStatus;
SELECT 'Created/updated tables: SystemSettings, SettingsHistory, ShareTokens, ShareAccessLogs' as Tables;
SELECT 'Created views: ShareStatsView' as Views;
SELECT 'Created triggers: update_last_accessed_at' as Triggers;
SELECT 'Inserted default system settings configuration' as Data;

-- 显示版本信息
SELECT 'CodeShare System Settings v1.0.0' as Version;
SELECT 'SQLite Database Migration' as MigrationType;