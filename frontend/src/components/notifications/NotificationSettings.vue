<template>
  <div class="notification-settings" :class="containerClasses">
    <!-- 设置头部 -->
    <div class="notification-settings-header">
      <div class="notification-settings-title-section">
        <h2 class="notification-settings-title">{{ title }}</h2>
        <p class="notification-settings-description" v-if="description">{{ description }}</p>
      </div>
      
      <!-- 操作按钮 -->
      <div class="notification-settings-actions">
        <button
          @click="handleReset"
          class="notification-settings-btn notification-settings-btn-reset"
          :disabled="loading || !hasChanges"
        >
          重置
        </button>
        
        <button
          @click="handleCancel"
          class="notification-settings-btn notification-settings-btn-cancel"
          :disabled="loading"
        >
          取消
        </button>
        
        <button
          @click="handleSave"
          class="notification-settings-btn notification-settings-btn-save"
          :disabled="loading || !hasChanges"
        >
          {{ saving ? '保存中...' : '保存' }}
        </button>
      </div>
    </div>

    <!-- 设置内容 -->
    <div class="notification-settings-content">
      <!-- 基础设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">基础设置</h3>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.enableNotifications"
              @change="handleSettingsChange"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">启用通知</span>
          </label>
          
          <p class="notification-settings-switch-description">
            启用或禁用所有通知功能
          </p>
        </div>

        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.enableSound"
              @change="handleSettingsChange"
              :disabled="!settings.enableNotifications"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">启用声音</span>
          </label>
          
          <p class="notification-settings-switch-description">
            收到通知时播放提示音
          </p>
        </div>

        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.enableDesktopNotifications"
              @change="handleSettingsChange"
              :disabled="!settings.enableNotifications"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">桌面通知</span>
          </label>
          
          <p class="notification-settings-switch-description">
            在桌面显示通知弹窗
          </p>
        </div>
      </div>

      <!-- 通知渠道设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">通知渠道</h3>
        
        <div 
          v-for="channel in notificationChannelOptions" 
          :key="channel.value"
          class="notification-settings-channel"
        >
          <div class="notification-settings-channel-header">
            <div class="notification-settings-channel-info">
              <span class="notification-settings-channel-icon">{{ channel.icon }}</span>
              <div>
                <h4 class="notification-settings-channel-title">{{ channel.label }}</h4>
                <p class="notification-settings-channel-description">{{ channel.description }}</p>
              </div>
            </div>
            
            <label class="notification-settings-switch">
              <input
                type="checkbox"
                v-model="settings.channels[channel.value].enabled"
                @change="handleSettingsChange"
                :disabled="!settings.enableNotifications"
              />
              <span class="notification-settings-switch-slider"></span>
            </label>
          </div>
          
          <div 
            v-if="settings.channels[channel.value].enabled"
            class="notification-settings-channel-options"
          >
            <div class="notification-settings-group">
              <label class="notification-settings-switch">
                <input
                  type="checkbox"
                  v-model="settings.channels[channel.value].sound"
                  @change="handleSettingsChange"
                />
                <span class="notification-settings-switch-slider"></span>
                <span class="notification-settings-switch-label">声音提醒</span>
              </label>
            </div>
            
            <div class="notification-settings-group">
              <label class="notification-settings-label">优先级</label>
              <div class="notification-settings-checkboxes">
                <label 
                  v-for="priority in notificationPriorityOptions"
                  :key="priority.value"
                  class="notification-settings-checkbox"
                >
                  <input
                    type="checkbox"
                    :value="priority.value"
                    v-model="settings.channels[channel.value].priority"
                    @change="handleSettingsChange"
                  />
                  <span class="notification-settings-checkbox-label">{{ priority.label }}</span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 通知类型设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">通知类型</h3>
        
        <div 
          v-for="typeOption in notificationTypeOptions"
          :key="typeOption.value"
          class="notification-settings-type"
        >
          <div class="notification-settings-type-header">
            <div class="notification-settings-type-info">
              <span class="notification-settings-type-icon">{{ typeOption.icon }}</span>
              <div>
                <h4 class="notification-settings-type-title">{{ typeOption.label }}</h4>
                <p class="notification-settings-type-description">{{ typeOption.description }}</p>
              </div>
            </div>
            
            <label class="notification-settings-switch">
              <input
                type="checkbox"
                v-model="settings.types[typeOption.value].enabled"
                @change="handleSettingsChange"
                :disabled="!settings.enableNotifications"
              />
              <span class="notification-settings-switch-slider"></span>
            </label>
          </div>
          
          <div 
            v-if="settings.types[typeOption.value].enabled"
            class="notification-settings-type-options"
          >
            <div class="notification-settings-group">
              <label class="notification-settings-label">通知渠道</label>
              <div class="notification-settings-checkboxes">
                <label 
                  v-for="channel in notificationChannelOptions"
                  :key="channel.value"
                  class="notification-settings-checkbox"
                >
                  <input
                    type="checkbox"
                    :value="channel.value"
                    v-model="settings.types[typeOption.value].channel"
                    @change="handleSettingsChange"
                  />
                  <span class="notification-settings-checkbox-label">{{ channel.label }}</span>
                </label>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 免打扰设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">免打扰时间</h3>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.quietHours.enabled"
              @change="handleSettingsChange"
              :disabled="!settings.enableNotifications"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">启用免打扰</span>
          </label>
          
          <p class="notification-settings-switch-description">
            在指定时间段内不接收通知
          </p>
        </div>
        
        <div 
          v-if="settings.quietHours.enabled"
          class="notification-settings-time-range"
        >
          <div class="notification-settings-time-input">
            <label class="notification-settings-label">开始时间</label>
            <input
              type="time"
              v-model="settings.quietHours.startTime"
              @change="handleSettingsChange"
              class="notification-settings-input"
            />
          </div>
          
          <div class="notification-settings-time-input">
            <label class="notification-settings-label">结束时间</label>
            <input
              type="time"
              v-model="settings.quietHours.endTime"
              @change="handleSettingsChange"
              class="notification-settings-input"
            />
          </div>
        </div>
      </div>

      <!-- 频率设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">通知频率</h3>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.frequency.digest"
              @change="handleSettingsChange"
              :disabled="!settings.enableNotifications"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">启用摘要通知</span>
          </label>
          
          <p class="notification-settings-switch-description">
            将多个通知合并为一个摘要发送
          </p>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">摘要间隔</label>
          <input
            type="number"
            v-model.number="settings.frequency.digestInterval"
            @change="handleSettingsChange"
            :disabled="!settings.frequency.digest"
            class="notification-settings-input"
            min="1"
            max="1440"
          />
          <span class="notification-settings-unit">分钟</span>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">每小时最多通知数</label>
          <input
            type="number"
            v-model.number="settings.frequency.maxNotificationsPerHour"
            @change="handleSettingsChange"
            class="notification-settings-input"
            min="1"
            max="1000"
          />
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">每天最多通知数</label>
          <input
            type="number"
            v-model.number="settings.frequency.maxNotificationsPerDay"
            @change="handleSettingsChange"
            class="notification-settings-input"
            min="1"
            max="10000"
          />
        </div>
      </div>

      <!-- 外观设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">外观设置</h3>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">主题</label>
          <select
            v-model="settings.appearance.theme"
            @change="handleSettingsChange"
            class="notification-settings-select"
          >
            <option 
              v-for="theme in notificationThemeOptions"
              :key="theme.value"
              :value="theme.value"
            >
              {{ theme.label }}
            </option>
          </select>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">显示位置</label>
          <select
            v-model="settings.appearance.position"
            @change="handleSettingsChange"
            class="notification-settings-select"
          >
            <option 
              v-for="position in notificationPositionOptions"
              :key="position.value"
              :value="position.value"
            >
              {{ position.label }}
            </option>
          </select>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.appearance.animation"
              @change="handleSettingsChange"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">启用动画</span>
          </label>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">显示时长</label>
          <input
            type="number"
            v-model.number="settings.appearance.duration"
            @change="handleSettingsChange"
            class="notification-settings-input"
            min="1000"
            max="60000"
            step="1000"
          />
          <span class="notification-settings-unit">毫秒</span>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-label">最大显示数量</label>
          <input
            type="number"
            v-model.number="settings.appearance.maxVisible"
            @change="handleSettingsChange"
            class="notification-settings-input"
            min="1"
            max="20"
          />
        </div>
      </div>

      <!-- 隐私设置 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">隐私设置</h3>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.privacy.showContent"
              @change="handleSettingsChange"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">显示通知内容</span>
          </label>
          
          <p class="notification-settings-switch-description">
            在通知预览中显示完整内容
          </p>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.privacy.showPreview"
              @change="handleSettingsChange"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">显示预览</span>
          </label>
          
          <p class="notification-settings-switch-description">
            在通知列表中显示内容预览
          </p>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.privacy.showSender"
              @change="handleSettingsChange"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">显示发送者</span>
          </label>
          
          <p class="notification-settings-switch-description">
            在通知中显示发送者信息
          </p>
        </div>
        
        <div class="notification-settings-group">
          <label class="notification-settings-switch">
            <input
              type="checkbox"
              v-model="settings.privacy.allowReadReceipts"
              @change="handleSettingsChange"
            />
            <span class="notification-settings-switch-slider"></span>
            <span class="notification-settings-switch-label">允许已读回执</span>
          </label>
          
          <p class="notification-settings-switch-description">
            向发送者发送已读状态
          </p>
        </div>
      </div>

      <!-- 测试区域 -->
      <div class="notification-settings-section">
        <h3 class="notification-settings-section-title">测试通知</h3>
        
        <div class="notification-settings-test">
          <button
            v-for="channel in notificationChannelOptions"
            :key="channel.value"
            @click="handleTestChannel(channel.value)"
            class="notification-settings-btn notification-settings-btn-test"
            :disabled="loading || !settings.channels[channel.value].enabled"
          >
            <span class="notification-settings-btn-icon">{{ channel.icon }}</span>
            测试{{ channel.label }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useNotificationsStore } from '@/stores/notifications'
