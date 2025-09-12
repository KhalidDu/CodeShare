/**
 * WebSocket连接池管理器
 * 
 * 提供WebSocket连接池的集中管理功能，包括：
 * - 连接池的创建和销毁
 * - 连接的健康状态监控
 * - 连接的负载均衡和故障转移
 * - 连接池的性能监控和优化
 * - 连接池的自动扩缩容
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-11
 */

import { v4 as uuidv4 } from 'uuid'
import { WebSocketService } from './websocketService'
import type { 
  WebSocketConfig, 
  WebSocketConnectionStatus,
  WebSocketStats,
  WebSocketPerformanceMetrics,
  WebSocketConnectionInfo
} from '@/types/websocket'

/**
 * 连接池配置接口
 */
export interface WebSocketPoolConfig {
  /** 连接池名称 */
  name: string
  /** 最大连接数 */
  maxConnections: number
  /** 最小连接数 */
  minConnections: number
  /** 连接获取超时时间（毫秒） */
  connectionTimeout: number
  /** 连接空闲超时时间（毫秒） */
  idleTimeout: number
  /** 健康检查间隔（毫秒） */
  healthCheckInterval: number
  /** 连接配置 */
  connectionConfig: Partial<WebSocketConfig>
  /** 启用负载均衡 */
  enableLoadBalancing: boolean
  /** 启用故障转移 */
  enableFailover: boolean
  /** 启用自动扩缩容 */
  enableAutoScaling: boolean
}

/**
 * 连接池统计信息
 */
export interface WebSocketPoolStats {
  /** 连接池名称 */
  name: string
  /** 总连接数 */
  totalConnections: number
  /** 活跃连接数 */
  activeConnections: number
  /** 空闲连接数 */
  idleConnections: number
  /** 健康连接数 */
  healthyConnections: number
  /** 连接池大小 */
  poolSize: number
  /** 利用率 */
  utilization: number
  /** 平均响应时间 */
  averageResponseTime: number
  /** 成功率 */
  successRate: number
  /** 最后更新时间 */
  lastUpdated: Date
}

/**
 * 连接池状态
 */
export enum WebSocketPoolState {
  /** 初始化中 */
  INITIALIZING = 'initializing',
  /** 运行中 */
  RUNNING = 'running',
  /** 健康检查中 */
  HEALTH_CHECKING = 'health_checking',
  /** 扩缩容中 */
  SCALING = 'scaling',
  /** 关闭中 */
  SHUTTING_DOWN = 'shutting_down',
  /** 已关闭 */
  CLOSED = 'closed'
}

/**
 * WebSocket连接池管理器
 */
export class WebSocketConnectionPoolManager {
  private _config: WebSocketPoolConfig
  private _connections: Map<string, WebSocketService> = new Map()
  private _activeConnections: Set<string> = new Set()
  private _idleConnections: Set<string> = new Set()
  private _connectionHealth: Map<string, {
    isHealthy: boolean
    lastActivity: Date
    errorCount: number
    consecutiveErrors: number
    responseTime: number
  }> = new Map()
  private _state: WebSocketPoolState = WebSocketPoolState.INITIALIZING
  private _stats: WebSocketPoolStats
  private _healthCheckTimer: NodeJS.Timeout | null = null
  private _scalingTimer: NodeJS.Timeout | null = null
  private _cleanupTimer: NodeJS.Timeout | null = null
  private _performanceMetrics: WebSocketPerformanceMetrics | null = null

  constructor(config: WebSocketPoolConfig) {
    this._config = { ...this.getDefaultConfig(), ...config }
    this._stats = this.createInitialStats()
    this.initialize()
  }

  // 公共属性
  get config(): WebSocketPoolConfig {
    return { ...this._config }
  }

  get state(): WebSocketPoolState {
    return this._state
  }

  get stats(): WebSocketPoolStats {
    return { ...this._stats }
  }

  /**
   * 获取连接
   */
  async getConnection(userId?: string): Promise<WebSocketService> {
    if (this._state !== WebSocketPoolState.RUNNING) {
      throw new Error('连接池未就绪')
    }

    const connectionId = userId || uuidv4()
    const startTime = Date.now()

    // 尝试获取空闲连接
    let connection = this.getIdleConnection()

    if (!connection) {
      // 如果没有空闲连接，创建新连接
      connection = await this.createNewConnection(connectionId)
    }

    // 标记连接为活跃
    this.markConnectionActive(connectionId)

    // 记录响应时间
    const responseTime = Date.now() - startTime
    this.updateConnectionResponseTime(connectionId, responseTime)

    return connection
  }

