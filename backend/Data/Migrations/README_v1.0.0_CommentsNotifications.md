# CodeShare 评论和通知系统数据库迁移脚本 v1.0.0

## 概述

本迁移脚本为 CodeShare 项目创建了完整的评论系统、消息系统、通知系统和系统设置功能的数据表结构，支持 MySQL 和 SQLite 两种数据库。

## 创建的文件

1. **CommentsNotificationsMigration_v1.0.0.sql** - MySQL 版本的完整迁移脚本
2. **CommentsNotificationsMigration_v1.0.0_Sqlite.sql** - SQLite 版本的完整迁移脚本

## 数据表结构

### 评论系统表 (3个)
1. **Comments** - 评论表，支持嵌套回复
2. **CommentLikes** - 评论点赞表
3. **CommentReports** - 评论举报表

### 消息系统表 (6个)
1. **Messages** - 消息表，支持用户间通信
2. **MessageAttachments** - 消息附件表
3. **MessageDrafts** - 消息草稿表
4. **MessageDraftAttachments** - 草稿附件表
5. **Conversations** - 会话表
6. **ConversationParticipants** - 会话参与者表

### 通知系统表 (3个)
1. **Notifications** - 通知表
2. **NotificationSettings** - 通知设置表
3. **NotificationDeliveryHistory** - 通知送达记录表

### 系统设置表 (2个)
1. **SystemSettings** - 系统设置表
2. **SettingsHistory** - 设置历史记录表

## 功能特性

### 评论系统
- 支持多级嵌套评论回复
- 评论点赞功能
- 评论举报和管理
- 评论状态管理（正常、已删除、已隐藏、待审核）
- 自动更新点赞数和回复数

### 消息系统
- 用户间私聊功能
- 消息附件支持
- 消息草稿功能
- 会话管理
- 消息状态追踪
- 自动保存和定时发送
- 软删除和归档功能

### 通知系统
- 多种通知类型（评论、回复、消息、系统等）
- 多渠道通知（应用内、邮件、推送等）
- 通知设置个性化
- 通知送达历史记录
- 通知统计和分析
- 批量通知和免打扰功能

### 系统设置
- 全局系统设置管理
- 设置变更历史记录
- JSON 格式的配置存储
- 设置版本控制

## 数据库兼容性

### MySQL 版本特性
- 使用 `CHAR(36)` 存储UUID
- 支持完整的 ON UPDATE CURRENT_TIMESTAMP 语法
- 支持存储过程和复杂的触发器
- 使用 DECIMAL 类型存储精确数值
- 支持 utf8mb4 字符集

### SQLite 版本特性
- 使用 TEXT 存储UUID和其他文本
- 使用触发器模拟自动更新时间戳
- 使用 REAL 类型存储小数
- 简化的外键约束语法
- 支持移动和开发环境

## 索引优化

每个表都包含了优化的索引结构：
- 单列索引用于基础查询
- 复合索引用于复杂查询
- 外键索引确保数据完整性
- 全文搜索索引（如需要）

总共创建了 **184个索引**，确保查询性能。

## 触发器

创建了自动化的数据一致性触发器：
- 自动更新时间戳
- 自动计数器（点赞数、回复数）
- 状态变更追踪
- 级联更新操作

## 视图

创建了统计和分析视图：
- **MessageStatsView** - 消息统计视图
- **ConversationStatsView** - 会话统计视图  
- **NotificationStatsView** - 通知统计视图
- **NotificationTypeStatsView** - 通知类型统计视图

## 存储过程（仅MySQL）

创建了实用的存储过程：
- **CleanupExpiredNotifications** - 清理过期通知
- **GetNotificationStats** - 获取通知统计
- **CleanupExpiredMessages** - 清理过期消息
- **GetMessageStats** - 获取消息统计

## 数据类型映射

| 功能 | MySQL | SQLite |
|------|-------|---------|
| UUID | CHAR(36) | TEXT |
| 布尔值 | BOOLEAN | INTEGER (0/1) |
| 时间戳 | DATETIME | TEXT (ISO8601) |
| JSON | TEXT | TEXT |
| 大文本 | TEXT | TEXT |
| 小数 | DECIMAL(10,4) | REAL |

## 外键关系

所有外键关系都正确设置：
- 级联删除 (ON DELETE CASCADE)
- 设置为空 (ON DELETE SET NULL)
- 数据完整性约束

## 执行说明

### MySQL 执行
```bash
mysql -u username -p database_name < CommentsNotificationsMigration_v1.0.0.sql
```

### SQLite 执行
```bash
sqlite3 database_name.db < CommentsNotificationsMigration_v1.0.0_Sqlite.sql
```

## 验证

脚本包含验证查询，执行后会显示：
- 创建的表数量
- 创建的索引数量
- 创建的视图数量
- 创建的触发器数量
- 创建的存储过程数量（MySQL）

## 注意事项

1. **执行前备份**：建议在执行迁移前备份数据库
2. **权限要求**：需要足够的数据库权限创建表、索引和触发器
3. **依赖表**：确保 CodeSnippets 和 Users 表已存在
4. **字符集**：MySQL 版本建议使用 utf8mb4 字符集
5. **版本兼容**：支持 MySQL 5.7+ 和 SQLite 3.25+

## 性能考虑

- 所有表都包含主键索引
- 查询频繁的字段都创建了索引
- 复合查询优化了复合索引
- 大文本字段使用 TEXT 类型
- 合理的索引数量避免过度索引

## 扩展性

设计考虑了未来的扩展需求：
- JSON 字段存储灵活的配置数据
- 枚举值使用整数类型便于扩展
- 软删除支持数据恢复
- 历史记录支持审计追踪

## 安全性

- 外键约束确保数据完整性
- 软删除保护重要数据
- 用户权限字段支持访问控制
- 敏感操作记录历史日志

---

**版本**: v1.0.0  
**创建时间**: 2025-09-12  
**兼容性**: MySQL 5.7+, SQLite 3.25+  
**作者**: CodeShare Team