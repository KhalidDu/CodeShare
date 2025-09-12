/**
 * 评论相关类型定义
 * 
 * 本文件定义了评论系统前端所需的完整类型定义
 * 与后端API接口保持一致，支持Vue 3 + TypeScript开发
 */

// 评论状态枚举 - 对应后端CommentStatus枚举
export enum CommentStatus {
  NORMAL = 0,
  DELETED = 1,
  HIDDEN = 2,
  PENDING = 3
}

// 举报原因枚举 - 对应后端ReportReason枚举
export enum ReportReason {
  SPAM = 0,
  INAPPROPRIATE = 1,
  HARASSMENT = 2,
  HATE_SPEECH = 3,
  MISINFORMATION = 4,
  COPYRIGHT_VIOLATION = 5,
  OTHER = 99
}

// 举报状态枚举 - 对应后端ReportStatus枚举
export enum ReportStatus {
  PENDING = 0,
  RESOLVED = 1,
  REJECTED = 2,
  UNDER_INVESTIGATION = 3
}

// 评论排序枚举 - 对应后端CommentSort枚举
export enum CommentSort {
  CREATED_AT_DESC = 0,
  CREATED_AT_ASC = 1,
  LIKE_COUNT_DESC = 2,
  LIKE_COUNT_ASC = 3,
  REPLY_COUNT_DESC = 4,
  REPLY_COUNT_ASC = 5
}

// 评论举报排序枚举 - 对应后端ReportSort枚举
export enum ReportSort {
  CREATED_AT_DESC = 0,
  CREATED_AT_ASC = 1,
  REASON = 2,
  STATUS = 3
}

// 评论操作类型枚举 - 对应后端CommentOperation枚举
export enum CommentOperation {
  DELETE = 0,
  HIDE = 1,
  SHOW = 2,
  APPROVE = 3
}

// 评论批量操作类型枚举 - 对应后端CommentBatchOperationType枚举
export enum CommentBatchOperationType {
  DELETE = 0,
  HIDE = 1,
  SHOW = 2,
  APPROVE = 3,
  REJECT = 4,
  MOVE_TO_TRASH = 5,
  RESTORE = 6
}

// 点赞排序枚举 - 对应后端LikeSort枚举
export enum LikeSort {
  CREATED_AT_DESC = 0,
  CREATED_AT_ASC = 1,
  USER_ID = 2
}

// 评论搜索范围枚举 - 对应后端CommentSearchScope枚举
export enum CommentSearchScope {
  ALL = 0,
  CONTENT = 1,
  USER_NAME = 2,
  SNIPPET_TITLE = 3
}

// 评论搜索排序枚举 - 对应后端CommentSearchSort枚举
export enum CommentSearchSort {
  RELEVANCE = 0,
  CREATED_AT_DESC = 1,
  CREATED_AT_ASC = 2,
  LIKE_COUNT_DESC = 3,
  REPLY_COUNT_DESC = 4
}

// 评论接口 - 对应后端CommentDto
export interface Comment {
  id: string
  content: string
  snippetId: string
  userId: string
  userName: string
  userAvatar?: string
  parentId?: string
  parentPath?: string
  depth: number
  likeCount: number
  replyCount: number
  status: CommentStatus
  createdAt: string
  updatedAt: string
  isLikedByCurrentUser: boolean
  canEdit: boolean
  canDelete: boolean
  canReport: boolean
  replies: Comment[]
}

// 评论简略信息接口 - 对应后端CommentSummaryDto
export interface CommentSummary {
  id: string
  contentSummary: string
  contentLength: number
  snippetId: string
  snippetTitle: string
  userId: string
  userName: string
  userAvatar?: string
  likeCount: number
  replyCount: number
  status: CommentStatus
  createdAt: string
  updatedAt: string
  isLikedByCurrentUser: boolean
}

// 创建评论请求 - 对应后端CreateCommentDto
export interface CreateCommentRequest {
  content: string
  snippetId: string
  parentId?: string
}

// 更新评论请求 - 对应后端UpdateCommentDto
export interface UpdateCommentRequest {
  content: string
}

// 评论点赞请求 - 对应后端CommentLikeDto
export interface CommentLikeRequest {
  commentId: string
  userId?: string
}