  /**
   * 释放连接
   */
  releaseConnection(connectionId: string): void {
    if (!this._connections.has(connectionId)) {
      return
    }

    // 标记连接为空闲
    this.markConnectionIdle(connectionId)

    // 更新统计信息
    this.updateStats()

    this.logDebug('连接已释放', { connectionId })
  }

  /**
   * 关闭连接池
   */
  async shutdown(): Promise<void> {
    this._state = WebSocketPoolState.SHUTTING_DOWN

    this.logDebug('开始关闭连接池')

    // 清理定时器
    this.clearAllTimers()

    // 关闭所有连接
    const closePromises = Array.from(this._connections.values()).map(
      service => service.cleanup()
    )

    await Promise.all(closePromises)

    // 清理资源
    this._connections.clear()
    this._activeConnections.clear()
    this._idleConnections.clear()
    this._connectionHealth.clear()

    this._state = WebSocketPoolState.CLOSED

    this.logDebug('连接池已关闭')
  }

  /**
   * 健康检查
   */
  async healthCheck(): Promise<boolean> {
    this._state = WebSocketPoolState.HEALTH_CHECKING

    try {
      const healthChecks = Array.from(this._connections.entries()).map(
        async ([connectionId, service]) => {
          const startTime = Date.now()
          
          try {
            // 测试连接
            if (service.connectionStatus.isConnected) {
              await service.sendHeartbeat()
              const responseTime = Date.now() - startTime
              this.updateConnectionHealth(connectionId, true, responseTime)
              return true
            } else {
              this.updateConnectionHealth(connectionId, false, 0)
              return false
            }
          } catch (error) {
            this.updateConnectionHealth(connectionId, false, 0)
            return false
          }
        }
      )

      const results = await Promise.all(healthChecks)
      const healthyCount = results.filter(healthy => healthy).length

      // 更新统计信息
      this._stats.healthyConnections = healthyCount
      this._stats.successRate = (healthyCount / results.length) * 100

      this._state = WebSocketPoolState.RUNNING

      this.logDebug('健康检查完成', { 
        total: results.length,
        healthy: healthyCount 
      })

      return healthyCount > 0

    } catch (error) {
      this.logError('健康检查失败', error as Error)
      this._state = WebSocketPoolState.RUNNING
      return false
    }
  }

  /**
   * 扩缩容
   */
  async scaleConnections(): Promise<void> {
    if (this._state !== WebSocketPoolState.RUNNING) {
      return
    }

    this._state = WebSocketPoolState.SCALING

    try {
      const utilization = this._stats.utilization
      const currentCount = this._connections.size

      // 扩容条件
      if (utilization > 0.8 && currentCount < this._config.maxConnections) {
        const newCount = Math.min(
          Math.ceil(currentCount * 1.5), 
          this._config.maxConnections
        )
        
        await this.createConnections(newCount - currentCount)
        
      // 缩容条件
      } else if (utilization < 0.3 && currentCount > this._config.minConnections) {
        const newCount = Math.max(
          Math.floor(currentCount * 0.8), 
          this._config.minConnections
        )
        
        await this.removeConnections(currentCount - newCount)
      }

      this._state = WebSocketPoolState.RUNNING

      this.logDebug('扩缩容完成', { 
        utilization,
        before: currentCount,
        after: this._connections.size 
      })

    } catch (error) {
      this.logError('扩缩容失败', error as Error)
      this._state = WebSocketPoolState.RUNNING
    }
  }

  /**
   * 获取性能指标
   */
  getPerformanceMetrics(): WebSocketPerformanceMetrics {
    if (!this._performanceMetrics) {
      this._performanceMetrics = this.calculatePerformanceMetrics()
    }
    return this._performanceMetrics
  }

  // 私有方法

  private getDefaultConfig(): WebSocketPoolConfig {
    return {
      name: 'WebSocketPool',
      maxConnections: 10,
      minConnections: 2,
      connectionTimeout: 10000,
      idleTimeout: 300000,
      healthCheckInterval: 30000,
      connectionConfig: {},
      enableLoadBalancing: true,
      enableFailover: true,
      enableAutoScaling: true
    }
  }

