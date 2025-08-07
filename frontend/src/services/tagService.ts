import api from './api'
import type { Tag, CreateTagRequest } from '@/types'

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
}

export const tagService = new TagService()