// 评论审核请求 - 对应后端ModerateCommentDto
export interface ModerateCommentRequest {
  status: CommentStatus
  reason?: string
}

// 创建评论举报请求 - 对应后端CreateCommentReportDto
export interface CreateCommentReportRequest {
  commentId: string
  reason: ReportReason
  description?: string
}

// 处理评论举报请求 - 对应后端HandleCommentReportDto
export interface HandleCommentReportRequest {
  status: ReportStatus
  resolution?: string
}

// 评论举报接口 - 对应后端CommentReportDto
export interface CommentReport {
  id: string
  commentId: string
  userId: string
  userName: string
  reason: ReportReason
  description?: string
  status: ReportStatus
  createdAt: string
  handledAt?: string
  handlerName?: string
  resolution?: string
}

// 评论点赞记录 - 对应后端CommentLike
export interface CommentLike {
  id: string
  commentId: string
  userId: string
  createdAt: string
}

// 评论点赞响应 - 对应后端CommentLikeResponseDto
export interface CommentLikeResponse {
  id: string
  commentId: string
  userId: string
  userName: string
  userAvatar?: string
  createdAt: string
  isCurrentUser: boolean
}

// 评论筛选条件 - 对应后端CommentFilter
export interface CommentFilter {
  snippetId?: string
  userId?: string
  status?: CommentStatus
  parentId?: string
  minDepth?: number
  maxDepth?: number
  startDate?: string
  endDate?: string
  search?: string
  minLikeCount?: number
  maxLikeCount?: number
  minReplyCount?: number
  maxReplyCount?: number
  sortBy: CommentSort
  page: number
  pageSize: number
  includeUser?: boolean
  includeReplies?: boolean
  includeLikes?: boolean
  includeReports?: boolean
  currentUserId?: string
}

// 评论举报筛选条件 - 对应后端CommentReportFilter
export interface CommentReportFilter {
  status?: ReportStatus
  reason?: ReportReason
  commentId?: string
  userId?: string
  handledBy?: string
  startDate?: string
  endDate?: string
  search?: string
  sortBy: ReportSort
  page: number
  pageSize: number
  includeComment?: boolean
  includeReporter?: boolean
  includeHandler?: boolean
  highPriorityOnly?: boolean
}

// 评论点赞筛选条件 - 对应后端CommentLikeFilter
export interface CommentLikeFilter {
  commentId?: string
  userId?: string
  startDate?: string
  endDate?: string
  sortBy: LikeSort
  page: number
  pageSize: number
  includeUser?: boolean
  includeComment?: boolean
}

// 评论搜索筛选条件 - 对应后端CommentSearchFilterDto
export interface CommentSearchFilter {
  keyword: string
  snippetId?: string
  userId?: string
  status?: CommentStatus
  startDate?: string
  endDate?: string
  searchScope: CommentSearchScope
  sortBy: CommentSearchSort
  page: number
  pageSize: number
}

// 评论统计信息
export interface CommentStats {
  snippetId: string
  totalComments: number
  rootComments: number
  replyComments: number
  totalLikes: number
  activeUsers: number
  latestCommentAt?: string
}

// 批量操作评论请求 - 对应后端BatchCommentOperationDto
export interface BatchCommentOperationRequest {
  commentIds: string[]
  operation: CommentOperation
  reason?: string
}

// 评论批量操作请求 - 对应后端CommentBatchOperation
export interface CommentBatchOperation {
  commentIds: string[]
  operation: CommentBatchOperationType
  reason?: string
  operatorId: string
  sendNotification?: boolean
}

// 批量操作结果 - 对应后端BatchOperationResultDto
export interface BatchOperationResult {
  totalCount: number
  successCount: number
  failedCount: number
  errorMessages: string[]
  successfulIds: string[]
  failedIds: string[]
}

// 评论搜索结果 - 对应后端CommentSearchResultDto
export interface CommentSearchResult {
  id: string
  content: string
  highlightedContent: string
  snippetId: string
  snippetTitle: string
  userId: string
  userName: string
  userAvatar?: string
  likeCount: number
  replyCount: number
  status: CommentStatus
  createdAt: string
  searchScore: number
  matchedKeywords: string[]
}

// 评论树节点
export interface CommentTreeNode {
  comment: Comment
  children: CommentTreeNode[]
}

