-- 代码片段管理工具数据库初始化脚本
-- 遵循设计文档中的数据库结构

-- 创建数据库
CREATE DATABASE IF NOT EXISTS CodeSnippetManager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE CodeSnippetManager;

-- 用户表
CREATE TABLE Users (
    Id CHAR(36) PRIMARY KEY,
    Username VARCHAR(50) UNIQUE NOT NULL,
    Email VARCHAR(100) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Role ENUM('Viewer', 'Editor', 'Admin') NOT NULL DEFAULT 'Viewer',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    
    INDEX idx_username (Username),
    INDEX idx_email (Email),
    INDEX idx_role (Role)
);

-- 代码片段表
CREATE TABLE CodeSnippets (
    Id CHAR(36) PRIMARY KEY,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    Code LONGTEXT NOT NULL,
    Language VARCHAR(50) NOT NULL,
    CreatedBy CHAR(36) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsPublic BOOLEAN DEFAULT FALSE,
    ViewCount INT DEFAULT 0,
    CopyCount INT DEFAULT 0,
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE,
    INDEX idx_language (Language),
    INDEX idx_created_by (CreatedBy),
    INDEX idx_created_at (CreatedAt),
    INDEX idx_is_public (IsPublic),
    FULLTEXT INDEX idx_title_description (Title, Description)
);

-- 代码片段版本表
CREATE TABLE SnippetVersions (
    Id CHAR(36) PRIMARY KEY,
    SnippetId CHAR(36) NOT NULL,
    VersionNumber INT NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Description TEXT,
    Code LONGTEXT NOT NULL,
    Language VARCHAR(50) NOT NULL,
    CreatedBy CHAR(36) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ChangeDescription TEXT,
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    UNIQUE KEY unique_snippet_version (SnippetId, VersionNumber),
    INDEX idx_snippet_id (SnippetId),
    INDEX idx_created_at (CreatedAt)
);

-- 标签表
CREATE TABLE Tags (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(50) UNIQUE NOT NULL,
    Color VARCHAR(7) DEFAULT '#007bff',
    CreatedBy CHAR(36) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
    INDEX idx_name (Name)
);

-- 代码片段标签关联表
CREATE TABLE SnippetTags (
    SnippetId CHAR(36) NOT NULL,
    TagId CHAR(36) NOT NULL,
    
    PRIMARY KEY (SnippetId, TagId),
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
);

-- 剪贴板历史表
CREATE TABLE ClipboardHistory (
    Id CHAR(36) PRIMARY KEY,
    UserId CHAR(36) NOT NULL,
    SnippetId CHAR(36) NOT NULL,
    CopiedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    INDEX idx_user_copied_at (UserId, CopiedAt),
    INDEX idx_snippet_id (SnippetId)
);

-- 插入默认管理员用户
-- 密码: admin123 (BCrypt 哈希)
INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt, UpdatedAt, IsActive) VALUES 
(UUID(), 'admin', 'admin@example.com', '$2a$11$rQZrXkOKGKUZ8N8KGsKGKOYrQZrXkOKGKUZ8N8KGsKGKOYrQZrXkO', 'Admin', NOW(), NOW(), TRUE);

-- 插入示例标签
INSERT INTO Tags (Id, Name, Color, CreatedBy, CreatedAt) VALUES 
(UUID(), 'JavaScript', '#f7df1e', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), 'Python', '#3776ab', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), 'C#', '#239120', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), 'HTML', '#e34f26', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), 'CSS', '#1572b6', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), '工具函数', '#6c757d', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), '算法', '#dc3545', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), '前端', '#007bff', (SELECT Id FROM Users WHERE Username = 'admin'), NOW()),
(UUID(), '后端', '#28a745', (SELECT Id FROM Users WHERE Username = 'admin'), NOW());

-- 插入示例代码片段
SET @admin_id = (SELECT Id FROM Users WHERE Username = 'admin');
SET @js_tag_id = (SELECT Id FROM Tags WHERE Name = 'JavaScript');
SET @util_tag_id = (SELECT Id FROM Tags WHERE Name = '工具函数');
SET @frontend_tag_id = (SELECT Id FROM Tags WHERE Name = '前端');

INSERT INTO CodeSnippets (Id, Title, Description, Code, Language, CreatedBy, CreatedAt, UpdatedAt, IsPublic, ViewCount, CopyCount) VALUES 
(UUID(), '防抖函数', '防止函数频繁调用的工具函数', 'function debounce(func, wait) {\n  let timeout;\n  return function executedFunction(...args) {\n    const later = () => {\n      clearTimeout(timeout);\n      func(...args);\n    };\n    clearTimeout(timeout);\n    timeout = setTimeout(later, wait);\n  };\n}', 'javascript', @admin_id, NOW(), NOW(), TRUE, 0, 0);

