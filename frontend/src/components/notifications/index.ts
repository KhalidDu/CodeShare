/**
 * CodeShare项目通知系统组件导出文件
 * 
 * 本文件统一导出通知系统的所有Vue组件，包括：
 * - NotificationList: 通知列表组件
 * - NotificationItem: 通知项组件  
 * - NotificationSettings: 通知设置组件
 * 
 * @version 1.0.0
 * @lastUpdated 2025-09-12
 */

// 导出Vue组件
export { default as NotificationList } from './NotificationList.vue'
export { default as NotificationItem } from './NotificationItem.vue'
export { default as NotificationSettings } from './NotificationSettings.vue'

// 导出组件类型
export type {
  NotificationListProps,
  NotificationListEmits
} from './NotificationList.vue'

export type {
  NotificationItemProps,
  NotificationItemEmits
} from './NotificationItem.vue'

export type {
  NotificationSettingsProps,
  NotificationSettingsEmits
} from './NotificationSettings.vue'

// 导出组合式函数（如果有的话）
// export { useNotificationList } from './composables/useNotificationList'
// export { useNotificationItem } from './composables/useNotificationItem'
// export { useNotificationSettings } from './composables/useNotificationSettings'

// 导出工具函数（如果有的话）
// export { notificationUtils } from './utils/notificationUtils'

/**
 * 通知系统组件使用示例：
 * 
 * ```vue
 * <template>
 *   <NotificationList
 *     :max-items="10"
 *     :show-filters="true"
 *     :show-search="true"
 *     @notification-click="handleNotificationClick"
 *   />
 * </template>
 * 
 * <script setup lang="ts">
 * import { NotificationList } from '@/components/notifications'
 * 
 * function handleNotificationClick(notification) {
 *   console.log('通知被点击:', notification)
 * }
 * </script>
 * ```
 */

/**
 * 组件特性说明：
 * 
 * 1. NotificationList:
 *    - 支持分页、搜索、筛选
 *    - 支持虚拟滚动（大数据量优化）
 *    - 支持实时更新
 *    - 支持批量操作
 *    - 响应式设计
 *    - 完整的可访问性支持
 * 
 * 2. NotificationItem:
 *    - 支持多种通知类型显示
 *    - 支持优先级指示器
 *    - 支持操作按钮和菜单
 *    - 支持展开/收起详情
 *    - 支持附件预览
 *    - 支持历史记录查看
 * 
 * 3. NotificationSettings:
 *    - 支持基础设置配置
 *    - 支持通知渠道管理
 *    - 支持通知类型设置
 *    - 支持免打扰时间设置
 *    - 支持频率控制设置
 *    - 支持外观和隐私设置
 *    - 支持通知测试功能
 */