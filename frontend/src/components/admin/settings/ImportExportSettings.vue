<template>
  <div class="import-export-settings">
    <div class="section-header">
      <h2>导入导出</h2>
      <p class="section-description">备份和恢复系统设置，或在不同环境间迁移配置</p>
    </div>

    <div class="row">
      <!-- 导出设置 -->
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">
            <h3><i class="fas fa-download"></i> 导出设置</h3>
          </div>
          <div class="card-body">
            <p class="text-muted">
              将当前系统设置导出为文件，可用于备份或迁移到其他环境
            </p>
            
            <div class="mb-3">
              <label class="form-label">导出格式</label>
              <select v-model="exportFormat" class="form-select">
                <option value="json">JSON 格式</option>
                <option value="csv">CSV 格式</option>
                <option value="excel">Excel 格式</option>
              </select>
            </div>
            
            <div class="form-check mb-3">
              <input 
                id="includeHistory" 
                v-model="includeHistory" 
                type="checkbox" 
                class="form-check-input"
              />
              <label for="includeHistory" class="form-check-label">
                包含设置历史记录
              </label>
            </div>
            
            <button 
              @click="exportSettings" 
              class="btn btn-primary w-100"
              :disabled="exporting"
            >
              <i class="fas fa-download" :class="{ 'fa-spin': exporting }"></i>
              {{ exporting ? '导出中...' : '导出设置' }}
            </button>
          </div>
        </div>
      </div>

      <!-- 导入设置 -->
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">
            <h3><i class="fas fa-upload"></i> 导入设置</h3>
          </div>
          <div class="card-body">
            <p class="text-muted">
              从文件导入系统设置，将覆盖当前的设置配置
            </p>
            
            <div class="mb-3">
              <label class="form-label">选择文件</label>
              <input 
                ref="fileInput"
                type="file" 
                class="form-control"
                accept=".json,.csv,.xlsx"
                @change="handleFileSelect"
              />
              <small class="form-text text-muted">
                支持 JSON、CSV、Excel 格式
              </small>
            </div>
            
            <div v-if="selectedFile" class="mb-3">
              <div class="selected-file">
                <i class="fas fa-file"></i>
                {{ selectedFile.name }}
                <span class="file-size">({{ formatFileSize(selectedFile.size) }})</span>
              </div>
            </div>
            
            <div class="form-check mb-3">
              <input 
                id="validateOnly" 
                v-model="validateOnly" 
                type="checkbox" 
                class="form-check-input"
              />
              <label for="validateOnly" class="form-check-label">
                仅验证不导入
              </label>
            </div>
            
            <button 
              @click="importSettings" 
              class="btn btn-success w-100"
              :disabled="!selectedFile || importing"
            >
              <i class="fas fa-upload" :class="{ 'fa-spin': importing }"></i>
              {{ importing ? '导入中...' : '导入设置' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 备份恢复 -->
    <div class="row mt-4">
      <div class="col-md-6">
        <div class="card">
          <div class="card-header">
            <h3><i class="fas fa-database"></i> 生成备份</h3>
          </div>
          <div class="card-body">
            <p class="text-muted">
              创建完整的系统设置备份，包含所有配置和历史记录
            </p>
            
            <div class="form-check mb-3">
              <input 
                id="backupIncludeHistory" 
                v-model="backupIncludeHistory" 
                type="checkbox" 
                class="form-check-input"
                checked
              />
              <label for="backupIncludeHistory" class="form-check-label">
                包含历史记录
              </label>
            </div>
            
            <button 
              @click="generateBackup" 
              class="btn btn-info w-100"
              :disabled="backingUp"
            >
              <i class="fas fa-database" :class="{ 'fa-spin': backingUp }"></i>
              {{ backingUp ? '生成中...' : '生成备份' }}
            </button>
          </div>
        </div>
      </div>

      <div class="col-md-6">
        <div class="card">
          <div class="card-header">
            <h3><i class="fas fa-undo"></i> 恢复备份</h3>
          </div>
          <div class="card-body">
            <p class="text-muted">
              从备份文件恢复系统设置，将完全覆盖当前配置
            </p>
            
            <div class="mb-3">
              <label class="form-label">选择备份文件</label>
              <input 
                ref="backupFileInput"
                type="file" 
                class="form-control"
                accept=".json"
                @change="handleBackupFileSelect"
              />
              <small class="form-text text-muted">
                仅支持 JSON 格式的备份文件
              </small>
            </div>
            
            <div v-if="selectedBackupFile" class="mb-3">
              <div class="selected-file">
                <i class="fas fa-file"></i>
                {{ selectedBackupFile.name }}
                <span class="file-size">({{ formatFileSize(selectedBackupFile.size) }})</span>
              </div>
            </div>
            
            <button 
              @click="restoreBackup" 
              class="btn btn-warning w-100"
              :disabled="!selectedBackupFile || restoring"
            >
              <i class="fas fa-undo" :class="{ 'fa-spin': restoring }"></i>
              {{ restoring ? '恢复中...' : '恢复备份' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 操作历史 -->
    <div class="mt-4">
      <div class="card">
        <div class="card-header">
          <h3><i class="fas fa-history"></i> 操作历史</h3>
        </div>
        <div class="card-body">
          <div v-if="operationHistory.length === 0" class="text-center text-muted">
            暂无操作记录
          </div>
          <div v-else class="operation-list">
            <div 
              v-for="operation in operationHistory" 
              :key="operation.id"
              class="operation-item"
            >
              <div class="operation-info">
                <div class="operation-type">
                  <i :class="getOperationIcon(operation.type)"></i>
                  {{ operation.type }}
                </div>
                <div class="operation-time">{{ formatTime(operation.timestamp) }}</div>
              </div>
              <div class="operation-status">
                <span 
                  class="badge"
                  :class="operation.success ? 'bg-success' : 'bg-danger'"
                >
                  {{ operation.success ? '成功' : '失败' }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 验证结果模态框 -->
    <div v-if="showValidationResult" class="modal-backdrop">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">验证结果</h5>
            <button type="button" class="btn-close" @click="closeValidationResult"></button>
          </div>
          <div class="modal-body">
            <div v-if="validationResult">
              <div v-if="validationResult.isValid" class="alert alert-success">
                <i class="fas fa-check-circle"></i>
                配置文件验证通过，可以安全导入
              </div>
              <div v-else>
                <div class="alert alert-danger">
                  <i class="fas fa-exclamation-circle"></i>
                  配置文件验证失败，请修复以下问题：
                </div>
                <ul class="error-list">
                  <li v-for="(errors, field) in validationResult.errors" :key="field">
                    <strong>{{ field }}:</strong>
                    <ul>
                      <li v-for="error in errors" :key="error">{{ error }}</li>
                    </ul>
                  </li>
                </ul>
              </div>
              
              <div v-if="validationResult.warnings && Object.keys(validationResult.warnings).length > 0">
                <div class="alert alert-warning mt-3">
                  <i class="fas fa-exclamation-triangle"></i>
                  警告信息：
                </div>
                <ul class="warning-list">
                  <li v-for="(warnings, field) in validationResult.warnings" :key="field">
                    <strong>{{ field }}:</strong>
                    <ul>
                      <li v-for="warning in warnings" :key="warning">{{ warning }}</li>
                    </ul>
                  </li>
                </ul>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="closeValidationResult">
              关闭
            </button>
            <button 
              v-if="validationResult?.isValid && !validateOnly"
              type="button" 
              class="btn btn-primary"
              @click="proceedWithImport"
            >
              继续导入
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useToast } from '@/composables/useToast'
import type { SettingsValidationResult } from '@/types/settings'

interface Emits {
  (e: 'export', format: 'json' | 'csv' | 'excel'): void
  (e: 'import', jsonData: string): void
  (e: 'backup', includeHistory: boolean): void
  (e: 'restore', backupData: string): void
}

const emit = defineEmits<Emits>()
const toast = useToast()

// 响应式状态
const exportFormat = ref<'json' | 'csv' | 'excel'>('json')
const includeHistory = ref(false)
const validateOnly = ref(false)
const backupIncludeHistory = ref(true)

const exporting = ref(false)
const importing = ref(false)
const backingUp = ref(false)
const restoring = ref(false)

const selectedFile = ref<File | null>(null)
const selectedBackupFile = ref<File | null>(null)
const validationResult = ref<SettingsValidationResult | null>(null)
const showValidationResult = ref(false)
const pendingImportData = ref<string>('')

const fileInput = ref<HTMLInputElement | null>(null)
const backupFileInput = ref<HTMLInputElement | null>(null)

// 操作历史
const operationHistory = ref<Array<{
  id: string
  type: string
  timestamp: Date
  success: boolean
  message?: string
}>>([])

// 方法
function handleFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    selectedFile.value = file
  }
}

function handleBackupFileSelect(event: Event) {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (file) {
    selectedBackupFile.value = file
  }
}

async function exportSettings() {
  try {
    exporting.value = true
    emit('export', exportFormat.value)
    addOperationHistory('导出设置', true)
    toast.success('设置已导出')
  } catch (error) {
    addOperationHistory('导出设置', false, error instanceof Error ? error.message : '未知错误')
    toast.error('导出设置失败')
  } finally {
    exporting.value = false
  }
}

async function importSettings() {
  if (!selectedFile.value) return

  try {
    importing.value = true
    const fileContent = await readFileAsText(selectedFile.value)
    
    if (validateOnly.value) {
      // 这里应该调用验证API
      showValidationResult.value = true
      addOperationHistory('验证导入文件', true)
    } else {
      emit('import', fileContent)
      addOperationHistory('导入设置', true)
      toast.success('设置已导入')
      selectedFile.value = null
      if (fileInput.value) {
        fileInput.value.value = ''
      }
    }
  } catch (error) {
    addOperationHistory('导入设置', false, error instanceof Error ? error.message : '未知错误')
    toast.error('导入设置失败')
  } finally {
    importing.value = false
  }
}

async function generateBackup() {
  try {
    backingUp.value = true
    emit('backup', backupIncludeHistory.value)
    addOperationHistory('生成备份', true)
    toast.success('备份已生成')
  } catch (error) {
    addOperationHistory('生成备份', false, error instanceof Error ? error.message : '未知错误')
    toast.error('生成备份失败')
  } finally {
    backingUp.value = false
  }
}

async function restoreBackup() {
  if (!selectedBackupFile.value) return

  try {
    restoring.value = true
    const fileContent = await readFileAsText(selectedBackupFile.value)
    emit('restore', fileContent)
    addOperationHistory('恢复备份', true)
    toast.success('备份已恢复')
    selectedBackupFile.value = null
    if (backupFileInput.value) {
      backupFileInput.value = ''
    }
  } catch (error) {
    addOperationHistory('恢复备份', false, error instanceof Error ? error.message : '未知错误')
    toast.error('恢复备份失败')
  } finally {
    restoring.value = false
  }
}

function readFileAsText(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = (e) => {
      resolve(e.target?.result as string)
    }
    reader.onerror = () => {
      reject(new Error('文件读取失败'))
    }
    reader.readAsText(file)
  })
}

