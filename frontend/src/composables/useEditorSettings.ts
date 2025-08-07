import { ref, watch } from 'vue'
import type { EditorOptions } from '@/types'

/**
 * 编辑器设置管理组合式函数
 */
export function useEditorSettings() {
  // 默认设置
  const defaultSettings: EditorOptions = {
    theme: 'vs',
    fontSize: 14,
    tabSize: 2,
    insertSpaces: true,
    wordWrap: 'on',
    minimap: { enabled: true },
    lineNumbers: 'on',
    renderWhitespace: 'selection',
    folding: true,
    showFoldingControls: 'always'
  }

  // 从 localStorage 加载设置
  const loadSettings = (): EditorOptions => {
    try {
      const saved = localStorage.getItem('editor-settings')
      if (saved) {
        const parsed = JSON.parse(saved)
        return { ...defaultSettings, ...parsed }
      }
    } catch (error) {
      console.warn('Failed to load editor settings:', error)
    }
    return { ...defaultSettings }
  }

  // 保存设置到 localStorage
  const saveSettings = (settings: EditorOptions) => {
    try {
      localStorage.setItem('editor-settings', JSON.stringify(settings))
    } catch (error) {
      console.warn('Failed to save editor settings:', error)
    }
  }

  // 响应式设置
  const settings = ref<EditorOptions>(loadSettings())

  // 监听设置变化并自动保存
  watch(settings, (newSettings) => {
    saveSettings(newSettings)
  }, { deep: true })

  /**
   * 更新设置
   */
  const updateSettings = (newSettings: Partial<EditorOptions>) => {
    settings.value = { ...settings.value, ...newSettings }
  }

  /**
   * 重置为默认设置
   */
  const resetSettings = () => {
    settings.value = { ...defaultSettings }
  }

  /**
   * 获取特定设置值
   */
  const getSetting = <K extends keyof EditorOptions>(key: K): EditorOptions[K] => {
    return settings.value[key]
  }

  /**
   * 设置特定值
   */
  const setSetting = <K extends keyof EditorOptions>(key: K, value: EditorOptions[K]) => {
    settings.value[key] = value
  }

  return {
    settings,
    updateSettings,
    resetSettings,
    getSetting,
    setSetting,
    defaultSettings
  }
}
