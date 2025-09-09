import api from './api'
import type {
  ShareToken,
  CreateShareTokenRequest,
  UpdateShareTokenRequest,
  ShareStats,
  ShareAccessRecord,
  ShareTokenFilter,
  ShareAccessFilter,
  ValidateShareTokenRequest,
  ValidateShareTokenResponse,
  AccessShareRequest,
  AccessShareResponse,
  RevokeShareTokenRequest,
  RevokeShareTokenResponse,
  BatchShareTokenRequest,
  BatchShareTokenResponse,
  ShareStatsSummary,
  ShareSettings,
  UserShareQuota,
  PaginatedResult
} from '@/types'

export class ShareService {
  /**
   * 创建分享令牌
   * @param request 创建分享令牌请求
   * @returns 创建的分享令牌
   */
  async createShareToken(request: CreateShareTokenRequest): Promise<ShareToken> {
    const response = await api.post<ShareToken>('/share/tokens', request)
    return response.data
  }

  /**
   * 获取分享令牌列表
   * @param filter 过滤条件
   * @returns 分页的分享令牌列表
   */
  async getShareTokens(filter: ShareTokenFilter): Promise<PaginatedResult<ShareToken>> {
    const response = await api.get<PaginatedResult<ShareToken>>('/share/tokens', {
      params: filter
    })
    return response.data
  }

  /**
   * 根据ID获取分享令牌
   * @param id 分享令牌ID
   * @returns 分享令牌详情
   */
  async getShareToken(id: string): Promise<ShareToken> {
    const response = await api.get<ShareToken>(`/share/tokens/${id}`)
    return response.data
  }

  /**
   * 根据令牌字符串获取分享令牌
   * @param token 分享令牌字符串
   * @returns 分享令牌详情
   */
  async getShareTokenByToken(token: string): Promise<ShareToken> {
    const response = await api.get<ShareToken>(`/share/tokens/token/${token}`)
    return response.data
  }

  /**
   * 更新分享令牌
   * @param id 分享令牌ID
   * @param request 更新请求
   * @returns 更新后的分享令牌
   */
  async updateShareToken(id: string, request: UpdateShareTokenRequest): Promise<ShareToken> {
    const response = await api.put<ShareToken>(`/share/tokens/${id}`, request)
    return response.data
  }

  /**
   * 删除分享令牌
   * @param id 分享令牌ID
   */
  async deleteShareToken(id: string): Promise<void> {
    await api.delete(`/share/tokens/${id}`)
  }

  /**
   * 撤销分享令牌
   * @param request 撤销请求
   * @returns 撤销结果
   */
  async revokeShareToken(request: RevokeShareTokenRequest): Promise<RevokeShareTokenResponse> {
    const response = await api.post<RevokeShareTokenResponse>('/share/tokens/revoke', request)
    return response.data
  }

  /**
   * 验证分享令牌
   * @param request 验证请求
   * @returns 验证结果
   */
  async validateShareToken(request: ValidateShareTokenRequest): Promise<ValidateShareTokenResponse> {
    const response = await api.post<ValidateShareTokenResponse>('/share/tokens/validate', request)
    return response.data
  }

  /**
   * 访问分享内容
   * @param request 访问请求
   * @returns 访问结果
   */
  async accessShare(request: AccessShareRequest): Promise<AccessShareResponse> {
    const response = await api.post<AccessShareResponse>('/share/access', request)
    return response.data
  }

  /**
   * 获取分享统计信息
   * @param tokenId 分享令牌ID
   * @returns 分享统计信息
   */
  async getShareStats(tokenId: string): Promise<ShareStats> {
    const response = await api.get<ShareStats>(`/share/stats/${tokenId}`)
    return response.data
  }

  /**
   * 获取分享访问记录
   * @param filter 过滤条件
   * @returns 分页的访问记录列表
   */
  async getShareAccessRecords(filter: ShareAccessFilter): Promise<PaginatedResult<ShareAccessRecord>> {
    const response = await api.get<PaginatedResult<ShareAccessRecord>>('/share/access-records', {
      params: filter
    })
    return response.data
  }

  /**
   * 获取代码片段的所有分享令牌
   * @param snippetId 代码片段ID
   * @returns 分享令牌列表
   */
  async getSnippetShareTokens(snippetId: string): Promise<ShareToken[]> {
    const response = await api.get<ShareToken[]>(`/share/snippet/${snippetId}/tokens`)
    return response.data
  }

