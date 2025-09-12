# 通知Pinia Store使用文档

## 概述

`useNotificationStore` 是一个基于 Pinia 的通知状态管理器，提供了完整的通知系统前端状态管理功能。该 Store 支持通知的 CRUD 操作、批量操作、实时更新、设置管理和持久化存储。

## 主要功能

### 1. 通知数据管理
- 获取通知列表（支持分页、筛选、排序）
- 创建新通知
- 更新通知内容
- 删除通知
- 标记已读/未读状态

### 2. 批量操作
- 批量标记已读
- 批量删除通知
- 批量归档/取消归档

### 3. 通知设置管理
- 获取用户通知设置
- 创建/更新/删除通知设置
- 重置为默认设置

### 4. 实时通信
- WebSocket 连接管理
- 实时通知更新
- 自动重连机制

### 5. 统计和分析
- 通知统计信息
- 通知摘要
- 按类型/状态/优先级分组

### 6. 性能优化
- 本地持久化存储
- 自动刷新机制
- 防抖处理
- 缓存优化

## 基本使用

### 1. 初始化 Store

```typescript
import { useNotificationStore } from '@/stores/notification'

const notificationStore = useNotificationStore()

// 初始化 Store（通常在应用启动时调用）
await notificationStore.initialize(userId)
```

### 2. 获取通知列表

```typescript
// 获取所有通知
await notificationStore.fetchNotifications()

// 带筛选条件获取通知
await notificationStore.fetchNotifications({
  userId: 'user123',
  type: NotificationType.COMMENT,
  isRead: false,
  page: 1,
  pageSize: 20
})
```

### 3. 通知操作

```typescript
// 创建通知
const newNotification = await notificationStore.createNotification({
  userId: 'user123',
  type: NotificationType.COMMENT,
  title: '新评论通知',
  content: '您的代码片段收到了新评论',
  priority: NotificationPriority.NORMAL
})

// 标记已读
await notificationStore.markAsRead(['notification1', 'notification2'])

// 标记所有已读
await notificationStore.markAllAsRead()

// 删除通知
await notificationStore.deleteNotification('notification1')
```

### 4. 通知设置管理

```typescript
// 获取通知设置
await notificationStore.fetchSettings('user123')

// 创建通知设置
const newSetting = await notificationStore.createSetting({
  userId: 'user123',
  enableInApp: true,
  enableEmail: false,
  enablePush: true,
  frequency: NotificationFrequency.IMMEDIATE
})

// 更新通知设置
await notificationStore.updateSetting('setting1', {
  enableEmail: true,
  frequency: NotificationFrequency.DAILY
})
```

### 5. 实时通信

```typescript
// 连接 WebSocket
await notificationStore.connectWebSocket('user123')

// 断开连接
notificationStore.disconnectWebSocket()

// 检查连接状态
if (notificationStore.isWebSocketConnected) {
  console.log('WebSocket 已连接')
}
```

### 6. 搜索和筛选

```typescript
// 搜索通知
const searchResults = notificationStore.searchNotifications('关键词')

// 设置筛选条件
notificationStore.setFilter({
  type: NotificationType.COMMENT,
  priority: NotificationPriority.HIGH,
  isRead: false
})

// 重置筛选条件
notificationStore.resetFilter()
```

## 状态和计算属性

### 状态

```typescript
// 基础状态
notificationStore.notifications      // 通知列表
notificationStore.settings          // 通知设置列表
notificationStore.loading           // 加载状态
notificationStore.saving            // 保存状态
notificationStore.error             // 错误信息
notificationStore.unreadCount       // 未读通知数量

// 分页信息
notificationStore.pagination       // 分页信息
notificationStore.totalPages       // 总页数
notificationStore.hasNextPage      // 是否有下一页
notificationStore.hasPreviousPage  // 是否有上一页

// WebSocket 状态
notificationStore.wsConnection     // WebSocket 连接信息
notificationStore.wsStatus         // WebSocket 连接状态
```

### 计算属性

```typescript
// 通知分组
notificationStore.unreadNotifications         // 未读通知
notificationStore.highPriorityNotifications    // 高优先级通知
notificationStore.systemNotifications          // 系统通知
notificationStore.userNotifications           // 用户通知
notificationStore.notificationsByType         // 按类型分组
notificationStore.notificationsByStatus       // 按状态分组

// 状态检查
notificationStore.isInitialized               // 是否已初始化
notificationStore.hasError                   // 是否有错误
notificationStore.isLoading                  // 是否正在加载
notificationStore.isSaving                   // 是否正在保存
notificationStore.hasUnreadNotifications      // 是否有未读通知
notificationStore.isWebSocketConnected       // WebSocket 是否已连接
```

