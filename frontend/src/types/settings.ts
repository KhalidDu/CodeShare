// 系统设置相关的TypeScript类型定义
export interface SystemSettings {
  id: string
  createdAt: string
  updatedAt: string
  updatedBy: string
  siteSettings: SiteSettings
  securitySettings: SecuritySettings
  featureSettings: FeatureSettings
  emailSettings: EmailSettings
}

export interface SiteSettings {
  siteName: string
  siteDescription: string
  logoUrl: string
  theme: string
  language: string
  pageSize: number
  allowRegistration: boolean
  announcement: string
  customCss: string
  customJs: string
}

export interface SecuritySettings {
  minPasswordLength: number
  requireUppercase: boolean
  requireLowercase: boolean
  requireNumbers: boolean
  requireSpecialChars: boolean
  maxLoginAttempts: number
  accountLockoutDuration: number
  sessionTimeout: number
  enableTwoFactorAuth: boolean
  enableCors: boolean
  allowedCorsOrigins: string[]
  enableHttpsRedirection: boolean
  apiRateLimit: number
  enableLoginLogging: boolean
  enableActionLogging: boolean
}

export interface FeatureSettings {
  enableCodeSnippets: boolean
  enableSharing: boolean
  enableTags: boolean
  enableComments: boolean
  enableFavorites: boolean
  enableSearch: boolean
  enableExport: boolean
  enableImport: boolean
  enableApi: boolean
  enableWebHooks: boolean
  enableFileUpload: boolean
  maxFileSize: number
  allowedFileTypes: string[]
  enableRealTimeNotifications: boolean
  enableAnalytics: boolean
}

export interface EmailSettings {
  smtpHost: string
  smtpPort: number
  smtpUsername: string
  smtpPassword: string
  fromEmail: string
  fromName: string
  enableSsl: boolean
  enableTls: boolean
  templatePath: string
  enableEmailQueue: boolean
  maxRetryAttempts: number
  emailTimeout: number
  enableEmailLogging: boolean
  testEmailRecipient: string
}

// 更新请求类型
export interface UpdateSiteSettingsRequest {
  siteName?: string
  siteDescription?: string
  logoUrl?: string
  theme?: string
  language?: string
  pageSize?: number
  allowRegistration?: boolean
  announcement?: string
  customCss?: string
  customJs?: string
}

export interface UpdateSecuritySettingsRequest {
  minPasswordLength?: number
  requireUppercase?: boolean
  requireLowercase?: boolean
  requireNumbers?: boolean
  requireSpecialChars?: boolean
  maxLoginAttempts?: number
  accountLockoutDuration?: number
  sessionTimeout?: number
  enableTwoFactorAuth?: boolean
  enableCors?: boolean
  allowedCorsOrigins?: string[]
  enableHttpsRedirection?: boolean
  apiRateLimit?: number
  enableLoginLogging?: boolean
  enableActionLogging?: boolean
}

export interface UpdateFeatureSettingsRequest {
  enableCodeSnippets?: boolean
  enableSharing?: boolean
  enableTags?: boolean
  enableComments?: boolean
  enableFavorites?: boolean
  enableSearch?: boolean
  enableExport?: boolean
  enableImport?: boolean
  enableApi?: boolean
  enableWebHooks?: boolean
  enableFileUpload?: boolean
  maxFileSize?: number
  allowedFileTypes?: string[]
  enableRealTimeNotifications?: boolean
  enableAnalytics?: boolean
}

export interface UpdateEmailSettingsRequest {
  smtpHost?: string
  smtpPort?: number
  smtpUsername?: string
  smtpPassword?: string
  fromEmail?: string
  fromName?: string
  enableSsl?: boolean
  enableTls?: boolean
  templatePath?: string
  enableEmailQueue?: boolean
  maxRetryAttempts?: number
  emailTimeout?: number
  enableEmailLogging?: boolean
  testEmailRecipient?: string
}

// 设置历史相关类型
export interface SettingsHistory {
  id: string
  createdAt: string
  settingType: string
  settingKey: string
  oldValue: string
  newValue: string
  changedBy: string
  changeCategory: string
  clientIp: string
  userAgent: string
  isImportant: boolean
  status: string
  errorMessage: string
}

export interface SettingsHistoryRequest {
  pageNumber?: number
  pageSize?: number
  startDate?: string
  endDate?: string
  settingType?: string
  changedBy?: string
  isImportant?: boolean
}

