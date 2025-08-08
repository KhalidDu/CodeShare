import api from './api'
import type {
  CodeSnippet,
  CreateSnippetRequest,
  UpdateSnippetRequest,
  PaginatedResult,
  SnippetFilter,
  SnippetVersion,
  VersionComparison
} from '@/types'

export class CodeSnippetService {
  async getSnippets(filter: SnippetFilter): Promise<PaginatedResult<CodeSnippet>> {
    const response = await api.get<PaginatedResult<CodeSnippet>>('/codesnippets', {
      params: filter
    })
    return response.data
  }

  async getSnippet(id: string): Promise<CodeSnippet> {
    const response = await api.get<CodeSnippet>(`/codesnippets/${id}`)
    return response.data
  }

  async createSnippet(snippet: CreateSnippetRequest): Promise<CodeSnippet> {
    const response = await api.post<CodeSnippet>('/codesnippets', snippet)
    return response.data
  }

  async updateSnippet(id: string, snippet: UpdateSnippetRequest): Promise<CodeSnippet> {
    const response = await api.put<CodeSnippet>(`/codesnippets/${id}`, snippet)
    return response.data
  }

  async deleteSnippet(id: string): Promise<void> {
    await api.delete(`/codesnippets/${id}`)
  }

  async copySnippet(id: string): Promise<void> {
    await api.post(`/codesnippets/${id}/copy`)
  }

  // 版本管理相关方法
  async getVersionHistory(snippetId: string): Promise<SnippetVersion[]> {
    const response = await api.get<SnippetVersion[]>(`/versions/snippet/${snippetId}`)
    return response.data
  }

  async getVersion(versionId: string): Promise<SnippetVersion> {
    const response = await api.get<SnippetVersion>(`/versions/${versionId}`)
    return response.data
  }

  async restoreVersion(snippetId: string, versionId: string): Promise<void> {
    await api.post(`/versions/snippet/${snippetId}/restore/${versionId}`)
  }

  async compareVersions(fromVersionId: string, toVersionId: string): Promise<VersionComparison> {
    const response = await api.get<VersionComparison>(`/versions/compare/${fromVersionId}/${toVersionId}`)
    return response.data
  }

  /**
   * 创建新版本
   * @param snippetId 代码片段ID
   * @param request 创建版本请求
   * @returns 创建的版本
   */
  async createVersion(snippetId: string, request: { changeDescription?: string }): Promise<SnippetVersion> {
    const response = await api.post<SnippetVersion>(`/versions/snippet/${snippetId}`, request)
    return response.data
  }
}

export const codeSnippetService = new CodeSnippetService()
