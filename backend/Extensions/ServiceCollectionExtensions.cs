using CodeSnippetManager.Api.Data;
using CodeSnippetManager.Api.Interfaces;
using CodeSnippetManager.Api.Repositories;
using CodeSnippetManager.Api.Services;
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
    /// 注册数据访问层服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="connectionString">数据库连接字符串</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddDataAccessServices(this IServiceCollection services, string connectionString)
    {
        // 注册数据库连接工厂 - 单例服务，整个应用程序生命周期内只有一个实例
        services.AddSingleton<IDbConnectionFactory>(provider => 
            new MySqlConnectionFactory(connectionString));

        // 注册仓储接口和实现 - Scoped 生命周期，每个HTTP请求一个实例
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICodeSnippetRepository, CodeSnippetRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ISnippetVersionRepository, SnippetVersionRepository>();
        services.AddScoped<IClipboardHistoryRepository, ClipboardHistoryRepository>();

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

        // 注册领域服务 - Scoped 生命周期
        services.AddScoped<IVersionManagementService, VersionManagementService>();
        services.AddScoped<IPermissionService, PermissionService>();

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
                policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
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
            
            // 验证仓储服务
            var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
            var snippetRepo = serviceProvider.GetRequiredService<ICodeSnippetRepository>();
            var tagRepo = serviceProvider.GetRequiredService<ITagRepository>();
            
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
    /// 添加所有应用程序服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 获取数据库连接字符串
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=CodeSnippetManager;Uid=root;Pwd=password;";

        // 注册各层服务
        services.AddDataAccessServices(connectionString);
        services.AddBusinessServices();
        services.AddJwtAuthentication(configuration);
        services.AddCorsPolicy();

        // 在开发环境中验证配置
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            services.ValidateDependencyInjection();
        }

        return services;
    }
}