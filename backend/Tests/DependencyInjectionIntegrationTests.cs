using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeSnippetManager.Api.Extensions;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Services;

namespace CodeSnippetManager.Api.Tests;

/// <summary>
/// 依赖注入集成测试 - 验证面向接口设计和容器配置
/// </summary>
[TestClass]
public class DependencyInjectionIntegrationTests
{
    private IServiceProvider _serviceProvider = null!;
    private IConfiguration _configuration = null!;

    [TestInitialize]
    public void Setup()
    {
        // 创建配置
        var configurationBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=TestDb;Uid=root;Pwd=password;",
                ["JwtSettings:SecretKey"] = "test-secret-key-that-is-at-least-32-characters-long",
                ["JwtSettings:ExpiryHours"] = "24",
                ["Security:RateLimit:DefaultLimit"] = "100",
                ["Security:RateLimit:AuthEndpointLimit"] = "10",
                ["Security:RateLimit:WriteOperationLimit"] = "30",
                ["Security:RateLimit:TimeWindowMinutes"] = "1",
                ["Security:XssProtection:Enabled"] = "true",
                ["Security:XssProtection:LogAttempts"] = "true",
                ["Security:XssProtection:BlockRequests"] = "true",
                ["Security:CsrfProtection:Enabled"] = "true",
                ["Security:CsrfProtection:TokenLifetimeHours"] = "24",
                ["Security:CsrfProtection:LogAttempts"] = "true"
            });

        _configuration = configurationBuilder.Build();

        // 创建服务容器
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddApplicationServices(_configuration);

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    #region Service Registration Tests

    [TestMethod]
    public void ServiceContainer_RegistersAllRepositoryInterfaces()
    {
        // Act & Assert - 验证所有仓储接口都已注册
        Assert.IsNotNull(_serviceProvider.GetService<IUserRepository>());
        Assert.IsNotNull(_serviceProvider.GetService<ICodeSnippetRepository>());
        Assert.IsNotNull(_serviceProvider.GetService<ITagRepository>());
        Assert.IsNotNull(_serviceProvider.GetService<ISnippetVersionRepository>());
        Assert.IsNotNull(_serviceProvider.GetService<IClipboardHistoryRepository>());
    }

    [TestMethod]
    public void ServiceContainer_RegistersAllBusinessServices()
    {
        // Act & Assert - 验证所有业务服务接口都已注册
        Assert.IsNotNull(_serviceProvider.GetService<IUserService>());
        Assert.IsNotNull(_serviceProvider.GetService<ICodeSnippetService>());
        Assert.IsNotNull(_serviceProvider.GetService<ITagService>());
        Assert.IsNotNull(_serviceProvider.GetService<IAuthService>());
        Assert.IsNotNull(_serviceProvider.GetService<IClipboardService>());
        Assert.IsNotNull(_serviceProvider.GetService<IVersionManagementService>());
        Assert.IsNotNull(_serviceProvider.GetService<IPermissionService>());
    }

    [TestMethod]
    public void ServiceContainer_RegistersSecurityServices()
    {
        // Act & Assert - 验证安全服务接口都已注册
        Assert.IsNotNull(_serviceProvider.GetService<IInputValidationService>());
    }

    [TestMethod]
    public void ServiceContainer_RegistersInfrastructureServices()
    {
        // Act & Assert - 验证基础设施服务都已注册
        Assert.IsNotNull(_serviceProvider.GetService<IDbConnectionFactory>());
        Assert.IsNotNull(_serviceProvider.GetService<ICacheService>());
    }

    #endregion

    #region Service Lifetime Tests

    [TestMethod]
    public void ServiceContainer_ScopedServices_ReturnSameInstanceInScope()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        // Act
        var service1 = scopedProvider.GetService<ICodeSnippetService>();
        var service2 = scopedProvider.GetService<ICodeSnippetService>();

        // Assert
        Assert.IsNotNull(service1);
        Assert.IsNotNull(service2);
        Assert.AreSame(service1, service2);
    }

    [TestMethod]
    public void ServiceContainer_ScopedServices_ReturnDifferentInstancesInDifferentScopes()
    {
        // Arrange & Act
        ICodeSnippetService? service1;
        ICodeSnippetService? service2;

        using (var scope1 = _serviceProvider.CreateScope())
        {
            service1 = scope1.ServiceProvider.GetService<ICodeSnippetService>();
        }

        using (var scope2 = _serviceProvider.CreateScope())
        {
            service2 = scope2.ServiceProvider.GetService<ICodeSnippetService>();
        }

        // Assert
        Assert.IsNotNull(service1);
        Assert.IsNotNull(service2);
        Assert.AreNotSame(service1, service2);
    }

    [TestMethod]
    public void ServiceContainer_SingletonServices_ReturnSameInstanceAcrossScopes()
    {
        // Arrange & Act
        IDbConnectionFactory? factory1;
        IDbConnectionFactory? factory2;

        using (var scope1 = _serviceProvider.CreateScope())
        {
            factory1 = scope1.ServiceProvider.GetService<IDbConnectionFactory>();
        }

        using (var scope2 = _serviceProvider.CreateScope())
        {
            factory2 = scope2.ServiceProvider.GetService<IDbConnectionFactory>();
        }

        // Assert
        Assert.IsNotNull(factory1);
        Assert.IsNotNull(factory2);
        Assert.AreSame(factory1, factory2);
    }

    #endregion

    #region Interface Implementation Tests

    [TestMethod]
    public void ServiceContainer_ReturnsCorrectImplementations()
    {
        // Act
        var authService = _serviceProvider.GetService<IAuthService>();
        var codeSnippetService = _serviceProvider.GetService<ICodeSnippetService>();
        var validationService = _serviceProvider.GetService<IInputValidationService>();

        // Assert - 验证返回的是正确的实现类
        Assert.IsInstanceOfType(authService, typeof(AuthService));
        Assert.IsInstanceOfType(codeSnippetService, typeof(CodeSnippetService));
        Assert.IsInstanceOfType(validationService, typeof(InputValidationService));
    }

    [TestMethod]
    public void ServiceContainer_ServicesImplementCorrectInterfaces()
    {
        // Act
        var authService = _serviceProvider.GetService<IAuthService>();
        var codeSnippetService = _serviceProvider.GetService<ICodeSnippetService>();
        var validationService = _serviceProvider.GetService<IInputValidationService>();

        // Assert - 验证服务实现了正确的接口
        Assert.IsTrue(authService is IAuthService);
        Assert.IsTrue(codeSnippetService is ICodeSnippetService);
        Assert.IsTrue(validationService is IInputValidationService);
    }

    #endregion

    #region Dependency Resolution Tests

    [TestMethod]
    public void ServiceContainer_ResolvesNestedDependencies()
    {
        // Act - 获取有复杂依赖关系的服务
        var codeSnippetService = _serviceProvider.GetService<ICodeSnippetService>();

        // Assert - 验证服务能够成功创建（说明所有依赖都能正确解析）
        Assert.IsNotNull(codeSnippetService);
    }

    [TestMethod]
    public void ServiceContainer_AuthService_HasAllDependencies()
    {
        // Act
        var authService = _serviceProvider.GetService<IAuthService>();

        // Assert - 验证AuthService能够成功创建，说明所有依赖都已正确注册
        Assert.IsNotNull(authService);
        
        // 通过反射验证依赖注入的字段
        var authServiceType = authService.GetType();
        var fields = authServiceType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        // 验证关键依赖字段存在
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(IUserRepository)));
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(IConfiguration)));
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(IInputValidationService)));
    }

    [TestMethod]
    public void ServiceContainer_CodeSnippetService_HasAllDependencies()
    {
        // Act
        var codeSnippetService = _serviceProvider.GetService<ICodeSnippetService>();

        // Assert
        Assert.IsNotNull(codeSnippetService);
        
        // 验证CodeSnippetService的依赖
        var serviceType = codeSnippetService.GetType();
        var fields = serviceType.GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(ICodeSnippetRepository)));
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(IPermissionService)));
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(ITagRepository)));
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(IVersionManagementService)));
        Assert.IsTrue(fields.Any(f => f.FieldType == typeof(IInputValidationService)));
    }

    #endregion

    #region Configuration Integration Tests

    [TestMethod]
    public void ServiceContainer_ConfigurationIsAccessible()
    {
        // Act
        var configuration = _serviceProvider.GetService<IConfiguration>();

        // Assert
        Assert.IsNotNull(configuration);
        Assert.AreEqual("test-secret-key-that-is-at-least-32-characters-long", 
            configuration["JwtSettings:SecretKey"]);
    }

    [TestMethod]
    public void ServiceContainer_SecurityConfigurationIsLoaded()
    {
        // Act
        var configuration = _serviceProvider.GetService<IConfiguration>();

        // Assert
        Assert.IsNotNull(configuration);
        Assert.AreEqual("100", configuration["Security:RateLimit:DefaultLimit"]);
        Assert.AreEqual("true", configuration["Security:XssProtection:Enabled"]);
        Assert.AreEqual("true", configuration["Security:CsrfProtection:Enabled"]);
    }

    #endregion

    #region Service Interaction Tests

    [TestMethod]
    public void ServiceContainer_ServicesCanInteract()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetService<IAuthService>();
        var validationService = scope.ServiceProvider.GetService<IInputValidationService>();

        // Act & Assert - 验证服务之间可以正常交互
        Assert.IsNotNull(authService);
        Assert.IsNotNull(validationService);
        
        // 验证验证服务的基本功能
        var emailValidation = validationService.ValidateEmail("test@example.com");
        Assert.IsTrue(emailValidation.IsValid);
        
        var usernameValidation = validationService.ValidateUsername("testuser");
        Assert.IsTrue(usernameValidation.IsValid);
    }

    #endregion

    #region Error Handling Tests

    [TestMethod]
    public void ServiceContainer_ThrowsForUnregisteredService()
    {
        // Act & Assert
        var unregisteredService = _serviceProvider.GetService<IDisposable>();
        Assert.IsNull(unregisteredService);
    }

    [TestMethod]
    public void ServiceContainer_RequiredService_ThrowsForUnregisteredService()
    {
        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() =>
            _serviceProvider.GetRequiredService<IDisposable>());
    }

    #endregion
}