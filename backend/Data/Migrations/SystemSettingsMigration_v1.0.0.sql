-- CodeShare 系统设置功能数据库迁移脚本
-- 版本: v1.0.0
-- 创建日期: 2025-01-01
-- 描述: 为系统设置功能添加必要的数据表和初始化数据
-- 支持MySQL和SQLite两种数据库语法

-- =============================================
-- 执行前检查
-- =============================================

-- 检查是否已经存在SystemSettings表
SET @table_exists = 0;
SELECT COUNT(*) INTO @table_exists 
FROM information_schema.tables 
WHERE table_schema = DATABASE() AND table_name = 'SystemSettings';

-- 如果表已存在，显示警告并跳过创建
SET @message = IF(@table_exists > 0, 
    'WARNING: SystemSettings table already exists. Skipping table creation.', 
    'Creating SystemSettings table...');
SELECT @message as Status;

-- =============================================
-- 创建SystemSettings表
-- =============================================

-- 仅在表不存在时创建
CREATE TABLE IF NOT EXISTS SystemSettings (
    Id CHAR(36) PRIMARY KEY,                    -- UUID主键
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

-- =============================================
-- 创建SettingsHistory表
-- =============================================

CREATE TABLE IF NOT EXISTS SettingsHistory (
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
-- 创建ShareTokens表（用于代码片段分享功能）
-- =============================================

CREATE TABLE IF NOT EXISTS ShareTokens (
    Id CHAR(36) PRIMARY KEY,                    -- UUID主键
    Token VARCHAR(64) NOT NULL UNIQUE,          -- 加密的分享令牌
    SnippetId CHAR(36) NOT NULL,                -- 关联的代码片段ID
    CreatedBy CHAR(36) NOT NULL,                -- 创建者用户ID
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    ExpiresAt DATETIME NULL,                    -- 过期时间
    Permission TINYINT NOT NULL DEFAULT 1,     -- 分享权限 (0:只读, 1:允许复制)
    IsActive BOOLEAN NOT NULL DEFAULT 1,        -- 是否激活
    ViewCount INT NOT NULL DEFAULT 0,           -- 查看次数
    CopyCount INT NOT NULL DEFAULT 0,           -- 复制次数
    LastAccessedAt DATETIME NULL,               -- 最后访问时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_share_tokens_token (Token),
    INDEX idx_share_tokens_snippet (SnippetId),
    INDEX idx_share_tokens_created_by (CreatedBy),
    INDEX idx_share_tokens_expires_at (ExpiresAt),
    INDEX idx_share_tokens_is_active (IsActive),
    INDEX idx_share_tokens_created_at (CreatedAt),
    INDEX idx_share_tokens_permission (Permission),
    
    -- 复合索引
    INDEX idx_share_tokens_active_expires (IsActive, ExpiresAt),
    INDEX idx_share_tokens_snippet_active (SnippetId, IsActive)
);

-- =============================================
-- 创建ShareAccessLogs表（用于分享访问统计）
-- =============================================

CREATE TABLE IF NOT EXISTS ShareAccessLogs (
    Id CHAR(36) PRIMARY KEY,
    ShareTokenId CHAR(36) NOT NULL,
    AccessTime DATETIME NOT NULL,
    IPAddress VARCHAR(45) NOT NULL,
    UserAgent TEXT,
    AccessType TINYINT NOT NULL,               -- 访问类型 (0:查看, 1:复制)
    SessionId VARCHAR(64),                      -- 会话ID，用于统计唯一访客
    Referrer VARCHAR(512),                      -- 来源页面
    
    FOREIGN KEY (ShareTokenId) REFERENCES ShareTokens(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_share_access_logs_share_token (ShareTokenId),
    INDEX idx_share_access_logs_access_time (AccessTime),
    INDEX idx_share_access_logs_ip_address (IPAddress),
    INDEX idx_share_access_logs_access_type (AccessType),
    INDEX idx_share_access_logs_session_id (SessionId),
    
    -- 复合索引
    INDEX idx_share_access_logs_token_time (ShareTokenId, AccessTime),
    INDEX idx_share_access_logs_time_type (AccessTime, AccessType),
    INDEX idx_share_access_logs_ip_session (IPAddress, SessionId)
);

-- =============================================
-- 创建视图
-- =============================================

-- 分享统计视图
CREATE OR REPLACE VIEW ShareStatsView AS
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
DELIMITER //
CREATE TRIGGER IF NOT EXISTS update_last_accessed_at
AFTER INSERT ON ShareAccessLogs
FOR EACH ROW
BEGIN
    UPDATE ShareTokens 
    SET LastAccessedAt = NEW.AccessTime,
        ViewCount = ViewCount + CASE WHEN NEW.AccessType = 0 THEN 1 ELSE 0 END,
        CopyCount = CopyCount + CASE WHEN NEW.AccessType = 1 THEN 1 ELSE 0 END
    WHERE Id = NEW.ShareTokenId;
END//
DELIMITER ;

-- =============================================
-- 创建存储过程
-- =============================================

-- 清理过期的分享令牌
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

-- 获取分享统计信息
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

-- =============================================
-- 插入默认系统设置
-- =============================================

-- 插入默认系统设置记录
INSERT INTO SystemSettings (Id, CreatedAt, UpdatedAt, UpdatedBy, SiteSettingsJson, SecuritySettingsJson, FeatureSettingsJson, EmailSettingsJson) 
VALUES (
    UUID(), 
    NOW(), 
    NOW(), 
    'System', 
    '{"siteName":"CodeShare","siteDescription":"代码片段分享平台","siteKeywords":"代码,片段,分享,编程","logoUrl":"","faviconUrl":"","footerText":"© 2025 CodeShare","defaultLanguage":"zh-CN","theme":"light","maxUploadSize":10,"maxSnippetLength":50000,"enableComments":true,"enableRatings":true,"enableSharing":true,"enableClipboard":true,"enableSearch":true,"customCss":"","customJs":"","seoTitle":"","seoDescription":"","seoKeywords":""}',
    '{"minPasswordLength":8,"maxPasswordLength":128,"maxLoginAttempts":5,"accountLockoutDuration":15,"sessionTimeout":30,"enableRememberMe":true,"enable2FA":false,"enableSecurityLogging":true,"enablePasswordStrength":true,"requireEmailVerification":false,"allowedLoginAttempts":3,"lockoutDurationMinutes":15,"sessionTimeoutMinutes":30,"passwordMinLength":8,"passwordMaxLength":128,"passwordRequireUppercase":true,"passwordRequireLowercase":true,"passwordRequireNumbers":true,"passwordRequireSpecialChars":true,"enableAccountLockout":true,"maxFailedLoginAttempts":5,"lockoutDurationMinutes":15,"enableSessionTimeout":true,"sessionTimeoutMinutes":30,"enableRememberMe":true,"rememberMeDays":30,"enable2FA":false,"enableSecurityLogging":true,"logFailedLoginAttempts":true,"logSuccessfulLogins":true,"logPasswordChanges":true,"logProfileChanges":true,"enablePasswordStrengthMeter":true,"enablePasswordExpiration":false,"passwordExpirationDays":90,"enableSso":false,"ssoProviders":[],"enableCaptcha":false,"captchaType":"recaptcha","recaptchaSiteKey":"","recaptchaSecretKey":""}',
    '{"enableCodeSnippets":true,"enableSharing":true,"enableClipboard":true,"enableFileUpload":true,"enableSearch":true,"enableApi":true,"enablePublicSnippets":true,"enablePrivateSnippets":true,"enableCategories":true,"enableTags":true,"enableComments":true,"enableRatings":true,"enableFavorites":true,"enableReports":true,"enableAnalytics":true,"enableNotifications":true,"enableEmailNotifications":true,"enablePushNotifications":false,"enableWebhooks":false,"maxSnippetLength":50000,"maxUploadSize":10,"maxFileCount":5,"maxFileSize":5,"allowedFileTypes":[".txt",".json",".xml",".csv",".log",".md"],"enableSyntaxHighlighting":true,"enableLineNumbers":true,"enableCopyButton":true,"enableFullScreen":true,"enableThemeSwitching":true,"enableLanguageDetection":true,"enableAutoSave":true,"autoSaveInterval":30,"enableVersioning":true,"maxVersions":10,"enableExport":true,"exportFormats":["json","xml","csv","txt"],"enableImport":true,"importFormats":["json","xml","csv","txt"],"enableApiRateLimiting":true,"apiRateLimit":100,"apiRateLimitWindow":60,"enableApiAuthentication":true,"apiKeyExpiration":365,"enableWebhooks":false,"webhookEvents":[],"webhookUrl":"","webhookSecret":""}',
    '{"smtpHost":"","smtpPort":587,"smtpUsername":"","smtpPassword":"","enableSsl":true,"enableTls":false,"fromEmail":"noreply@codeshare.com","fromName":"CodeShare","enableEmailNotifications":true,"enableUserRegistrationEmail":true,"enablePasswordResetEmail":true,"enableWelcomeEmail":true,"enableNotificationEmail":true,"enableQueue":true,"queueMaxRetries":3,"queueRetryDelay":5,"enableBcc":false,"bccEmail":"","enableTracking":true,"trackingDomain":"","enableAnalytics":true,"analyticsProvider":"default","testEmail":""}'
) 
ON DUPLICATE KEY UPDATE 
    UpdatedAt = NOW(),
    UpdatedBy = 'Migration';

-- =============================================
-- 创建索引优化
-- =============================================

-- 为SystemSettings表添加额外的索引
CREATE INDEX IF NOT EXISTS idx_system_settings_json_site ON SystemSettings((CAST(SiteSettingsJson AS CHAR(255))));
CREATE INDEX IF NOT EXISTS idx_system_settings_json_security ON SystemSettings((CAST(SecuritySettingsJson AS CHAR(255))));
CREATE INDEX IF NOT EXISTS idx_system_settings_json_feature ON SystemSettings((CAST(FeatureSettingsJson AS CHAR(255))));
CREATE INDEX IF NOT EXISTS idx_system_settings_json_email ON SystemSettings((CAST(EmailSettingsJson AS CHAR(255))));

-- =============================================
-- 验证脚本执行结果
-- =============================================

-- 显示迁移完成信息
SELECT 'CodeShare System Settings Database Migration completed successfully!' as Status;
SELECT NOW() as MigrationTime;

-- 显示表创建状态
SELECT 
    'SystemSettings' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'SystemSettings') as IsCreated
UNION ALL
SELECT 
    'SettingsHistory' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'SettingsHistory') as IsCreated
UNION ALL
SELECT 
    'ShareTokens' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ShareTokens') as IsCreated
UNION ALL
SELECT 
    'ShareAccessLogs' as TableName,
    (SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ShareAccessLogs') as IsCreated;

-- 显示索引创建状态
SELECT 
    TABLE_NAME as TableName,
    COUNT(*) as IndexCount
FROM information_schema.statistics 
WHERE TABLE_SCHEMA = DATABASE() 
AND TABLE_NAME IN ('SystemSettings', 'SettingsHistory', 'ShareTokens', 'ShareAccessLogs')
GROUP BY TABLE_NAME;

-- 显示视图创建状态
SELECT 
    'ShareStatsView' as ViewName,
    (SELECT COUNT(*) FROM information_schema.views WHERE table_schema = DATABASE() AND table_name = 'ShareStatsView') as IsCreated;

-- 显示存储过程创建状态
SELECT 
    routine_name as ProcedureName,
    routine_type as Type
FROM information_schema.routines 
WHERE routine_schema = DATABASE() 
AND routine_name IN ('CleanupExpiredShareTokens', 'GetShareStats');

-- 显示触发器创建状态
SELECT 
    trigger_name as TriggerName,
    event_object_table as TableName
FROM information_schema.triggers 
WHERE trigger_schema = DATABASE() 
AND trigger_name = 'update_last_accessed_at';

-- 显示默认数据插入状态
SELECT 
    'SystemSettings' as TableName,
    COUNT(*) as RecordCount
FROM SystemSettings;

-- =============================================
-- 迁移完成
-- =============================================

SELECT 'Database migration completed successfully!' as FinalStatus;
SELECT CONCAT('Created/updated tables: SystemSettings, SettingsHistory, ShareTokens, ShareAccessLogs') as Tables;
SELECT CONCAT('Created views: ShareStatsView') as Views;
SELECT CONCAT('Created stored procedures: CleanupExpiredShareTokens, GetShareStats') as Procedures;
SELECT CONCAT('Created triggers: update_last_accessed_at') as Triggers;
SELECT CONCAT('Inserted default system settings configuration') as Data;