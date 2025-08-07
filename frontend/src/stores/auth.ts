import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/authService'
import type { User, LoginRequest, RegisterRequest } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  // 状态
  const user = ref<User | null>(null)
  const token = ref<string | null>(null)
  const isLoading = ref(false)

  // 计算属性
  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => user.value?.role === 2) // UserRole.Admin
  const isEditor = computed(() => user.value?.role === 1 || user.value?.role === 2) // Editor or Admin

  // 初始化 - 从本地存储恢复状态
  function initialize() {
    const savedToken = authService.getToken()
    const savedUser = authService.getCurrentUser()

    if (savedToken && savedUser) {
      token.value = savedToken
      user.value = savedUser
    }
  }

  // 登录
  async function login(credentials: LoginRequest) {
    isLoading.value = true
    try {
      const response = await authService.login(credentials)
      token.value = response.token
      user.value = response.user
      return response
    } finally {
      isLoading.value = false
    }
  }

  // 注册
  async function register(userData: RegisterRequest) {
    isLoading.value = true
    try {
      return await authService.register(userData)
    } finally {
      isLoading.value = false
    }
  }

  // 登出
  async function logout() {
    isLoading.value = true
    try {
      await authService.logout()
    } finally {
      token.value = null
      user.value = null
      isLoading.value = false
    }
  }

  // 更新用户信息
  function updateUser(updatedUser: User) {
    user.value = updatedUser
    localStorage.setItem('user', JSON.stringify(updatedUser))
  }

  // 初始化状态
  initialize()

  return {
    // 状态
    user,
    token,
    isLoading,
    // 计算属性
    isAuthenticated,
    isAdmin,
    isEditor,
    // 方法
    login,
    register,
    logout,
    initialize,
    updateUser
  }
})
