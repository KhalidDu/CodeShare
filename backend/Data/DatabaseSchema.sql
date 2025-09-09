-- Code Sharing功能数据库迁移脚本
-- 为代码片段分享功能添加必要的数据表
-- 支持MySQL和SQLite两种数据库语法

-- =============================================
-- MySQL 版本
-- =============================================

-- 创建ShareTokens表 - 存储分享令牌和权限设置
CREATE TABLE ShareTokens (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
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
    INDEX idx_token (Token),
    INDEX idx_snippet (SnippetId),
    INDEX idx_created_by (CreatedBy),
    INDEX idx_expires_at (ExpiresAt),
    INDEX idx_is_active (IsActive),
    INDEX idx_created_at (CreatedAt),
    INDEX idx_permission (Permission),
    
    -- 复合索引
    INDEX idx_active_expires (IsActive, ExpiresAt),
    INDEX idx_snippet_active (SnippetId, IsActive)
);

-- 创建ShareAccessLogs表 - 记录访问统计
CREATE TABLE ShareAccessLogs (
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
    INDEX idx_share_token (ShareTokenId),
    INDEX idx_access_time (AccessTime),
    INDEX idx_ip_address (IPAddress),
    INDEX idx_access_type (AccessType),
    INDEX idx_session_id (SessionId),
    
    -- 复合索引
    INDEX idx_token_time (ShareTokenId, AccessTime),
    INDEX idx_time_type (AccessTime, AccessType),
    INDEX idx_ip_session (IPAddress, SessionId)
);

-- 创建视图：分享统计视图
CREATE VIEW ShareStatsView AS
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

-- 创建触发器：自动更新分享令牌的最后访问时间
DELIMITER //
CREATE TRIGGER update_last_accessed_at
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

-- 创建存储过程：清理过期的分享令牌
DELIMITER //
CREATE PROCEDURE CleanupExpiredShareTokens()
BEGIN
    -- 将过期的分享令牌标记为非活跃
    UPDATE ShareTokens 
    SET IsActive = 0 
    WHERE ExpiresAt IS NOT NULL 
    AND ExpiresAt < NOW() 
    AND IsActive = 1;
    
    -- 返回影响的行数
    SELECT ROW_COUNT() as ExpiredTokensCount;
END//
DELIMITER ;

-- 创建存储过程：获取分享统计信息
DELIMITER //
CREATE PROCEDURE GetShareStats(IN pShareTokenId CHAR(36))
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
-- SQLite 版本
-- =============================================

