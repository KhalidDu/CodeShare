/**
 * WebSocket客户端类型定义
 * 
 * 本文件定义了WebSocket通信系统的完整TypeScript类型接口，包括：
 * - 连接管理和状态监控
 * - 消息类型和优先级
 * - 重连和错误处理机制
 * - 心跳检测和性能优化
 * - 认证和授权支持
 * - 消息队列和缓存管理
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-11
 */

// WebSocket连接状态枚举
export enum WebSocketConnectionState {
  /** 已连接 */
  CONNECTED = 'connected',
  /** 连接中 */
  CONNECTING = 'connecting',
  /** 已断开 */
  DISCONNECTED = 'disconnected',
  /** 重连中 */
  RECONNECTING = 'reconnecting',
  /** 错误状态 */
  ERROR = 'error',
  /** 超时状态 */
  TIMEOUT = 'timeout'
}

// WebSocket消息类型枚举
export enum WebSocketMessageType {
  /** 通知消息 */
  NOTIFICATION = 'notification',
  /** 系统消息 */
  SYSTEM = 'system',
  /** 用户消息 */
  USER = 'user',
  /** 状态更新 */
  STATUS_UPDATE = 'status_update',
  /** 错误消息 */
  ERROR = 'error',
  /** 确认消息 */
  ACKNOWLEDGMENT = 'acknowledgment',
  /** 心跳消息 */
  HEARTBEAT = 'heartbeat',
  /** 自定义消息 */
  CUSTOM = 'custom'
}

// WebSocket消息优先级枚举
export enum WebSocketMessagePriority {
  /** 低优先级 */
  LOW = 'low',
  /** 普通优先级 */
  NORMAL = 'normal',
  /** 高优先级 */
  HIGH = 'high',
  /** 紧急优先级 */
  URGENT = 'urgent'
}

// WebSocket消息状态枚举
export enum WebSocketMessageStatus {
  /** 待发送 */
  PENDING = 'pending',
  /** 发送中 */
  SENDING = 'sending',
  /** 已发送 */
  SENT = 'sent',
  /** 发送失败 */
  FAILED = 'failed',
  /** 已过期 */
  EXPIRED = 'expired',
  /** 已取消 */
  CANCELLED = 'cancelled'
}

// WebSocket队列状态枚举
export enum WebSocketQueueState {
  /** 正常 */
  NORMAL = 'normal',
  /** 繁忙 */
  BUSY = 'busy',
  /** 停止 */
  STOPPED = 'stopped',
  /** 错误 */
  ERROR = 'error'
}

// WebSocket连接事件类型枚举
export enum WebSocketConnectionEventType {
  /** 连接建立 */
  CONNECTED = 'connected',
  /** 连接断开 */
  DISCONNECTED = 'disconnected',
  /** 连接重连 */
  RECONNECTED = 'reconnected',
  /** 连接错误 */
  ERROR = 'error',
  /** 连接超时 */
  TIMEOUT = 'timeout',
  /** 心跳检测 */
  HEARTBEAT = 'heartbeat',
  /** 强制断开 */
  FORCE_DISCONNECTED = 'force_disconnected'
}

// WebSocket消息事件类型枚举
export enum WebSocketMessageEventType {
  /** 消息发送 */
  SENT = 'sent',
  /** 消息接收 */
  RECEIVED = 'received',
  /** 消息失败 */
  FAILED = 'failed',
  /** 消息重试 */
  RETRY = 'retry',
  /** 消息过期 */
  EXPIRED = 'expired',
  /** 消息取消 */
  CANCELLED = 'cancelled',
  /** 消息确认 */
  ACKNOWLEDGMENT = 'acknowledgment'
}

// WebSocket消息发送状态枚举
export enum WebSocketMessageSendStatus {
  /** 成功 */
  SUCCESS = 'success',
  /** 失败 */
  FAILED = 'failed',
  /** 超时 */
  TIMEOUT = 'timeout',
  /** 拒绝 */
  REJECTED = 'rejected',
  /** 队列满 */
  QUEUE_FULL = 'queue_full'
}