// 评论管理权限
export interface CommentPermissions {
  canCreate: boolean
  canEdit: boolean
  canDelete: boolean
  canReport: boolean
  canLike: boolean
  canModerate: boolean
}

// 评论设置
export interface CommentSettings {
  enableComments: boolean
  allowAnonymousComments: boolean
  requireApproval: boolean
  maxCommentLength: number
  maxReplyDepth: number
  enableLikeFeature: boolean
  enableReportFeature: boolean
  autoDeleteSpam: boolean
  spamKeywords: string[]
  restrictedWords: string[]
}

// 评论统计详细信息 - 对应后端CommentStatsDetailDto
export interface CommentStatsDetail {
  snippetId: string
  totalComments: number
  rootComments: number
  replyComments: number
  totalLikes: number
  activeUsers: number
  latestCommentAt?: string
  averageCommentLength: number
  dailyStats: DailyCommentStats[]
  userStats: UserCommentStats[]
  statusDistribution: Record<CommentStatus, number>
}

// 每日评论统计 - 对应后端DailyCommentStatsDto
export interface DailyCommentStats {
  date: string
  commentCount: number
  likeCount: number
  newUsers: number
}

// 用户评论统计 - 对应后端UserCommentStatsDto
export interface UserCommentStats {
  userId: string
  userName: string
  userAvatar?: string
  commentCount: number
  likesReceived: number
  lastCommentAt?: string
}

// 评论统计筛选条件 - 对应后端CommentStatsFilter
export interface CommentStatsFilter {
  snippetIds?: string[]
  userIds?: string[]
  startDate?: string
  endDate?: string
  groupByDay?: boolean
  groupByUser?: boolean
  groupBySnippet?: boolean
}

// 评论导出数据 - 对应后端CommentExportDto
export interface CommentExportData {
  id: string
  content: string
  snippetId: string
  snippetTitle: string
  snippetLanguage: string
  userId: string
  userName: string
  userEmail?: string
  parentId?: string
  depth: number
  likeCount: number
  replyCount: number
  status: CommentStatus
  createdAt: string
  updatedAt: string
  deletedAt?: string
}

// 评论导出选项
export interface CommentExportOptions {
  format: 'json' | 'csv' | 'excel'
  snippetId?: string
  status?: CommentStatus
  startDate?: string
  endDate?: string
  includeReplies: boolean
  includeUserInfo: boolean
}

// 评论导入数据
export interface CommentImportData {
  comments: ImportComment[]
  snippetId: string
}

// 导入评论数据
export interface ImportComment {
  content: string
  userId?: string
  userName?: string
  parentId?: string
  createdAt?: string
  status?: CommentStatus
}

// 评论分析数据
export interface CommentAnalytics {
  totalComments: number
  totalLikes: number
  totalReports: number
  averageCommentsPerSnippet: number
  averageLikesPerComment: number
  topCommenters: Array<{
    userId: string
    userName: string
    commentCount: number
    likeCount: number
  }>
  mostLikedComments: Comment[]
  mostActiveSnippets: Array<{
    snippetId: string
    snippetTitle: string
    commentCount: number
  }>
  commentTrends: Array<{
    date: string
    commentCount: number
    likeCount: number
    reportCount: number
  }>
}

// 评论通知设置
export interface CommentNotificationSettings {
  onNewComment: boolean
  onReply: boolean
  onLike: boolean
  onMention: boolean
  onReport: boolean
  emailNotifications: boolean
  pushNotifications: boolean
}

// 评论通知
export interface CommentNotification {
  id: string
  type: 'new_comment' | 'reply' | 'like' | 'mention' | 'report'
  commentId: string
  snippetId: string
  snippetTitle: string
  triggeredBy: {
    userId: string
    userName: string
  }
  message: string
  createdAt: string
  isRead: boolean
  data?: any
}

// 评论分页结果类型
export type PaginatedComments = PaginatedResult<Comment>
export type PaginatedCommentReports = PaginatedResult<CommentReport>
export type PaginatedCommentLikes = PaginatedResult<CommentLikeResponse>
export type PaginatedCommentSearchResults = PaginatedResult<CommentSearchResult>
export type PaginatedCommentSummaries = PaginatedResult<CommentSummary>

// 重新导入分页结果类型
import type { PaginatedResult } from './index'