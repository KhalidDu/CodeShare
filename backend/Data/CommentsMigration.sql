-- 评论功能数据库迁移脚本
-- 为评论功能添加必要的数据表
-- 支持MySQL和SQLite两种数据库语法

-- =============================================
-- MySQL 版本
-- =============================================

-- 创建Comments表 - 存储评论信息
CREATE TABLE Comments (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    Content TEXT NOT NULL,                      -- 评论内容
    SnippetId CHAR(36) NOT NULL,                -- 关联的代码片段ID
    UserId CHAR(36) NOT NULL,                   -- 评论者用户ID
    ParentId CHAR(36) NULL,                     -- 父评论ID（用于回复）
    ParentPath VARCHAR(1000) NULL,              -- 父评论路径（用于层级查询）
    Depth INT NOT NULL DEFAULT 0,               -- 评论深度
    LikeCount INT NOT NULL DEFAULT 0,           -- 点赞数量
    ReplyCount INT NOT NULL DEFAULT 0,          -- 回复数量
    Status TINYINT NOT NULL DEFAULT 0,          -- 评论状态 (0:正常, 1:已删除, 2:已隐藏, 3:待审核)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,  -- 更新时间
    DeletedAt DATETIME NULL,                     -- 删除时间（软删除）
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    
    -- 索引优化
    INDEX idx_snippet_id (SnippetId),
    INDEX idx_user_id (UserId),
    INDEX idx_parent_id (ParentId),
    INDEX idx_parent_path (ParentPath(255)),
    INDEX idx_depth (Depth),
    INDEX idx_status (Status),
    INDEX idx_created_at (CreatedAt),
    INDEX idx_updated_at (UpdatedAt),
    INDEX idx_deleted_at (DeletedAt),
    INDEX idx_like_count (LikeCount),
    INDEX idx_reply_count (ReplyCount),
    
    -- 复合索引
    INDEX idx_snippet_status (SnippetId, Status),
    INDEX idx_snippet_created (SnippetId, CreatedAt),
    INDEX idx_parent_path_created (ParentPath(255), CreatedAt),
    INDEX idx_user_created (UserId, CreatedAt),
    INDEX idx_status_created (Status, CreatedAt)
);

-- 创建CommentLikes表 - 存储评论点赞记录
CREATE TABLE CommentLikes (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CommentId CHAR(36) NOT NULL,                -- 评论ID
    UserId CHAR(36) NOT NULL,                   -- 用户ID
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    -- 唯一约束：一个用户只能给同一个评论点赞一次
    UNIQUE KEY uk_comment_user (CommentId, UserId),
    
    -- 索引优化
    INDEX idx_comment_id (CommentId),
    INDEX idx_user_id (UserId),
    INDEX idx_created_at (CreatedAt),
    
    -- 复合索引
    INDEX idx_comment_created (CommentId, CreatedAt),
    INDEX idx_user_created (UserId, CreatedAt)
);

-- 创建CommentReports表 - 存储评论举报记录
CREATE TABLE CommentReports (
    Id CHAR(36) PRIMARY KEY,                    -- UUID
    CommentId CHAR(36) NOT NULL,                -- 评论ID
    UserId CHAR(36) NOT NULL,                   -- 举报者用户ID
    Reason TINYINT NOT NULL,                     -- 举报原因
    Description TEXT NULL,                       -- 举报描述
    Status TINYINT NOT NULL DEFAULT 0,          -- 举报状态 (0:待处理, 1:已处理, 2:已驳回, 3:调查中)
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    HandledAt DATETIME NULL,                     -- 处理时间
    HandledBy CHAR(36) NULL,                    -- 处理者用户ID
    Resolution TEXT NULL,                        -- 处理结果说明
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (HandledBy) REFERENCES Users(Id) ON DELETE SET NULL,
    
    -- 索引优化
    INDEX idx_comment_id (CommentId),
    INDEX idx_user_id (UserId),
    INDEX idx_reason (Reason),
    INDEX idx_status (Status),
    INDEX idx_created_at (CreatedAt),
    INDEX idx_handled_at (HandledAt),
    INDEX idx_handled_by (HandledBy),
    
    -- 复合索引
    INDEX idx_comment_status (CommentId, Status),
    INDEX idx_status_created (Status, CreatedAt),
    INDEX idx_reason_status (Reason, Status),
    INDEX idx_user_created (UserId, CreatedAt),
    INDEX idx_handled_created (HandledBy, HandledAt)
);