import { useAuthStore } from '@/stores/auth'
import type { 
  NotificationSettings,
  NotificationChannel,
  NotificationType,
  NotificationPriority
} from '@/types/notifications'
import { 
  DEFAULT_NOTIFICATION_SETTINGS,
  NOTIFICATION_CHANNEL_OPTIONS,
  NOTIFICATION_TYPE_OPTIONS,
  NOTIFICATION_PRIORITY_OPTIONS,
  NOTIFICATION_THEME_OPTIONS,
  NOTIFICATION_POSITION_OPTIONS
} from '@/types/notifications'

// Props 接口
interface Props {
  settings?: NotificationSettings
  title?: string
  description?: string
  compact?: boolean
}

// Emits 接口
interface Emits {
  (e: 'save', settings: NotificationSettings): void
  (e: 'cancel'): void
  (e: 'test', channel: NotificationChannel): void
}

const props = withDefaults(defineProps<Props>(), {
  title: '通知设置',
  description: '自定义您的通知偏好设置',
  compact: false
})

const emit = defineEmits<Emits>()
const notificationsStore = useNotificationsStore()
const authStore = useAuthStore()

// 状态
const loading = ref(false)
const saving = ref(false)
const originalSettings = ref<NotificationSettings | null>(null)
const settings = ref<NotificationSettings>({ ...DEFAULT_NOTIFICATION_SETTINGS })