// WebSocket连接配置接口
export interface WebSocketConfig {
  /** WebSocket服务器URL */
  url: string
  /** 连接超时时间（毫秒） */
  connectTimeout?: number
  /** 消息发送超时时间（毫秒） */
  sendTimeout?: number
  /** 心跳间隔时间（毫秒） */
  heartbeatInterval?: number
  /** 心跳超时时间（毫秒） */
  heartbeatTimeout?: number
  /** 重连间隔时间（毫秒） */
  reconnectInterval?: number
  /** 最大重连次数 */
  maxReconnectAttempts?: number
  /** 重连延迟时间（毫秒） */
  reconnectDelay?: number
  /** 消息队列最大大小 */
  maxQueueSize?: number
  /** 消息重试最大次数 */
  maxRetryAttempts?: number
  /** 启用调试模式 */
  debug?: boolean
  /** 启用自动重连 */
  autoReconnect?: boolean
  /** 启用心跳检测 */
  enableHeartbeat?: boolean
  /** 启用消息队列 */
  enableQueue?: boolean
  /** 启用消息压缩 */
  enableCompression?: boolean
  /** 启用连接池 */
  enableConnectionPool?: boolean
  /** 连接池大小 */
  connectionPoolSize?: number
  /** 消息序列化方式 */
  serialization?: 'json' | 'msgpack' | 'protobuf'
}

// WebSocket连接信息接口
export interface WebSocketConnectionInfo {
  /** 连接ID */
  connectionId: string
  /** 用户ID */
  userId: string
  /** 用户名 */
  username?: string
  /** 连接时间 */
  connectedAt: Date
  /** 最后活动时间 */
  lastActivityAt: Date
  /** 连接状态 */
  state: WebSocketConnectionState
  /** 连接IP地址 */
  ipAddress?: string
  /** 用户代理 */
  userAgent?: string
  /** 连接详情 */
  connectionDetails?: Record<string, any>
  /** 重连次数 */
  reconnectCount: number
  /** 错误信息 */
  error?: string
}

// WebSocket消息接口
export interface WebSocketMessage {
  /** 消息ID */
  messageId: string
  /** 消息类型 */
  type: WebSocketMessageType
  /** 消息内容 */
  data: any
  /** 消息优先级 */
  priority: WebSocketMessagePriority
  /** 发送者用户ID */
  senderId?: string
  /** 发送者用户名 */
  senderUsername?: string
  /** 接收者用户ID */
  receiverId?: string
  /** 接收者组名 */
  receiverGroup?: string
  /** 发送时间 */
  sentAt: Date
  /** 过期时间 */
  expiresAt?: Date
  /** 消息状态 */
  status: WebSocketMessageStatus
  /** 重试次数 */
  retryCount: number
  /** 最大重试次数 */
  maxRetries: number
  /** 错误信息 */
  error?: string
  /** 消息元数据 */
  metadata?: Record<string, any>
}

// WebSocket消息队列项接口
export interface WebSocketMessageQueueItem {
  /** 消息ID */
  messageId: string
  /** 目标用户ID */
  targetUserId?: string
  /** 目标组名 */
  targetGroup?: string
  /** 消息类型 */
  messageType: WebSocketMessageType
  /** 消息优先级 */
  priority: WebSocketMessagePriority
  /** 消息内容 */
  message: any
  /** 创建时间 */
  createdAt: Date
  /** 计划发送时间 */
  scheduledAt?: Date
  /** 重试次数 */
  retryCount: number
  /** 最大重试次数 */
  maxRetries: number
  /** 过期时间 */
  expiresAt?: Date
  /** 消息状态 */
  status: WebSocketMessageStatus
  /** 错误信息 */
  error?: string
  /** 消息元数据 */
  metadata?: Record<string, any>
}

// WebSocket连接状态接口
export interface WebSocketConnectionStatus {
  /** 是否连接 */
  isConnected: boolean
  /** 连接ID */
  connectionId?: string
  /** 连接时间 */
  connectedAt?: Date
  /** 最后活动时间 */
  lastActivityAt?: Date
  /** 连接状态 */
  state: WebSocketConnectionState
  /** 连接详情 */
  connectionDetails?: Record<string, any>
  /** 错误信息 */
  error?: string
  /** 重连次数 */
  reconnectCount: number
  /** 心跳状态 */
  heartbeatStatus: 'active' | 'inactive' | 'timeout'
  /** 最后心跳时间 */
  lastHeartbeatAt?: Date
}

// WebSocket连接结果接口
export interface WebSocketConnectionResult {
  /** 是否成功 */
  success: boolean
  /** 连接ID */
  connectionId?: string
  /** 用户ID */
  userId: string
  /** 连接时间 */
  connectedAt: Date
  /** 连接状态 */
  state: WebSocketConnectionState
  /** 错误信息 */
  error?: string
  /** 连接详情 */
  connectionDetails?: Record<string, any>
}

