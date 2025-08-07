using CodeSnippetManager.Api.Extensions;
using CodeSnippetManager.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

// 使用 CORS
app.UseCors("AllowFrontend");

// 使用JWT中间件
app.UseMiddleware<JwtMiddleware>();

// 使用认证和授权
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