  /**
   * 批量操作分享令牌
   * @param request 批量操作请求
   * @returns 批量操作结果
   */
  async batchShareTokens(request: BatchShareTokenRequest): Promise<BatchShareTokenResponse> {
    const response = await api.post<BatchShareTokenResponse>('/share/tokens/batch', request)
    return response.data
  }

  /**
   * 获取分享统计汇总
   * @param snippetId 代码片段ID（可选）
   * @returns 分享统计汇总
   */
  async getShareStatsSummary(snippetId?: string): Promise<ShareStatsSummary> {
    const params = snippetId ? { snippetId } : {}
    const response = await api.get<ShareStatsSummary>('/share/stats/summary', { params })
    return response.data
  }

  /**
   * 获取分享设置
   * @returns 分享设置
   */
  async getShareSettings(): Promise<ShareSettings> {
    const response = await api.get<ShareSettings>('/share/settings')
    return response.data
  }

  /**
   * 更新分享设置
   * @param settings 分享设置
   * @returns 更新后的分享设置
   */
  async updateShareSettings(settings: Partial<ShareSettings>): Promise<ShareSettings> {
    const response = await api.put<ShareSettings>('/share/settings', settings)
    return response.data
  }

  /**
   * 获取用户分享配额
   * @param userId 用户ID（可选，默认为当前用户）
   * @returns 用户分享配额信息
   */
  async getUserShareQuota(userId?: string): Promise<UserShareQuota> {
    const params = userId ? { userId } : {}
    const response = await api.get<UserShareQuota>('/share/quota', { params })
    return response.data
  }

  /**
   * 检查用户是否可以创建分享令牌
   * @param snippetId 代码片段ID
   * @returns 检查结果
   */
  async canCreateShareToken(snippetId: string): Promise<{ canCreate: boolean; reason?: string }> {
    const response = await api.get<{ canCreate: boolean; reason?: string }>(`/share/can-create/${snippetId}`)
    return response.data
  }

  /**
   * 刷新分享令牌
   * @param tokenId 分享令牌ID
   * @returns 刷新后的分享令牌
   */
  async refreshShareToken(tokenId: string): Promise<ShareToken> {
    const response = await api.post<ShareToken>(`/share/tokens/${tokenId}/refresh`)
    return response.data
  }

  /**
   * 复制分享令牌
   * @param tokenId 分享令牌ID
   * @returns 新创建的分享令牌
   */
  async copyShareToken(tokenId: string): Promise<ShareToken> {
    const response = await api.post<ShareToken>(`/share/tokens/${tokenId}/copy`)
    return response.data
  }

  /**
   * 获取分享链接
   * @param token 分享令牌
   * @returns 完整的分享链接
   */
  getShareLink(token: string): string {
    const baseUrl = window.location.origin
    return `${baseUrl}/share/${token}`
  }

  /**
   * 验证分享请求参数
   * @param request 创建分享令牌请求
   * @returns 验证结果
   */
  validateShareRequest(request: CreateShareTokenRequest): { isValid: boolean; errors: string[] } {
    const errors: string[] = []

    if (!request.snippetId) {
      errors.push('代码片段ID不能为空')
    }

    if (!request.permission) {
      errors.push('分享权限不能为空')
    }

    if (request.expiresAt) {
      const expiresAt = new Date(request.expiresAt)
      const now = new Date()
      if (expiresAt <= now) {
        errors.push('过期时间必须晚于当前时间')
      }
    }

    if (request.maxAccessCount !== undefined && request.maxAccessCount <= 0) {
      errors.push('最大访问次数必须大于0')
    }

    return {
      isValid: errors.length === 0,
      errors
    }
  }

  /**
   * 验证访问分享请求参数
   * @param request 访问分享请求
   * @returns 验证结果
   */
  validateAccessRequest(request: AccessShareRequest): { isValid: boolean; errors: string[] } {
    const errors: string[] = []

    if (!request.token) {
      errors.push('分享令牌不能为空')
    }

    if (request.password !== undefined && typeof request.password !== 'string') {
      errors.push('密码必须是字符串')
    }

    return {
      isValid: errors.length === 0,
      errors
    }
  }
}

export const shareService = new ShareService()