-- =============================================
-- SQLite 版本
-- =============================================

-- 创建Comments表 - SQLite版本
CREATE TABLE Comments (
    Id TEXT PRIMARY KEY,                         -- UUID
    Content TEXT NOT NULL,                       -- 评论内容
    SnippetId TEXT NOT NULL,                     -- 关联的代码片段ID
    UserId TEXT NOT NULL,                        -- 评论者用户ID
    ParentId TEXT NULL,                          -- 父评论ID（用于回复）
    ParentPath TEXT NULL,                        -- 父评论路径（用于层级查询）
    Depth INTEGER NOT NULL DEFAULT 0,            -- 评论深度
    LikeCount INTEGER NOT NULL DEFAULT 0,        -- 点赞数量
    ReplyCount INTEGER NOT NULL DEFAULT 0,       -- 回复数量
    Status INTEGER NOT NULL DEFAULT 0,           -- 评论状态 (0:正常, 1:已删除, 2:已隐藏, 3:待审核)
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP,    -- 更新时间
    DeletedAt TEXT NULL,                         -- 删除时间（软删除）
    
    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (ParentId) REFERENCES Comments(Id) ON DELETE CASCADE
);

-- 创建CommentLikes表 - SQLite版本
CREATE TABLE CommentLikes (
    Id TEXT PRIMARY KEY,                         -- UUID
    CommentId TEXT NOT NULL,                     -- 评论ID
    UserId TEXT NOT NULL,                        -- 用户ID
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    
    UNIQUE (CommentId, UserId)  -- 一个用户只能给同一个评论点赞一次
);

-- 创建CommentReports表 - SQLite版本
CREATE TABLE CommentReports (
    Id TEXT PRIMARY KEY,                         -- UUID
    CommentId TEXT NOT NULL,                     -- 评论ID
    UserId TEXT NOT NULL,                        -- 举报者用户ID
    Reason INTEGER NOT NULL,                     -- 举报原因
    Description TEXT NULL,                       -- 举报描述
    Status INTEGER NOT NULL DEFAULT 0,           -- 举报状态 (0:待处理, 1:已处理, 2:已驳回, 3:调查中)
    CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,  -- 创建时间
    HandledAt TEXT NULL,                         -- 处理时间
    HandledBy TEXT NULL,                         -- 处理者用户ID
    Resolution TEXT NULL,                        -- 处理结果说明
    
    FOREIGN KEY (CommentId) REFERENCES Comments(Id) ON DELETE CASCADE,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (HandledBy) REFERENCES Users(Id) ON DELETE SET NULL
);

-- =============================================
-- 索引创建 - SQLite版本
-- =============================================

-- Comments表索引
CREATE INDEX idx_comments_snippet_id ON Comments(SnippetId);
CREATE INDEX idx_comments_user_id ON Comments(UserId);
CREATE INDEX idx_comments_parent_id ON Comments(ParentId);
CREATE INDEX idx_comments_parent_path ON Comments(ParentPath);
CREATE INDEX idx_comments_depth ON Comments(Depth);
CREATE INDEX idx_comments_status ON Comments(Status);
CREATE INDEX idx_comments_created_at ON Comments(CreatedAt);
CREATE INDEX idx_comments_updated_at ON Comments(UpdatedAt);
CREATE INDEX idx_comments_deleted_at ON Comments(DeletedAt);
CREATE INDEX idx_comments_like_count ON Comments(LikeCount);
CREATE INDEX idx_comments_reply_count ON Comments(ReplyCount);