// WebSocket断开连接结果接口
export interface WebSocketDisconnectionResult {
  /** 是否成功 */
  success: boolean
  /** 连接ID */
  connectionId?: string
  /** 用户ID */
  userId: string
  /** 断开时间 */
  disconnectedAt: Date
  /** 连接时长（秒） */
  duration: number
  /** 断开原因 */
  reason?: string
  /** 错误信息 */
  error?: string
}

// WebSocket发送消息结果接口
export interface WebSocketSendMessageResult {
  /** 是否成功 */
  success: boolean
  /** 连接ID */
  connectionId?: string
  /** 用户ID */
  userId?: string
  /** 消息类型 */
  messageType: WebSocketMessageType
  /** 发送时间（毫秒） */
  sendTimeMs: number
  /** 消息ID */
  messageId?: string
  /** 错误信息 */
  error?: string
}

// WebSocket广播结果接口
export interface WebSocketBroadcastResult {
  /** 是否成功 */
  success: boolean
  /** 目标用户数量 */
  targetCount: number
  /** 成功发送数量 */
  successCount: number
  /** 失败发送数量 */
  failedCount: number
  /** 消息类型 */
  messageType: WebSocketMessageType
  /** 发送时间（毫秒） */
  sendTimeMs: number
  /** 错误信息 */
  error?: string
  /** 发送结果详情 */
  results?: Record<string, WebSocketSendMessageResult>
}

// WebSocket组操作结果接口
export interface WebSocketGroupOperationResult {
  /** 是否成功 */
  success: boolean
  /** 操作类型 */
  operation: string
  /** 组名 */
  groupName: string
  /** 连接ID */
  connectionId?: string
  /** 用户ID */
  userId?: string
  /** 错误信息 */
  error?: string
}

// WebSocket组统计信息接口
export interface WebSocketGroupStats {
  /** 组名 */
  groupName: string
  /** 连接数量 */
  connectionCount: number
  /** 用户数量 */
  userCount: number
  /** 创建时间 */
  createdAt: Date
  /** 最后活动时间 */
  lastActivityAt: Date
  /** 消息数量 */
  messageCount: number
  /** 组详情 */
  groupDetails?: Record<string, any>
}

// WebSocket队列结果接口
export interface WebSocketQueueResult {
  /** 是否成功 */
  success: boolean
  /** 消息ID */
  messageId?: string
  /** 队列位置 */
  queuePosition?: number
  /** 预计发送时间 */
  estimatedSendTime?: Date
  /** 错误信息 */
  error?: string
}

// WebSocket队列处理结果接口
export interface WebSocketQueueProcessResult {
  /** 是否成功 */
  success: boolean
  /** 处理的消息数量 */
  processedCount: number
  /** 成功发送数量 */
  successCount: number
  /** 失败发送数量 */
  failedCount: number
  /** 重试数量 */
  retryCount: number
  /** 处理时间（毫秒） */
  processingTimeMs: number
  /** 错误信息 */
  error?: string
}

// WebSocket重试结果接口
export interface WebSocketRetryResult {
  /** 是否成功 */
  success: boolean
  /** 重试的消息数量 */
  retriedCount: number
  /** 成功数量 */
  successCount: number
  /** 失败数量 */
  failedCount: number
  /** 处理时间（毫秒） */
  processingTimeMs: number
  /** 错误信息 */
  error?: string
}

// WebSocket队列状态接口
export interface WebSocketQueueStatus {
  /** 队列状态 */
  state: WebSocketQueueState
  /** 待处理消息数量 */
  pendingCount: number
  /** 处理中消息数量 */
  processingCount: number
  /** 失败消息数量 */
  failedCount: number
  /** 总消息数量 */
  totalCount: number
  /** 队列大小 */
  queueSizeBytes: number
  /** 最后处理时间 */
  lastProcessedAt?: Date
  /** 处理速率（每秒） */
  processingRate: number
  /** 平均处理时间（毫秒） */
  averageProcessingTimeMs: number
  /** 错误信息 */
  error?: string
}

// WebSocket统计信息接口
export interface WebSocketStats {
  /** 总连接数 */
  totalConnections: number
  /** 活跃连接数 */
  activeConnections: number
  /** 在线用户数 */
  onlineUsers: number
  /** 总消息数 */
  totalMessages: number
  /** 今日消息数 */
  todayMessages: number
  /** 平均连接时长（秒） */
  averageConnectionDuration: number
  /** 平均消息发送时间（毫秒） */
  averageMessageSendTime: number
  /** 消息成功率 */
  messageSuccessRate: number
  /** 系统运行时间 */
  uptime: number
  /** 最后更新时间 */
  lastUpdated: Date
  /** 详细的统计信息 */
  detailedStats?: Record<string, any>
}

