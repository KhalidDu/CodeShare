/**
 * WebSocket认证服务
 * 
 * 提供WebSocket连接的认证和授权功能，包括：
 * - WebSocket连接的认证处理
 * - 令牌管理和刷新
 * - 权限验证
 * - 连接安全验证
 * - 会话管理
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-11
 */

import { v4 as uuidv4 } from 'uuid'
import { authService } from './authService'
import type { 
  WebSocketConfig, 
  WebSocketConnectionResult,
  WebSocketMessage,
  WebSocketMessageType,
  WebSocketConnectionStatus
} from '@/types/websocket'

/**
 * 认证状态枚举
 */
export enum WebSocketAuthState {
  /** 未认证 */
  UNAUTHENTICATED = 'unauthenticated',
  /** 认证中 */
  AUTHENTICATING = 'authenticating',
  /** 已认证 */
  AUTHENTICATED = 'authenticated',
  /** 认证失败 */
  AUTHENTICATION_FAILED = 'authentication_failed',
  /** 令牌过期 */
  TOKEN_EXPIRED = 'token_expired',
  /** 权限不足 */
  INSUFFICIENT_PERMISSIONS = 'insufficient_permissions'
}

/**
 * 权限类型
 */
export enum WebSocketPermission {
  /** 发送消息 */
  SEND_MESSAGE = 'send_message',
  /** 接收消息 */
  RECEIVE_MESSAGE = 'receive_message',
  /** 广播消息 */
  BROADCAST_MESSAGE = 'broadcast_message',
  /** 加入群组 */
  JOIN_GROUP = 'join_group',
  /** 离开群组 */
  LEAVE_GROUP = 'leave_group',
  /** 管理用户 */
  MANAGE_USERS = 'manage_users',
  /** 系统管理 */
  SYSTEM_ADMIN = 'system_admin'
}

/**
 * WebSocket认证信息接口
 */
export interface WebSocketAuthInfo {
  /** 用户ID */
  userId: string
  /** 用户名 */
  username: string
  /** 访问令牌 */
  accessToken: string
  /** 刷新令牌 */
  refreshToken: string
  /** 令牌过期时间 */
  tokenExpiry: Date
  /** 权限列表 */
  permissions: WebSocketPermission[]
  /** 会话ID */
  sessionId: string
  /** 连接ID */
  connectionId: string
  /** 认证时间 */
  authenticatedAt: Date
  /** 最后活动时间 */
  lastActivityAt: Date
}

/**
 * WebSocket认证配置接口
 */
export interface WebSocketAuthConfig {
  /** 启用认证 */
  enableAuthentication: boolean
  /** 启用令牌自动刷新 */
  enableTokenRefresh: boolean
  /** 令牌刷新提前时间（毫秒） */
  tokenRefreshThreshold: number
  /** 会话超时时间（毫秒） */
  sessionTimeout: number
  /** 最大并发连接数 */
  maxConcurrentConnections: number
  /** 权限验证间隔（毫秒） */
  permissionCheckInterval: number
  /** 启用连接安全验证 */
  enableConnectionSecurity: boolean
  /** 启用消息签名验证 */
  enableMessageSigning: boolean
  /** 允许的域名列表 */
  allowedOrigins: string[]
}

/**
 * 认证结果接口
 */
export interface WebSocketAuthResult {
  /** 是否成功 */
  success: boolean
  /** 认证状态 */
  state: WebSocketAuthState
  /** 认证信息 */
  authInfo?: WebSocketAuthInfo
  /** 错误信息 */
  error?: string
  /** 连接ID */
  connectionId?: string
}

/**
 * 权限验证结果接口
 */
export interface WebSocketPermissionResult {
  /** 是否有权限 */
  hasPermission: boolean
  /** 权限类型 */
  permission: WebSocketPermission
  /** 错误信息 */
  error?: string
  /** 额外信息 */
  details?: Record<string, any>
}

/**
 * WebSocket认证服务
 */
