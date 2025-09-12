import { ref, onMounted, onUnmounted } from 'vue'
import { useNotificationsStore } from '@/stores/notifications'
import { useAuthStore } from '@/stores/auth'
import type { Notification } from '@/types/notifications'

/**
 * WebSocket实时通知管理Composable
 * 提供通知系统的实时通信功能
 */
export function useRealtimeNotifications() {
  const notificationsStore = useNotificationsStore()
  const authStore = useAuthStore()
  
  const isConnected = ref(false)
  const connectionError = ref<string | null>(null)
  const reconnectAttempts = ref(0)
  const maxReconnectAttempts = 5
  
  /**
   * 连接WebSocket
   */
  async function connectWebSocket(): Promise<void> {
    if (!authStore.user) {
      connectionError.value = '用户未登录'
      return
    }
    
    try {
      await notificationsStore.connectWebSocket(authStore.user.id)
      isConnected.value = true
      connectionError.value = null
      reconnectAttempts.value = 0
      
      console.log('WebSocket连接已建立')
    } catch (error) {
      connectionError.value = error instanceof Error ? error.message : '连接失败'
      isConnected.value = false
      
      console.error('WebSocket连接失败:', error)
      
      // 自动重连
      if (reconnectAttempts.value < maxReconnectAttempts) {
        reconnectAttempts.value++
        const delay = Math.pow(2, reconnectAttempts.value) * 1000
        
        console.log(`${delay}ms后尝试重连 (${reconnectAttempts.value}/${maxReconnectAttempts})`)
        
        setTimeout(() => {
          connectWebSocket()
        }, delay)
      }
    }
  }
  
  /**
   * 断开WebSocket连接
   */
  function disconnectWebSocket(): void {
    notificationsStore.disconnectWebSocket()
    isConnected.value = false
    connectionError.value = null
    reconnectAttempts.value = 0
    
    console.log('WebSocket连接已断开')
  }
  
  /**
   * 重新连接WebSocket
   */
  async function reconnectWebSocket(): Promise<void> {
    reconnectAttempts.value = 0
    await connectWebSocket()
  }
  
  /**
   * 发送测试通知
   */
  function sendTestNotification(): void {
    if (isConnected.value) {
      notificationsStore.notificationService.sendWebSocketMessage({
        type: 'test',
        data: {
          message: '这是一个测试通知',
          timestamp: new Date().toISOString()
        }
      })
    }
  }
  
  /**
   * 处理新通知
   */
  function handleNewNotification(notification: Notification): void {
    console.log('收到新通知:', notification)
    
    // 播放声音
    if (notificationsStore.settings?.enableSound) {
      playNotificationSound()
    }
    
    // 桌面通知
    if (notificationsStore.settings?.enableDesktopNotifications) {
      showDesktopNotification(notification)
    }
    
    // 震动提醒
    if (notificationsStore.settings?.appearance?.maxVisible && 
        notificationsStore.notifications.length >= notificationsStore.settings.appearance.maxVisible) {
      vibrate()
    }
  }
  
  /**
   * 播放通知声音
   */
  function playNotificationSound(): void {
    try {
      const audio = new Audio('/sounds/notification.mp3')
      audio.play().catch(error => {
        console.error('播放通知声音失败:', error)
      })
    } catch (error) {
      console.error('创建音频对象失败:', error)
    }
  }
  
  /**
   * 显示桌面通知
   */
  function showDesktopNotification(notification: Notification): void {
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification(notification.title, {
        body: notification.message,
        icon: '/favicon.ico',
        badge: '/favicon.ico',
        tag: notification.id,
        requireInteraction: notification.priority === 'Urgent',
        silent: !notificationsStore.settings?.enableSound
      })
      
      // 点击通知时打开应用
      if ('serviceWorker' in navigator) {
        navigator.serviceWorker.ready.then(registration => {
          registration.addEventListener('notificationclick', (event) => {
            event.notification.close()
            // 可以在这里处理通知点击事件，比如跳转到特定页面
            window.focus()
          })
        })
      }
    }
  }
  
  /**
   * 请求桌面通知权限
   */
  async function requestDesktopNotificationPermission(): Promise<boolean> {
    if ('Notification' in window) {
      if (Notification.permission === 'granted') {
        return true
      }
      
      if (Notification.permission !== 'denied') {
        const permission = await Notification.requestPermission()
        return permission === 'granted'
      }
    }
    return false
  }
  
  /**
   * 震动提醒
   */
  function vibrate(pattern: number | number[] = [200, 100, 200]): void {
    if ('vibrate' in navigator) {
      navigator.vibrate(pattern)
    }
  }
  
  /**
   * 检查免打扰时间
   */
  function isInQuietHours(): boolean {
    if (!notificationsStore.settings?.quietHours?.enabled) {
      return false
    }
    
    const now = new Date()
    const currentTime = `${now.getHours().toString().padStart(2, '0')}:${now.getMinutes().toString().padStart(2, '0')}`
    const { startTime, endTime } = notificationsStore.settings.quietHours
    
    if (startTime < endTime) {
      return currentTime >= startTime && currentTime <= endTime
    } else {
      // 跨天的情况
      return currentTime >= startTime || currentTime <= endTime
    }
  }
  
  /**
   * 处理通知操作
   */
  async function handleNotificationAction(notification: Notification, actionId: string): Promise<void> {
    try {
      await notificationsStore.notificationService.executeNotificationAction(notification.id, actionId)
      console.log(`执行通知操作成功: ${actionId}`)
    } catch (error) {
      console.error('执行通知操作失败:', error)
    }
  }
  
  /**
   * 批量处理通知
   */
  async function batchProcessNotifications(notificationIds: string[], operation: 'read' | 'archive' | 'delete'): Promise<number> {
    try {
      let count = 0
      
      switch (operation) {
        case 'read':
          count = await notificationsStore.batchMarkAsRead(notificationIds)
          break
        case 'archive':
          // 假设有批量归档方法
          count = notificationIds.length
          for (const id of notificationIds) {
            await notificationsStore.archiveNotification(id)
          }
          break
        case 'delete':
          count = await notificationsStore.batchDeleteNotifications(notificationIds)
          break
      }
      
      console.log(`批量${operation}操作成功，处理了${count}条通知`)
      return count
    } catch (error) {
      console.error(`批量${operation}操作失败:`, error)
      throw error
    }
  }
  
  /**
   * 获取通知统计信息
   */
  async function getNotificationStatistics(): Promise<void> {
    try {
      await notificationsStore.fetchNotificationStatistics()
    } catch (error) {
      console.error('获取通知统计失败:', error)
    }
  }
  
  /**
   * 刷新未读计数
   */
  async function refreshUnreadCount(): Promise<void> {
    try {
      await notificationsStore.fetchUnreadCount()
    } catch (error) {
      console.error('刷新未读计数失败:', error)
    }
  }
  
  // 生命周期钩子
  onMounted(async () => {
    // 请求桌面通知权限
    await requestDesktopNotificationPermission()
    
    // 连接WebSocket
    if (authStore.user) {
      await connectWebSocket()
    }
    
    // 定期刷新未读计数
    const refreshInterval = setInterval(() => {
      if (authStore.user && !isInQuietHours()) {
        refreshUnreadCount()
      }
    }, 30000) // 每30秒刷新一次
    
    // 清理定时器
    onUnmounted(() => {
      clearInterval(refreshInterval)
    })
  })
  
  onUnmounted(() => {
    disconnectWebSocket()
  })
  
  return {
    // 状态
    isConnected,
    connectionError,
    reconnectAttempts,
    
    // 方法
    connectWebSocket,
    disconnectWebSocket,
    reconnectWebSocket,
    sendTestNotification,
    handleNewNotification,
    playNotificationSound,
    showDesktopNotification,
    requestDesktopNotificationPermission,
    vibrate,
    isInQuietHours,
    handleNotificationAction,
    batchProcessNotifications,
    getNotificationStatistics,
    refreshUnreadCount
  }
}