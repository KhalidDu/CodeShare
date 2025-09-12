# WebSocket 客户端组件

CodeShare 项目提供了完整的 WebSocket 客户端解决方案，包含服务层、组件层和状态管理层。

## 组件概览

### 主要组件

1. **WebSocketClient.vue** - 主要的 WebSocket 客户端组件
2. **WebSocketConnectionManager.vue** - 连接管理器组件
3. **WebSocketMessageHandler.vue** - 消息处理器组件

### 核心服务

1. **websocketService.ts** - WebSocket 服务核心
2. **websocketAuthService.ts** - 认证服务
3. **websocketConnectionPool.ts** - 连接池管理
4. **useWebSocket.ts** - Vue 3 组合式函数

## 使用方法

### 基础使用

```vue
<template>
  <div>
    <WebSocketClient 
      :auto-connect="true"
      :show-details="false"
      :show-stats="true"
      @connected="handleConnected"
      @disconnected="handleDisconnected"
      @messageReceived="handleMessageReceived"
    />
  </div>
</template>

<script setup>
import WebSocketClient from '@/components/common/WebSocketClient.vue'

const handleConnected = (connectionId, userId) => {
  console.log('WebSocket 已连接:', connectionId, userId)
}

const handleDisconnected = (connectionId, userId, reason) => {
  console.log('WebSocket 已断开:', connectionId, userId, reason)
}

const handleMessageReceived = (message) => {
  console.log('收到消息:', message)
}
</script>
```

### 高级配置

```vue
<template>
  <div>
    <WebSocketClient 
      :config="customConfig"
      :show-details="true"
      :show-stats="true"
      :show-queue="true"
      :debug="true"
      :auto-connect="true"
      :show-message-sender="true"
      :show-message-display="true"
      :show-realtime-stats="true"
      :show-performance-chart="true"
      @connected="onConnected"
      @messageSent="onMessageSent"
      @messageClick="onMessageClick"
    />
  </div>
</template>

<script setup>
import WebSocketClient from '@/components/common/WebSocketClient.vue'
import { WebSocketMessageType, WebSocketMessagePriority } from '@/types/websocket'

const customConfig = {
  url: 'wss://your-websocket-server.com',
  reconnectInterval: 5000,
  maxReconnectAttempts: 10,
  enableHeartbeat: true,
  heartbeatInterval: 30000,
  messageQueueSize: 1000,
  enableCompression: true,
  debug: true
}

const onConnected = (connectionId, userId) => {
  console.log('连接已建立')
}

const onMessageSent = (result) => {
  if (result.success) {
    console.log('消息发送成功')
  } else {
    console.error('消息发送失败:', result.error)
  }
}

const onMessageClick = (message) => {
  console.log('点击了消息:', message)
}
</script>
```

## 组件属性

### WebSocketClient 组件属性

| 属性 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `service` | WebSocketService | - | 自定义 WebSocket 服务实例 |
| `showDetails` | boolean | false | 是否显示连接详细信息 |
| `showStats` | boolean | true | 是否显示统计信息 |
| `showQueue` | boolean | false | 是否显示队列状态 |
| `debug` | boolean | false | 是否启用调试模式 |
| `autoConnect` | boolean | false | 是否自动连接 |
| `config` | Partial<WebSocketConfig> | - | WebSocket 配置 |
| `showMessageSender` | boolean | true | 是否显示消息发送器 |
| `showMessageDisplay` | boolean | true | 是否显示消息显示器 |
| `showRealtimeStats` | boolean | true | 是否显示实时统计 |
| `showPerformanceChart` | boolean | false | 是否显示性能图表 |
| `showMessageFilter` | boolean | true | 是否显示消息过滤器 |
| `showMessageStats` | boolean | true | 是否显示消息统计 |
| `showMessagePagination` | boolean | false | 是否显示消息分页 |
| `enableMessageScroll` | boolean | true | 是否启用消息滚动 |
| `messagePageSize` | number | 20 | 消息页面大小 |
| `maxMessageHistory` | number | 1000 | 最大消息历史记录 |
| `autoMarkMessagesAsRead` | boolean | false | 自动标记消息已读 |
| `enableMessageSound` | boolean | false | 启用消息声音 |

### 事件

| 事件 | 参数 | 说明 |
|------|------|------|
| `connected` | (connectionId: string, userId: string) | 连接成功 |
| `disconnected` | (connectionId: string, userId: string, reason?: string) | 连接断开 |
| `error` | (error: Error) | 连接错误 |
| `reconnecting` | (connectionId: string, attempt: number) | 正在重连 |
| `reconnected` | (connectionId: string) | 重连成功 |
| `messageSent` | (result: WebSocketSendMessageResult) | 消息发送结果 |
| `messageReceived` | (message: WebSocketMessage) | 收到消息 |
| `messageClick` | (message: WebSocketMessage) | 点击消息 |
| `messageRead` | (message: WebSocketMessage) | 消息已读 |
| `messageDelete` | (message: WebSocketMessage) | 消息删除 |
| `filterChange` | (filters: any) | 过滤器变化 |
| `statsUpdate` | (stats: any) | 统计信息更新 |

## 独立使用组件

### 使用连接管理器

```vue
<template>
  <div>
    <WebSocketConnectionManager
      :auto-connect="true"
      :show-details="true"
      :show-stats="true"
      @connected="handleConnected"
    />
  </div>
</template>
```

### 使用消息处理器

