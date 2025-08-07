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
    const response = await api.get<SnippetVersion[]>(`/codesnippets/${snippetId}/versions`)
    return response.data
  }

  async getVersion(snippetId: string, versionId: string): Promise<SnippetVersion> {
    const response = await api.get<SnippetVersion>(`/codesnippets/${snippetId}/versions/${versionId}`)
    return response.data
  }

  async restoreVersion(snippetId: string, versionId: string): Promise<CodeSnippet> {
    const response = await api.post<CodeSnippet>(`/codesnippets/${snippetId}/versions/${versionId}/restore`)
    return response.data
  }

  async compareVersions(snippetId: string, oldVersionId: string, newVersionId: string): Promise<VersionComparison> {
    const response = await api.get<VersionComparison>(`/codesnippets/${snippetId}/versions/compare`, {
      params: {
        oldVersionId,
        newVersionId
      }
    })
    return response.data
  }
}

export const codeSnippetService = new CodeSnippetService()