export class WebSocketAuthService {
  private _config: WebSocketAuthConfig
  private _authInfo: Map<string, WebSocketAuthInfo> = new Map()
  private _authState: Map<string, WebSocketAuthState> = new Map()
  private _sessionTimers: Map<string, NodeJS.Timeout> = new Map()
  private _tokenRefreshTimers: Map<string, NodeJS.Timeout> = new Map()
  private _permissionCheckTimer: NodeJS.Timeout | null = null
  private _activeConnections: Set<string> = new Set()
  private _connectionStats: Map<string, {
    messageCount: number
    lastMessageTime: Date
    averageMessageTime: number
  }> = new Map()

  constructor(config: Partial<WebSocketAuthConfig> = {}) {
    this._config = { ...this.getDefaultConfig(), ...config }
    this.initialize()
  }

  // 公共属性
  get config(): WebSocketAuthConfig {
    return { ...this._config }
  }

  /**
   * 认证WebSocket连接
   */
  async authenticateConnection(
    connectionId: string,
    token?: string
  ): Promise<WebSocketAuthResult> {
    try {
      this.logDebug('开始认证WebSocket连接', { connectionId })

      // 设置认证状态
      this._authState.set(connectionId, WebSocketAuthState.AUTHENTICATING)

      // 获取访问令牌
      const accessToken = token || this.getAccessToken()
      if (!accessToken) {
        return {
          success: false,
          state: WebSocketAuthState.UNAUTHENTICATED,
          error: '未提供访问令牌'
        }
      }

      // 验证访问令牌
      const authResult = await this.validateAccessToken(accessToken)
      if (!authResult.success) {
        return {
          success: false,
          state: WebSocketAuthState.AUTHENTICATION_FAILED,
          error: authResult.error
        }
      }

      // 创建认证信息
      const authInfo: WebSocketAuthInfo = {
        userId: authResult.userId!,
        username: authResult.username!,
        accessToken,
        refreshToken: authResult.refreshToken!,
        tokenExpiry: authResult.tokenExpiry!,
        permissions: authResult.permissions!,
        sessionId: uuidv4(),
        connectionId,
        authenticatedAt: new Date(),
        lastActivityAt: new Date()
      }

      // 保存认证信息
      this._authInfo.set(connectionId, authInfo)
      this._authState.set(connectionId, WebSocketAuthState.AUTHENTICATED)
      this._activeConnections.add(connectionId)

      // 启动会话超时检测
      this.startSessionTimeout(connectionId)

      // 启动令牌刷新
      if (this._config.enableTokenRefresh) {
        this.startTokenRefresh(connectionId)
      }

      // 初始化连接统计
      this.initializeConnectionStats(connectionId)

      this.logDebug('WebSocket连接认证成功', { 
        connectionId,
        userId: authInfo.userId,
        permissions: authInfo.permissions.length 
      })

      return {
        success: true,
        state: WebSocketAuthState.AUTHENTICATED,
        authInfo,
        connectionId
      }

    } catch (error) {
      this.logError('WebSocket连接认证失败', error as Error)
      
      this._authState.set(connectionId, WebSocketAuthState.AUTHENTICATION_FAILED)
      
      return {
        success: false,
        state: WebSocketAuthState.AUTHENTICATION_FAILED,
        error: error instanceof Error ? error.message : '认证失败',
        connectionId
      }
    }
  }

  /**
   * 验证权限
   */
  async checkPermission(
    connectionId: string,
    permission: WebSocketPermission
  ): Promise<WebSocketPermissionResult> {
    const authInfo = this._authInfo.get(connectionId)
    
    if (!authInfo) {
      return {
        hasPermission: false,
        permission,
        error: '连接未认证'
      }
    }

    // 检查令牌是否过期
    if (this.isTokenExpired(authInfo.tokenExpiry)) {
      return {
        hasPermission: false,
        permission,
        error: '访问令牌已过期'
      }
    }

    // 检查权限
    const hasPermission = authInfo.permissions.includes(permission)
    
    // 更新最后活动时间
    authInfo.lastActivityAt = new Date()
    this.updateConnectionStats(connectionId)

    return {
      hasPermission,
      permission,
      details: {
        userId: authInfo.userId,
        permissions: authInfo.permissions
      }
    }
  }

