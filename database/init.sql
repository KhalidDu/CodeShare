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

-- 显示创建结果
SELECT 'Database initialization completed successfully!' as Status;
SELECT COUNT(*) as UserCount FROM Users;
SELECT COUNT(*) as TagCount FROM Tags;
SELECT COUNT(*) as SnippetCount FROM CodeSnippets;
SELECT COUNT(*) as VersionCount FROM SnippetVersions;