// 用户相关类型
export interface User {
  id: string
  username: string
  email: string
  role: UserRole
  createdAt: string
  isActive: boolean
}

export enum UserRole {
  Viewer = 0,
  Editor = 1,
  Admin = 2
}

// 代码片段相关类型
export interface CodeSnippet {
  id: string
  title: string
  description: string
  code: string
  language: string
  createdBy: string
  creatorName: string
  createdAt: string
  updatedAt: string
  isPublic: boolean
  viewCount: number
  copyCount: number
  tags: Tag[]
}

export interface CreateSnippetRequest {
  title: string
  description: string
  code: string
  language: string
  isPublic: boolean
  tags: string[]
}

export interface UpdateSnippetRequest {
  title?: string
  description?: string
  code?: string
  language?: string
  isPublic?: boolean
  tags?: string[]
}

// 标签相关类型
export interface Tag {
  id: string
  name: string
  color: string
  createdBy: string
  createdAt: string
}

export interface CreateTagRequest {
  name: string
  color: string
}

// 认证相关类型
export interface LoginRequest {
  username: string
  password: string
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}

export interface AuthResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: User
}

// 分页相关类型
export interface PaginatedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface SnippetFilter {
  search?: string
  language?: string
  tag?: string
  page: number
  pageSize: number
}