  /**
   * 刷新访问令牌
   */
  async refreshToken(connectionId: string): Promise<WebSocketAuthResult> {
    const authInfo = this._authInfo.get(connectionId)
    if (!authInfo) {
      return {
        success: false,
        state: WebSocketAuthState.UNAUTHENTICATED,
        error: '连接未认证'
      }
    }

    try {
      this.logDebug('刷新WebSocket访问令牌', { connectionId })

      // 刷新令牌
      const newAuthResponse = await authService.refreshToken(authInfo.refreshToken)
      
      // 更新认证信息
      authInfo.accessToken = newAuthResponse.token
      authInfo.refreshToken = newAuthResponse.refreshToken || authInfo.refreshToken
      authInfo.tokenExpiry = this.calculateTokenExpiry(newAuthResponse.token)
      
      // 更新权限
      authInfo.permissions = this.getUserPermissions(newAuthResponse.user)

      // 重新启动令牌刷新
      this.clearTokenRefreshTimer(connectionId)
      this.startTokenRefresh(connectionId)

      this.logDebug('WebSocket访问令牌刷新成功', { connectionId })

      return {
        success: true,
        state: WebSocketAuthState.AUTHENTICATED,
        authInfo,
        connectionId
      }

    } catch (error) {
      this.logError('刷新WebSocket访问令牌失败', error as Error)
      
      // 标记为令牌过期
      this._authState.set(connectionId, WebSocketAuthState.TOKEN_EXPIRED)
      
      return {
        success: false,
        state: WebSocketAuthState.TOKEN_EXPIRED,
        error: error instanceof Error ? error.message : '令牌刷新失败',
        connectionId
      }
    }
  }

  /**
   * 注销连接
   */
  async logoutConnection(connectionId: string): Promise<void> {
    const authInfo = this._authInfo.get(connectionId)
    if (!authInfo) {
      return
    }

    this.logDebug('注销WebSocket连接', { connectionId })

    try {
      // 清理定时器
      this.clearSessionTimer(connectionId)
      this.clearTokenRefreshTimer(connectionId)

      // 清理认证信息
      this._authInfo.delete(connectionId)
      this._authState.delete(connectionId)
      this._activeConnections.delete(connectionId)
      this._connectionStats.delete(connectionId)

      // 如果是最后一个连接，执行全局注销
      if (this._activeConnections.size === 0) {
        await authService.logout()
      }

      this.logDebug('WebSocket连接注销完成', { connectionId })

    } catch (error) {
      this.logError('注销WebSocket连接失败', error as Error)
    }
  }

  /**
   * 验证消息签名
   */
  async verifyMessageSignature(
    connectionId: string,
    message: WebSocketMessage
  ): Promise<boolean> {
    if (!this._config.enableMessageSigning) {
      return true
    }

    const authInfo = this._authInfo.get(connectionId)
    if (!authInfo) {
      return false
    }

    try {
      // 验证消息签名（简化版本）
      const expectedSignature = this.generateMessageSignature(message, authInfo.accessToken)
      const actualSignature = message.metadata?.signature

      return expectedSignature === actualSignature

    } catch (error) {
      this.logError('验证消息签名失败', error as Error)
      return false
    }
  }

  /**
   * 获取连接认证状态
   */
  getConnectionAuthState(connectionId: string): WebSocketAuthState {
    return this._authState.get(connectionId) || WebSocketAuthState.UNAUTHENTICATED
  }

  /**
   * 获取连接认证信息
   */
  getConnectionAuthInfo(connectionId: string): WebSocketAuthInfo | undefined {
    return this._authInfo.get(connectionId)
  }

  /**
   * 获取所有活跃连接
   */
  getActiveConnections(): string[] {
    return Array.from(this._activeConnections)
  }