-- 复合索引
CREATE INDEX idx_comments_snippet_status ON Comments(SnippetId, Status);
CREATE INDEX idx_comments_snippet_created ON Comments(SnippetId, CreatedAt);
CREATE INDEX idx_comments_parent_path_created ON Comments(ParentPath, CreatedAt);
CREATE INDEX idx_comments_user_created ON Comments(UserId, CreatedAt);
CREATE INDEX idx_comments_status_created ON Comments(Status, CreatedAt);

-- CommentLikes表索引
CREATE INDEX idx_comment_likes_comment_id ON CommentLikes(CommentId);
CREATE INDEX idx_comment_likes_user_id ON CommentLikes(UserId);
CREATE INDEX idx_comment_likes_created_at ON CommentLikes(CreatedAt);
CREATE INDEX idx_comment_likes_comment_created ON CommentLikes(CommentId, CreatedAt);
CREATE INDEX idx_comment_likes_user_created ON CommentLikes(UserId, CreatedAt);

-- CommentReports表索引
CREATE INDEX idx_comment_reports_comment_id ON CommentReports(CommentId);
CREATE INDEX idx_comment_reports_user_id ON CommentReports(UserId);
CREATE INDEX idx_comment_reports_reason ON CommentReports(Reason);
CREATE INDEX idx_comment_reports_status ON CommentReports(Status);
CREATE INDEX idx_comment_reports_created_at ON CommentReports(CreatedAt);
CREATE INDEX idx_comment_reports_handled_at ON CommentReports(HandledAt);
CREATE INDEX idx_comment_reports_handled_by ON CommentReports(HandledBy);

-- 复合索引
CREATE INDEX idx_comment_reports_comment_status ON CommentReports(CommentId, Status);
CREATE INDEX idx_comment_reports_status_created ON CommentReports(Status, CreatedAt);
CREATE INDEX idx_comment_reports_reason_status ON CommentReports(Reason, Status);
CREATE INDEX idx_comment_reports_user_created ON CommentReports(UserId, CreatedAt);
CREATE INDEX idx_comment_reports_handled_created ON CommentReports(HandledBy, HandledAt);

-- =============================================
-- 触发器 - SQLite版本
-- =============================================

-- 更新UpdatedAt触发器
CREATE TRIGGER update_comments_updated_at 
    AFTER UPDATE ON Comments
    FOR EACH ROW
    BEGIN
        UPDATE Comments SET UpdatedAt = CURRENT_TIMESTAMP WHERE Id = NEW.Id;
    END;

-- 自动更新LikeCount触发器
CREATE TRIGGER update_comment_like_count
    AFTER INSERT ON CommentLikes
    FOR EACH ROW
    BEGIN
        UPDATE Comments SET LikeCount = LikeCount + 1 WHERE Id = NEW.CommentId;
    END;

CREATE TRIGGER update_comment_like_count_delete
    AFTER DELETE ON CommentLikes
    FOR EACH ROW
    BEGIN
        UPDATE Comments SET LikeCount = LikeCount - 1 WHERE Id = OLD.CommentId;
    END;

-- 自动更新ReplyCount触发器
CREATE TRIGGER update_comment_reply_count
    AFTER INSERT ON Comments
    FOR EACH ROW
    BEGIN
        UPDATE Comments SET ReplyCount = ReplyCount + 1 WHERE Id = NEW.ParentId;
    END;

CREATE TRIGGER update_comment_reply_count_delete
    AFTER DELETE ON Comments
    FOR EACH ROW
    BEGIN
        UPDATE Comments SET ReplyCount = ReplyCount - 1 WHERE Id = OLD.ParentId;
    END;

-- =============================================
-- 初始化数据
-- =============================================

-- 为现有代码片段初始化评论统计数据
UPDATE CodeSnippets SET 
    CommentCount = 0,
    LastCommentAt = NULL
WHERE CommentCount IS NULL;