function formatFileSize(bytes: number): string {
  if (bytes === 0) return '0 Bytes'
  const k = 1024
  const sizes = ['Bytes', 'KB', 'MB', 'GB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i]
}

function formatTime(timestamp: Date): string {
  return new Intl.DateTimeFormat('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  }).format(timestamp)
}

function getOperationIcon(type: string): string {
  const icons: Record<string, string> = {
    '导出设置': 'fas fa-download',
    '导入设置': 'fas fa-upload',
    '验证导入文件': 'fas fa-check-circle',
    '生成备份': 'fas fa-database',
    '恢复备份': 'fas fa-undo'
  }
  return icons[type] || 'fas fa-cog'
}

function addOperationHistory(type: string, success: boolean, message?: string) {
  operationHistory.value.unshift({
    id: Date.now().toString(),
    type,
    timestamp: new Date(),
    success,
    message
  })
  
  // 只保留最近20条记录
  if (operationHistory.value.length > 20) {
    operationHistory.value = operationHistory.value.slice(0, 20)
  }
}

function closeValidationResult() {
  showValidationResult.value = false
  validationResult.value = null
  pendingImportData.value = ''
}

function proceedWithImport() {
  if (pendingImportData.value) {
    emit('import', pendingImportData.value)
    pendingImportData.value = ''
    closeValidationResult()
  }
}
</script>

<style scoped>
.import-export-settings {
  max-width: 1000px;
}

.section-header {
  margin-bottom: 2rem;
}

.section-header h2 {
  font-size: 1.5rem;
  font-weight: 600;
  margin: 0 0 0.5rem 0;
  color: #2c3e50;
}

.section-description {
  color: #6c757d;
  margin: 0;
}

.card {
  border: none;
  box-shadow: 0 0.125rem 0.25rem rgba(0, 0, 0, 0.075);
  margin-bottom: 1.5rem;
}

.card-header {
  background: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
  padding: 1rem;
}

.card-header h3 {
  font-size: 1.1rem;
  font-weight: 600;
  margin: 0;
  color: #495057;
}

.card-header i {
  margin-right: 0.5rem;
  color: #6c757d;
}

.selected-file {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem;
  background: #f8f9fa;
  border-radius: 0.375rem;
  font-size: 0.875rem;
}

.selected-file i {
  color: #6c757d;
}

.file-size {
  color: #6c757d;
  font-size: 0.75rem;
}

.operation-list {
  max-height: 300px;
  overflow-y: auto;
}

.operation-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.75rem;
  border-bottom: 1px solid #dee2e6;
}

.operation-item:last-child {
  border-bottom: none;
}

.operation-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.operation-type {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 500;
}

.operation-time {
  font-size: 0.75rem;
  color: #6c757d;
}

.modal-backdrop {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-dialog {
  background: white;
  border-radius: 0.5rem;
  max-width: 600px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
}

.modal-header {
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body {
  padding: 1rem;
}

.modal-footer {
  padding: 1rem;
  border-top: 1px solid #dee2e6;
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
}

.error-list, .warning-list {
  margin-bottom: 0;
  padding-left: 1.5rem;
}

.error-list li, .warning-list li {
  margin-bottom: 0.25rem;
}

@media (max-width: 768px) {
  .import-export-settings {
    padding: 0 1rem;
  }
  
  .modal-dialog {
    width: 95%;
    margin: 1rem;
  }
}
</style>