/**
 * 评论错误处理和验证服务
 * 
 * 本文件提供评论系统的错误处理、验证和工具功能：
 * - 错误分类和处理
 * - 数据验证
 * - 工具函数
 * - 常量定义
 * 
 * 遵循Vue 3 + Composition API开发模式，使用TypeScript确保类型安全
 */

import type { CommentStatus, ReportReason, CommentSort, ReportSort } from '@/types/comment'

/**
 * 评论错误类型
 */
export enum CommentErrorType {
  NETWORK_ERROR = 'NETWORK_ERROR',
  VALIDATION_ERROR = 'VALIDATION_ERROR',
  AUTHENTICATION_ERROR = 'AUTHENTICATION_ERROR',
  AUTHORIZATION_ERROR = 'AUTHORIZATION_ERROR',
  NOT_FOUND_ERROR = 'NOT_FOUND_ERROR',
  RATE_LIMIT_ERROR = 'RATE_LIMIT_ERROR',
  SERVER_ERROR = 'SERVER_ERROR',
  UNKNOWN_ERROR = 'UNKNOWN_ERROR'
}

/**
 * 评论错误信息
 */
export interface CommentError {
  type: CommentErrorType
  code: string
  message: string
  details?: any
  timestamp: string
}

/**
 * 评论验证规则
 */
export const COMMENT_VALIDATION_RULES = {
  // 评论内容验证
  content: {
    minLength: 1,
    maxLength: 1000,
    required: true,
    pattern: /^[\s\S]*$/ // 允许所有字符
  },
  
  // 评论标题验证
  title: {
    minLength: 1,
    maxLength: 100,
    required: false
  },
  
  // 举报描述验证
  reportDescription: {
    minLength: 10,
    maxLength: 500,
    required: false
  },
  
  // 审核原因验证
  moderationReason: {
    minLength: 1,
    maxLength: 500,
    required: false
  },
  
  // 分页验证
  pagination: {
    minPage: 1,
    maxPage: 1000,
    minPageSize: 1,
    maxPageSize: 100
  }
}

/**
 * 评论状态配置
 */
export const COMMENT_STATUS_CONFIG = {
  [CommentStatus.NORMAL]: {
    label: '正常',
    color: 'success',
    icon: 'check-circle'
  },
  [CommentStatus.DELETED]: {
    label: '已删除',
    color: 'danger',
    icon: 'trash'
  },
  [CommentStatus.HIDDEN]: {
    label: '已隐藏',
    color: 'warning',
    icon: 'eye-slash'
  },
  [CommentStatus.PENDING]: {
    label: '待审核',
    color: 'info',
    icon: 'clock'
  }
}

/**
 * 举报原因配置
 */
export const REPORT_REASON_CONFIG = {
  [ReportReason.SPAM]: {
    label: '垃圾信息',
    description: '广告、垃圾邮件等无关内容',
    severity: 'low'
  },
  [ReportReason.INAPPROPRIATE]: {
    label: '不当内容',
    description: '包含不当或不适宜的内容',
    severity: 'medium'
  },
  [ReportReason.HARASSMENT]: {
    label: '骚扰行为',
    description: '骚扰、欺凌或威胁行为',
    severity: 'high'
  },
  [ReportReason.HATE_SPEECH]: {
    label: '仇恨言论',
    description: '基于种族、宗教等的仇恨言论',
    severity: 'high'
  },
  [ReportReason.MISINFORMATION]: {
    label: '虚假信息',
    description: '传播虚假或误导性信息',
    severity: 'medium'
  },
  [ReportReason.COPYRIGHT_VIOLATION]: {
    label: '版权侵权',
    description: '侵犯版权的内容',
    severity: 'medium'
  },
  [ReportReason.OTHER]: {
    label: '其他',
    description: '其他需要报告的问题',
    severity: 'low'
  }
}

/**
 * 评论排序选项
 */
export const COMMENT_SORT_OPTIONS = [
  { value: CommentSort.CREATED_AT_DESC, label: '最新发布' },
  { value: CommentSort.CREATED_AT_ASC, label: '最早发布' },
  { value: CommentSort.LIKE_COUNT_DESC, label: '最多点赞' },
  { value: CommentSort.LIKE_COUNT_ASC, label: '最少点赞' },
  { value: CommentSort.REPLY_COUNT_DESC, label: '最多回复' },
  { value: CommentSort.REPLY_COUNT_ASC, label: '最少回复' }
]

/**
 * 举报排序选项
 */