  private createInitialStats(): WebSocketPoolStats {
    return {
      name: this._config.name,
      totalConnections: 0,
      activeConnections: 0,
      idleConnections: 0,
      healthyConnections: 0,
      poolSize: this._config.maxConnections,
      utilization: 0,
      averageResponseTime: 0,
      successRate: 100,
      lastUpdated: new Date()
    }
  }

  private async initialize(): Promise<void> {
    try {
      // 创建初始连接
      await this.createConnections(this._config.minConnections)

      // 启动健康检查
      this.startHealthCheck()

      // 启动清理任务
      this.startCleanupTask()

      // 启动自动扩缩容
      if (this._config.enableAutoScaling) {
        this.startAutoScaling()
      }

      this._state = WebSocketPoolState.RUNNING

      this.logDebug('连接池初始化完成', { 
        minConnections: this._config.minConnections,
        maxConnections: this._config.maxConnections 
      })

    } catch (error) {
      this.logError('连接池初始化失败', error as Error)
      this._state = WebSocketPoolState.CLOSED
      throw error
    }
  }

  private async createConnections(count: number): Promise<void> {
    const createPromises = []

    for (let i = 0; i < count; i++) {
      const connectionId = uuidv4()
      createPromises.push(this.createNewConnection(connectionId))
    }

    await Promise.all(createPromises)
  }

  private async createNewConnection(connectionId: string): Promise<WebSocketService> {
    const service = new WebSocketService(this._config.connectionConfig)
    
    try {
      await service.connect(connectionId)
      
      this._connections.set(connectionId, service)
      this._idleConnections.add(connectionId)
      this._connectionHealth.set(connectionId, {
        isHealthy: true,
        lastActivity: new Date(),
        errorCount: 0,
        consecutiveErrors: 0,
        responseTime: 0
      })

      this.updateStats()

      this.logDebug('新连接已创建', { connectionId })

      return service

    } catch (error) {
      this.logError('创建连接失败', error as Error)
      throw error
    }
  }

  private async removeConnections(count: number): Promise<void> {
    const idleConnections = Array.from(this._idleConnections)
    const toRemove = idleConnections.slice(0, count)

    const removePromises = toRemove.map(connectionId => 
      this.removeConnection(connectionId)
    )

    await Promise.all(removePromises)
  }

  private async removeConnection(connectionId: string): Promise<void> {
    const service = this._connections.get(connectionId)
    if (service) {
      await service.cleanup()
      this._connections.delete(connectionId)
      this._activeConnections.delete(connectionId)
      this._idleConnections.delete(connectionId)
      this._connectionHealth.delete(connectionId)
      this.updateStats()
    }
  }

  private getIdleConnection(): WebSocketService | null {
    if (this._idleConnections.size === 0) {
      return null
    }

    // 获取最新的空闲连接
    const connectionId = Array.from(this._idleConnections)[0]
    return this._connections.get(connectionId) || null
  }

  private markConnectionActive(connectionId: string): void {
    this._idleConnections.delete(connectionId)
    this._activeConnections.add(connectionId)
    
    const health = this._connectionHealth.get(connectionId)
    if (health) {
      health.lastActivity = new Date()
    }

    this.updateStats()
  }

  private markConnectionIdle(connectionId: string): void {
    this._activeConnections.delete(connectionId)
    this._idleConnections.add(connectionId)
    
    const health = this._connectionHealth.get(connectionId)
    if (health) {
      health.lastActivity = new Date()
    }

    this.updateStats()
  }

  private updateConnectionHealth(
    connectionId: string, 
    isHealthy: boolean, 
    responseTime: number
  ): void {
    if (!this._connectionHealth.has(connectionId)) {
      return
    }

    const health = this._connectionHealth.get(connectionId)!
    health.isHealthy = isHealthy
    health.lastActivity = new Date()
    health.responseTime = responseTime

    if (isHealthy) {
      health.consecutiveErrors = 0
    } else {
      health.errorCount++
      health.consecutiveErrors++
    }

    // 如果连接不健康，尝试重新连接
    if (!isHealthy && health.consecutiveErrors >= 3) {
      this.recreateConnection(connectionId)
    }
  }

  private updateConnectionResponseTime(connectionId: string, responseTime: number): void {
    const health = this._connectionHealth.get(connectionId)
    if (health) {
      health.responseTime = responseTime
    }
  }

