import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { User, UserRole } from '@/types'
import { userService, type CreateUserRequest, type UpdateUserRequest } from '@/services/userService'

/**
 * 用户管理状态管理 - 管理用户列表和相关操作
 */
export const useUsersStore = defineStore('users', () => {
  // 状态
  const users = ref<User[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  // 计算属性
  const activeUsers = computed(() => users.value.filter(user => user.isActive))
  const inactiveUsers = computed(() => users.value.filter(user => !user.isActive))
  const adminUsers = computed(() => users.value.filter(user => user.role === 2)) // UserRole.Admin
  const editorUsers = computed(() => users.value.filter(user => user.role === 1)) // UserRole.Editor
  const viewerUsers = computed(() => users.value.filter(user => user.role === 0)) // UserRole.Viewer

  /**
   * 获取用户统计信息
   */
  const userStats = computed(() => ({
    total: users.value.length,
    active: activeUsers.value.length,
    inactive: inactiveUsers.value.length,
    admins: adminUsers.value.length,
    editors: editorUsers.value.length,
    viewers: viewerUsers.value.length
  }))

  // 操作方法
  /**
   * 加载所有用户
   */
  async function fetchUsers() {
    loading.value = true
    error.value = null
    
    try {
      users.value = await userService.getAllUsers()
    } catch (err: any) {
      error.value = err.response?.data?.message || '获取用户列表失败'
      console.error('获取用户列表失败:', err)
    } finally {
      loading.value = false
    }
  }

  /**
   * 根据ID获取用户
   */
  function getUserById(id: string): User | undefined {
    return users.value.find(user => user.id === id)
  }

  /**
   * 创建新用户
   */
  async function createUser(userData: CreateUserRequest): Promise<User | null> {
    loading.value = true
    error.value = null

    try {
      const newUser = await userService.createUser(userData)
      users.value.push(newUser)
      return newUser
    } catch (err: any) {
      error.value = err.response?.data?.message || '创建用户失败'
      console.error('创建用户失败:', err)
      return null
    } finally {
      loading.value = false
    }
  }

  /**
   * 更新用户信息
   */
  async function updateUser(id: string, userData: UpdateUserRequest): Promise<User | null> {
    loading.value = true
    error.value = null

    try {
      const updatedUser = await userService.updateUser(id, userData)
      const index = users.value.findIndex(user => user.id === id)
      if (index !== -1) {
        users.value[index] = updatedUser
      }
      return updatedUser
    } catch (err: any) {
      error.value = err.response?.data?.message || '更新用户失败'
      console.error('更新用户失败:', err)
      return null
    } finally {
      loading.value = false
    }
  }

  /**
   * 删除用户
   */
  async function deleteUser(id: string): Promise<boolean> {
    loading.value = true
    error.value = null

    try {
      await userService.deleteUser(id)
      users.value = users.value.filter(user => user.id !== id)
      return true
    } catch (err: any) {
      error.value = err.response?.data?.message || '删除用户失败'
      console.error('删除用户失败:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  /**
   * 切换用户状态
   */
  async function toggleUserStatus(id: string, isActive: boolean): Promise<boolean> {
    loading.value = true
    error.value = null

    try {
      const updatedUser = await userService.toggleUserStatus(id, isActive)
      const index = users.value.findIndex(user => user.id === id)
      if (index !== -1) {
        users.value[index] = updatedUser
      }
      return true
    } catch (err: any) {
      error.value = err.response?.data?.message || '切换用户状态失败'
      console.error('切换用户状态失败:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  /**
   * 重置用户密码
   */
  async function resetPassword(id: string, newPassword: string): Promise<boolean> {
    loading.value = true
    error.value = null

    try {
      await userService.resetPassword(id, newPassword)
      return true
    } catch (err: any) {
      error.value = err.response?.data?.message || '重置密码失败'
      console.error('重置密码失败:', err)
      return false
    } finally {
      loading.value = false
    }
  }

  /**
   * 清除错误信息
   */
  function clearError() {
    error.value = null
  }

  /**
   * 重置状态
   */
  function resetState() {
    users.value = []
    loading.value = false
    error.value = null
  }

  return {
    // 状态
    users,
    loading,
    error,
    
    // 计算属性
    activeUsers,
    inactiveUsers,
    adminUsers,
    editorUsers,
    viewerUsers,
    userStats,
    
    // 方法
    fetchUsers,
    getUserById,
    createUser,
    updateUser,
    deleteUser,
    toggleUserStatus,
    resetPassword,
    clearError,
    resetState
  }
})