  /**
   * 获取认证统计信息
   */
  getAuthStats(): {
    totalConnections: number
    authenticatedConnections: number
    activeConnections: number
    totalMessages: number
    averageMessageTime: number
    sessionTimeouts: number
    tokenRefreshes: number
  } {
    let totalMessages = 0
    let totalMessageTime = 0
    let activeConnections = 0

    for (const stats of this._connectionStats.values()) {
      totalMessages += stats.messageCount
      totalMessageTime += stats.averageMessageTime * stats.messageCount
      
      // 检查连接是否活跃（5分钟内有活动）
      if (Date.now() - stats.lastMessageTime.getTime() < 300000) {
        activeConnections++
      }
    }

    return {
      totalConnections: this._authInfo.size,
      authenticatedConnections: this._authInfo.size,
      activeConnections,
      totalMessages,
      averageMessageTime: totalMessages > 0 ? totalMessageTime / totalMessages : 0,
      sessionTimeouts: this._sessionTimers.size,
      tokenRefreshes: this._tokenRefreshTimers.size
    }
  }

  /**
   * 清理所有连接
   */
  async cleanup(): Promise<void> {
    this.logDebug('开始清理WebSocket认证服务')

    // 清理所有定时器
    this.clearPermissionCheckTimer()
    
    // 注销所有连接
    const logoutPromises = Array.from(this._activeConnections).map(
      connectionId => this.logoutConnection(connectionId)
    )
    
    await Promise.all(logoutPromises)

    // 清理所有映射
    this._authInfo.clear()
    this._authState.clear()
    this._sessionTimers.clear()
    this._tokenRefreshTimers.clear()
    this._connectionStats.clear()
    this._activeConnections.clear()

    this.logDebug('WebSocket认证服务清理完成')
  }

  // 私有方法

  private getDefaultConfig(): WebSocketAuthConfig {
    return {
      enableAuthentication: true,
      enableTokenRefresh: true,
      tokenRefreshThreshold: 300000, // 5分钟
      sessionTimeout: 1800000, // 30分钟
      maxConcurrentConnections: 10,
      permissionCheckInterval: 60000, // 1分钟
      enableConnectionSecurity: true,
      enableMessageSigning: false,
      allowedOrigins: ['*']
    }
  }

  private initialize(): void {
    // 启动权限检查定时器
    this.startPermissionCheck()
    
    this.logDebug('WebSocket认证服务初始化完成')
  }

  private getAccessToken(): string | null {
    return authService.getToken()
  }