SET @snippet_id = LAST_INSERT_ID();

-- 关联标签
INSERT INTO SnippetTags (SnippetId, TagId) VALUES 
((SELECT Id FROM CodeSnippets WHERE Title = '防抖函数'), @js_tag_id),
((SELECT Id FROM CodeSnippets WHERE Title = '防抖函数'), @util_tag_id),
((SELECT Id FROM CodeSnippets WHERE Title = '防抖函数'), @frontend_tag_id);

-- 创建初始版本
INSERT INTO SnippetVersions (Id, SnippetId, VersionNumber, Title, Description, Code, Language, CreatedBy, CreatedAt, ChangeDescription) VALUES 
(UUID(), (SELECT Id FROM CodeSnippets WHERE Title = '防抖函数'), 1, '防抖函数', '防止函数频繁调用的工具函数', 'function debounce(func, wait) {\n  let timeout;\n  return function executedFunction(...args) {\n    const later = () => {\n      clearTimeout(timeout);\n      func(...args);\n    };\n    clearTimeout(timeout);\n    timeout = setTimeout(later, wait);\n  };\n}', 'javascript', @admin_id, NOW(), '初始版本');

-- =============================================
-- 分享功能表结构
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
-- 分享功能示例数据
-- =============================================

-- 插入示例分享令牌（用于测试）
SET @snippet_id = (SELECT Id FROM CodeSnippets WHERE Title = '防抖函数');

INSERT INTO ShareTokens (Id, Token, SnippetId, CreatedBy, CreatedAt, ExpiresAt, Permission, IsActive, ViewCount, CopyCount) VALUES 
(UUID(), 'demo_share_token_123', @snippet_id, @admin_id, NOW(), DATE_ADD(NOW(), INTERVAL 7 DAY), 1, 1, 0, 0),
(UUID(), 'demo_share_token_456', @snippet_id, @admin_id, NOW(), DATE_ADD(NOW(), INTERVAL 30 DAY), 0, 1, 0, 0),
(UUID(), 'demo_share_token_789', @snippet_id, @admin_id, NOW(), NULL, 1, 1, 0, 0); -- 永不过期

-- 插入示例访问日志
SET @share_token_1 = (SELECT Id FROM ShareTokens WHERE Token = 'demo_share_token_123');
SET @share_token_2 = (SELECT Id FROM ShareTokens WHERE Token = 'demo_share_token_456');

INSERT INTO ShareAccessLogs (Id, ShareTokenId, AccessTime, IPAddress, UserAgent, AccessType, SessionId, Referrer) VALUES 
(UUID(), @share_token_1, DATE_SUB(NOW(), INTERVAL 1 DAY), '192.168.1.100', 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36', 0, 'session_abc_123', 'https://google.com'),
(UUID(), @share_token_1, DATE_SUB(NOW(), INTERVAL 12 HOUR), '192.168.1.101', 'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36', 1, 'session_def_456', 'https://github.com'),
(UUID(), @share_token_2, DATE_SUB(NOW(), INTERVAL 6 HOUR), '192.168.1.102', 'Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36', 0, 'session_ghi_789', 'https://stackoverflow.com'),
(UUID(), @share_token_2, DATE_SUB(NOW(), INTERVAL 3 HOUR), '192.168.1.103', 'Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/605.1.15', 0, 'session_jkl_012', 'https://twitter.com');

-- 显示创建结果
SELECT 'Database initialization completed successfully!' as Status;
SELECT COUNT(*) as UserCount FROM Users;
SELECT COUNT(*) as TagCount FROM Tags;
SELECT COUNT(*) as SnippetCount FROM CodeSnippets;
SELECT COUNT(*) as VersionCount FROM SnippetVersions;
SELECT COUNT(*) as ShareTokenCount FROM ShareTokens;
SELECT COUNT(*) as ShareAccessLogCount FROM ShareAccessLogs;

-- 显示分享功能统计
SELECT 'Share functionality initialized successfully!' as ShareStatus;
SELECT 
    COUNT(*) as TotalShareTokens,
    SUM(CASE WHEN IsActive = 1 THEN 1 ELSE 0 END) as ActiveShareTokens,
    SUM(CASE WHEN ExpiresAt IS NOT NULL AND ExpiresAt > NOW() THEN 1 ELSE 0 END) as NonExpiredTokens,
    SUM(ViewCount) as TotalViews,
    SUM(CopyCount) as TotalCopies
FROM ShareTokens;