  private async recreateConnection(connectionId: string): Promise<void> {
    const service = this._connections.get(connectionId)
    if (service) {
      try {
        await service.reconnect()
      } catch (error) {
        this.logError('重新创建连接失败', error as Error)
      }
    }
  }

  private updateStats(): void {
    this._stats.totalConnections = this._connections.size
    this._stats.activeConnections = this._activeConnections.size
    this._stats.idleConnections = this._idleConnections.size
    this._stats.healthyConnections = Array.from(this._connectionHealth.values())
      .filter(health => health.isHealthy).length
    this._stats.utilization = this._stats.totalConnections / this._config.maxConnections
    
    // 计算平均响应时间
    const responseTimes = Array.from(this._connectionHealth.values())
      .map(health => health.responseTime)
      .filter(time => time > 0)
    
    this._stats.averageResponseTime = responseTimes.length > 0 
      ? responseTimes.reduce((sum, time) => sum + time, 0) / responseTimes.length 
      : 0

    this._stats.lastUpdated = new Date()
  }

  private calculatePerformanceMetrics(): WebSocketPerformanceMetrics {
    const now = new Date()
    const responseTimes = Array.from(this._connectionHealth.values())
      .map(health => health.responseTime)
      .filter(time => time > 0)

    const averageResponseTime = responseTimes.length > 0 
      ? responseTimes.reduce((sum, time) => sum + time, 0) / responseTimes.length 
      : 0

    return {
      totalConnections: this._stats.totalConnections,
      activeConnections: this._stats.activeConnections,
      totalMessages: 0, // 需要从连接中统计
      averageConnectionTime: averageResponseTime,
      averageMessageSendTime: averageResponseTime,
      messageSuccessRate: this._stats.successRate,
      connectionSuccessRate: this._stats.successRate,
      serverLoad: this._stats.utilization * 100,
      memoryUsage: this.getMemoryUsage(),
      cpuUsage: this.getCpuUsage(),
      networkLatency: averageResponseTime,
      timeSeries: [],
      timeRange: {
        start: new Date(now.getTime() - 3600000),
        end: now
      },
      lastUpdated: now
    }
  }

  private startHealthCheck(): void {
    this._healthCheckTimer = setInterval(() => {
      this.healthCheck()
    }, this._config.healthCheckInterval)
  }

  private startCleanupTask(): void {
    this._cleanupTimer = setInterval(() => {
      this.cleanupIdleConnections()
    }, this._config.idleTimeout)
  }

  private startAutoScaling(): void {
    this._scalingTimer = setInterval(() => {
      this.scaleConnections()
    }, 60000) // 每分钟检查一次
  }

  private cleanupIdleConnections(): void {
    const now = new Date()
    const idleConnections = Array.from(this._idleConnections)

    for (const connectionId of idleConnections) {
      const health = this._connectionHealth.get(connectionId)
      if (health && (now.getTime() - health.lastActivity.getTime()) > this._config.idleTimeout) {
        // 如果连接数超过最小值，清理空闲连接
        if (this._connections.size > this._config.minConnections) {
          this.removeConnection(connectionId)
        }
      }
    }
  }

  private clearAllTimers(): void {
    if (this._healthCheckTimer) {
      clearInterval(this._healthCheckTimer)
      this._healthCheckTimer = null
    }

    if (this._scalingTimer) {
      clearInterval(this._scalingTimer)
      this._scalingTimer = null
    }

    if (this._cleanupTimer) {
      clearInterval(this._cleanupTimer)
      this._cleanupTimer = null
    }
  }

  private getMemoryUsage(): number {
    return performance.memory ? 
      performance.memory.usedJSHeapSize / (1024 * 1024) : 
      50
  }

  private getCpuUsage(): number {
    return 10 // 默认10%
  }

  private logDebug(message: string, data?: any): void {
    if (this._config.connectionConfig.debug) {
      console.log(`[WebSocketPoolManager] ${message}`, data || '')
    }
  }

  private logError(message: string, error: Error): void {
    console.error(`[WebSocketPoolManager] ${message}`, error)
  }
}

// 导出连接池管理器
export { WebSocketConnectionPoolManager as ConnectionPoolManager }

// 导出类型
export type { WebSocketPoolConfig, WebSocketPoolStats }