# CodeShare 系统设置功能数据库迁移指南

## 概述

本指南说明了如何部署 CodeShare 系统设置功能的数据库迁移脚本。迁移脚本为系统设置功能添加了必要的数据表、索引、视图、触发器和存储过程。

## 文件说明

### 迁移脚本
- `SystemSettingsMigration_v1.0.0.sql` - MySQL 版本迁移脚本
- `SystemSettingsMigration_v1.0.0_Sqlite.sql` - SQLite 版本迁移脚本

### 回滚脚本
- `SystemSettingsRollback_v1.0.0.sql` - MySQL 版本回滚脚本
- `SystemSettingsRollback_v1.0.0_Sqlite.sql` - SQLite 版本回滚脚本

## 迁移内容

### 新增数据表
1. **SystemSettings** - 系统配置信息表
   - 存储站点设置、安全设置、功能设置、邮件设置
   - 使用 JSON 格式存储配置数据，提供最大灵活性

2. **SettingsHistory** - 设置变更历史表
   - 记录所有设置变更的详细信息
   - 支持审计追踪和变更分析

3. **ShareTokens** - 分享令牌表
   - 管理代码片段分享的令牌和权限
   - 支持过期时间和访问控制

4. **ShareAccessLogs** - 分享访问日志表
   - 记录分享访问的详细信息
   - 支持访问统计和分析

### 新增视图
- **ShareStatsView** - 分享统计视图
  - 提供分享令牌的统计信息
  - 包括访问次数、访客数量等

### 新增存储过程
- **CleanupExpiredShareTokens** - 清理过期分享令牌
- **GetShareStats** - 获取特定分享的统计信息

### 新增触发器
- **update_last_accessed_at** - 自动更新分享令牌的访问时间和计数器

## 部署步骤

### 1. 备份数据库
在执行迁移之前，请务必备份现有数据库：

```bash
# MySQL 备份
mysqldump -u username -p database_name > backup_$(date +%Y%m%d_%H%M%S).sql

# SQLite 备份
sqlite3 database.db ".backup backup_$(date +%Y%m%d_%H%M%S).db"
```

### 2. 选择合适的迁移脚本
根据您的数据库类型选择相应的迁移脚本：

- **生产环境**：使用 MySQL 版本 (`SystemSettingsMigration_v1.0.0.sql`)
- **开发环境**：使用 SQLite 版本 (`SystemSettingsMigration_v1.0.0_Sqlite.sql`)

### 3. 执行迁移脚本

#### MySQL 环境
```bash
# 使用 MySQL 命令行工具
mysql -u username -p database_name < SystemSettingsMigration_v1.0.0.sql

# 或者在 MySQL 客户端中执行
source /path/to/SystemSettingsMigration_v1.0.0.sql;
```

#### SQLite 环境
```bash
# 使用 SQLite 命令行工具
sqlite3 database.db < SystemSettingsMigration_v1.0.0_Sqlite.sql

# 或者在 SQLite 客户端中执行
.read /path/to/SystemSettingsMigration_v1.0.0_Sqlite.sql
```

### 4. 验证迁移结果
迁移脚本执行后会显示详细的验证信息，包括：

- 表创建状态
- 索引创建状态
- 视图创建状态
- 存储过程创建状态
- 触发器创建状态
- 默认数据插入状态

### 5. 测试应用程序功能
迁移完成后，请测试以下功能：

1. 系统设置页面访问
2. 站点设置配置
3. 安全设置配置
4. 功能设置配置
5. 邮件设置配置
6. 设置历史查看
7. 导入导出功能
8. 代码片段分享功能

## 回滚步骤

如果需要回滚迁移，请执行相应的回滚脚本：

### MySQL 回滚
```bash
mysql -u username -p database_name < SystemSettingsRollback_v1.0.0.sql
```

### SQLite 回滚
```bash
sqlite3 database.db < SystemSettingsRollback_v1.0.0_Sqlite.sql
```

## 故障排除

### 常见问题

1. **权限错误**
   - 确保数据库用户有创建表、索引、视图、存储过程和触发器的权限
   - MySQL 用户需要 CREATE、ALTER、DROP、INDEX、CREATE ROUTINE、TRIGGER 权限

2. **外键约束错误**
   - 确保 CodeSnippets 和 Users 表已存在
   - 检查外键约束是否正确

3. **JSON 字段支持**
   - MySQL 5.7+ 或 MariaDB 10.2+ 支持 JSON 字段
   - 较低版本需要使用 TEXT 字段替代

4. **SQLite 版本兼容性**
   - SQLite 3.31+ 支持更丰富的 JSON 功能
   - 较低版本可能需要调整 JSON 相关功能

### 验证步骤

执行以下 SQL 查询验证迁移是否成功：

```sql
-- 检查表是否存在
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = DATABASE() 
AND table_name IN ('SystemSettings', 'SettingsHistory', 'ShareTokens', 'ShareAccessLogs');

-- 检查默认数据
SELECT COUNT(*) as system_settings_count FROM SystemSettings;

-- 检查索引
SELECT index_name, table_name 
FROM information_schema.statistics 
WHERE table_schema = DATABASE()
AND table_name IN ('SystemSettings', 'SettingsHistory', 'ShareTokens', 'ShareAccessLogs');
```

## 性能优化建议

1. **索引优化**
   - 迁移脚本已包含所有必要的索引
   - 根据查询模式可以进一步优化索引

2. **分区表**
   - 对于大型系统，考虑对 SettingsHistory 表进行分区
   - 可以按时间或设置类型进行分区

3. **归档策略**
   - 定期归档旧的设置历史记录
   - 考虑创建归档表存储历史数据

4. **缓存策略**
   - 系统设置数据适合缓存
   - 考虑使用 Redis 或内存缓存提高性能

## 安全考虑

1. **数据加密**
   - 敏感配置数据应该在应用层加密
   - 考虑使用数据库级别的加密功能

2. **访问控制**
   - 限制对 SystemSettings 表的直接访问
   - 使用存储过程进行数据操作

3. **审计日志**
   - SettingsHistory 表提供完整的审计跟踪
   - 定期审查设置变更记录

## 联系信息

如果在迁移过程中遇到问题，请联系开发团队或查看项目文档获取更多帮助。

---

**版本**: v1.0.0  
**创建日期**: 2025-01-01  
**最后更新**: 2025-01-01