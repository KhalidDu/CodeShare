import { ref } from 'vue'
import { clipboardService } from '@/services/clipboardService'
import { useToast } from './useToast'

interface CopyOptions {
  /** 代码片段ID，用于记录统计 */
  snippetId?: string
  /** 是否显示成功提示 */
  showSuccessToast?: boolean
  /** 是否显示错误提示 */
  showErrorToast?: boolean
  /** 成功提示消息 */
  successMessage?: string
  /** 错误提示消息 */
  errorMessage?: string
}

/**
 * 复制功能 composable
 * 提供统一的复制到剪贴板功能
 */
export function useCopy() {
  const toast = useToast()
  
  // 响应式状态
  const copying = ref(false)
  const lastCopiedText = ref('')
  const lastCopyTime = ref<Date | null>(null)

  /**
   * 复制文本到剪贴板
   * @param text 要复制的文本
   * @param options 复制选项
   */
  const copyToClipboard = async (text: string, options: CopyOptions = {}): Promise<boolean> => {
    const {
      snippetId,
      showSuccessToast = true,
      showErrorToast = true,
      successMessage = '已复制到剪贴板',
      errorMessage = '复制失败，请手动复制'
    } = options

    if (copying.value || !text) {
      return false
    }

    copying.value = true

    try {
      // 尝试使用现代剪贴板 API
      if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(text)
      } else {
        // 降级处理：使用传统方法
        await fallbackCopyToClipboard(text)
      }

      // 记录复制统计（如果提供了 snippetId）
      if (snippetId) {
        try {
          await clipboardService.recordCopy(snippetId)
        } catch (recordError) {
          console.warn('Failed to record copy statistics:', recordError)
          // 不影响复制操作的成功状态
        }
      }

      // 更新状态
      lastCopiedText.value = text
      lastCopyTime.value = new Date()

      // 显示成功提示
      if (showSuccessToast) {
        toast.success(successMessage)
      }

      return true

    } catch (error) {
      console.error('Copy failed:', error)
      
      // 显示错误提示
      if (showErrorToast) {
        toast.error(errorMessage)
      }

      return false
    } finally {
      copying.value = false
    }
  }

  /**
   * 降级复制方法（用于不支持现代剪贴板 API 的环境）
   */
  const fallbackCopyToClipboard = (text: string): Promise<void> => {
    return new Promise((resolve, reject) => {
      try {
        const textArea = document.createElement('textarea')
        textArea.value = text
        textArea.style.position = 'fixed'
        textArea.style.left = '-999999px'
        textArea.style.top = '-999999px'
        textArea.style.opacity = '0'
        textArea.setAttribute('readonly', '')
        textArea.setAttribute('aria-hidden', 'true')
        
        document.body.appendChild(textArea)
        textArea.focus()
        textArea.select()
        textArea.setSelectionRange(0, text.length)
        
        const successful = document.execCommand('copy')
        document.body.removeChild(textArea)
        
        if (successful) {
          resolve()
        } else {
          reject(new Error('execCommand copy failed'))
        }
      } catch (error) {
        reject(error)
      }
    })
  }

  /**
   * 检查是否支持剪贴板 API
   */
  const isClipboardSupported = (): boolean => {
    return !!(navigator.clipboard && window.isSecureContext)
  }

  /**
   * 读取剪贴板内容（需要用户权限）
   */
  const readFromClipboard = async (): Promise<string | null> => {
    if (!isClipboardSupported()) {
      console.warn('Clipboard API not supported')
      return null
    }

    try {
      const text = await navigator.clipboard.readText()
      return text
    } catch (error) {
      console.error('Failed to read from clipboard:', error)
      return null
    }
  }

  /**
   * 复制代码片段（包含格式化）
   */
  const copyCodeSnippet = async (
    code: string, 
    language: string, 
    snippetId?: string
  ): Promise<boolean> => {
    // 可以在这里添加代码格式化逻辑
    const formattedCode = code

    return await copyToClipboard(formattedCode, {
      snippetId,
      successMessage: `${language.toUpperCase()} 代码已复制到剪贴板`,
      errorMessage: '代码复制失败，请手动选择复制'
    })
  }

  /**
   * 批量复制多个文本（用换行符连接）
   */
  const copyMultiple = async (
    texts: string[], 
    separator: string = '\n',
    options: CopyOptions = {}
  ): Promise<boolean> => {
    const combinedText = texts.join(separator)
    return await copyToClipboard(combinedText, options)
  }

  return {
    // 状态
    copying: readonly(copying),
    lastCopiedText: readonly(lastCopiedText),
    lastCopyTime: readonly(lastCopyTime),

    // 方法
    copyToClipboard,
    copyCodeSnippet,
    copyMultiple,
    readFromClipboard,
    isClipboardSupported
  }
}

// 只读包装函数
function readonly<T>(ref: import('vue').Ref<T>) {
  return ref as Readonly<import('vue').Ref<T>>
}