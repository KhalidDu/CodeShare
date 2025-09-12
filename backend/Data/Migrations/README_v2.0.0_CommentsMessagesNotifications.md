# CodeShare 评论、消息、通知系统数据库迁移脚本

## 概述
本迁移脚本为 CodeShare 项目添加完整的评论、消息和通知系统功能，支持 MySQL 和 SQLite 两种数据库。

**版本**: v2.0.0  
**任务编号**: DS-04  
**创建日期**: 2025-01-09  
**适用数据库**: MySQL 5.7+, SQLite 3.25+

## 文件清单

### 主要迁移脚本
1. `CommentsMessagesNotificationsMigration_v2.0.0.sql` - MySQL 版本迁移脚本
2. `CommentsMessagesNotificationsMigration_v2.0.0_Sqlite.sql` - SQLite 版本迁移脚本

### 回滚脚本
3. `CommentsMessagesNotificationsRollback_v2.0.0.sql` - MySQL 版本回滚脚本
4. `CommentsMessagesNotificationsRollback_v2.0.0_Sqlite.sql` - SQLite 版本回滚脚本

## 新增数据表

### 评论系统 (Comments System)
- **Comments** - 评论主表
- **CommentLikes** - 评论点赞记录
- **CommentReports** - 评论举报记录

### 消息系统 (Messages System)
- **Messages** - 消息主表
- **MessageAttachments** - 消息附件
- **MessageDrafts** - 消息草稿
- **MessageDraftAttachments** - 草稿附件
- **Conversations** - 会话管理
- **ConversationParticipants** - 会话参与者

### 通知系统 (Notifications System)
- **Notifications** - 通知主表
- **NotificationSettings** - 用户通知设置
- **NotificationDeliveryHistory** - 通知发送历史

## 新增视图

### 统计视图
- **CommentStatsView** - 评论统计
- **MessageStatsView** - 消息统计
- **ConversationStatsView** - 会话统计
- **NotificationStatsView** - 通知统计
- **NotificationTypeStatsView** - 通知类型统计

## 新增触发器

### 自动更新触发器
- `update_comments_updated_at` - 更新评论时间
- `update_comment_like_count` - 更新评论点赞数
- `update_comment_reply_count` - 更新评论回复数
- `update_message_read_status` - 更新消息阅读状态
- `update_message_reply_count` - 更新消息回复状态
- `update_conversation_activity` - 更新会话活动时间
- `update_notification_read_status` - 更新通知阅读状态
- `update_notification_confirmation` - 更新通知确认状态

## 新增存储过程

### 管理存储过程
- `CleanupExpiredComments` - 清理过期评论
- `CleanupExpiredMessages` - 清理过期消息
- `CleanupExpiredNotifications` - 清理过期通知
- `GetCommentStats` - 获取评论统计
- `GetMessageStats` - 获取消息统计
- `GetNotificationStats` - 获取通知统计
- `CleanupExpiredShareTokens` - 清理过期分享令牌
- `GetShareStats` - 获取分享统计

## 执行说明

### MySQL 环境
```bash
# 执行迁移
mysql -u username -p database_name < CommentsMessagesNotificationsMigration_v2.0.0.sql

# 执行回滚
mysql -u username -p database_name < CommentsMessagesNotificationsRollback_v2.0.0.sql
```

### SQLite 环境
```bash
# 执行迁移
sqlite3 database_name.db < CommentsMessagesNotificationsMigration_v2.0.0_Sqlite.sql

# 执行回滚
sqlite3 database_name.db < CommentsMessagesNotificationsRollback_v2.0.0_Sqlite.sql
```

## 数据库要求

### MySQL 要求
- MySQL 5.7 或更高版本
- 支持 JSON 数据类型
- 支持 UUID 函数
- 支持外键约束

### SQLite 要求
- SQLite 3.25 或更高版本
- 支持外键约束
- 支持 JSON 扩展

## 性能优化

### 索引策略
- 为所有外键创建索引
- 为常用查询字段创建复合索引
- 为状态和时间字段创建优化索引
- 为全文搜索字段创建全文索引

### 查询优化
- 使用覆盖索引减少回表操作
- 合理使用复合索引提高查询效率
- 为统计查询创建专用视图

## 数据完整性

### 外键约束
- 所有关系表都有适当的外键约束
- 级联删除确保数据一致性
- 设置 NULL 保持数据完整性

### 数据验证
- 使用触发器自动维护计数器
- 状态变更时自动更新时间戳
- 确保数据的一致性和准确性

## 安全考虑

### 权限控制
- 评论举报和审核机制
- 消息发送和接收权限
- 通知设置的个性化配置

### 数据保护
- 软删除机制保护重要数据
- 敏感操作记录日志
- 用户隐私数据保护

## 监控和维护

### 定期清理
- 过期评论自动清理
- 过期消息自动归档
- 过期通知自动处理

### 性能监控
- 数据库查询性能监控
- 索引使用情况分析
- 定期优化数据库表

## 回滚策略

### 完整回滚
- 删除所有新增表
- 删除所有新增视图
- 删除所有新增触发器
- 删除所有新增存储过程

### 数据安全
- 回滚前数据备份
- 分步骤执行回滚
- 回滚结果验证

## 测试建议

### 功能测试
- 评论创建、回复、点赞功能
- 消息发送、接收、附件功能
- 通知创建、发送、阅读功能

### 性能测试
- 大量评论的查询性能
- 消息发送的并发性能
- 通知推送的实时性能

### 安全测试
- 评论内容安全检查
- 消息内容过滤
- 通知权限控制

## 常见问题

### Q: 迁移脚本执行失败怎么办？
A: 检查数据库版本是否满足要求，确保有足够的权限执行脚本。

### Q: 如何验证迁移是否成功？
A: 执行迁移脚本后，检查输出的验证结果，确保所有表、视图、触发器都创建成功。

### Q: 回滚后数据如何恢复？
A: 建议在执行迁移前备份数据库，回滚后可以从备份恢复。

### Q: SQLite 和 MySQL 版本有什么区别？
A: SQLite 版本使用不同的数据类型和语法，但功能完全相同，主要用于开发环境。

## 技术支持

如有问题或建议，请联系开发团队或创建 Issue。

---
**注意**: 本迁移脚本包含完整的数据结构定义，请在生产环境执行前进行充分测试。