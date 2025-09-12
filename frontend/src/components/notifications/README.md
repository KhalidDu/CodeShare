# CodeShare前端通知系统

CodeShare项目的前端通知系统，基于Vue 3、TypeScript和Pinia构建，提供了完整的通知管理功能。

## 📋 系统特性

### 🔧 技术栈
- **Vue 3 Composition API** - 现代化的Vue开发模式
- **TypeScript** - 完整的类型安全保障
- **Pinia** - 轻量级状态管理
- **Tailwind CSS** - 现代化的样式解决方案
- **Element Plus** - UI组件库集成
- **WebSocket** - 实时通知推送

### 🎯 核心功能

#### 1. 通知管理
- ✅ 通知列表显示与分页
- ✅ 通知搜索与筛选
- ✅ 通知状态管理（已读/未读/归档/删除）
- ✅ 批量操作支持
- ✅ 通知排序功能

#### 2. 通知展示
- ✅ 多种通知类型（信息/成功/警告/错误/系统/用户/安全/更新）
- ✅ 优先级指示器（低/普通/高/紧急）
- ✅ 多渠道通知（应用内/邮件/推送/短信）
- ✅ 响应式设计，支持移动端
- ✅ 暗色模式支持

#### 3. 实时功能
- ✅ WebSocket实时推送
- ✅ 自动刷新机制
- ✅ 连接状态监控
- ✅ 重连机制

#### 4. 设置管理
- ✅ 基础设置配置
- ✅ 通知渠道管理
- ✅ 通知类型设置
- ✅ 免打扰时间设置
- ✅ 频率控制设置
- ✅ 外观和隐私设置

#### 5. 高级功能
- ✅ 虚拟滚动（大数据量优化）
- ✅ 通知分组显示
- ✅ 通知统计信息
- ✅ 通知历史记录
- ✅ 通知导出功能
- ✅ 通知模板管理

## 📁 文件结构

```
frontend/src/
├── components/notifications/
│   ├── index.ts                 # 统一导出文件
│   ├── NotificationList.vue     # 通知列表组件 (1,146行)
│   ├── NotificationItem.vue     # 通知项组件 (1,234行)
│   └── NotificationSettings.vue # 通知设置组件 (1,099行)
├── stores/
│   ├── notificationStore.ts     # 主要通知状态管理 (2,453行)
│   └── notifications.ts         # 简化通知状态管理 (578行)
├── types/
│   └── notifications.ts         # 完整类型定义 (507行)
└── services/
    └── notificationService.ts   # 通知服务API (52+行)
```

## 🚀 快速开始

### 1. 安装依赖

```bash
# 确保已安装必要的依赖
npm install vue@next pinia @vueuse/core
```

### 2. 在主应用中注册

```typescript
// main.ts
import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.mount('#app')
```

### 3. 使用通知列表组件

```vue
<template>
  <div>
    <!-- 基础通知列表 -->
    <NotificationList />
    
    <!-- 带配置的通知列表 -->
    <NotificationList
      :max-items="20"
      :show-filters="true"
      :show-search="true"
      :show-pagination="true"
      :show-stats="true"
      :auto-refresh="true"
      :refresh-interval="30000"
      @notification-click="handleNotificationClick"
      @notification-read="handleNotificationRead"
      @settings-click="handleSettingsClick"
    />
  </div>
</template>

<script setup lang="ts">
import { NotificationList } from '@/components/notifications'

function handleNotificationClick(notification) {
  console.log('通知被点击:', notification)
}

function handleNotificationRead(notification) {
  console.log('通知已读:', notification)
}

function handleSettingsClick() {
  console.log('打开设置页面')
}
</script>
```

### 4. 使用通知设置组件

```vue
<template>
  <div>
    <NotificationSettings
      @save="handleSettingsSave"
      @cancel="handleSettingsCancel"
      @test="handleTestNotification"
    />
  </div>
</template>

<script setup lang="ts">
import { NotificationSettings } from '@/components/notifications'

function handleSettingsSave(settings) {
  console.log('设置已保存:', settings)
}

function handleSettingsCancel() {
  console.log('取消设置更改')
}

function handleTestNotification(channel) {
  console.log('测试通知渠道:', channel)
}
</script>
```

### 5. 使用通知Store

```typescript
import { useNotificationsStore } from '@/stores/notifications'

const notificationsStore = useNotificationsStore()

// 获取通知列表
await notificationsStore.fetchNotifications({
  pageNumber: 1,
  pageSize: 20,
  sortBy: 'createdAt',
  sortOrder: 'desc'
})

// 标记通知为已读
await notificationsStore.markAsRead(notificationId)

// 标记所有通知为已读
await notificationsStore.markAllAsRead()

// 删除通知
await notificationsStore.deleteNotification(notificationId)

// 获取通知设置
const settings = await notificationsStore.fetchNotificationSettings()

// 更新通知设置
await notificationsStore.updateNotificationSettings(newSettings)
```

## 🎨 组件API

### NotificationList Props

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| maxItems | number | undefined | 最大显示数量 |
| showHeader | boolean | true | 显示头部 |
| showStats | boolean | true | 显示统计信息 |
| showFilters | boolean | true | 显示筛选器 |
| showSearch | boolean | true | 显示搜索框 |
| showPagination | boolean | true | 显示分页 |
| showLoadMore | boolean | false | 显示加载更多 |
| showRefresh | boolean | true | 显示刷新按钮 |
| showMarkAllRead | boolean | true | 显示全部标记已读按钮 |
| showSettings | boolean | true | 显示设置按钮 |
| autoRefresh | boolean | false | 自动刷新 |
| refreshInterval | number | 30000 | 刷新间隔（毫秒） |
| title | string | '通知中心' | 标题 |
| description | string | '管理您的通知和消息' | 描述 |
| compact | boolean | false | 紧凑模式 |