export const REPORT_SORT_OPTIONS = [
  { value: ReportSort.CREATED_AT_DESC, label: '最新举报' },
  { value: ReportSort.CREATED_AT_ASC, label: '最早举报' },
  { value: ReportSort.REASON, label: '按原因分类' },
  { value: ReportSort.STATUS, label: '按状态分类' }
]

/**
 * 错误处理工具类
 */
export class CommentErrorHandler {
  /**
   * 处理API错误
   * @param error 原始错误
   * @returns 标准化的错误信息
   */
  static handleApiError(error: any): CommentError {
    const timestamp = new Date().toISOString()
    
    // 网络错误
    if (!error.response) {
      if (error.code === 'ECONNABORTED') {
        return {
          type: CommentErrorType.NETWORK_ERROR,
          code: 'TIMEOUT_ERROR',
          message: '请求超时，请检查网络连接',
          timestamp
        }
      }
      
      if (error.message === 'Network Error') {
        return {
          type: CommentErrorType.NETWORK_ERROR,
          code: 'NETWORK_ERROR',
          message: '网络连接失败，请检查网络设置',
          timestamp
        }
      }
      
      return {
        type: CommentErrorType.NETWORK_ERROR,
        code: 'NETWORK_ERROR',
        message: '网络连接失败',
        timestamp
      }
    }
    
    // HTTP错误
    const { status, data } = error.response
    
    switch (status) {
      case 400:
        return {
          type: CommentErrorType.VALIDATION_ERROR,
          code: data?.code || 'VALIDATION_ERROR',
          message: data?.message || '请求参数无效',
          details: data?.errors,
          timestamp
        }
        
      case 401:
        return {
          type: CommentErrorType.AUTHENTICATION_ERROR,
          code: 'UNAUTHORIZED',
          message: '请先登录',
          timestamp
        }
        
      case 403:
        return {
          type: CommentErrorType.AUTHORIZATION_ERROR,
          code: 'FORBIDDEN',
          message: '您没有权限执行此操作',
          timestamp
        }
        
      case 404:
        return {
          type: CommentErrorType.NOT_FOUND_ERROR,
          code: 'NOT_FOUND',
          message: '评论不存在',
          timestamp
        }
        
      case 429:
        return {
          type: CommentErrorType.RATE_LIMIT_ERROR,
          code: 'RATE_LIMITED',
          message: '操作过于频繁，请稍后再试',
          timestamp
        }
        
      case 500:
        return {
          type: CommentErrorType.SERVER_ERROR,
          code: 'SERVER_ERROR',
          message: '服务器内部错误',
          timestamp
        }
        
      default:
        return {
          type: CommentErrorType.UNKNOWN_ERROR,
          code: `HTTP_${status}`,
          message: data?.message || '未知错误',
          timestamp
        }
    }
  }
  
  /**
   * 获取用户友好的错误消息
   * @param error 错误对象
   * @returns 用户友好的错误消息
   */
  static getUserFriendlyMessage(error: CommentError): string {
    const messages = {
      [CommentErrorType.NETWORK_ERROR]: '网络连接出现问题，请检查网络设置',
      [CommentErrorType.VALIDATION_ERROR]: '输入数据格式不正确，请检查后重试',
      [CommentErrorType.AUTHENTICATION_ERROR]: '请先登录后再试',
      [CommentErrorType.AUTHORIZATION_ERROR]: '您没有权限执行此操作',
      [CommentErrorType.NOT_FOUND_ERROR]: '评论不存在或已被删除',
      [CommentErrorType.RATE_LIMIT_ERROR]: '操作过于频繁，请稍后再试',
      [CommentErrorType.SERVER_ERROR]: '服务器繁忙，请稍后再试',
      [CommentErrorType.UNKNOWN_ERROR]: '操作失败，请重试'
    }
    
    return messages[error.type] || error.message
  }
}

/**
 * 评论验证工具类
 */
export class CommentValidator {
  /**
   * 验证评论内容
   * @param content 评论内容
   * @returns 验证结果
   */
  static validateContent(content: string): { isValid: boolean; errors: string[] } {
    const errors: string[] = []
    const rules = COMMENT_VALIDATION_RULES.content
    
    if (!content || content.trim().length === 0) {
      errors.push('评论内容不能为空')
    }
    
    if (content.length < rules.minLength) {
      errors.push(`评论内容至少需要${rules.minLength}个字符`)
    }
    
    if (content.length > rules.maxLength) {
      errors.push(`评论内容不能超过${rules.maxLength}个字符`)
    }
    
    // 检查是否只包含空白字符
    if (content.trim().length === 0 && content.length > 0) {
      errors.push('评论内容不能只包含空白字符')
    }
    
    return {
      isValid: errors.length === 0,
      errors
    }
  }
  
