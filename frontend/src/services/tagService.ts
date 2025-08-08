import api from './api'
import type { Tag, CreateTagRequest } from '@/types'

export interface TagUsageStatistics {
  id: string
  name: string
  color: string
  usageCount: number
  createdAt: string
}

export class TagService {
  async getTags(): Promise<Tag[]> {
    const response = await api.get<Tag[]>('/tags')
    return response.data
  }

  async getTag(id: string): Promise<Tag> {
    const response = await api.get<Tag>(`/tags/${id}`)
    return response.data
  }

  async createTag(tag: CreateTagRequest): Promise<Tag> {
    const response = await api.post<Tag>('/tags', tag)
    return response.data
  }

  async updateTag(id: string, tag: Partial<CreateTagRequest>): Promise<Tag> {
    const response = await api.put<Tag>(`/tags/${id}`, tag)
    return response.data
  }

  async deleteTag(id: string): Promise<void> {
    await api.delete(`/tags/${id}`)
  }

  /**
   * 搜索标签（按前缀匹配）
   * @param prefix 搜索前缀
   * @param limit 结果数量限制
   * @returns 匹配的标签列表
   */
  async searchTags(prefix: string, limit: number = 10): Promise<Tag[]> {
    const response = await api.get<Tag[]>('/tags/search', {
      params: { prefix, limit }
    })
    return response.data
  }

  /**
   * 获取标签使用统计
   * @returns 标签使用统计列表
   */
  async getTagUsageStatistics(): Promise<TagUsageStatistics[]> {
    const response = await api.get<TagUsageStatistics[]>('/tags/statistics')
    return response.data
  }

  /**
   * 获取最常用的标签
   * @param limit 结果数量限制
   * @returns 最常用标签列表
   */
  async getMostUsedTags(limit: number = 20): Promise<Tag[]> {
    const response = await api.get<Tag[]>('/tags/most-used', {
      params: { limit }
    })
    return response.data
  }

  /**
   * 检查标签是否可以删除
   * @param id 标签ID
   * @returns 是否可以删除
   */
  async canDeleteTag(id: string): Promise<boolean> {
    const response = await api.get<boolean>(`/tags/${id}/can-delete`)
    return response.data
  }

  /**
   * 根据代码片段ID获取关联的标签
   * @param snippetId 代码片段ID
   * @returns 关联的标签列表
   */
  async getTagsBySnippetId(snippetId: string): Promise<Tag[]> {
    const response = await api.get<Tag[]>(`/tags/by-snippet/${snippetId}`)
    return response.data
  }
}

export const tagService = new TagService()
