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
  confirmPassword: string
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
  creator?: string
  showPublic?: boolean
  showPrivate?: boolean
  sortBy?: string
  page: number
  pageSize: number
}

// 版本相关类型
export interface SnippetVersion {
  id: string
  snippetId: string
  versionNumber: number
  title: string
  description: string
  code: string
  language: string
  createdBy: string
  creatorName: string
  createdAt: string
  changeDescription: string
}

export interface VersionComparison {
  oldVersion: SnippetVersion
  newVersion: SnippetVersion
  differences: VersionDifference[]
}

export interface VersionDifference {
  type: 'added' | 'removed' | 'modified'
  lineNumber: number
  oldContent?: string
  newContent?: string
}

// 编辑器相关类型
export interface SupportedLanguage {
  value: string
  label: string
}

export type EditorTheme = 'vs' | 'vs-dark' | 'hc-black'

export interface EditorOptions {
  language?: string
  theme?: EditorTheme
  readonly?: boolean
  height?: string
  fontSize?: number
  tabSize?: number
  insertSpaces?: boolean
  wordWrap?: 'on' | 'off' | 'wordWrapColumn' | 'bounded'
  minimap?: {
    enabled: boolean
  }
  lineNumbers?: 'on' | 'off' | 'relative' | 'interval'
  renderWhitespace?: 'none' | 'boundary' | 'selection' | 'trailing' | 'all'
  folding?: boolean
  showFoldingControls?: 'always' | 'mouseover'
}

export interface EditorInstance {
  setValue: (value: string) => void
  getValue: () => string
  setLanguage: (language: string) => void
  setTheme: (theme: string) => void
  focus: () => void
  formatCode: () => Promise<void>
}

export interface EditorEvents {
  'update:modelValue': (value: string) => void
  'language-change': (language: string) => void
  'theme-change': (theme: string) => void
}

// 剪贴板历史相关类型
export interface ClipboardHistoryItem {
  id: string
  userId: string
  snippetId: string
  snippetTitle: string
  snippetCode: string
  snippetLanguage: string
  copiedAt: string
}

export interface ClipboardHistoryFilter {
  page: number
  pageSize: number
  startDate?: string
  endDate?: string
}
