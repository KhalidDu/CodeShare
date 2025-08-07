using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using CodeSnippetManager.Api.Services;
using CodeSnippetManager.Api.Repositories;
using CodeSnippetManager.Api.Data;
using CodeSnippetManager.Api.Models;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 剪贴板功能测试 - 验证历史记录限制和批量操作功能
/// </summary>
[TestClass]
public class ClipboardFunctionalTests
{
    [TestMethod]
    public void ClipboardService_Constructor_WithValidParameters_DoesNotThrow()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
        var connectionFactory = new MySqlConnectionFactory(connectionString);
        var clipboardRepository = new ClipboardHistoryRepository(connectionFactory);
        var codeSnippetRepository = new CodeSnippetRepository(connectionFactory);
        
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"ClipboardSettings:MaxHistoryCount", "100"}
        });
        var configuration = configurationBuilder.Build();

        // Act & Assert - 验证构造函数不抛出异常
        var service = new ClipboardService(clipboardRepository, codeSnippetRepository, configuration);
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void ClipboardHistoryRepository_Constructor_WithValidConnectionFactory_DoesNotThrow()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
        var connectionFactory = new MySqlConnectionFactory(connectionString);

        // Act & Assert - 验证构造函数不抛出异常
        var repository = new ClipboardHistoryRepository(connectionFactory);
        Assert.IsNotNull(repository);
    }

    [TestMethod]
    public void ClipboardHistoryRepository_Constructor_WithNullConnectionFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new ClipboardHistoryRepository(null!));
    }

    [TestMethod]
    public void ClipboardService_Constructor_WithNullParameters_ThrowsArgumentNullException()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
        var connectionFactory = new MySqlConnectionFactory(connectionString);
        var clipboardRepository = new ClipboardHistoryRepository(connectionFactory);
        var codeSnippetRepository = new CodeSnippetRepository(connectionFactory);
        
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();

        // Act & Assert - 测试各种 null 参数
        Assert.ThrowsException<ArgumentNullException>(() => 
            new ClipboardService(null!, codeSnippetRepository, configuration));

        Assert.ThrowsException<ArgumentNullException>(() => 
            new ClipboardService(clipboardRepository, null!, configuration));

        Assert.ThrowsException<ArgumentNullException>(() => 
            new ClipboardService(clipboardRepository, codeSnippetRepository, null!));
    }

    [TestMethod]
    public void MySqlConnectionFactory_Constructor_WithValidConnectionString_DoesNotThrow()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";

        // Act & Assert
        var factory = new MySqlConnectionFactory(connectionString);
        Assert.IsNotNull(factory);
    }

    [TestMethod]
    public void MySqlConnectionFactory_Constructor_WithNullConnectionString_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new MySqlConnectionFactory(null!));
    }

    [TestMethod]
    public void MySqlConnectionFactory_CreateConnection_ReturnsValidConnection()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
        var factory = new MySqlConnectionFactory(connectionString);

        // Act
        using var connection = factory.CreateConnection();

        // Assert
        Assert.IsNotNull(connection);
        // MySQL 连接字符串可能会被格式化，所以只检查关键部分
        Assert.IsTrue(connection.ConnectionString.Contains("localhost"));
        Assert.IsTrue(connection.ConnectionString.Contains("test"));
    }

    [TestMethod]
    public void ClipboardHistory_Model_PropertiesCanBeSetAndGet()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var snippetId = Guid.NewGuid();
        var copiedAt = DateTime.UtcNow;

        // Act
        var history = new ClipboardHistory
        {
            Id = id,
            UserId = userId,
            SnippetId = snippetId,
            CopiedAt = copiedAt
        };

        // Assert
        Assert.AreEqual(id, history.Id);
        Assert.AreEqual(userId, history.UserId);
        Assert.AreEqual(snippetId, history.SnippetId);
        Assert.AreEqual(copiedAt, history.CopiedAt);
    }

    [TestMethod]
    public async Task ClipboardService_GetCopyCountsBatchAsync_WithNullInput_ReturnsEmptyDictionary()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
        var connectionFactory = new MySqlConnectionFactory(connectionString);
        var clipboardRepository = new ClipboardHistoryRepository(connectionFactory);
        var codeSnippetRepository = new CodeSnippetRepository(connectionFactory);
        
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();
        var service = new ClipboardService(clipboardRepository, codeSnippetRepository, configuration);

        // Act
        var result = await service.GetCopyCountsBatchAsync(null!);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task ClipboardService_GetCopyCountsBatchAsync_WithEmptyInput_ReturnsEmptyDictionary()
    {
        // Arrange
        var connectionString = "Server=localhost;Database=test;Uid=root;Pwd=password;";
        var connectionFactory = new MySqlConnectionFactory(connectionString);
        var clipboardRepository = new ClipboardHistoryRepository(connectionFactory);
        var codeSnippetRepository = new CodeSnippetRepository(connectionFactory);
        
        var configurationBuilder = new ConfigurationBuilder();
        var configuration = configurationBuilder.Build();
        var service = new ClipboardService(clipboardRepository, codeSnippetRepository, configuration);

        // Act
        var result = await service.GetCopyCountsBatchAsync(new List<Guid>());

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}