```vue
<template>
  <div>
    <WebSocketMessageHandler
      :show-filter="true"
      :show-stats="true"
      :show-pagination="true"
      :enable-sound="true"
      @messageClick="handleMessageClick"
    />
  </div>
</template>
```

## 使用组合式函数

### 基础使用

```vue
<script setup>
import { useWebSocket } from '@/composables/useWebSocket'

const { state, getters, actions } = useWebSocket({
  url: 'wss://your-server.com',
  autoConnect: true
})

// 连接 WebSocket
await actions.connect()

// 发送消息
await actions.sendMessage('Hello World', WebSocketMessageType.USER)

// 监听消息
actions.registerMessageHandler(WebSocketMessageType.NOTIFICATION, (message) => {
  console.log('收到通知:', message)
})
</script>
```

### 高级使用

```vue
<script setup>
import { useWebSocket } from '@/composables/useWebSocket'
import { ref, watch } from 'vue'

const { state, getters, actions } = useWebSocket()

const connectionStatus = computed(() => getters.connectionStatusText)
const isConnected = computed(() => getters.isConnected)

// 自动连接
onMounted(async () => {
  await actions.connect()
})

// 监听连接状态变化
watch(() => state.connectionStatus.isConnected, (newStatus) => {
  if (newStatus) {
    console.log('WebSocket 已连接')
  } else {
    console.log('WebSocket 已断开')
  }
})
</script>
```

## 消息类型

### 支持的消息类型

- `NOTIFICATION` - 通知消息
- `SYSTEM` - 系统消息
- `USER` - 用户消息
- `STATUS_UPDATE` - 状态更新
- `ERROR` - 错误消息
- `ACKNOWLEDGMENT` - 确认消息
- `HEARTBEAT` - 心跳消息
- `CUSTOM` - 自定义消息

### 消息优先级

- `LOW` - 低优先级
- `NORMAL` - 普通优先级
- `HIGH` - 高优先级
- `URGENT` - 紧急优先级

## 认证和安全

### 使用认证服务

```typescript
import { webSocketAuthService } from '@/services/websocketAuthService'

// 认证连接
const authResult = await webSocketAuthService.authenticateConnection(connectionId, token)

// 检查权限
const permissionResult = await webSocketAuthService.checkPermission(
  connectionId, 
  WebSocketPermission.SEND_MESSAGE
)
```

### 连接池管理

```typescript
import { WebSocketConnectionPoolManager } from '@/services/websocketConnectionPool'

const pool = new WebSocketConnectionPoolManager({
  name: 'MyWebSocketPool',
  maxConnections: 10,
  minConnections: 2,
  enableAutoScaling: true
})

// 获取连接
const connection = await pool.getConnection()

// 释放连接
pool.releaseConnection(connectionId)
```

## 配置选项

### WebSocket 配置

```typescript
interface WebSocketConfig {
  // 基础配置
  url: string
  protocol?: string
  timeout?: number
  
  // 重连配置
  reconnectInterval?: number
  maxReconnectAttempts?: number
  enableReconnect?: boolean
  
  // 心跳配置
  enableHeartbeat?: boolean
  heartbeatInterval?: number
  heartbeatTimeout?: number
  
  // 消息配置
  messageQueueSize?: number
  enableCompression?: boolean
  enableMessageSigning?: boolean
  
  // 性能配置
  enablePerformanceMetrics?: boolean
  metricsCollectionInterval?: number
  
  // 调试配置
  debug?: boolean
  enableLogging?: boolean
  logLevel?: 'debug' | 'info' | 'warn' | 'error'
}
```

## 性能优化

### 连接池优化

- 使用连接池复用连接
- 启用自动扩缩容
- 配置合理的超时时间
- 监控连接健康状态

### 消息优化

- 启用消息压缩
- 使用消息优先级队列
- 批量处理消息
- 实现消息去重

### 缓存策略

- 消息历史缓存
- 连接状态缓存
- 性能指标缓存
- 错误信息缓存

## 错误处理

### 连接错误

```typescript
try {
  await actions.connect()
} catch (error) {
  console.error('连接失败:', error)
  // 可以尝试重连
  await actions.reconnect()
}
```

### 消息发送错误

```typescript
try {
  const result = await actions.sendMessage(message, type)
  if (!result.success) {
    console.error('消息发送失败:', result.error)
  }
} catch (error) {
  console.error('发送消息异常:', error)
}
```

## 调试和监控

### 启用调试模式

```vue
<WebSocketClient 
  :debug="true"
  :show-details="true"
  :show-performance-chart="true"
/>
```

### 性能监控

```typescript
// 获取性能指标
const metrics = actions.getPerformanceMetrics()

console.log('连接成功率:', metrics.connectionSuccessRate)
console.log('消息成功率:', metrics.messageSuccessRate)
console.log('平均延迟:', metrics.averageMessageSendTime)
```

## 最佳实践

### 1. 合理配置

- 根据应用需求配置连接池大小
- 设置合适的重连间隔和最大重连次数
- 启用心跳检测确保连接活跃

### 2. 错误处理

- 实现完整的错误处理机制
- 记录错误日志便于排查问题
- 提供用户友好的错误提示

### 3. 性能优化

- 使用连接池减少连接开销
- 启用消息压缩减少带宽占用
- 实现消息优先级处理重要消息

### 4. 安全考虑

- 使用认证服务保护连接安全
- 实现消息签名确保消息完整性
- 配置合理的权限控制

## 示例应用

查看 `examples/` 目录中的完整示例应用，了解如何在实际项目中使用 WebSocket 客户端组件。

## 支持

如需帮助或报告问题，请联系开发团队或查看项目文档。