/**
 * 服务导出文件
 * 
 * 本文件统一导出所有服务，便于组件导入使用
 */

// 主要API服务
export { commentService } from './commentService'
export { commentAdminService } from './commentAdminService'
export { messageService } from './messageService'
export { notificationService } from './notificationService'

// 组合式API
export {
  useComments,
  useComment,
  useCommentLikes,
  useCommentReports,
  useCommentStats,
  useCommentPermissions
} from '../composables/useComments'

export {
  useNotifications,
  useNotificationPermissions,
  useNotificationSettings,
  useNotificationWebSocket
} from '../composables/useNotifications'

// 工具函数
export {
  CommentErrorHandler,
  CommentValidator,
  CommentUtils,
  COMMENT_VALIDATION_RULES,
  COMMENT_STATUS_CONFIG,
  REPORT_REASON_CONFIG,
  COMMENT_SORT_OPTIONS,
  REPORT_SORT_OPTIONS,
  CommentErrorType,
  type CommentError
} from '../utils/commentUtils'

// 类型定义
export type * from '../types/comment'
export type * from '../types/message'
export type * from '../types/notification'