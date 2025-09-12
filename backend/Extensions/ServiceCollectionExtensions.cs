using CodeSnippetManager.Api.Data;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Repositories;
using CodeSnippetManager.Api.Services;
using CodeSnippetManager.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CodeSnippetManager.Api.Extensions;

/// <summary>
/// 服务注册扩展方法 - 遵循开闭原则便于扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 注册数据访问层服务 - 自动检测并使用可用的数据库
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册数据库连接检测器
        services.AddSingleton<DatabaseConnectionDetector>();
        
        // 注册数据库连接工厂 - 自动检测可用的数据库
        services.AddSingleton<IDbConnectionFactory>(provider => 
        {
            var detector = provider.GetRequiredService<DatabaseConnectionDetector>();
            return detector.CreateAvailableConnectionFactory();
        });

        // 注册仓储接口和实现 - Scoped 生命周期，每个HTTP请求一个实例
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICodeSnippetRepository, CodeSnippetRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ISnippetVersionRepository, SnippetVersionRepository>();
        services.AddScoped<IClipboardHistoryRepository, ClipboardHistoryRepository>();
        services.AddScoped<IShareTokenRepository, ShareTokenRepository>();
        services.AddScoped<IShareAccessLogRepository, ShareAccessLogRepository>();

        // 注册消息系统仓储 - Scoped 生命周期
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IMessageAttachmentRepository, MessageAttachmentRepository>();
        services.AddScoped<IMessageDraftRepository, MessageDraftRepository>();
        services.AddScoped<IMessageConversationRepository, MessageConversationRepository>();
        services.AddScoped<IMessageDraftAttachmentRepository, MessageDraftAttachmentRepository>();

        return services;
    }

    /// <summary>
    /// 注册业务逻辑层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        // 注册应用服务 - Scoped 生命周期，每个HTTP请求一个实例
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICodeSnippetService, CodeSnippetService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClipboardService, ClipboardService>();
        services.AddScoped<IShareService, ShareService>();

        // 注册领域服务 - Scoped 生命周期
        services.AddScoped<IVersionManagementService, VersionManagementService>();
        services.AddScoped<IPermissionService, PermissionService>();

        // 注册安全服务 - Scoped 生命周期
        services.AddScoped<IInputValidationService, InputValidationService>();

        // 注册消息系统服务 - Scoped 生命周期
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMessageAttachmentService, MessageAttachmentService>();
        services.AddScoped<IMessageDraftService, MessageDraftService>();

        return services;
    }

    /// <summary>
    /// 配置JWT认证
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long";
        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    /// <summary>
    /// 配置CORS策略
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:6677", "http://localhost:3000")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// 验证依赖注入配置的正确性
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection ValidateDependencyInjection(this IServiceCollection services)
    {
        // 在开发环境中验证服务注册
        var serviceProvider = services.BuildServiceProvider();
        
        try
        {
            // 验证关键服务是否正确注册
            var dbFactory = serviceProvider.GetRequiredService<IDbConnectionFactory>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var codeSnippetService = serviceProvider.GetRequiredService<ICodeSnippetService>();
            var authService = serviceProvider.GetRequiredService<IAuthService>();
            var shareService = serviceProvider.GetRequiredService<IShareService>();
            
            // 验证仓储服务
            var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
            var snippetRepo = serviceProvider.GetRequiredService<ICodeSnippetRepository>();
            var tagRepo = serviceProvider.GetRequiredService<ITagRepository>();
            var shareTokenRepo = serviceProvider.GetRequiredService<IShareTokenRepository>();
            var shareAccessLogRepo = serviceProvider.GetRequiredService<IShareAccessLogRepository>();
            
            Console.WriteLine("✅ 依赖注入配置验证成功 - 所有服务都已正确注册");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 依赖注入配置验证失败: {ex.Message}");
            throw;
        }
        finally
        {
            serviceProvider.Dispose();
        }

        return services;
    }

    /// <summary>
    /// 注册缓存服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddCacheServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册内存缓存
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000; // 设置缓存项数量限制
        });

        // 注册缓存服务接口和实现
        services.AddScoped<ICacheService, MemoryCacheService>();

        // 可选：注册 Redis 缓存（如果配置了 Redis）
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            // 这里可以添加 Redis 缓存实现
            // services.AddStackExchangeRedisCache(options =>
            // {
            //     options.Configuration = redisConnectionString;
            // });
        }

        return services;
    }

    /// <summary>
    /// 注册性能优化服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddPerformanceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置响应缓存
        services.AddResponseCaching(options =>
        {
            options.MaximumBodySize = 1024 * 1024; // 1MB
            options.UseCaseSensitivePaths = false;
        });

        // 配置响应压缩
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.BrotliCompressionProvider>();
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            });
        });

        return services;
    }

    /// <summary>
    /// 配置安全服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddSecurityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 配置频率限制
        services.Configure<RateLimitOptions>(options =>
        {
            options.DefaultLimit = configuration.GetValue<int>("Security:RateLimit:DefaultLimit", 100);
            options.AuthEndpointLimit = configuration.GetValue<int>("Security:RateLimit:AuthEndpointLimit", 10);
            options.WriteOperationLimit = configuration.GetValue<int>("Security:RateLimit:WriteOperationLimit", 30);
            options.TimeWindow = TimeSpan.FromMinutes(configuration.GetValue<int>("Security:RateLimit:TimeWindowMinutes", 1));
        });
        
        // 配置XSS防护
        services.Configure<XssProtectionOptions>(options =>
        {
            options.Enabled = configuration.GetValue<bool>("Security:XssProtection:Enabled", true);
            options.LogAttempts = configuration.GetValue<bool>("Security:XssProtection:LogAttempts", true);
            options.BlockRequests = configuration.GetValue<bool>("Security:XssProtection:BlockRequests", true);
        });
        
        // 配置CSRF防护
        services.Configure<CsrfProtectionOptions>(options =>
        {
            options.Enabled = configuration.GetValue<bool>("Security:CsrfProtection:Enabled", true);
            options.TokenLifetimeHours = configuration.GetValue<int>("Security:CsrfProtection:TokenLifetimeHours", 24);
            options.LogAttempts = configuration.GetValue<bool>("Security:CsrfProtection:LogAttempts", true);
        });

        return services;
    }

    /// <summary>
    /// 添加所有应用程序服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册各层服务
        services.AddCacheServices(configuration);
        services.AddPerformanceServices(configuration);
        services.AddSecurityServices(configuration);
        services.AddDataAccessServices(configuration);
        services.AddBusinessServices();
        services.AddJwtAuthentication(configuration);
        services.AddCorsPolicy();

        // 初始化SQLite数据库（如果使用SQLite）
        InitializeDatabaseIfNeeded(services, configuration);

        // 在开发环境中验证配置
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            services.ValidateDependencyInjection();
        }

        return services;
    }

    /// <summary>
    /// 初始化数据库（如果需要）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    private static void InitializeDatabaseIfNeeded(IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            // 检查是否可能使用SQLite
            var mySqlConnectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                                      configuration.GetConnectionString("MySQLConnection") ??
                                      "Server=localhost;Database=CodeSnippetManager;Uid=root;Pwd=root;CharSet=utf8mb4;";

            // 简单检测MySQL是否可用
            using var mySqlConnection = new MySql.Data.MySqlClient.MySqlConnection(mySqlConnectionString);
            try
            {
                mySqlConnection.Open();
                Console.WriteLine("✅ MySQL 数据库连接正常");
            }
            catch
            {
                Console.WriteLine("⚠️ MySQL 不可用，将使用 SQLite 数据库");
                
                // 初始化SQLite数据库
                var sqliteConnectionString = configuration.GetConnectionString("SQLiteConnection") ??
                                           "Data Source=CodeSnippetManager.db;";
                DatabaseConnectionDetector.EnsureSqliteDatabase(sqliteConnectionString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ 数据库初始化检查失败: {ex.Message}");
        }
    }
}