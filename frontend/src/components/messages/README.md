# 消息组件库

这是一个基于Vue 3 + Composition API + TypeScript的现代化消息组件库，提供了完整的消息功能实现。

## 组件列表

### 核心组件

#### 增强组件 (Enhanced Components)

为了提供更强大的功能，我们创建了增强版的消息组件：

##### EnhancedMessageList
增强版消息列表组件，支持虚拟滚动、无限滚动和实时更新。

**主要特性：**
- **虚拟滚动**：高效渲染大量消息
- **无限滚动**：自动加载更多消息
- **实时更新**：WebSocket实时消息推送
- **滚动模式切换**：支持多种滚动模式
- **连接状态指示**：显示实时连接状态

**使用方法：**
```vue
<template>
  <EnhancedMessageList
    :messages="messages"
    :enable-virtual-scroll="true"
    :enable-infinite-scroll="true"
    :enable-realtime="true"
    @realtime-message="handleRealtimeMessage"
  />
</template>
```

##### EnhancedMessageItem
增强版消息项组件，支持消息反应、在线编辑等功能。

**主要特性：**
- **消息反应**：Emoji表情反应
- **在线编辑**：直接编辑消息内容
- **右键菜单**：上下文操作菜单
- **消息状态**：完整的消息状态显示
- **操作按钮**：丰富的操作选项

##### EnhancedMessageForm
增强版消息表单组件，支持富文本编辑和定时发送。

**主要特性：**
- **富文本编辑**：完整的富文本编辑功能
- **@提及功能**：智能用户提及
- **定时发送**：支持定时发送消息
- **模板系统**：消息模板管理
- **拖拽上传**：直观的文件上传

#### MessageList
消息列表组件，支持搜索、过滤、分页和批量操作。

**基础特性：**
- 虚拟滚动，支持大量数据
- 实时搜索和过滤
- 批量选择和操作
- 响应式设计
- 主题切换支持

**增强特性：**
- **多滚动模式**：虚拟滚动、无限滚动、传统分页
- **实时更新**：WebSocket支持，实时消息推送
- **性能优化**：动态高度计算，懒加载
- **交互增强**：拖拽排序，快捷键支持

**使用方法：**
```vue
<template>
  <MessageList
    :messages="messages"
    :loading="loading"
    :pagination="pagination"
    @message-select="handleMessageSelect"
    @message-delete="handleMessageDelete"
    @filter-change="handleFilterChange"
    @page-change="handlePageChange"
  />
</template>

<script setup lang="ts">
import { MessageList } from '@/components/messages'
import { useMessageStore } from '@/stores/message'

const messageStore = useMessageStore()
const { messages, loading, pagination } = storeToRefs(messageStore)

const handleMessageSelect = (message: Message) => {
  // 处理消息选择
}

const handleMessageDelete = (message: Message) => {
  // 处理消息删除
}
</script>
```

#### MessageItem
单个消息组件，用于显示消息详情和操作按钮。

**基础特性：**
- 支持多种消息状态显示
- 优先级指示器
- 附件预览
- 操作按钮（回复、转发、删除等）
- 深色模式支持

**增强特性：**
- **消息反应**：Emoji表情反应支持
- **在线编辑**：直接编辑消息内容
- **右键菜单**：上下文操作菜单
- **重发功能**：失败消息重发
- **消息引用**：显示回复和转发关系

#### MessageForm
消息表单组件，支持创建和编辑消息。

**基础特性：**
- 富文本编辑器
- 文件上传
- 草稿保存
- 收件人选择
- 消息预览

**增强特性：**
- **@提及功能**：智能用户提及建议
- **定时发送**：支持定时发送消息
- **模板系统**：消息模板快速应用
- **拖拽上传**：支持拖拽上传文件
- **实时预览**：消息内容实时预览
- **快捷键支持**：常用操作快捷键

#### MessageConversation
对话界面组件，支持实时对话和消息线程。

**特性：**
- 实时消息更新
- 消息线程
- 参与者管理
- 输入状态显示
- Emoji支持

### 功能组件

#### MessageSearch
高级搜索组件，支持搜索建议和历史记录。

**特性：**
- 智能搜索建议
- 搜索历史管理
- 高级过滤选项
- 搜索结果导出

#### MessageFilter
消息过滤器组件，提供全面的过滤功能。

**特性：**
- 多维度过滤（类型、状态、优先级、日期等）
- 快速过滤按钮
- 预设过滤器管理
- 活跃过滤器显示

#### MessageAttachmentUpload
文件上传组件，支持拖拽上传。

**特性：**
- 拖拽上传
- 多文件上传
- 进度显示
- 文件类型验证
- 上传队列管理

#### MessageAttachmentDisplay
附件显示组件，支持多种文件类型预览。

**特性：**
- 图片预览
- 视频/音频播放
- 文档预览
- 代码高亮
- 缩放控制

#### AttachmentCard
附件卡片组件，用于显示单个附件信息。

**特性：**
- 文件类型图标
- 状态指示器
- 操作按钮
- 缩略图显示

## 配置和自定义

### 默认配置
```typescript
// 可以通过 props 或全局配置自定义组件行为
const messageConfig = {
  itemsPerPage: 20,
  maxFileSize: 10 * 1024 * 1024, // 10MB
  allowedFileTypes: ['image/*', 'application/pdf', 'text/*'],
  enableRealTime: true,
  enableNotifications: true
}
```

### 主题定制
```css
/* 自定义主题变量 */
:root {
  --message-primary-color: #3b82f6;
  --message-secondary-color: #6b7280;
  --message-success-color: #10b981;
  --message-error-color: #ef4444;
  --message-warning-color: #f59e0b;
}

/* 深色模式 */
.dark {
  --message-primary-color: #60a5fa;
  --message-secondary-color: #9ca3af;
  /* ... */
}
```

## 事件处理

### 主要事件
```typescript
// 消息选择
@message-select="(message: Message) => void"

// 消息删除
@message-delete="(message: Message) => void"

// 过滤器变化
@filter-change="(filters: MessageFilters) => void"

// 文件上传
@file-upload="(files: File[]) => void"

// 搜索
@search="(query: string) => void"
```

## 工具函数

### 格式化函数
```typescript
import { 
  formatMessageDate, 
  formatFileSize, 
  getMessageStatusText 
} from '@/components/messages'

// 格式化消息日期
const formattedDate = formatMessageDate(message.createdAt)

// 格式化文件大小
const formattedSize = formatFileSize(file.size)

// 获取消息状态文本
const statusText = getMessageStatusText(message.status)
```

### 验证函数
```typescript
import { 
  validateFileSize, 
  validateFileType 
} from '@/components/messages'

// 验证文件大小
const isValidSize = validateFileSize(file, maxSize)

// 验证文件类型
const isValidType = validateFileType(file, allowedTypes)
```

## 无障碍支持

所有组件都遵循WCAG 2.1指南，提供：
- 键盘导航支持
- 屏幕阅读器友好的ARIA标签
- 高对比度模式支持
- 减少动画选项

## 浏览器支持

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## 性能优化

- 虚拟滚动（大数据量）
- 懒加载（图片和附件）
- 防抖和节流（搜索和过滤）
- 组件懒加载
- 代码分割

## 故障排除

### 常见问题

1. **文件上传失败**
   - 检查文件大小限制
   - 验证文件类型
   - 确认API端点配置

2. **消息不显示**
   - 检查网络连接
   - 验证API响应格式
   - 确认权限设置

3. **样式问题**
   - 检查Tailwind CSS配置
   - 确认主题变量设置
   - 验证CSS导入

## 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建Pull Request

## 许可证

MIT License