// 计算属性
const hasChanges = computed(() => {
  if (!originalSettings.value) return false
  return JSON.stringify(settings.value) !== JSON.stringify(originalSettings.value)
})

// 选项
const notificationChannelOptions = NOTIFICATION_CHANNEL_OPTIONS
const notificationTypeOptions = NOTIFICATION_TYPE_OPTIONS
const notificationPriorityOptions = NOTIFICATION_PRIORITY_OPTIONS
const notificationThemeOptions = NOTIFICATION_THEME_OPTIONS
const notificationPositionOptions = NOTIFICATION_POSITION_OPTIONS

// 容器样式类
const containerClasses = computed(() => [
  `size-${props.compact ? 'compact' : 'normal'}`
])

// 方法
function handleSettingsChange(): void {
  // 设置已更改，等待保存
}

async function handleSave(): Promise<void> {
  if (!hasChanges.value) return
  
  try {
    saving.value = true
    
    // 更新用户ID
    if (authStore.user) {
      settings.value.userId = authStore.user.id
    }
    
    const updatedSettings = await notificationsStore.updateNotificationSettings(settings.value)
    originalSettings.value = JSON.parse(JSON.stringify(updatedSettings))
    
    emit('save', updatedSettings)
  } catch (error) {
    console.error('保存设置失败:', error)
  } finally {
    saving.value = false
  }
}

function handleCancel(): void {
  if (originalSettings.value) {
    settings.value = JSON.parse(JSON.stringify(originalSettings.value))
  }
  emit('cancel')
}

function handleReset(): void {
  if (originalSettings.value) {
    settings.value = JSON.parse(JSON.stringify(originalSettings.value))
  } else {
    settings.value = { ...DEFAULT_NOTIFICATION_SETTINGS }
  }
}

async function handleTestChannel(channel: NotificationChannel): Promise<void> {
  try {
    loading.value = true
    await notificationsStore.notificationService.testNotificationChannel(channel)
    emit('test', channel)
  } catch (error) {
    console.error('测试通知渠道失败:', error)
  } finally {
    loading.value = false
  }
}

