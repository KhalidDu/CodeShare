import api from './api'
import type {
  CodeSnippet,
  CreateSnippetRequest,
  UpdateSnippetRequest,
  PaginatedResult,
  SnippetFilter
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
}

export const codeSnippetService = new CodeSnippetService()
