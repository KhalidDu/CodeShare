import axios from 'axios'
import type { ApiErrorResponse } from '@/types/error'

// 创建 axios 实例
const api = axios.create({
  baseURL: 'http://localhost:6676/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器 - 添加认证 token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    // 处理请求错误
    console.error('Request error:', error)
    return Promise.reject(error)
  }
)

// 响应拦截器 - 处理错误
api.interceptors.response.use(
  (response) => response,
  (error) => {
    // 处理网络错误
    if (!error.response) {
      if (error.code === 'ECONNABORTED') {
        error.code = 'TIMEOUT_ERROR'
        error.message = '请求超时'
      } else if (error.message === 'Network Error') {
        error.code = 'NETWORK_ERROR'
        error.message = '网络连接失败'
      }
      return Promise.reject(error)
    }

    // 处理 HTTP 错误响应
    const { status, data } = error.response

    // 401 未授权 - 清除认证信息并跳转登录
    if (status === 401) {
      localStorage.removeItem('token')
      localStorage.removeItem('user')

      // 避免在登录页面重复跳转
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }

    // 标准化错误响应格式
    const apiError: ApiErrorResponse = {
      code: data?.code || `HTTP_${status}`,
      message: data?.message || error.message || '请求失败',
      details: data?.details,
      errors: data?.errors,
      timestamp: data?.timestamp || new Date().toISOString()
    }

    // 将标准化的错误信息附加到原始错误对象
    error.apiError = apiError

    return Promise.reject(error)
  }
)

export default api
