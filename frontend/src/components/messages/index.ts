// 消息组件导出文件
// 统一导出所有消息相关的Vue组件

// 主要组件
export { default as MessageList } from './MessageList.vue'
export { default as MessageItem } from './MessageItem.vue'
export { default as MessageForm } from './MessageForm.vue'
export { default as MessageConversation } from './MessageConversation.vue'

// 搜索和过滤组件
export { default as MessageSearch } from './MessageSearch.vue'
export { default as MessageFilter } from './MessageFilter.vue'

// 附件相关组件
export { default as MessageAttachmentUpload } from './MessageAttachmentUpload.vue'
export { default as MessageAttachmentDisplay } from './MessageAttachmentDisplay.vue'
export { default as AttachmentCard } from './AttachmentCard.vue'

// 组件类型定义
export type {
  MessageListProps,
  MessageItemProps,
  MessageFormProps,
  MessageConversationProps,
  MessageSearchProps,
  MessageFilterProps,
  MessageAttachmentUploadProps,
  MessageAttachmentDisplayProps,
  AttachmentCardProps
} from './types'

// 组件事件类型
export type {
  MessageListEmits,
  MessageItemEmits,
  MessageFormEmits,
  MessageConversationEmits,
  MessageSearchEmits,
  MessageFilterEmits,
  MessageAttachmentUploadEmits,
  MessageAttachmentDisplayEmits,
  AttachmentCardEmits
} from './types'

// 过滤器类型
export type {
  MessageFilters,
  FilterPreset,
  ActiveFilter,
  QuickFilter,
  DateRange
} from './types'

// 默认配置
export {
  defaultMessageConfig,
  defaultFilterConfig,
  defaultSearchConfig,
  defaultUploadConfig
} from './config'

// 工具函数
export {
  formatMessageDate,
  formatFileSize,
  getMessageStatusText,
  getMessagePriorityText,
  getMessageTypeText,
  getAvatarColor,
  getFileIcon,
  isImageFile,
  isVideoFile,
  isAudioFile,
  isDocumentFile,
  isPreviewableFile,
  validateFileSize,
  validateFileType,
  generateUUID,
  sanitizeFileName
} from './utils'

// 动画常量
export {
  animationDurations,
  animationEasings,
  transitionPresets
} from './animations'

// 样式常量
export {
  colorPresets,
  spacingPresets,
  typographyPresets
} from './styles'

// 组合式函数
export {
  useMessageActions,
  useMessageFilters,
  useMessageSearch,
  useMessageAttachments,
  useMessageAnimations,
  useMessageKeyboardShortcuts,
  useMessageDragAndDrop,
  useMessageInfiniteScroll,
  useMessageRealTime,
  useMessagePermissions
} from './composables'

// 插件
export {
  MessagePlugin,
  MessageDirective,
  createMessageStore
} from './plugin'

// 全局注册函数
export const registerMessageComponents = (app: any) => {
  // 注册所有组件
  app.component('MessageList', MessageList)
  app.component('MessageItem', MessageItem)
  app.component('MessageForm', MessageForm)
  app.component('MessageConversation', MessageConversation)
  app.component('MessageSearch', MessageSearch)
  app.component('MessageFilter', MessageFilter)
  app.component('MessageAttachmentUpload', MessageAttachmentUpload)
  app.component('MessageAttachmentDisplay', MessageAttachmentDisplay)
  app.component('AttachmentCard', AttachmentCard)
  
  // 注册插件
  app.use(MessagePlugin)
  
  // 注册指令
  app.directive('message', MessageDirective)
  
  return app
}

// 默认导出
export default {
  // 组件
  MessageList,
  MessageItem,
  MessageForm,
  MessageConversation,
  MessageSearch,
  MessageFilter,
  MessageAttachmentUpload,
  MessageAttachmentDisplay,
  AttachmentCard,
  
  // 插件和工具
  MessagePlugin,
  MessageDirective,
  registerMessageComponents,
  
  // 工具函数和配置
  ...utils,
  ...config,
  ...animations,
  ...styles
}