// 生命周期
onMounted(async () => {
  try {
    loading.value = true
    
    // 使用传入的设置或从store获取
    if (props.settings) {
      settings.value = JSON.parse(JSON.stringify(props.settings))
    } else {
      const fetchedSettings = await notificationsStore.fetchNotificationSettings()
      settings.value = fetchedSettings
    }
    
    originalSettings.value = JSON.parse(JSON.stringify(settings.value))
  } catch (error) {
    console.error('加载设置失败:', error)
  } finally {
    loading.value = false
  }
})

// 监听设置变化
watch(() => props.settings, (newSettings) => {
  if (newSettings) {
    settings.value = JSON.parse(JSON.stringify(newSettings))
    originalSettings.value = JSON.parse(JSON.stringify(newSettings))
  }
}, { deep: true })
</script>

<style scoped>
/* 基础样式 */
.notification-settings {
  background: white;
  border-radius: 1rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  overflow: hidden;
}

.size-compact {
  max-width: 600px;
}

.size-normal {
  max-width: 1000px;
}

/* 头部样式 */
.notification-settings-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

.notification-settings-title-section {
  flex: 1;
}

.notification-settings-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #212529;
  margin: 0 0 0.25rem 0;
}

.notification-settings-description {
  font-size: 0.875rem;
  color: #6c757d;
  margin: 0;
}

.notification-settings-actions {
  display: flex;
  gap: 0.5rem;
}

/* 按钮样式 */
.notification-settings-btn {
  border: none;
  border-radius: 0.5rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  padding: 0.5rem 1rem;
  font-size: 0.875rem;
  font-weight: 500;
}

.notification-settings-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  transform: none;
}

.notification-settings-btn-reset {
  background: rgba(108, 117, 125, 0.1);
  color: #6c757d;
}

.notification-settings-btn-reset:hover:not(:disabled) {
  background: rgba(108, 117, 125, 0.15);
}

.notification-settings-btn-cancel {
  background: rgba(220, 53, 69, 0.1);
  color: #dc3545;
}

.notification-settings-btn-cancel:hover:not(:disabled) {
  background: rgba(220, 53, 69, 0.15);
}

.notification-settings-btn-save {
  background: #007bff;
  color: white;
}

.notification-settings-btn-save:hover:not(:disabled) {
  background: #0056b3;
  transform: translateY(-1px);
  box-shadow: 0 2px 4px rgba(0, 123, 255, 0.3);
}

.notification-settings-btn-test {
  background: rgba(40, 167, 69, 0.1);
  color: #28a745;
  padding: 0.75rem 1rem;
}

.notification-settings-btn-test:hover:not(:disabled) {
  background: rgba(40, 167, 69, 0.15);
  transform: translateY(-1px);
}

.notification-settings-btn-icon {
  margin-right: 0.5rem;
}

/* 内容样式 */
.notification-settings-content {
  padding: 1.5rem;
}

/* 区块样式 */
.notification-settings-section {
  margin-bottom: 2rem;
}

.notification-settings-section:last-child {
  margin-bottom: 0;
}

.notification-settings-section-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #212529;
  margin: 0 0 1rem 0;
  padding-bottom: 0.5rem;
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}

/* 组样式 */
.notification-settings-group {
  margin-bottom: 1.5rem;
}

.notification-settings-group:last-child {
  margin-bottom: 0;
}

/* 开关样式 */
.notification-settings-switch {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  cursor: pointer;
  user-select: none;
}

.notification-settings-switch input[type="checkbox"] {
  display: none;
}

.notification-settings-switch-slider {
  position: relative;
  width: 3rem;
  height: 1.5rem;
  background: rgba(0, 0, 0, 0.15);
  border-radius: 1.5rem;
  transition: all 0.3s ease;
}

.notification-settings-switch-slider::before {
  content: '';
  position: absolute;
  top: 0.125rem;
  left: 0.125rem;
  width: 1.25rem;
  height: 1.25rem;
  background: white;
  border-radius: 50%;
  transition: all 0.3s ease;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.notification-settings-switch input[type="checkbox"]:checked + .notification-settings-switch-slider {
  background: #007bff;
}

.notification-settings-switch input[type="checkbox"]:checked + .notification-settings-switch-slider::before {
  transform: translateX(1.5rem);
}

.notification-settings-switch input[type="checkbox"]:disabled + .notification-settings-switch-slider {
  opacity: 0.5;
  cursor: not-allowed;
}

.notification-settings-switch-label {
  font-weight: 500;
  color: #212529;
}

.notification-settings-switch-description {
  font-size: 0.813rem;
  color: #6c757d;
  margin: 0.25rem 0 0 3.75rem;
  line-height: 1.4;
}

/* 渠道样式 */
.notification-settings-channel {
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 0.75rem;
  margin-bottom: 1rem;
  overflow: hidden;
}

.notification-settings-channel-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background: rgba(0, 123, 255, 0.02);
}

