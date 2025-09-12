<template>
  <div class="system-settings-view">
    <div class="page-header">
      <div class="header-content">
        <h1 class="page-title">系统设置</h1>
        <p class="page-description">管理系统的各项配置和参数设置</p>
      </div>
      <div class="header-actions">
        <button 
          @click="refreshSettings" 
          class="btn btn-outline-primary"
          :disabled="isLoading"
        >
          <i class="fas fa-sync-alt" :class="{ 'fa-spin': isLoading }"></i>
          刷新
        </button>
        <button 
          @click="saveAllSettings" 
          class="btn btn-primary"
          :disabled="!hasChanges || isSaving"
        >
          <i class="fas fa-save" :class="{ 'fa-spin': isSaving }"></i>
          保存所有更改
        </button>
      </div>
    </div>

    <!-- 错误提示 -->
    <div v-if="error" class="alert alert-danger alert-dismissible fade show" role="alert">
      <i class="fas fa-exclamation-circle"></i>
      {{ error }}
      <button type="button" class="btn-close" @click="clearError"></button>
    </div>

    <!-- 未初始化提示 -->
    <div v-if="!isInitialized" class="alert alert-warning">
      <i class="fas fa-exclamation-triangle"></i>
      系统设置尚未初始化，请点击下方按钮进行初始化。
      <button @click="initializeSettings" class="btn btn-warning btn-sm ms-2">
        初始化设置
      </button>
    </div>

    <!-- 主要内容区域 -->
    <div class="settings-container">
      <!-- 侧边栏导航 -->
      <div class="settings-sidebar">
        <div class="sidebar-header">
          <h3>设置分类</h3>
        </div>
        <nav class="sidebar-nav">
          <ul class="nav flex-column">
            <li class="nav-item">
              <a 
                class="nav-link" 
                :class="{ active: activeTab === 'site' }"
                href="#"
                @click.prevent="setActiveTab('site')"
              >
                <i class="fas fa-globe"></i>
                站点设置
              </a>
            </li>
            <li class="nav-item">
              <a 
                class="nav-link" 
                :class="{ active: activeTab === 'security' }"
                href="#"
                @click.prevent="setActiveTab('security')"
              >
                <i class="fas fa-shield-alt"></i>
                安全设置
              </a>
            </li>
            <li class="nav-item">
              <a 
                class="nav-link" 
                :class="{ active: activeTab === 'features' }"
                href="#"
                @click.prevent="setActiveTab('features')"
              >
                <i class="fas fa-cogs"></i>
                功能设置
              </a>
            </li>
            <li class="nav-item">
              <a 
                class="nav-link" 
                :class="{ active: activeTab === 'email' }"
                href="#"
                @click.prevent="setActiveTab('email')"
              >
                <i class="fas fa-envelope"></i>
                邮件设置
              </a>
            </li>
            <li class="nav-item">
              <a 
                class="nav-link" 
                :class="{ active: activeTab === 'history' }"
                href="#"
                @click.prevent="setActiveTab('history')"
              >
                <i class="fas fa-history"></i>
                设置历史
              </a>
            </li>
            <li class="nav-item">
              <a 
                class="nav-link" 
                :class="{ active: activeTab === 'import-export' }"
                href="#"
                @click.prevent="setActiveTab('import-export')"
              >
                <i class="fas fa-exchange-alt"></i>
                导入导出
              </a>
            </li>
          </ul>
        </nav>
      </div>

      <!-- 内容区域 -->
      <div class="settings-content">
        <!-- 加载状态 -->
        <div v-if="isLoading" class="loading-overlay">
          <div class="spinner-border text-primary" role="status">
            <span class="visually-hidden">加载中...</span>
          </div>
          <p class="loading-text">正在加载设置...</p>
        </div>

        <!-- 站点设置 -->
        <div v-else-if="activeTab === 'site'" class="settings-tab">
          <SiteSettingsView 
            :settings="siteSettings"
            :loading="isSaving"
            @update:settings="updateSiteSettings"
          />
        </div>

        <!-- 安全设置 -->
        <div v-else-if="activeTab === 'security'" class="settings-tab">
          <SecuritySettingsView 
            :settings="securitySettings"
            :loading="isSaving"
            @update:settings="updateSecuritySettings"
          />
        </div>

        <!-- 功能设置 -->
        <div v-else-if="activeTab === 'features'" class="settings-tab">
          <FeatureSettingsView 
            :settings="featureSettings"
            :loading="isSaving"
            @update:settings="updateFeatureSettings"
          />
        </div>

        <!-- 邮件设置 -->
        <div v-else-if="activeTab === 'email'" class="settings-tab">
          <EmailSettingsView 
            :settings="emailSettings"
            :loading="isSaving"
            @update:settings="updateEmailSettings"
            @send-test="sendTestEmail"
          />
        </div>

        <!-- 设置历史 -->
        <div v-else-if="activeTab === 'history'" class="settings-tab">
          <SettingsHistoryView />
        </div>

        <!-- 导入导出 -->
        <div v-else-if="activeTab === 'import-export'" class="settings-tab">
          <ImportExportSettings 
            @export="exportSettings"
            @import="importSettings"
            @backup="generateBackup"
            @restore="restoreFromBackup"
          />
        </div>
      </div>
    </div>

    <!-- 保存提示 -->
    <div v-if="hasChanges" class="save-prompt">
      <div class="save-prompt-content">
        <i class="fas fa-info-circle"></i>
        <span>您有未保存的更改</span>
        <button @click="saveAllSettings" class="btn btn-primary btn-sm">
          保存更改
        </button>
        <button @click="resetChanges" class="btn btn-outline-secondary btn-sm">
          重置
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useSettingsStore } from '@/stores/settings'
import { useToast } from '@/composables/useToast'
import SiteSettingsView from './SiteSettingsView.vue'
import SecuritySettingsView from './SecuritySettingsView.vue'
import FeatureSettingsView from './FeatureSettingsView.vue'
import EmailSettingsView from './EmailSettingsView.vue'
import SettingsHistoryView from './SettingsHistoryView.vue'
import ImportExportSettings from '@/components/admin/settings/ImportExportSettings.vue'
import type { 
  SiteSettings, 
  SecuritySettings, 
  FeatureSettings, 
  EmailSettings,
  UpdateSiteSettingsRequest,
  UpdateSecuritySettingsRequest,
  UpdateFeatureSettingsRequest,
  UpdateEmailSettingsRequest
} from '@/types/settings'