  private async validateAccessToken(token: string): Promise<{
    success: boolean
    userId?: string
    username?: string
    refreshToken?: string
    tokenExpiry?: Date
    permissions?: WebSocketPermission[]
    error?: string
  }> {
    try {
      // 验证令牌（简化版本，实际应该调用后端API）
      const user = authService.getCurrentUser()
      if (!user) {
        return {
          success: false,
          error: '无效的访问令牌'
        }
      }

      return {
        success: true,
        userId: user.id,
        username: user.username,
        refreshToken: '', // 实际应该从安全存储中获取
        tokenExpiry: this.calculateTokenExpiry(token),
        permissions: this.getUserPermissions(user)
      }

    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : '令牌验证失败'
      }
    }
  }

  private calculateTokenExpiry(token: string): Date {
    // 简化版本，实际应该解析JWT令牌
    return new Date(Date.now() + 3600000) // 1小时后过期
  }

  private getUserPermissions(user: any): WebSocketPermission[] {
    // 根据用户角色获取权限（简化版本）
    const permissions: WebSocketPermission[] = [
      WebSocketPermission.SEND_MESSAGE,
      WebSocketPermission.RECEIVE_MESSAGE
    ]

    if (user.role === 'admin') {
      permissions.push(
        WebSocketPermission.BROADCAST_MESSAGE,
        WebSocketPermission.JOIN_GROUP,
        WebSocketPermission.LEAVE_GROUP,
        WebSocketPermission.MANAGE_USERS,
        WebSocketPermission.SYSTEM_ADMIN
      )
    } else if (user.role === 'moderator') {
      permissions.push(
        WebSocketPermission.BROADCAST_MESSAGE,
        WebSocketPermission.JOIN_GROUP,
        WebSocketPermission.LEAVE_GROUP
      )
    }

    return permissions
  }

  private isTokenExpired(tokenExpiry: Date): boolean {
    return new Date() > tokenExpiry
  }

  private startSessionTimeout(connectionId: string): void {
    this.clearSessionTimer(connectionId)

    this._sessionTimers.set(connectionId, setTimeout(() => {
      this.handleSessionTimeout(connectionId)
    }, this._config.sessionTimeout))
  }

  private startTokenRefresh(connectionId: string): void {
    this.clearTokenRefreshTimer(connectionId)

    const authInfo = this._authInfo.get(connectionId)
    if (!authInfo) return

    // 计算刷新时间（令牌过期前threshold时间）
    const timeToRefresh = authInfo.tokenExpiry.getTime() - Date.now() - this._config.tokenRefreshThreshold
    
    if (timeToRefresh > 0) {
      this._tokenRefreshTimers.set(connectionId, setTimeout(() => {
        this.refreshToken(connectionId)
      }, timeToRefresh))
    }
  }

  private startPermissionCheck(): void {
    if (this._permissionCheckTimer) return

    this._permissionCheckTimer = setInterval(() => {
      this.checkAllConnectionsPermissions()
    }, this._config.permissionCheckInterval)
  }

  private handleSessionTimeout(connectionId: string): void {
    this.logDebug('WebSocket会话超时', { connectionId })

    this._authState.set(connectionId, WebSocketAuthState.TOKEN_EXPIRED)
    
    // 清理连接
    this.logoutConnection(connectionId).catch(error => {
      this.logError('清理超时连接失败', error)
    })
  }

  private async checkAllConnectionsPermissions(): Promise<void> {
    for (const connectionId of this._activeConnections) {
      const authInfo = this._authInfo.get(connectionId)
      if (!authInfo) continue

      // 检查令牌是否过期
      if (this.isTokenExpired(authInfo.tokenExpiry)) {
        await this.refreshToken(connectionId)
      }
    }
  }

  private generateMessageSignature(message: WebSocketMessage, token: string): string {
    // 简化版本的消息签名生成
    const data = JSON.stringify(message.data) + message.sentAt.toISOString() + token
    return btoa(data).slice(0, 32)
  }

  private initializeConnectionStats(connectionId: string): void {
    this._connectionStats.set(connectionId, {
      messageCount: 0,
      lastMessageTime: new Date(),
      averageMessageTime: 0
    })
  }

  private updateConnectionStats(connectionId: string): void {
    const stats = this._connectionStats.get(connectionId)
    if (stats) {
      stats.lastMessageTime = new Date()
    }
  }

  private clearSessionTimer(connectionId: string): void {
    const timer = this._sessionTimers.get(connectionId)
    if (timer) {
      clearTimeout(timer)
      this._sessionTimers.delete(connectionId)
    }
  }

  private clearTokenRefreshTimer(connectionId: string): void {
    const timer = this._tokenRefreshTimers.get(connectionId)
    if (timer) {
      clearTimeout(timer)
      this._tokenRefreshTimers.delete(connectionId)
    }
  }

  private clearPermissionCheckTimer(): void {
    if (this._permissionCheckTimer) {
      clearInterval(this._permissionCheckTimer)
      this._permissionCheckTimer = null
    }
  }

  private logDebug(message: string, data?: any): void {
    console.log(`[WebSocketAuthService] ${message}`, data || '')
  }

  private logError(message: string, error: Error): void {
    console.error(`[WebSocketAuthService] ${message}`, error)
  }
}

// 创建默认实例
export const webSocketAuthService = new WebSocketAuthService()

// 导出单例工厂函数
export function createWebSocketAuthService(config?: Partial<WebSocketAuthConfig>): WebSocketAuthService {
  return new WebSocketAuthService(config)
}

// 导出类型
export type { WebSocketAuthService }

// 导出工具函数
export { WebSocketAuthState, WebSocketPermission }