-- SQLite语法适配（用于开发环境）
/*
-- 创建ShareTokens表 - SQLite版本
CREATE TABLE ShareTokens (
    Id TEXT PRIMARY KEY,                        -- UUID
    Token TEXT NOT NULL UNIQUE,                 -- 加密的分享令牌
    SnippetId TEXT NOT NULL,                    -- 关联的代码片段ID
    CreatedBy TEXT NOT NULL,                    -- 创建者用户ID
    CreatedAt TEXT NOT NULL,                    -- 创建时间 (ISO8601格式)
    ExpiresAt TEXT,                             -- 过期时间
    Permission INTEGER NOT NULL DEFAULT 1,     -- 分享权限 (0:只读, 1:允许复制)
    IsActive INTEGER NOT NULL DEFAULT 1,        -- 是否激活
    ViewCount INTEGER NOT NULL DEFAULT 0,       -- 查看次数
    CopyCount INTEGER NOT NULL DEFAULT 0,       -- 复制次数
    LastAccessedAt TEXT,                        -- 最后访问时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,   -- 更新时间
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE
);

-- 创建ShareAccessLogs表 - SQLite版本
CREATE TABLE ShareAccessLogs (
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

-- 创建索引 - SQLite版本
CREATE INDEX idx_share_tokens_token ON ShareTokens(Token);
CREATE INDEX idx_share_tokens_snippet ON ShareTokens(SnippetId);
CREATE INDEX idx_share_tokens_created_by ON ShareTokens(CreatedBy);
CREATE INDEX idx_share_tokens_expires_at ON ShareTokens(ExpiresAt);
CREATE INDEX idx_share_tokens_is_active ON ShareTokens(IsActive);
CREATE INDEX idx_share_tokens_created_at ON ShareTokens(CreatedAt);
CREATE INDEX idx_share_tokens_permission ON ShareTokens(Permission);
CREATE INDEX idx_share_tokens_active_expires ON ShareTokens(IsActive, ExpiresAt);
CREATE INDEX idx_share_tokens_snippet_active ON ShareTokens(SnippetId, IsActive);

CREATE INDEX idx_share_access_logs_share_token ON ShareAccessLogs(ShareTokenId);
CREATE INDEX idx_share_access_logs_access_time ON ShareAccessLogs(AccessTime);
CREATE INDEX idx_share_access_logs_ip_address ON ShareAccessLogs(IPAddress);
CREATE INDEX idx_share_access_logs_access_type ON ShareAccessLogs(AccessType);
CREATE INDEX idx_share_access_logs_session_id ON ShareAccessLogs(SessionId);
CREATE INDEX idx_share_access_logs_token_time ON ShareAccessLogs(ShareTokenId, AccessTime);
CREATE INDEX idx_share_access_logs_time_type ON ShareAccessLogs(AccessTime, AccessType);
CREATE INDEX idx_share_access_logs_ip_session ON ShareAccessLogs(IPAddress, SessionId);

-- 创建触发器：SQLite版本
CREATE TRIGGER update_last_accessed_at
AFTER INSERT ON ShareAccessLogs
FOR EACH ROW
BEGIN
    UPDATE ShareTokens 
    SET LastAccessedAt = NEW.AccessTime,
        ViewCount = ViewCount + CASE WHEN NEW.AccessType = 0 THEN 1 ELSE 0 END,
        CopyCount = CopyCount + CASE WHEN NEW.AccessType = 1 THEN 1 ELSE 0 END
    WHERE Id = NEW.ShareTokenId;
END;
*/

-- =============================================
-- 初始化数据（可选）
-- =============================================

-- 插入示例分享令牌（仅用于测试环境）
/*
INSERT INTO ShareTokens (Id, Token, SnippetId, CreatedBy, CreatedAt, ExpiresAt, Permission, IsActive) VALUES 
(UUID(), 'test_token_1', (SELECT Id FROM CodeSnippets WHERE Title = '防抖函数' LIMIT 1), (SELECT Id FROM Users WHERE Username = 'admin' LIMIT 1), NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), 1, 1),
(UUID(), 'test_token_2', (SELECT Id FROM CodeSnippets WHERE Title = '防抖函数' LIMIT 1), (SELECT Id FROM Users WHERE Username = 'admin' LIMIT 1), NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 0, 1);
*/

-- =============================================
-- 验证脚本执行结果
-- =============================================

-- 显示创建结果
SELECT 'Share tokens database schema migration completed successfully!' as Status;
SELECT COUNT(*) as ShareTokensTableCount FROM information_schema.tables WHERE table_name = 'ShareTokens';
SELECT COUNT(*) as ShareAccessLogsTableCount FROM information_schema.tables WHERE table_name = 'ShareAccessLogs';
SELECT COUNT(*) as ViewsCount FROM information_schema.views WHERE table_name = 'ShareStatsView';
SELECT COUNT(*) as TriggersCount FROM information_schema.triggers WHERE trigger_name = 'update_last_accessed_at';
SELECT COUNT(*) as ProceduresCount FROM information_schema.routines WHERE routine_name IN ('CleanupExpiredShareTokens', 'GetShareStats');

-- 显示表结构信息
SELECT 
    TABLE_NAME as TableName,
    COLUMN_NAME as ColumnName,
    DATA_TYPE as DataType,
    IS_NULLABLE as IsNullable,
    COLUMN_DEFAULT as DefaultValue,
    COLUMN_COMMENT as Comment
FROM information_schema.columns 
WHERE TABLE_NAME IN ('ShareTokens', 'ShareAccessLogs')
ORDER BY TABLE_NAME, ORDINAL_POSITION;