export interface SettingsHistoryResponse {
  items: SettingsHistory[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface SettingsHistoryStatistics {
  totalChanges: number
  changesByType: Record<string, number>
  changesByUser: Record<string, number>
  changesByCategory: Record<string, number>
  recentChanges: SettingsHistory[]
  topUsers: Array<{
    username: string
    changeCount: number
  }>
}

// 验证相关类型
export interface SettingsValidationResult {
  isValid: boolean
  errors: Record<string, string[]>
  warnings: Record<string, string[]>
}

export interface ValidationError {
  field: string
  message: string
  severity: 'error' | 'warning'
}

// 邮件测试相关类型
export interface TestEmailRequest {
  recipientEmail: string
  subject?: string
  body?: string
}

export interface TestEmailResponse {
  success: boolean
  message: string
  sentAt: string
  errorDetails?: string
}

// 导入导出相关类型
export interface ImportSettingsRequest {
  jsonData: string
  validateOnly?: boolean
}

export interface ValidateImportDataRequest {
  jsonData: string
}

export interface SettingsHistoryExportRequest {
  format: 'json' | 'csv'
  startDate?: string
  endDate?: string
  settingType?: string
  changedBy?: string
  isImportant?: boolean
}

export interface SettingsHistoryExportResponse {
  success: boolean
  data?: string
  fileName: string
  contentType: string
  fileSize: number
  errorMessage?: string
}

export interface BatchDeleteSettingsHistoryRequest {
  historyIds: string[]
}

export interface RestoreFromBackupRequest {
  backupData: string
  validateOnly?: boolean
}

// 设置类型枚举
export enum SettingType {
  Site = 'Site',
  Security = 'Security',
  Feature = 'Feature',
  Email = 'Email'
}

export enum ChangeCategory {
  Configuration = 'Configuration',
  Security = 'Security',
  Feature = 'Feature',
  Email = 'Email',
  System = 'System',
  Other = 'Other'
}

export enum ChangeStatus {
  Success = 'Success',
  Failed = 'Failed',
  Warning = 'Warning'
}

// 主题选项
export const THEME_OPTIONS = [
  { value: 'light', label: '浅色主题' },
  { value: 'dark', label: '深色主题' },
  { value: 'auto', label: '跟随系统' }
] as const

// 语言选项
export const LANGUAGE_OPTIONS = [
  { value: 'zh-CN', label: '简体中文' },
  { value: 'zh-TW', label: '繁体中文' },
  { value: 'en-US', label: 'English' }
] as const

// 文件类型选项
export const FILE_TYPE_OPTIONS = [
  { value: 'image', label: '图片文件', extensions: ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.svg'] },
  { value: 'document', label: '文档文件', extensions: ['.pdf', '.doc', '.docx', '.txt', '.md'] },
  { value: 'code', label: '代码文件', extensions: ['.js', '.ts', '.vue', '.html', '.css', '.scss'] },
  { value: 'archive', label: '压缩文件', extensions: ['.zip', '.rar', '.7z', '.tar', '.gz'] }
] as const

// 页面大小选项
export const PAGE_SIZE_OPTIONS = [
  { value: 10, label: '10 条/页' },
  { value: 20, label: '20 条/页' },
  { value: 50, label: '50 条/页' },
  { value: 100, label: '100 条/页' }
] as const

// 导出格式选项
export const EXPORT_FORMAT_OPTIONS = [
  { value: 'json', label: 'JSON 格式', extension: '.json' },
  { value: 'csv', label: 'CSV 格式', extension: '.csv' },
  { value: 'excel', label: 'Excel 格式', extension: '.xlsx' }
] as const

// API响应类型
export interface ApiResponse<T> {
  success: boolean
  data?: T
  message?: string
  errors?: string[]
}

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

// 设置表单状态
export interface SettingsFormState {
  loading: boolean
  saving: boolean
  error: string | null
  validationErrors: ValidationError[]
  lastSavedAt: string | null
}

// 设置页面状态
export interface SettingsPageState {
  activeTab: string
  loading: boolean
  saving: boolean
  error: string | null
  hasChanges: boolean
  originalSettings: SystemSettings | null
  currentSettings: SystemSettings | null
}

// 设置历史查询状态
export interface SettingsHistoryState {
  loading: boolean
  items: SettingsHistory[]
  totalCount: number
  currentPage: number
  pageSize: number
  filters: SettingsHistoryRequest
  error: string | null
}