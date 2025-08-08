using CodeSnippetManager.Api.Extensions;
using CodeSnippetManager.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加健康检查服务
builder.Services.AddHealthChecks();

// 使用扩展方法注册所有应用程序服务 - 遵循开闭原则和依赖倒置原则
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 使用安全头中间件
app.UseMiddleware<SecurityHeadersMiddleware>();

// 使用频率限制中间件
app.UseMiddleware<RateLimitingMiddleware>();

// 使用XSS防护中间件
app.UseMiddleware<XssProtectionMiddleware>();

// 使用CSRF防护中间件
app.UseMiddleware<CsrfProtectionMiddleware>();

// 使用响应压缩
app.UseResponseCompression();

// 使用响应缓存
app.UseResponseCaching();

// 使用 CORS
app.UseCors("AllowFrontend");

// 使用JWT中间件
app.UseMiddleware<JwtMiddleware>();

// 使用认证和授权
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 添加健康检查端点
app.MapHealthChecks("/health");

app.Run();