  /**
   * 验证举报描述
   * @param description 举报描述
   * @returns 验证结果
   */
  static validateReportDescription(description: string): { isValid: boolean; errors: string[] } {
    const errors: string[] = []
    const rules = COMMENT_VALIDATION_RULES.reportDescription
    
    if (description && description.length > 0) {
      if (description.length < rules.minLength) {
        errors.push(`举报描述至少需要${rules.minLength}个字符`)
      }
      
      if (description.length > rules.maxLength) {
        errors.push(`举报描述不能超过${rules.maxLength}个字符`)
      }
    }
    
    return {
      isValid: errors.length === 0,
      errors
    }
  }
  
  /**
   * 验证分页参数
   * @param page 页码
   * @param pageSize 每页大小
   * @returns 验证结果
   */
  static validatePagination(page: number, pageSize: number): { isValid: boolean; errors: string[] } {
    const errors: string[] = []
    const rules = COMMENT_VALIDATION_RULES.pagination
    
    if (page < rules.minPage) {
      errors.push(`页码不能小于${rules.minPage}`)
    }
    
    if (page > rules.maxPage) {
      errors.push(`页码不能大于${rules.maxPage}`)
    }
    
    if (pageSize < rules.minPageSize) {
      errors.push(`每页大小不能小于${rules.minPageSize}`)
    }
    
    if (pageSize > rules.maxPageSize) {
      errors.push(`每页大小不能大于${rules.maxPageSize}`)
    }
    
    return {
      isValid: errors.length === 0,
      errors
    }
  }
}

/**
 * 评论工具函数
 */
export class CommentUtils {
  /**
   * 格式化时间戳
   * @param timestamp 时间戳
   * @returns 格式化的时间字符串
   */
  static formatTime(timestamp: string): string {
    const date = new Date(timestamp)
    const now = new Date()
    const diff = now.getTime() - date.getTime()
    
    // 小于1分钟
    if (diff < 60000) {
      return '刚刚'
    }
    
    // 小于1小时
    if (diff < 3600000) {
      return `${Math.floor(diff / 60000)}分钟前`
    }
    
    // 小于1天
    if (diff < 86400000) {
      return `${Math.floor(diff / 3600000)}小时前`
    }
    
    // 小于7天
    if (diff < 604800000) {
      return `${Math.floor(diff / 86400000)}天前`
    }
    
    // 超过7天显示具体日期
    return date.toLocaleDateString('zh-CN')
  }
  
  /**
   * 截断文本
   * @param text 原文本
   * @param maxLength 最大长度
   * @returns 截断后的文本
   */
  static truncateText(text: string, maxLength: number): string {
    if (text.length <= maxLength) {
      return text
    }
    
    return text.substring(0, maxLength) + '...'
  }
  
  /**
   * 高亮关键词
   * @param text 原文本
   * @param keywords 关键词数组
   * @returns 高亮后的HTML
   */
  static highlightKeywords(text: string, keywords: string[]): string {
    if (!keywords || keywords.length === 0) {
      return text
    }
    
    let result = text
    keywords.forEach(keyword => {
      const regex = new RegExp(`(${keyword})`, 'gi')
      result = result.replace(regex, '<mark>$1</mark>')
    })
    
    return result
  }
  
  /**
   * 生成随机ID
   * @returns 随机ID
   */
  static generateId(): string {
    return Math.random().toString(36).substring(2, 15) + 
           Math.random().toString(36).substring(2, 15)
  }
  
  /**
   * 防抖函数
   * @param func 原函数
   * @param delay 延迟时间
   * @returns 防抖后的函数
   */
  static debounce<T extends (...args: any[]) => any>(func: T, delay: number): T {
    let timeoutId: NodeJS.Timeout
    
    return ((...args: any[]) => {
      clearTimeout(timeoutId)
      timeoutId = setTimeout(() => func.apply(null, args), delay)
    }) as T
  }
  
  /**
   * 节流函数
   * @param func 原函数
   * @param delay 延迟时间
   * @returns 节流后的函数
   */
  static throttle<T extends (...args: any[]) => any>(func: T, delay: number): T {
    let lastCall = 0
    
    return ((...args: any[]) => {
      const now = Date.now()
      if (now - lastCall >= delay) {
        lastCall = now
        func.apply(null, args)
      }
    }) as T
  }
}