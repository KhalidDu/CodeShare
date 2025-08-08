import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

const app = createApp(App)

// 创建 Pinia 实例
const pinia = createPinia()

app.use(pinia)
app.use(router)

// 初始化错误处理系统
import { useErrorStore } from '@/stores/error'
import { ErrorType, ErrorSeverity } from '@/types/error'
const errorStore = useErrorStore()

// 初始化网络状态监控
errorStore.initializeNetworkMonitoring()

// 全局错误处理
app.config.errorHandler = (error: any, instance: any, info: string) => {
  console.error('Global error handler:', error, info)

  errorStore.addError(
    ErrorType.UNKNOWN,
    '应用错误',
    error.message || '应用运行时发生错误',
    {
      severity: ErrorSeverity.HIGH,
      details: error.stack,
      context: { componentInfo: info },
      dismissible: true,
      autoHide: false
    }
  )
}

app.mount('#app')
