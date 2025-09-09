/**
 * 分享相关类型定义
 */

// 分享令牌状态枚举
export enum ShareTokenStatus {
  ACTIVE = 'ACTIVE',
  EXPIRED = 'EXPIRED',
  REVOKED = 'REVOKED'
}

// 分享权限类型枚举
export enum SharePermission {
  VIEW = 'VIEW',
  EDIT = 'EDIT'
}

// 分享令牌接口
export interface ShareToken {
  id: string
  token: string
  snippetId: string
  snippetTitle: string
  snippetCode: string
  snippetLanguage: string
  createdBy: string
  creatorName: string
  createdAt: string
  expiresAt?: string
  isPasswordProtected: boolean
  permission: SharePermission
  status: ShareTokenStatus
  maxAccessCount?: number
  currentAccessCount: number
  allowDownload: boolean
  lastAccessedAt?: string
}

// 创建分享令牌请求
export interface CreateShareTokenRequest {
  snippetId: string
  expiresAt?: string
  password?: string
  permission: SharePermission
  maxAccessCount?: number
  allowDownload: boolean
}

// 创建分享令牌DTO
export interface CreateShareTokenDto {
  snippetId: string
  expiresAt?: Date
  password?: string
  permission: SharePermission
  maxAccessCount?: number
  allowDownload: boolean
}

// 更新分享令牌请求
export interface UpdateShareTokenRequest {
  expiresAt?: string
  password?: string
  permission?: SharePermission
  maxAccessCount?: number
  allowDownload?: boolean
}

// 更新分享令牌DTO
export interface UpdateShareTokenDto {
  expiresAt?: Date
  password?: string
  permission?: SharePermission
  maxAccessCount?: number
  allowDownload?: boolean
}

// 分享统计信息
export interface ShareStats {
  shareTokenId: string
  totalAccessCount: number
  uniqueAccessCount: number
  lastAccessedAt?: string
  firstAccessedAt?: string
  dailyStats: DailyShareStats[]
  browserStats: BrowserShareStats[]
  osStats: OSShareStats[]
  locationStats: LocationShareStats[]
}

// 每日分享统计
export interface DailyShareStats {
  date: string
  accessCount: number
  uniqueAccessCount: number
}

// 浏览器分享统计
export interface BrowserShareStats {
  browser: string
  version?: string
  accessCount: number
  percentage: number
}

// 操作系统分享统计
export interface OSShareStats {
  os: string
  version?: string
  accessCount: number
  percentage: number
}

// 地理位置分享统计
export interface LocationShareStats {
  country: string
  region?: string
  city?: string
  accessCount: number
  percentage: number
}

// 分享访问记录
export interface ShareAccessRecord {
  id: string
  shareTokenId: string
  accessedAt: string
  ipAddress: string
  userAgent: string
  browser: string
  browserVersion?: string
  os: string
  osVersion?: string
  device: string
  country?: string
  region?: string
  city?: string
  wasSuccessful: boolean
  errorMessage?: string
}

// 分享访问详情
export interface ShareAccessDetail {
  id: string
  shareToken: ShareToken
  accessedAt: string
  ipAddress: string
  userAgent: string
  browser: string
  browserVersion?: string
  os: string
  osVersion?: string
  device: string
  country?: string
  region?: string
  city?: string
  wasSuccessful: boolean
  errorMessage?: string
}

// 分享令牌过滤器
export interface ShareTokenFilter {
  search?: string
  snippetId?: string
  status?: ShareTokenStatus
  permission?: SharePermission
  createdBy?: string
  isExpired?: boolean
  hasPassword?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  page: number
  pageSize: number
}

// 分享访问记录过滤器
export interface ShareAccessFilter {
  shareTokenId?: string
  snippetId?: string
  startDate?: string
  endDate?: string
  wasSuccessful?: boolean
  ipAddress?: string
  browser?: string
  os?: string
  country?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  page: number
  pageSize: number
}

// 验证分享令牌请求
export interface ValidateShareTokenRequest {
  token: string
  password?: string
}

// 验证分享令牌响应
export interface ValidateShareTokenResponse {
  isValid: boolean
  shareToken?: ShareToken
  errorMessage?: string
}

// 访问分享请求
export interface AccessShareRequest {
  token: string
  password?: string
  userAgent?: string
}

// 访问分享响应
export interface AccessShareResponse {
  success: boolean
  shareToken?: ShareToken
  accessRecord?: ShareAccessRecord
  errorMessage?: string
}

// 撤销分享令牌请求
export interface RevokeShareTokenRequest {
  tokenId: string
  reason?: string
}

// 撤销分享令牌响应
export interface RevokeShareTokenResponse {
  success: boolean
  message: string
  revokedAt: string
}

// 批量操作分享令牌请求
export interface BatchShareTokenRequest {
  tokenIds: string[]
  action: 'revoke' | 'extend' | 'update'
  data?: {
    expiresAt?: string
    password?: string
    permission?: SharePermission
    maxAccessCount?: number
    allowDownload?: boolean
  }
}

// 批量操作分享令牌响应
export interface BatchShareTokenResponse {
  success: boolean
  processedCount: number
  failedCount: number
  results: BatchShareTokenResult[]
}

// 批量操作单个结果
export interface BatchShareTokenResult {
  tokenId: string
  success: boolean
  message?: string
  error?: string
}

// 分享令牌分页结果
export type PaginatedShareTokens = PaginatedResult<ShareToken>

// 分享访问记录分页结果
export type PaginatedShareAccessRecords = PaginatedResult<ShareAccessRecord>

// 分享统计汇总
export interface ShareStatsSummary {
  totalShares: number
  activeShares: number
  expiredShares: number
  revokedShares: number
  totalAccessCount: number
  uniqueAccessCount: number
  mostAccessedSnippet?: {
    snippetId: string
    snippetTitle: string
    accessCount: number
  }
  recentActivity: ShareAccessRecord[]
}

// 分享设置
export interface ShareSettings {
  defaultPermission: SharePermission
  defaultExpiresInHours: number
  maxExpiresInHours: number
  allowPasswordProtection: boolean
  allowDownload: boolean
  maxAccessCountLimit: number
  requireAuthForStats: boolean
  enableAccessLogging: boolean
  retainAccessLogsDays: number
}

// 用户分享配额
export interface UserShareQuota {
  userId: string
  maxActiveShares: number
  currentActiveShares: number
  maxTotalShares: number
  currentTotalShares: number
  maxAccessCountPerShare: number
  resetAt?: string
}

// 重新导入分页结果类型
import type { PaginatedResult } from './index'