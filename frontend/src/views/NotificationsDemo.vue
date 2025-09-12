<template>
  <div class="notifications-demo">
    <div class="notifications-demo-header">
      <h1>通知系统演示</h1>
      <p>展示通知组件的各种功能和交互效果</p>
    </div>
    
    <div class="notifications-demo-content">
      <!-- 通知列表组件 -->
      <div class="notifications-demo-section">
        <h2>通知列表</h2>
        <NotificationList
          :max-items="10"
          :show-header="true"
          :show-stats="true"
          :show-filters="true"
          :show-search="true"
          :show-pagination="true"
          :show-load-more="false"
          :show-refresh="true"
          :show-mark-all-read="true"
          :show-settings="true"
          :show-websocket-status="true"
          :auto-refresh="false"
          :refresh-interval="30000"
          title="我的通知"
          description="管理您的通知和消息"
          @notification-click="handleNotificationClick"
          @notification-read="handleNotificationRead"
          @notification-archive="handleNotificationArchive"
          @notification-delete="handleNotificationDelete"
          @notification-action="handleNotificationAction"
          @image-click="handleImageClick"
          @refresh="handleRefresh"
          @load-more="handleLoadMore"
          @settings-click="handleSettingsClick"
        />
      </div>
      
      <!-- 通知设置组件 -->
      <div class="notifications-demo-section" v-if="showSettings">
        <h2>通知设置</h2>
        <NotificationSettings
          @save="handleSettingsSave"
          @cancel="handleSettingsCancel"
          @test="handleSettingsTest"
        />
      </div>
      
      <!-- 控制面板 -->
      <div class="notifications-demo-section">
        <h2>控制面板</h2>
        <div class="notifications-demo-controls">
          <div class="notifications-demo-control-group">
            <h3>模拟通知</h3>
            <div class="notifications-demo-buttons">
              <button @click="createInfoNotification" class="btn btn-info">
                创建信息通知
              </button>
              <button @click="createSuccessNotification" class="btn btn-success">
                创建成功通知
              </button>
              <button @click="createWarningNotification" class="btn btn-warning">
                创建警告通知
              </button>
              <button @click="createErrorNotification" class="btn btn-danger">
                创建错误通知
              </button>
            </div>
          </div>
          
          <div class="notifications-demo-control-group">
            <h3>WebSocket控制</h3>
            <div class="notifications-demo-buttons">
              <button 
                @click="toggleWebSocket" 
                :class="['btn', isWebSocketConnected ? 'btn-danger' : 'btn-primary']"
              >
                {{ isWebSocketConnected ? '断开连接' : '连接WebSocket' }}
              </button>
              <button 
                @click="sendTestNotification" 
                class="btn btn-secondary"
                :disabled="!isWebSocketConnected"
              >
                发送测试通知
              </button>
              <button 
                @click="requestDesktopPermission" 
                class="btn btn-info"
              >
                请求桌面通知权限
              </button>
            </div>
          </div>
          
          <div class="notifications-demo-control-group">
            <h3>批量操作</h3>
            <div class="notifications-demo-buttons">
              <button 
                @click="markAllAsRead" 
                class="btn btn-success"
                :disabled="!hasUnreadNotifications"
              >
                全部标记为已读
              </button>
              <button 
                @click="clearAllNotifications" 
                class="btn btn-danger"
                :disabled="!hasNotifications"
              >
                清空所有通知
              </button>
              <button @click="refreshNotifications" class="btn btn-primary">
                刷新通知
              </button>
            </div>
          </div>
        </div>
      </div>
      
      <!-- 状态显示 -->
      <div class="notifications-demo-section">
        <h2>状态信息</h2>
        <div class="notifications-demo-status">
          <div class="status-item">
            <span class="status-label">WebSocket连接状态:</span>
            <span :class="['status-value', isWebSocketConnected ? 'connected' : 'disconnected']">
              {{ isWebSocketConnected ? '已连接' : '未连接' }}
            </span>
          </div>
          <div class="status-item">
            <span class="status-label">通知总数:</span>
            <span class="status-value">{{ totalCount }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">未读通知:</span>
            <span class="status-value unread">{{ unreadCount }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">已读通知:</span>
            <span class="status-value">{{ readCount }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">桌面通知权限:</span>
            <span class="status-value">{{ desktopNotificationPermission }}</span>
          </div>
          <div class="status-item">
            <span class="status-label">免打扰时间:</span>
            <span class="status-value">{{ isInQuietHours ? '是' : '否' }}</span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useNotificationsStore } from '@/stores/notifications'
import { useAuthStore } from '@/stores/auth'
import { useRealtimeNotifications } from '@/composables/useRealtimeNotifications'
import NotificationList from '@/components/notifications/NotificationList.vue'
import NotificationSettings from '@/components/notifications/NotificationSettings.vue'
import type { 
  Notification, 
  NotificationAction,
  NotificationType,
  NotificationPriority,
  NotificationChannel
} from '@/types/notifications'

const notificationsStore = useNotificationsStore()
const authStore = useAuthStore()
const {
  isConnected: isWebSocketConnected,
  connectWebSocket,
  disconnectWebSocket,
  sendTestNotification: sendTestWebSocketNotification,
  requestDesktopNotificationPermission,
  isInQuietHours
} = useRealtimeNotifications()

// 状态
const showSettings = ref(false)
const desktopNotificationPermission = ref('未请求')

// 计算属性
const totalCount = computed(() => notificationsStore.totalCount)
const unreadCount = computed(() => notificationsStore.unreadCount)
const readCount = computed(() => notificationsStore.readCount)
const hasNotifications = computed(() => totalCount.value > 0)
const hasUnreadNotifications = computed(() => unreadCount.value > 0)

// 通知列表事件处理
function handleNotificationClick(notification: Notification): void {
  console.log('点击通知:', notification)
  // 可以在这里跳转到相关页面
}

function handleNotificationRead(notification: Notification): void {
  console.log('标记已读:', notification)
}

function handleNotificationArchive(notification: Notification): void {
  console.log('归档通知:', notification)
}

function handleNotificationDelete(notification: Notification): void {
  console.log('删除通知:', notification)
}

function handleNotificationAction(action: NotificationAction, notification: Notification): void {
  console.log('执行操作:', action, notification)
}

function handleImageClick(notification: Notification): void {
  console.log('点击图片:', notification)
}

function handleRefresh(): void {
  console.log('刷新通知列表')
}

function handleLoadMore(): void {
  console.log('加载更多通知')
}

function handleSettingsClick(): void {
  showSettings.value = !showSettings.value
}

// 通知设置事件处理
function handleSettingsSave(settings: any): void {
  console.log('保存设置:', settings)
  showSettings.value = false
}

function handleSettingsCancel(): void {
  console.log('取消设置')
  showSettings.value = false
}

function handleSettingsTest(channel: NotificationChannel): void {
  console.log('测试渠道:', channel)
}

// WebSocket控制
async function toggleWebSocket(): Promise<void> {
  if (isWebSocketConnected.value) {
    disconnectWebSocket()
  } else {
    await connectWebSocket()
  }
}

async function sendTestNotification(): Promise<void> {
  try {
    sendTestWebSocketNotification()
  } catch (error) {
    console.error('发送测试通知失败:', error)
  }
}

async function requestDesktopPermission(): Promise<void> {
  try {
    const granted = await requestDesktopNotificationPermission()
    desktopNotificationPermission.value = granted ? '已授权' : '未授权'
  } catch (error) {
    console.error('请求桌面通知权限失败:', error)
    desktopNotificationPermission.value = '请求失败'
  }
}

// 创建模拟通知
async function createNotification(
  type: NotificationType,
  priority: NotificationPriority,
  title: string,
  message: string
): Promise<void> {
  try {
    await notificationsStore.notificationService.createNotification({
      type,
      priority,
      title,
      message,
      channel: [NotificationChannel.InApp],
      data: {
        source: 'demo',
        timestamp: new Date().toISOString()
      }
    })
  } catch (error) {
    console.error('创建通知失败:', error)
  }
}

async function createInfoNotification(): Promise<void> {
  await createNotification(
    NotificationType.Info,
    NotificationPriority.Normal,
    '信息通知',
    '这是一个信息通知的示例内容。'
  )
}

async function createSuccessNotification(): Promise<void> {
  await createNotification(
    NotificationType.Success,
    NotificationPriority.Normal,
    '成功通知',
    '操作已成功完成！'
  )
}

async function createWarningNotification(): Promise<void> {
  await createNotification(
    NotificationType.Warning,
    NotificationPriority.High,
    '警告通知',
    '请注意，这是一个重要的警告信息。'
  )
}

async function createErrorNotification(): Promise<void> {
  await createNotification(
    NotificationType.Error,
    NotificationPriority.Urgent,
    '错误通知',
    '发生了一个错误，请及时处理。'
  )
}

// 批量操作
async function markAllAsRead(): Promise<void> {
  try {
    await notificationsStore.markAllAsRead()
  } catch (error) {
    console.error('全部标记为已读失败:', error)
  }
}

async function clearAllNotifications(): Promise<void> {
  try {
    // 获取所有通知ID
    const notificationIds = notificationsStore.notifications.map(n => n.id)
    if (notificationIds.length > 0) {
      await notificationsStore.batchDeleteNotifications(notificationIds)
    }
  } catch (error) {
    console.error('清空通知失败:', error)
  }
}

async function refreshNotifications(): Promise<void> {
  try {
    await notificationsStore.refreshNotifications()
  } catch (error) {
    console.error('刷新通知失败:', error)
  }
}

// 生命周期
onMounted(async () => {
  // 检查桌面通知权限
  if ('Notification' in window) {
    desktopNotificationPermission.value = 
      Notification.permission === 'granted' ? '已授权' : 
      Notification.permission === 'denied' ? '已拒绝' : '未授权'
  }
  
  // 加载通知设置
  await notificationsStore.fetchNotificationSettings()
  
  // 加载通知列表
  await notificationsStore.fetchNotifications()
  
  // 连接WebSocket
  if (authStore.user) {
    await connectWebSocket()
  }
})
</script>

<style scoped>
.notifications-demo {
  max-width: 1200px;
  margin: 0 auto;
  padding: 2rem;
}

.notifications-demo-header {
  text-align: center;
  margin-bottom: 3rem;
}

.notifications-demo-header h1 {
  font-size: 2.5rem;
  font-weight: 700;
  color: #212529;
  margin: 0 0 0.5rem 0;
}

.notifications-demo-header p {
  font-size: 1.125rem;
  color: #6c757d;
  margin: 0;
}

.notifications-demo-content {
  display: grid;
  gap: 2rem;
}

.notifications-demo-section {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.notifications-demo-section h2 {
  font-size: 1.5rem;
  font-weight: 600;
  color: #212529;
  margin: 0;
  padding: 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.notifications-demo-controls {
  padding: 1.5rem;
}

.notifications-demo-control-group {
  margin-bottom: 2rem;
}

.notifications-demo-control-group:last-child {
  margin-bottom: 0;
}

.notifications-demo-control-group h3 {
  font-size: 1.125rem;
  font-weight: 600;
  color: #212529;
  margin: 0 0 1rem 0;
}

.notifications-demo-buttons {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.btn {
  border: none;
  border-radius: 0.5rem;
  padding: 0.75rem 1.5rem;
  font-size: 0.875rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  text-decoration: none;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.btn-primary {
  background: #007bff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0056b3;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 123, 255, 0.3);
}

.btn-success {
  background: #28a745;
  color: white;
}

.btn-success:hover:not(:disabled) {
  background: #1e7e34;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(40, 167, 69, 0.3);
}

.btn-warning {
  background: #ffc107;
  color: #212529;
}

.btn-warning:hover:not(:disabled) {
  background: #e0a800;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(255, 193, 7, 0.3);
}

.btn-danger {
  background: #dc3545;
  color: white;
}

.btn-danger:hover:not(:disabled) {
  background: #c82333;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(220, 53, 69, 0.3);
}

.btn-info {
  background: #17a2b8;
  color: white;
}

.btn-info:hover:not(:disabled) {
  background: #138496;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(23, 162, 184, 0.3);
}

.btn-secondary {
  background: #6c757d;
  color: white;
}

.btn-secondary:hover:not(:disabled) {
  background: #545b62;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(108, 117, 125, 0.3);
}

.notifications-demo-status {
  padding: 1.5rem;
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 1rem;
}

.status-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  background: rgba(0, 123, 255, 0.02);
  border-radius: 0.5rem;
  border: 1px solid rgba(0, 123, 255, 0.1);
}

.status-label {
  font-weight: 500;
  color: #495057;
}

.status-value {
  font-weight: 600;
  color: #212529;
}

.status-value.connected {
  color: #28a745;
}

.status-value.disconnected {
  color: #dc3545;
}

.status-value.unread {
  color: #007bff;
  font-weight: 700;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .notifications-demo {
    padding: 1rem;
  }
  
  .notifications-demo-header h1 {
    font-size: 2rem;
  }
  
  .notifications-demo-section h2 {
    font-size: 1.25rem;
    padding: 1rem;
  }
  
  .notifications-demo-controls {
    padding: 1rem;
  }
  
  .notifications-demo-buttons {
    flex-direction: column;
  }
  
  .btn {
    width: 100%;
  }
  
  .notifications-demo-status {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 480px) {
  .notifications-demo-header h1 {
    font-size: 1.5rem;
  }
  
  .notifications-demo-header p {
    font-size: 1rem;
  }
  
  .notifications-demo-section h2 {
    font-size: 1.125rem;
  }
}
</style>