// WebSocket连接事件接口
export interface WebSocketConnectionEvent {
  /** 事件ID */
  eventId: string
  /** 事件类型 */
  eventType: WebSocketConnectionEventType
  /** 连接ID */
  connectionId: string
  /** 用户ID */
  userId: string
  /** 事件时间 */
  eventTime: Date
  /** 连接状态 */
  connectionState: WebSocketConnectionState
  /** IP地址 */
  ipAddress?: string
  /** 用户代理 */
  userAgent?: string
  /** 事件详情 */
  eventDetails?: Record<string, any>
  /** 错误信息 */
  error?: string
}

// WebSocket消息事件接口
export interface WebSocketMessageEvent {
  /** 事件ID */
  eventId: string
  /** 事件类型 */
  eventType: WebSocketMessageEventType
  /** 消息ID */
  messageId: string
  /** 连接ID */
  connectionId: string
  /** 用户ID */
  userId: string
  /** 事件时间 */
  eventTime: Date
  /** 消息类型 */
  messageType: WebSocketMessageType
  /** 消息优先级 */
  priority: WebSocketMessagePriority
  /** 目标用户ID */
  targetUserId?: string
  /** 目标组名 */
  targetGroup?: string
  /** 发送状态 */
  sendStatus: WebSocketMessageSendStatus
  /** 发送时间（毫秒） */
  sendTimeMs: number
  /** 事件详情 */
  eventDetails?: Record<string, any>
  /** 错误信息 */
  error?: string
}

// WebSocket性能指标接口
export interface WebSocketPerformanceMetrics {
  /** 总连接数 */
  totalConnections: number
  /** 活跃连接数 */
  activeConnections: number
  /** 总消息数 */
  totalMessages: number
  /** 平均连接建立时间（毫秒） */
  averageConnectionTime: number
  /** 平均消息发送时间（毫秒） */
  averageMessageSendTime: number
  /** 消息成功率 */
  messageSuccessRate: number
  /** 连接成功率 */
  connectionSuccessRate: number
  /** 服务器负载 */
  serverLoad: number
  /** 内存使用量（MB） */
  memoryUsage: number
  /** CPU使用率 */
  cpuUsage: number
  /** 网络延迟（毫秒） */
  networkLatency: number
  /** 时间序列数据 */
  timeSeries: WebSocketPerformanceTimeSeries[]
  /** 统计时间范围 */
  timeRange: {
    start: Date
    end: Date
  }
  /** 最后更新时间 */
  lastUpdated: Date
}

// WebSocket性能时间序列接口
export interface WebSocketPerformanceTimeSeries {
  /** 时间戳 */
  timestamp: Date
  /** 活跃连接数 */
  activeConnections: number
  /** 消息数量 */
  messageCount: number
  /** 平均发送时间（毫秒） */
  averageSendTime: number
  /** 成功率 */
  successRate: number
  /** 服务器负载 */
  serverLoad: number
  /** 内存使用量（MB） */
  memoryUsage: number
  /** CPU使用率 */
  cpuUsage: number
}

// WebSocket事件处理器接口
export interface WebSocketEventHandler {
  /** 连接建立事件 */
  onConnected?: (connectionId: string, userId: string) => void
  /** 连接断开事件 */
  onDisconnected?: (connectionId: string, userId: string, reason?: string) => void
  /** 连接错误事件 */
  onError?: (connectionId: string, error: Error) => void
  /** 消息接收事件 */
  onMessage?: (message: WebSocketMessage) => void
  /** 连接重连事件 */
  onReconnecting?: (connectionId: string, attempt: number) => void
  /** 连接重连成功事件 */
  onReconnected?: (connectionId: string) => void
  /** 心跳事件 */
  onHeartbeat?: (connectionId: string) => void
  /** 消息发送成功事件 */
  onMessageSent?: (result: WebSocketSendMessageResult) => void
  /** 消息发送失败事件 */
  onMessageFailed?: (error: Error) => void
  /** 队列状态变化事件 */
  onQueueStatusChange?: (status: WebSocketQueueStatus) => void
  /** 连接状态变化事件 */
  onConnectionStateChange?: (state: WebSocketConnectionState) => void
}

// WebSocket客户端接口
export interface WebSocketClient {
  /** 连接配置 */
  config: WebSocketConfig
  /** 连接状态 */
  connectionStatus: WebSocketConnectionStatus
  /** 事件处理器 */
  eventHandlers: WebSocketEventHandler
  /** 消息队列 */
  messageQueue: WebSocketMessageQueueItem[]
  /** 连接信息 */
  connectionInfo?: WebSocketConnectionInfo
  