.notification-settings-channel-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.notification-settings-channel-icon {
  font-size: 1.25rem;
}

.notification-settings-channel-title {
  font-size: 1rem;
  font-weight: 600;
  color: #212529;
  margin: 0 0 0.125rem 0;
}

.notification-settings-channel-description {
  font-size: 0.813rem;
  color: #6c757d;
  margin: 0;
}

.notification-settings-channel-options {
  padding: 1rem;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
}

/* 类型样式 */
.notification-settings-type {
  border: 1px solid rgba(0, 0, 0, 0.08);
  border-radius: 0.75rem;
  margin-bottom: 1rem;
  overflow: hidden;
}

.notification-settings-type-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background: rgba(0, 123, 255, 0.02);
}

.notification-settings-type-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.notification-settings-type-icon {
  font-size: 1.25rem;
}

.notification-settings-type-title {
  font-size: 1rem;
  font-weight: 600;
  color: #212529;
  margin: 0 0 0.125rem 0;
}

.notification-settings-type-description {
  font-size: 0.813rem;
  color: #6c757d;
  margin: 0;
}

.notification-settings-type-options {
  padding: 1rem;
  border-top: 1px solid rgba(0, 0, 0, 0.05);
}

/* 标签样式 */
.notification-settings-label {
  display: block;
  font-weight: 500;
  color: #212529;
  margin-bottom: 0.5rem;
}

/* 输入样式 */
.notification-settings-input,
.notification-settings-select {
  width: 100%;
  padding: 0.5rem 0.75rem;
  border: 1px solid rgba(0, 0, 0, 0.15);
  border-radius: 0.375rem;
  font-size: 0.875rem;
  transition: all 0.2s ease;
  background: white;
}

.notification-settings-input:focus,
.notification-settings-select:focus {
  outline: none;
  border-color: #007bff;
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
}

.notification-settings-input:disabled,
.notification-settings-select:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.notification-settings-unit {
  margin-left: 0.5rem;
  font-size: 0.813rem;
  color: #6c757d;
}

/* 时间范围样式 */
.notification-settings-time-range {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}

.notification-settings-time-input {
  display: flex;
  flex-direction: column;
}

/* 复选框样式 */
.notification-settings-checkboxes {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
}

.notification-settings-checkbox {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  cursor: pointer;
  user-select: none;
}

.notification-settings-checkbox input[type="checkbox"] {
  width: 1rem;
  height: 1rem;
  accent-color: #007bff;
}

.notification-settings-checkbox-label {
  font-size: 0.875rem;
  color: #495057;
}

/* 测试区域样式 */
.notification-settings-test {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 0.75rem;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .notification-settings-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 1rem;
  }

  .notification-settings-actions {
    width: 100%;
    justify-content: flex-end;
  }

  .notification-settings-time-range {
    grid-template-columns: 1fr;
  }

  .notification-settings-test {
    grid-template-columns: 1fr;
  }

  .size-compact {
    max-width: 100%;
  }

  .notification-settings-content {
    padding: 1rem;
  }
}

@media (max-width: 480px) {
  .notification-settings-actions {
    flex-direction: column;
    gap: 0.5rem;
  }

  .notification-settings-btn {
    width: 100%;
    justify-content: center;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .notification-settings-switch-slider,
  .notification-settings-switch-slider::before,
  .notification-settings-btn {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .notification-settings {
    border: 2px solid currentColor;
  }

  .notification-settings-channel,
  .notification-settings-type {
    border-width: 2px;
  }

  .notification-settings-input,
  .notification-settings-select {
    border-width: 2px;
  }

  .notification-settings-switch-slider {
    border: 1px solid currentColor;
  }
}

/* 焦点样式 */
.notification-settings-btn:focus-visible,
.notification-settings-input:focus-visible,
.notification-settings-select:focus-visible,
.notification-settings-checkbox input:focus-visible {
  outline: 2px solid #007bff;
  outline-offset: 2px;
}
</style>