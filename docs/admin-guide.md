# 管理员指南

## 目录

1. [系统概述](#系统概述)
2. [用户管理](#用户管理)
3. [权限管理](#权限管理)
4. [系统配置](#系统配置)
5. [数据库管理](#数据库管理)
6. [安全设置](#安全设置)
7. [性能监控](#性能监控)
8. [备份和恢复](#备份和恢复)
9. [故障排除](#故障排除)

## 系统概述

代码片段管理工具采用三层架构设计，包含以下主要组件：

- **前端**：Vue 3 + TypeScript 单页应用
- **后端**：.NET 8 Web API，遵循 SOLID 原则
- **数据库**：MySQL 8.0+，使用 Dapper ORM
- **认证**：JWT Token 认证机制
- **安全**：XSS 防护、CSRF 保护、请求频率限制

### 系统架构

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   前端 SPA   │───▶│  后端 API   │───▶│  MySQL DB   │
│  Vue 3 + TS │    │  .NET 8     │    │   Dapper    │
└─────────────┘    └─────────────┘    └─────────────┘
```

### 用户角色

- **管理员 (Admin)**：完整系统访问权限
- **编辑者 (Editor)**：可创建和管理自己的代码片段
- **查看者 (Viewer)**：只能查看和复制公开的代码片段

## 用户管理

### 创建用户

1. 登录管理员账户
2. 进入"用户管理"页面
3. 点击"新建用户"按钮
4. 填写用户信息：
   - 用户名（唯一）
   - 邮箱地址（唯一）
   - 初始密码
   - 用户角色
   - 账户状态（启用/禁用）
5. 点击"创建用户"按钮

### 编辑用户

1. 在用户列表中找到要编辑的用户
2. 点击"编辑"按钮
3. 修改用户信息
4. 点击"保存更改"按钮

### 重置用户密码

1. 在用户列表中找到用户
2. 点击"重置密码"按钮
3. 输入新密码或使用系统生成的密码
4. 点击"确认重置"按钮
5. 将新密码告知用户

### 禁用/启用用户

1. 在用户列表中找到用户
2. 点击"禁用"或"启用"按钮
3. 确认操作

**注意**：禁用用户会立即终止其所有会话。

### 删除用户

1. 在用户列表中找到用户
2. 点击"删除"按钮
3. 确认删除操作

**警告**：删除用户会同时删除其创建的所有代码片段，此操作不可恢复。

## 权限管理

### 角色权限矩阵

| 功能 | 查看者 | 编辑者 | 管理员 |
|------|--------|--------|--------|
| 查看公开片段 | ✅ | ✅ | ✅ |
| 复制代码片段 | ✅ | ✅ | ✅ |
| 创建代码片段 | ❌ | ✅ | ✅ |
| 编辑自己的片段 | ❌ | ✅ | ✅ |
| 删除自己的片段 | ❌ | ✅ | ✅ |
| 查看所有片段 | ❌ | ❌ | ✅ |
| 编辑任意片段 | ❌ | ❌ | ✅ |
| 删除任意片段 | ❌ | ❌ | ✅ |
| 用户管理 | ❌ | ❌ | ✅ |
| 系统设置 | ❌ | ❌ | ✅ |

### 修改用户角色

1. 在用户管理页面找到用户
2. 点击"编辑"按钮
3. 在"角色"下拉菜单中选择新角色
4. 点击"保存更改"按钮

### 批量权限操作

1. 在用户列表中选择多个用户（使用复选框）
2. 点击"批量操作"按钮
3. 选择操作类型：
   - 批量修改角色
   - 批量启用/禁用
   - 批量删除
4. 确认操作

## 系统配置

### 基本设置

#### JWT 配置

编辑 `appsettings.Production.json`：

```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secure-jwt-secret-key",
    "ExpiryHours": 8,
    "Issuer": "https://your-domain.com",
    "Audience": "https://your-domain.com"
  }
}
```

#### 数据库连接

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql-server;Database=CodeSnippetManager;Uid=app_user;Pwd=secure_password;CharSet=utf8mb4;SslMode=Required;"
  }
}
```

#### 剪贴板设置

```json
{
  "ClipboardSettings": {
    "MaxHistoryCount": 500,
    "DefaultHistoryLimit": 100,
    "CleanupIntervalDays": 90
  }
}
```

### 安全配置

#### 请求频率限制

```json
{
  "Security": {
    "RateLimit": {
      "DefaultLimit": 1000,
      "AuthEndpointLimit": 50,
      "WriteOperationLimit": 200,
      "TimeWindowMinutes": 1
    }
  }
}
```

#### XSS 和 CSRF 防护

```json
{
  "Security": {
    "XssProtection": {
      "Enabled": true,
      "LogAttempts": true,
      "BlockRequests": true
    },
    "CsrfProtection": {
      "Enabled": true,
      "TokenLifetimeHours": 8,
      "LogAttempts": true
    }
  }
}
```

### 性能配置

#### 缓存设置

```json
{
  "Performance": {
    "CacheSettings": {
      "DefaultExpirationMinutes": 30,
      "MaxCacheSize": "100MB"
    }
  }
}
```

## 数据库管理

### 数据库结构

主要数据表：
- `Users` - 用户信息
- `CodeSnippets` - 代码片段
- `SnippetVersions` - 版本历史
- `Tags` - 标签
- `SnippetTags` - 片段标签关联
- `ClipboardHistory` - 剪贴板历史

### 数据库维护

#### 定期清理

1. **清理过期的剪贴板历史**：
```sql
DELETE FROM ClipboardHistory 
WHERE CopiedAt < DATE_SUB(NOW(), INTERVAL 90 DAY);
```

2. **清理未使用的标签**：
```sql
DELETE FROM Tags 
WHERE Id NOT IN (SELECT DISTINCT TagId FROM SnippetTags);
```

3. **清理过期的版本**（保留最近 10 个版本）：
```sql
DELETE sv1 FROM SnippetVersions sv1
INNER JOIN (
    SELECT SnippetId, VersionNumber
    FROM SnippetVersions sv2
    WHERE sv2.SnippetId = sv1.SnippetId
    ORDER BY VersionNumber DESC
    LIMIT 10, 18446744073709551615
) sv2 ON sv1.SnippetId = sv2.SnippetId 
AND sv1.VersionNumber = sv2.VersionNumber;
```

#### 数据库优化

1. **重建索引**：
```sql
OPTIMIZE TABLE Users, CodeSnippets, SnippetVersions, Tags, SnippetTags, ClipboardHistory;
```

2. **分析表统计信息**：
```sql
ANALYZE TABLE Users, CodeSnippets, SnippetVersions, Tags, SnippetTags, ClipboardHistory;
```

3. **检查表完整性**：
```sql
CHECK TABLE Users, CodeSnippets, SnippetVersions, Tags, SnippetTags, ClipboardHistory;
```

### 数据库监控

#### 性能监控查询

1. **查看慢查询**：
```sql
SELECT * FROM mysql.slow_log 
WHERE start_time > DATE_SUB(NOW(), INTERVAL 1 DAY)
ORDER BY start_time DESC;
```

2. **查看表大小**：
```sql
SELECT 
    table_name AS 'Table',
    ROUND(((data_length + index_length) / 1024 / 1024), 2) AS 'Size (MB)'
FROM information_schema.tables 
WHERE table_schema = 'CodeSnippetManager'
ORDER BY (data_length + index_length) DESC;
```

3. **查看连接状态**：
```sql
SHOW PROCESSLIST;
```

## 安全设置

### SSL/TLS 配置

1. **获取 SSL 证书**（推荐使用 Let's Encrypt）
2. **配置 Nginx**：
```nginx
server {
    listen 443 ssl http2;
    server_name your-domain.com;
    
    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    # ... 其他 SSL 配置
}
```

### 防火墙配置

1. **开放必要端口**：
   - 80 (HTTP，重定向到 HTTPS)
   - 443 (HTTPS)
   - 22 (SSH，仅管理员访问)

2. **限制数据库访问**：
   - 3306 端口仅允许应用服务器访问

### 安全审计

#### 查看登录日志

```sql
SELECT 
    u.Username,
    'Login' as Action,
    NOW() as Timestamp
FROM Users u
WHERE u.LastLoginAt > DATE_SUB(NOW(), INTERVAL 1 DAY);
```

#### 查看操作日志

检查应用程序日志文件：
```bash
tail -f /var/log/codesnippet/app.log
```

### 安全最佳实践

1. **定期更新密码策略**
2. **启用双因素认证**（如果支持）
3. **定期审查用户权限**
4. **监控异常登录活动**
5. **定期备份数据**
6. **保持系统和依赖项更新**

## 性能监控

### 系统资源监控

#### CPU 和内存使用

```bash
# 查看系统资源使用情况
top
htop

# 查看特定进程资源使用
ps aux | grep dotnet
ps aux | grep nginx
```

#### 磁盘使用

```bash
# 查看磁盘使用情况
df -h

# 查看目录大小
du -sh /var/lib/mysql
du -sh /var/log
```

### 应用程序监控

#### API 响应时间

检查 Nginx 访问日志：
```bash
tail -f /var/log/nginx/access.log | grep -E "POST|PUT|DELETE"
```

#### 数据库性能

```sql
-- 查看当前运行的查询
SHOW FULL PROCESSLIST;

-- 查看查询缓存命中率
SHOW STATUS LIKE 'Qcache%';

-- 查看 InnoDB 状态
SHOW ENGINE INNODB STATUS;
```

### 性能优化建议

1. **数据库优化**：
   - 定期分析和优化查询
   - 适当添加索引
   - 配置查询缓存

2. **应用程序优化**：
   - 启用响应压缩
   - 使用缓存机制
   - 优化静态资源加载

3. **服务器优化**：
   - 配置适当的连接池大小
   - 调整内存分配
   - 使用 CDN 加速静态资源

## 备份和恢复

### 数据库备份

#### 自动备份脚本

创建 `/scripts/backup.sh`：

```bash
#!/bin/bash
BACKUP_DIR="/backup/mysql"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="CodeSnippetManager"

# 创建备份目录
mkdir -p $BACKUP_DIR

# 执行备份
mysqldump -u root -p$MYSQL_ROOT_PASSWORD \
    --single-transaction \
    --routines \
    --triggers \
    $DB_NAME > $BACKUP_DIR/backup_$DATE.sql

# 压缩备份文件
gzip $BACKUP_DIR/backup_$DATE.sql

# 删除 7 天前的备份
find $BACKUP_DIR -name "backup_*.sql.gz" -mtime +7 -delete

echo "Backup completed: backup_$DATE.sql.gz"
```

#### 设置定时备份

```bash
# 编辑 crontab
crontab -e

# 添加每日凌晨 2 点备份
0 2 * * * /scripts/backup.sh
```

### 数据恢复

#### 从备份恢复

```bash
# 解压备份文件
gunzip backup_20240101_020000.sql.gz

# 恢复数据库
mysql -u root -p CodeSnippetManager < backup_20240101_020000.sql
```

#### 部分数据恢复

```sql
-- 恢复特定表
SOURCE /backup/mysql/backup_20240101_020000.sql;

-- 或者只恢复特定数据
INSERT INTO CodeSnippets SELECT * FROM backup_CodeSnippets WHERE Id = 'specific-id';
```

### 应用程序备份

#### 配置文件备份

```bash
# 备份配置文件
tar -czf config_backup_$(date +%Y%m%d).tar.gz \
    backend/appsettings.Production.json \
    nginx.conf \
    docker-compose.prod.yml \
    .env.production
```

#### 日志文件备份

```bash
# 归档日志文件
tar -czf logs_backup_$(date +%Y%m%d).tar.gz /var/log/codesnippet/
```

## 故障排除

### 常见问题

#### 1. 应用程序无法启动

**症状**：Docker 容器启动失败

**排查步骤**：
1. 检查 Docker 日志：
```bash
docker logs codesnippet-backend-prod
docker logs codesnippet-frontend-prod
```

2. 检查配置文件：
```bash
# 验证 JSON 格式
cat backend/appsettings.Production.json | jq .
```

3. 检查环境变量：
```bash
docker exec codesnippet-backend-prod env
```

#### 2. 数据库连接失败

**症状**：API 返回数据库连接错误

**排查步骤**：
1. 检查数据库服务状态：
```bash
docker logs codesnippet-mysql-prod
```

2. 测试数据库连接：
```bash
mysql -h mysql-server -u app_user -p CodeSnippetManager
```

3. 检查网络连接：
```bash
docker network ls
docker network inspect codesnippet-prod-network
```

#### 3. 性能问题

**症状**：API 响应缓慢

**排查步骤**：
1. 检查系统资源：
```bash
top
iostat -x 1
```

2. 分析慢查询：
```sql
SELECT * FROM mysql.slow_log ORDER BY start_time DESC LIMIT 10;
```

3. 检查应用程序日志：
```bash
docker logs codesnippet-backend-prod | grep -i "slow\|timeout\|error"
```

#### 4. 认证问题

**症状**：用户无法登录或 Token 验证失败

**排查步骤**：
1. 检查 JWT 配置：
```bash
# 验证 JWT 密钥是否正确设置
docker exec codesnippet-backend-prod env | grep JWT
```

2. 检查时间同步：
```bash
date
docker exec codesnippet-backend-prod date
```

3. 查看认证日志：
```bash
docker logs codesnippet-backend-prod | grep -i "auth\|jwt\|token"
```

### 紧急恢复程序

#### 1. 服务完全不可用

1. **立即回滚到上一个版本**：
```bash
./scripts/deploy.sh production rollback
```

2. **如果回滚失败，使用备份恢复**：
```bash
# 停止所有服务
docker-compose -f docker-compose.prod.yml down

# 恢复数据库
mysql -u root -p CodeSnippetManager < /backup/mysql/latest_backup.sql

# 重新启动服务
docker-compose -f docker-compose.prod.yml up -d
```

#### 2. 数据损坏

1. **立即停止写操作**
2. **评估损坏程度**
3. **从最近的备份恢复**
4. **通知用户服务中断**

### 联系支持

如果遇到无法解决的问题：

1. **收集诊断信息**：
   - 错误日志
   - 系统状态
   - 配置文件
   - 操作步骤

2. **联系技术支持**：
   - 邮箱：support@your-domain.com
   - 电话：+86-xxx-xxxx-xxxx
   - 在线支持：https://support.your-domain.com

3. **提供详细信息**：
   - 问题描述
   - 错误消息
   - 重现步骤
   - 环境信息