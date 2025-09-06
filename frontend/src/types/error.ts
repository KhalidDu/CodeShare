/**
 * 错误相关类型定义
 */

// 错误类型枚举
export enum ErrorType {
  NETWORK = 'NETWORK',
  VALIDATION = 'VALIDATION',
  AUTHENTICATION = 'AUTHENTICATION',
  AUTHORIZATION = 'AUTHORIZATION',
  NOT_FOUND = 'NOT_FOUND',
  SERVER = 'SERVER',
  TIMEOUT = 'TIMEOUT',
  UNKNOWN = 'UNKNOWN'
}

// 错误严重程度
export enum ErrorSeverity {
  LOW = 'LOW',
  MEDIUM = 'MEDIUM',
  HIGH = 'HIGH',
  CRITICAL = 'CRITICAL'
}

// 应用错误接口
export interface AppError {
  id: string
  type: ErrorType
  severity: ErrorSeverity
  title: string
  message: string
  details?: string
  timestamp: Date
  context?: Record<string, unknown>
  retryable?: boolean
  dismissible?: boolean
}

// API 错误响应接口
export interface ApiErrorResponse {
  code: string
  message: string
  details?: string
  errors?: Record<string, string[]>
  timestamp?: string
}

// 网络错误接口
export interface NetworkError {
  code: string
  message: string
  status?: number
  timeout?: boolean
}

// 验证错误接口
export interface ValidationError {
  field: string
  message: string
  code?: string
}

// 错误处理选项
export interface ErrorHandlingOptions {
  showNotification?: boolean
  logError?: boolean
  retryable?: boolean
  dismissible?: boolean
  autoHide?: boolean
  hideDelay?: number
}