const settingsStore = useSettingsStore()
const toast = useToast()

// 响应式状态
const activeTab = ref('site')

// 计算属性
const isLoading = computed(() => settingsStore.isLoading)
const isSaving = computed(() => settingsStore.isSaving)
const error = computed(() => settingsStore.error)
const isInitialized = computed(() => settingsStore.isInitialized)
const hasChanges = computed(() => settingsStore.hasChanges)
const siteSettings = computed(() => settingsStore.siteSettings)
const securitySettings = computed(() => settingsStore.securitySettings)
const featureSettings = computed(() => settingsStore.featureSettings)
const emailSettings = computed(() => settingsStore.emailSettings)

// 生命周期钩子
onMounted(async () => {
  await loadSettings()
})

// 方法
async function loadSettings() {
  try {
    await settingsStore.fetchOrCreateSettings()
  } catch (err) {
    toast.error('加载设置失败')
  }
}

function setActiveTab(tab: string) {
  activeTab.value = tab
  settingsStore.setActiveTab(tab)
}

async function refreshSettings() {
  try {
    await settingsStore.refreshCache()
    toast.success('设置已刷新')
  } catch (err) {
    toast.error('刷新设置失败')
  }
}

async function initializeSettings() {
  try {
    await settingsStore.initializeDefaultSettings()
    toast.success('系统设置已初始化')
  } catch (err) {
    toast.error('初始化设置失败')
  }
}

async function saveAllSettings() {
  if (!settingsStore.settings) return
  
  try {
    await settingsStore.updateSettings(settingsStore.settings)
    toast.success('设置已保存')
  } catch (err) {
    toast.error('保存设置失败')
  }
}

async function updateSiteSettings(request: UpdateSiteSettingsRequest) {
  try {
    await settingsStore.updateSiteSettings(request)
    toast.success('站点设置已更新')
  } catch (err) {
    toast.error('更新站点设置失败')
  }
}

async function updateSecuritySettings(request: UpdateSecuritySettingsRequest) {
  try {
    await settingsStore.updateSecuritySettings(request)
    toast.success('安全设置已更新')
  } catch (err) {
    toast.error('更新安全设置失败')
  }
}