### NotificationItem Props

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| notification | Notification | - | 通知对象（必需） |
| showIcon | boolean | true | 显示图标 |
| showTitle | boolean | true | 显示标题 |
| showMessage | boolean | true | 显示消息 |
| showTimestamp | boolean | true | 显示时间戳 |
| showPriority | boolean | false | 显示优先级 |
| showActions | boolean | true | 显示操作按钮 |
| showTags | boolean | false | 显示标签 |
| compact | boolean | false | 紧凑模式 |
| hoverable | boolean | true | 悬停效果 |
| clickable | boolean | true | 可点击 |

### NotificationSettings Props

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| settings | NotificationSettings | undefined | 设置对象 |
| title | string | '通知设置' | 标题 |
| description | string | '自定义您的通知偏好设置' | 描述 |
| compact | boolean | false | 紧凑模式 |

## 🔧 状态管理

### 主要Store功能

```typescript
// 通知Store
const notificationsStore = useNotificationsStore()

// 获取状态
const notifications = notificationsStore.notifications
const unreadCount = notificationsStore.unreadCount
const totalCount = notificationsStore.totalCount
const loading = notificationsStore.loading
const error = notificationsStore.error

// 操作方法
await notificationsStore.fetchNotifications(request)
await notificationsStore.markAsRead(id)
await notificationsStore.markAllAsRead()
await notificationsStore.deleteNotification(id)
await notificationsStore.fetchNotificationSettings()
await notificationsStore.updateNotificationSettings(settings)
```

### WebSocket连接

```typescript
// 连接WebSocket
await notificationsStore.connectWebSocket(userId)

// 断开连接
notificationsStore.disconnectWebSocket()

// 监听连接状态
const isConnected = notificationsStore.isWebSocketConnected
```

## 🎯 通知类型

### 通知类型枚举
- `Info` - 信息通知
- `Success` - 成功通知
- `Warning` - 警告通知
- `Error` - 错误通知
- `System` - 系统通知
- `User` - 用户通知
- `Security` - 安全通知
- `Update` - 更新通知

### 通知优先级
- `Low` - 低优先级
- `Normal` - 普通优先级
- `High` - 高优先级
- `Urgent` - 紧急优先级

### 通知状态
- `Unread` - 未读
- `Read` - 已读
- `Archived` - 已归档
- `Deleted` - 已删除

### 通知渠道
- `InApp` - 应用内通知
- `Email` - 邮件通知
- `Push` - 推送通知
- `SMS` - 短信通知

## 📱 响应式设计

### 断点支持
- **Desktop**: ≥ 1024px
- **Tablet**: 768px - 1023px
- **Mobile**: < 768px

### 响应式特性
- 自适应布局
- 触摸友好的交互
- 移动端优化的UI
- 横屏/竖屏支持

## 🌙 暗色模式支持

### 自动切换
- 跟随系统主题
- 手动切换选项
- 平滑过渡动画

### 暗色模式特性
- 优化的颜色对比度
- 减少眼睛疲劳
- 夜间使用友好

## ♿ 可访问性

### 支持特性
- **ARIA标签** - 完整的屏幕阅读器支持
- **键盘导航** - 完整的键盘操作支持
- **焦点管理** - 清晰的焦点指示器
- **高对比度** - 支持高对比度模式
- **减少动画** - 支持减少动画偏好

### 快捷键
- `Tab` - 导航焦点
- `Enter` - 激活按钮
- `Space` - 切换开关
- `Escape` - 关闭弹窗

## 🧪 测试

### 组件测试
```bash
# 运行组件测试
npm run test:unit
```

### E2E测试
```bash
# 运行端到端测试
npm run test:e2e
```

## 📊 性能优化

### 虚拟滚动
- 支持大数据量列表
- 按需渲染DOM元素
- 平滑滚动体验

### 缓存机制
- 通知数据缓存
- 设置配置缓存
- 智能缓存更新

### 懒加载
- 组件按需加载
- 图片懒加载
- 路由懒加载

## 🔒 安全特性

### 数据安全
- XSS防护
- CSRF防护
- 输入验证
- 输出转义

### 隐私保护
- 敏感信息脱敏
- 用户授权控制
- 数据加密传输

## 🛠️ 开发指南

### 代码规范
- 遵循Vue 3 Composition API
- 使用TypeScript类型检查
- 遵循ESLint规则
- 使用Prettier格式化

### 提交规范
- 使用Conventional Commits
- 清晰的提交信息
- 相关的Issue引用

## 📈 更新日志

### v1.0.0 (2025-09-12)
- ✅ 完成基础通知系统
- ✅ 实现三个核心组件
- ✅ 完整的状态管理
- ✅ 完善的类型定义
- ✅ 响应式设计
- ✅ 暗色模式支持
- ✅ 可访问性优化

## 🤝 贡献指南

1. Fork项目
2. 创建功能分支
3. 提交更改
4. 创建Pull Request
5. 代码审查

## 📄 许可证

MIT License

## 🆘 支持

如有问题或建议，请：
- 创建Issue
- 发送邮件
- 联系开发团队

---

**CodeShare前端通知系统** - 让通知管理变得简单高效！ 🚀