## 高级功能

### 1. 自动刷新

```typescript
// 启动自动刷新（默认30秒）
notificationStore.startAutoRefresh()

// 停止自动刷新
notificationStore.stopAutoRefresh()

// 设置自定义刷新间隔
notificationStore.setAutoRefreshInterval(60000) // 60秒
```

### 2. 持久化存储

Store 会自动将以下数据保存到 localStorage：
- 通知设置
- 筛选条件
- 未读通知数量
- 最后刷新时间

### 3. 错误处理

```typescript
// 检查错误
if (notificationStore.hasError) {
  console.error(notificationStore.error)
}

// 清除错误
notificationStore.clearError()
```

### 4. 重置状态

```typescript
// 重置所有状态
notificationStore.$reset()
```

## 事件监听

Store 提供了多个监听器来响应状态变化：

```typescript
// 监听未读数量变化（自动更新页面标题）
watch(notificationStore.unreadCount, (newCount) => {
  if (newCount > 0) {
    document.title = `(${newCount}) ${document.title.replace(/^\\(\\d+\\)\\s*/, '')}`
  }
})

// 监听设置变化（自动保存到本地存储）
watch(notificationStore.settings, () => {
  notificationStore.saveToStorage()
}, { deep: true })
```

## 完整示例

```typescript
<template>
  <div class="notification-panel">
    <!-- 通知列表 -->
    <div v-if="notificationStore.isLoading">加载中...</div>
    <div v-else-if="notificationStore.hasError">{{ notificationStore.error }}</div>
    <div v-else>
      <div v-for="notification in notificationStore.currentPageNotifications" 
           :key="notification.id"
           :class="{ 'unread': !notification.isRead }">
        <h3>{{ notification.title }}</h3>
        <p>{{ notification.content }}</p>
        <span>{{ notification.timeSinceCreated }}</span>
        <button @click="markAsRead(notification.id)">标记已读</button>
        <button @click="deleteNotification(notification.id)">删除</button>
      </div>
    </div>
    
    <!-- 分页控制 -->
    <div class="pagination">
      <button :disabled="!notificationStore.hasPreviousPage" 
              @click="notificationStore.fetchNotifications({ page: notificationStore.pagination.currentPage - 1 })">
        上一页
      </button>
      <span>第 {{ notificationStore.pagination.currentPage }} 页，共 {{ notificationStore.totalPages }} 页</span>
      <button :disabled="!notificationStore.hasNextPage" 
              @click="notificationStore.fetchNotifications({ page: notificationStore.pagination.currentPage + 1 })">
        下一页
      </button>
    </div>
    
    <!-- 批量操作 -->
    <div class="batch-actions">
      <button @click="notificationStore.markAllAsRead()">全部标记已读</button>
      <button @click="deleteSelectedNotifications">删除选中</button>
    </div>
  </div>
</template>

<script setup>
import { useNotificationStore } from '@/stores/notification'
import { onMounted } from 'vue'

const notificationStore = useNotificationStore()

onMounted(async () => {
  await notificationStore.initialize('user123')
})

const markAsRead = async (notificationId) => {
  try {
    await notificationStore.markAsRead([notificationId])
  } catch (error) {
    console.error('标记已读失败:', error)
  }
}

const deleteNotification = async (notificationId) => {
  try {
    await notificationStore.deleteNotification(notificationId)
  } catch (error) {
    console.error('删除通知失败:', error)
  }
}

const deleteSelectedNotifications = async () => {
  const selectedIds = ['notification1', 'notification2'] // 选中的通知ID
  try {
    await notificationStore.deleteNotifications(selectedIds)
  } catch (error) {
    console.error('批量删除失败:', error)
  }
}
</script>
```

## 注意事项

1. **初始化**: 确保在使用 Store 之前调用 `initialize()` 方法
2. **错误处理**: 所有异步操作都可能抛出错误，建议使用 try-catch 进行错误处理
3. **性能**: 大量通知时注意分页处理，避免一次性加载过多数据
4. **内存管理**: 及时断开 WebSocket 连接，避免内存泄漏
5. **类型安全**: 使用 TypeScript 时，确保正确导入和使用类型定义

## 类型定义

Store 导出了完整的 TypeScript 类型定义：

```typescript
export type NotificationStore = ReturnType<typeof useNotificationStore>
```

包含所有状态、计算属性和方法的类型定义。