async function updateFeatureSettings(request: UpdateFeatureSettingsRequest) {
  try {
    await settingsStore.updateFeatureSettings(request)
    toast.success('功能设置已更新')
  } catch (err) {
    toast.error('更新功能设置失败')
  }
}

async function updateEmailSettings(request: UpdateEmailSettingsRequest) {
  try {
    await settingsStore.updateEmailSettings(request)
    toast.success('邮件设置已更新')
  } catch (err) {
    toast.error('更新邮件设置失败')
  }
}

async function sendTestEmail(recipientEmail: string) {
  try {
    await settingsStore.sendTestEmail({ recipientEmail })
    toast.success('测试邮件已发送')
  } catch (err) {
    toast.error('发送测试邮件失败')
  }
}

async function exportSettings(format: 'json' | 'csv' | 'excel') {
  try {
    await settingsStore.exportSettings(format)
    toast.success('设置已导出')
  } catch (err) {
    toast.error('导出设置失败')
  }
}

async function importSettings(jsonData: string) {
  try {
    await settingsStore.importSettings(jsonData)
    toast.success('设置已导入')
  } catch (err) {
    toast.error('导入设置失败')
  }
}

async function generateBackup(includeHistory: boolean = true) {
  try {
    await settingsStore.generateBackup(includeHistory)
    toast.success('备份已生成')
  } catch (err) {
    toast.error('生成备份失败')
  }
}

async function restoreFromBackup(backupData: string) {
  try {
    await settingsStore.importSettings(backupData)
    toast.success('设置已从备份恢复')
  } catch (err) {
    toast.error('从备份恢复失败')
  }
}

function resetChanges() {
  settingsStore.resetChanges()
  toast.info('已重置所有更改')
}

function clearError() {
  settingsStore.error = null
}
</script>

<style scoped>
.system-settings-view {
  padding: 1.5rem;
  max-width: 1400px;
  margin: 0 auto;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
  padding-bottom: 1rem;
  border-bottom: 1px solid #e9ecef;
}

.header-content h1 {
  font-size: 2rem;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.page-description {
  color: #6c757d;
  margin: 0;
}

.header-actions {
  display: flex;
  gap: 0.5rem;
}

.settings-container {
  display: flex;
  gap: 2rem;
  min-height: 600px;
}

.settings-sidebar {
  width: 250px;
  flex-shrink: 0;
  background: #f8f9fa;
  border-radius: 0.5rem;
  padding: 1.5rem;
  height: fit-content;
}

.sidebar-header h3 {
  font-size: 1.1rem;
  font-weight: 600;
  margin: 0 0 1rem 0;
  color: #495057;
}

.sidebar-nav .nav-link {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  margin-bottom: 0.25rem;
  border-radius: 0.375rem;
  color: #6c757d;
  text-decoration: none;
  transition: all 0.2s ease;
}

.sidebar-nav .nav-link:hover {
  background: #e9ecef;
  color: #495057;
}

.sidebar-nav .nav-link.active {
  background: #007bff;
  color: white;
}

.sidebar-nav .nav-link i {
  width: 1.2rem;
  text-align: center;
}

.settings-content {
  flex: 1;
  background: white;
  border-radius: 0.5rem;
  padding: 2rem;
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
}

.loading-overlay {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: #6c757d;
}

.loading-text {
  margin-top: 1rem;
  font-size: 1rem;
}

.settings-tab {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.save-prompt {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  background: #fff3cd;
  border: 1px solid #ffeaa7;
  border-radius: 0.5rem;
  padding: 1rem 1.5rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
  z-index: 1000;
}

.save-prompt-content {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.save-prompt i {
  color: #856404;
}

.save-prompt span {
  color: #856404;
  font-weight: 500;
}

@media (max-width: 768px) {
  .system-settings-view {
    padding: 1rem;
  }
  
  .settings-container {
    flex-direction: column;
  }
  
  .settings-sidebar {
    width: 100%;
    margin-bottom: 1rem;
  }
  
  .page-header {
    flex-direction: column;
    gap: 1rem;
    align-items: flex-start;
  }
  
  .save-prompt {
    bottom: 1rem;
    right: 1rem;
    left: 1rem;
  }
  
  .save-prompt-content {
    flex-direction: column;
    gap: 0.5rem;
    text-align: center;
  }
}
</style>