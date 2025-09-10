using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Data.Sqlite;
using CodeSnippetManager.Api.Interfaces;

namespace CodeSnippetManager.Api.Data;

/// <summary>
/// 数据库连接检测器 - 自动检测可用的数据库连接
/// </summary>
public class DatabaseConnectionDetector
{
    private readonly IConfiguration _configuration;

    public DatabaseConnectionDetector(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 检测并创建可用的数据库连接工厂
    /// </summary>
    /// <returns>可用的数据库连接工厂</returns>
    public IDbConnectionFactory CreateAvailableConnectionFactory()
    {
        // 首先尝试 MySQL 连接
        var mySqlConnectionString = _configuration.GetConnectionString("DefaultConnection") ?? 
                                  _configuration.GetConnectionString("MySQLConnection") ??
                                  "Server=localhost;Database=CodeSnippetManager;Uid=root;Pwd=root;CharSet=utf8mb4;";

        if (IsMySqlAvailable(mySqlConnectionString))
        {
            Console.WriteLine("✅ 使用 MySQL 数据库");
            return new MySqlConnectionFactory(mySqlConnectionString);
        }

        // 如果 MySQL 不可用，尝试 SQLite
        var sqliteConnectionString = _configuration.GetConnectionString("SQLiteConnection") ??
                                   "Data Source=CodeSnippetManager.db;";
        
        Console.WriteLine("⚠️ MySQL 不可用，切换到 SQLite 数据库");
        return new SqliteConnectionFactory(sqliteConnectionString);
    }

    /// <summary>
    /// 检测 MySQL 是否可用
    /// </summary>
    /// <param name="connectionString">MySQL 连接字符串</param>
    /// <returns>是否可用</returns>
    private bool IsMySqlAvailable(string connectionString)
    {
        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ MySQL 连接失败: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 确保 SQLite 数据库和表结构存在
    /// </summary>
    /// <param name="connectionString">SQLite 连接字符串</param>
    public static void EnsureSqliteDatabase(string connectionString)
    {
        try
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            // 创建表结构（如果不存在）
            var createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id TEXT PRIMARY KEY,
                    Username TEXT NOT NULL UNIQUE,
                    Email TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Role INTEGER NOT NULL DEFAULT 0,
                    CreatedAt TEXT NOT NULL,
                    IsActive INTEGER NOT NULL DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS Tags (
                    Id TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Color TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
                );

                CREATE TABLE IF NOT EXISTS CodeSnippets (
                    Id TEXT PRIMARY KEY,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Code TEXT NOT NULL,
                    Language TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    CreatorName TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    IsPublic INTEGER NOT NULL DEFAULT 1,
                    ViewCount INTEGER NOT NULL DEFAULT 0,
                    CopyCount INTEGER NOT NULL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS SnippetTags (
                    SnippetId TEXT NOT NULL,
                    TagId TEXT NOT NULL,
                    PRIMARY KEY (SnippetId, TagId),
                    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
                    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS SnippetVersions (
                    Id TEXT PRIMARY KEY,
                    SnippetId TEXT NOT NULL,
                    VersionNumber INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    Code TEXT NOT NULL,
                    Language TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    CreatorName TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    ChangeDescription TEXT,
                    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ClipboardHistory (
                    Id TEXT PRIMARY KEY,
                    UserId TEXT NOT NULL,
                    SnippetId TEXT NOT NULL,
                    SnippetTitle TEXT NOT NULL,
                    SnippetCode TEXT NOT NULL,
                    SnippetLanguage TEXT NOT NULL,
                    CopiedAt TEXT NOT NULL,
                    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                    FOREIGN KEY (SnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_snippets_created_at ON CodeSnippets(CreatedAt);
                CREATE INDEX IF NOT EXISTS idx_snippets_language ON CodeSnippets(Language);
                CREATE INDEX IF NOT EXISTS idx_snippets_is_public ON CodeSnippets(IsPublic);
                CREATE INDEX IF NOT EXISTS idx_snippets_created_by ON CodeSnippets(CreatedBy);
                CREATE INDEX IF NOT EXISTS idx_versions_snippet_id ON SnippetVersions(SnippetId);
                CREATE INDEX IF NOT EXISTS idx_clipboard_user_id ON ClipboardHistory(UserId);
                CREATE INDEX IF NOT EXISTS idx_clipboard_copied_at ON ClipboardHistory(CopiedAt);

                CREATE TABLE IF NOT EXISTS ShareTokens (
                    Id TEXT PRIMARY KEY,
                    Token TEXT NOT NULL UNIQUE,
                    CodeSnippetId TEXT NOT NULL,
                    CreatedBy TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT NOT NULL,
                    ExpiresAt TEXT,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    AccessCount INTEGER NOT NULL DEFAULT 0,
                    MaxAccessCount INTEGER NOT NULL DEFAULT 0,
                    Permission INTEGER NOT NULL DEFAULT 1,
                    Description TEXT,
                    Password TEXT,
                    AllowDownload INTEGER NOT NULL DEFAULT 1,
                    AllowCopy INTEGER NOT NULL DEFAULT 1,
                    LastAccessedAt TEXT,
                    FOREIGN KEY (CodeSnippetId) REFERENCES CodeSnippets(Id) ON DELETE CASCADE,
                    FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS ShareAccessLogs (
                    Id TEXT PRIMARY KEY,
                    ShareTokenId TEXT NOT NULL,
                    AccessTime TEXT NOT NULL,
                    IPAddress TEXT NOT NULL,
                    UserAgent TEXT,
                    IsSuccess INTEGER NOT NULL DEFAULT 1,
                    FailureReason TEXT,
                    SessionId TEXT,
                    Referrer TEXT,
                    SourceType TEXT,
                    FOREIGN KEY (ShareTokenId) REFERENCES ShareTokens(Id) ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_share_tokens_token ON ShareTokens(Token);
                CREATE INDEX IF NOT EXISTS idx_share_codesnippet_id ON ShareTokens(CodeSnippetId);
                CREATE INDEX IF NOT EXISTS idx_share_tokens_created_by ON ShareTokens(CreatedBy);
                CREATE INDEX IF NOT EXISTS idx_share_tokens_expires_at ON ShareTokens(ExpiresAt);
                CREATE INDEX IF NOT EXISTS idx_share_tokens_is_active ON ShareTokens(IsActive);
                CREATE INDEX IF NOT EXISTS idx_share_tokens_permission ON ShareTokens(Permission);
                CREATE INDEX IF NOT EXISTS idx_share_access_logs_share_token ON ShareAccessLogs(ShareTokenId);
                CREATE INDEX IF NOT EXISTS idx_share_access_logs_access_time ON ShareAccessLogs(AccessTime);
                CREATE INDEX IF NOT EXISTS idx_share_access_logs_ip_address ON ShareAccessLogs(IPAddress);
            ";

            using var command = new SqliteCommand(createTablesSql, connection);
            command.ExecuteNonQuery();

            // 插入一些示例数据（如果表为空）
            InsertSampleDataIfEmpty(connection);

            Console.WriteLine("✅ SQLite 数据库初始化完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SQLite 数据库初始化失败: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 插入示例数据（如果表为空）
    /// </summary>
    /// <param name="connection">SQLite 连接</param>
    private static void InsertSampleDataIfEmpty(SqliteConnection connection)
    {
        try
        {
            // 检查是否已有用户数据
            using var checkCommand = new SqliteCommand("SELECT COUNT(*) FROM Users", connection);
            var userCount = Convert.ToInt32(checkCommand.ExecuteScalar());

            if (userCount == 0)
            {
                Console.WriteLine("📝 插入示例数据...");

                // 插入示例用户
                var insertUserSql = @"
                    INSERT INTO Users (Id, Username, Email, PasswordHash, Role, CreatedAt, IsActive) 
                    VALUES 
                        ('c5c0a9ff-59dc-4719-83a5-89c88addc6e1', 'demo', 'demo@example.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeZeUfkZMBs9kYZP6', 2, datetime('now'), 1),
                        ('12345678-1234-5678-9012-345678901234', 'admin', 'admin@example.com', '$2a$11$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LeZeUfkZMBs9kYZP6', 0, datetime('now'), 1);
                ";

                using var command = new SqliteCommand(insertUserSql, connection);
                command.ExecuteNonQuery();

                // 插入示例标签
                var insertTagsSql = @"
                    INSERT INTO Tags (Id, Name, Color, CreatedBy, CreatedAt) 
                    VALUES 
                        ('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 'JavaScript', '#f7df1e', 'c5c0a9ff-59dc-4719-83a5-89c88addc6e1', datetime('now')),
                        ('b2c3d4e5-f6a7-8901-bcde-f23456789012', '算法', '#dc3545', 'c5c0a9ff-59dc-4719-83a5-89c88addc6e1', datetime('now')),
                        ('c3d4e5f6-a7b8-9012-cdef-345678901234', '工具函数', '#007bff', 'c5c0a9ff-59dc-4719-83a5-89c88addc6e1', datetime('now'));
                ";

                using var command2 = new SqliteCommand(insertTagsSql, connection);
                command2.ExecuteNonQuery();

                // 插入示例代码片段
                var insertSnippetsSql = @"
                    INSERT INTO CodeSnippets (Id, Title, Description, Code, Language, CreatedBy, CreatorName, CreatedAt, UpdatedAt, IsPublic, ViewCount, CopyCount) 
                    VALUES 
                        ('d1e2f3a4-b5c6-7890-abcd-ef1234567890', '数组去重', '多种JavaScript数组去重方法', '// 方法1: 使用 Set\nconst uniqueArray = [...new Set(array)];\n\n// 方法2: 使用 filter + indexOf\nconst uniqueArray2 = array.filter((item, index) => array.indexOf(item) === index);\n\n// 方法3: 使用 reduce\nconst uniqueArray3 = array.reduce((unique, item) => {\n  return unique.includes(item) ? unique : [...unique, item];\n}, []);', 'javascript', 'c5c0a9ff-59dc-4719-83a5-89c88addc6e1', 'Demo User', datetime('now'), datetime('now'), 1, 15, 8),
                        ('e2f3a4b5-c6d7-8901-bcde-f23456789012', '二分查找', '经典的二分查找算法实现', 'function binarySearch(arr, target) {\n  let left = 0;\n  let right = arr.length - 1;\n  \n  while (left <= right) {\n    const mid = Math.floor((left + right) / 2);\n    \n    if (arr[mid] === target) {\n      return mid;\n    } else if (arr[mid] < target) {\n      left = mid + 1;\n    } else {\n      right = mid - 1;\n    }\n  }\n  \n  return -1;\n}', 'javascript', 'c5c0a9ff-59dc-4719-83a5-89c88addc6e1', 'Demo User', datetime('now'), datetime('now'), 1, 25, 12),
                        ('f3a4b5c6-d7e8-9012-cdef-345678901234', '防抖函数', 'JavaScript防抖函数实现', 'function debounce(func, wait) {\n  let timeout;\n  \n  return function executedFunction(...args) {\n    const later = () => {\n      clearTimeout(timeout);\n      func(...args);\n    };\n    \n    clearTimeout(timeout);\n    timeout = setTimeout(later, wait);\n  };\n}', 'javascript', 'c5c0a9ff-59dc-4719-83a5-89c88addc6e1', 'Demo User', datetime('now'), datetime('now'), 1, 30, 18);
                ";

                using var command3 = new SqliteCommand(insertSnippetsSql, connection);
                command3.ExecuteNonQuery();

                // 插入示例标签关联
                var insertSnippetTagsSql = @"
                    INSERT INTO SnippetTags (SnippetId, TagId) 
                    VALUES ('d1e2f3a4-b5c6-7890-abcd-ef1234567890', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'), 
                           ('d1e2f3a4-b5c6-7890-abcd-ef1234567890', 'c3d4e5f6-a7b8-9012-cdef-345678901234'), 
                           ('e2f3a4b5-c6d7-8901-bcde-f23456789012', 'b2c3d4e5-f6a7-8901-bcde-f23456789012'), 
                           ('f3a4b5c6-d7e8-9012-cdef-345678901234', 'a1b2c3d4-e5f6-7890-abcd-ef1234567890'), 
                           ('f3a4b5c6-d7e8-9012-cdef-345678901234', 'c3d4e5f6-a7b8-9012-cdef-345678901234');
                ";

                using var command4 = new SqliteCommand(insertSnippetTagsSql, connection);
                command4.ExecuteNonQuery();

                Console.WriteLine("✅ 示例数据插入完成");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ 插入示例数据时出错: {ex.Message}");
        }
    }
}