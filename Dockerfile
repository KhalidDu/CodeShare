# 多阶段构建 - 后端
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

# 构建阶段
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 复制项目文件并还原依赖
COPY ["backend/CodeSnippetManager.Api.csproj", "backend/"]
RUN dotnet restore "backend/CodeSnippetManager.Api.csproj"

# 复制源代码并构建
COPY backend/ backend/
WORKDIR "/src/backend"
RUN dotnet build "CodeSnippetManager.Api.csproj" -c Release -o /app/build

# 发布阶段
FROM build AS publish
RUN dotnet publish "CodeSnippetManager.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 最终镜像
FROM base AS final
WORKDIR /app

# 创建非 root 用户
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# 复制发布的应用程序
COPY --from=publish /app/publish .

# 健康检查
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "CodeSnippetManager.Api.dll"]