  /** 连接WebSocket */
  connect: (userId?: string, token?: string) => Promise<WebSocketConnectionResult>
  /** 断开连接 */
  disconnect: (reason?: string) => Promise<WebSocketDisconnectionResult>
  /** 重新连接 */
  reconnect: () => Promise<WebSocketConnectionResult>
  /** 发送消息 */
  sendMessage: (message: any, type: WebSocketMessageType, priority?: WebSocketMessagePriority) => Promise<WebSocketSendMessageResult>
  /** 广播消息 */
  broadcastMessage: (message: any, type: WebSocketMessageType, excludeUsers?: string[]) => Promise<WebSocketBroadcastResult>
  /** 发送消息到组 */
  sendToGroup: (groupName: string, message: any, type: WebSocketMessageType) => Promise<WebSocketSendMessageResult>
  /** 添加到组 */
  addToGroup: (groupName: string) => Promise<WebSocketGroupOperationResult>
  /** 从组中移除 */
  removeFromGroup: (groupName: string) => Promise<WebSocketGroupOperationResult>
  /** 获取连接状态 */
  getConnectionStatus: () => WebSocketConnectionStatus
  /** 获取队列状态 */
  getQueueStatus: () => WebSocketQueueStatus
  /** 获取统计信息 */
  getStats: () => WebSocketStats
  /** 获取性能指标 */
  getPerformanceMetrics: () => WebSocketPerformanceMetrics
  /** 注册事件处理器 */
  registerEventHandlers: (handlers: Partial<WebSocketEventHandler>) => void
  /** 移除事件处理器 */
  unregisterEventHandlers: (handlers: (keyof WebSocketEventHandler)[]) => void
  /** 发送心跳 */
  sendHeartbeat: () => Promise<void>
  /** 清理资源 */
  cleanup: () => Promise<void>
}

// WebSocket消息处理器接口
export interface WebSocketMessageHandler {
  /** 处理消息 */
  handle: (message: WebSocketMessage) => Promise<void>
  /** 支持的消息类型 */
  supportedTypes: WebSocketMessageType[]
  /** 处理优先级 */
  priority: number
}

// WebSocket连接池接口
export interface WebSocketConnectionPool {
  /** 获取连接 */
  getConnection: (userId: string) => WebSocketClient | null
  /** 创建连接 */
  createConnection: (userId: string, token?: string) => Promise<WebSocketClient>
  /** 释放连接 */
  releaseConnection: (userId: string) => void
  /** 关闭所有连接 */
  closeAll: () => Promise<void>
  /** 获取池状态 */
  getPoolStatus: () => {
    totalConnections: number
    activeConnections: number
    idleConnections: number
  }
}

// 默认配置
export const DEFAULT_WEBSOCKET_CONFIG: WebSocketConfig = {
  url: 'ws://localhost:6676/ws',
  connectTimeout: 10000,
  sendTimeout: 5000,
  heartbeatInterval: 30000,
  heartbeatTimeout: 10000,
  reconnectInterval: 5000,
  maxReconnectAttempts: 10,
  reconnectDelay: 1000,
  maxQueueSize: 1000,
  maxRetryAttempts: 3,
  debug: false,
  autoReconnect: true,
  enableHeartbeat: true,
  enableQueue: true,
  enableCompression: false,
  enableConnectionPool: false,
  connectionPoolSize: 5,
  serialization: 'json'
}

// 工具类型
export type WebSocketEventCallback<T = any> = (data: T) => void
export type WebSocketErrorHandler = (error: Error) => void
export type WebSocketConnectionCallback = (connected: boolean) => void

// 导出所有类型
export type {
  WebSocketConfig,
  WebSocketConnectionInfo,
  WebSocketMessage,
  WebSocketMessageQueueItem,
  WebSocketConnectionStatus,
  WebSocketConnectionResult,
  WebSocketDisconnectionResult,
  WebSocketSendMessageResult,
  WebSocketBroadcastResult,
  WebSocketGroupOperationResult,
  WebSocketGroupStats,
  WebSocketQueueResult,
  WebSocketQueueProcessResult,
  WebSocketRetryResult,
  WebSocketQueueStatus,
  WebSocketStats,
  WebSocketConnectionEvent,
  WebSocketMessageEvent,
  WebSocketPerformanceMetrics,
  WebSocketPerformanceTimeSeries,
  WebSocketEventHandler,
  WebSocketClient,
  WebSocketMessageHandler,
  WebSocketConnectionPool,
  WebSocketEventCallback,
  WebSocketErrorHandler,
  WebSocketConnectionCallback
}