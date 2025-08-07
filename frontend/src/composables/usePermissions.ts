import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

/**
 * 权限管理组合式函数 - 提供基于角色的权限检查
 */
export function usePermissions() {
  const authStore = useAuthStore()

  // 计算属性 - 用户角色检查
  const isAdmin = computed(() => authStore.user?.role === UserRole.Admin)
  const isEditor = computed(() =>
    authStore.user?.role === UserRole.Editor || authStore.user?.role === UserRole.Admin
  )
  const isViewer = computed(() => authStore.user?.role === UserRole.Viewer)

  // 权限检查函数
  const canCreateSnippet = computed(() => isEditor.value)
  const canEditSnippet = computed(() => isEditor.value)
  const canDeleteSnippet = computed(() => isEditor.value)
  const canManageUsers = computed(() => isAdmin.value)
  const canManageTags = computed(() => isEditor.value)

  /**
   * 检查是否可以编辑特定代码片段
   * @param snippetCreatorId 代码片段创建者ID
   * @returns 是否可以编辑
   */
  function canEditSpecificSnippet(snippetCreatorId: string): boolean {
    if (!authStore.user) return false

    // 管理员可以编辑任何代码片段
    if (isAdmin.value) return true

    // 创建者可以编辑自己的代码片段
    if (authStore.user.id === snippetCreatorId) return true

    return false
  }

  /**
   * 检查是否可以删除特定代码片段
   * @param snippetCreatorId 代码片段创建者ID
   * @returns 是否可以删除
   */
  function canDeleteSpecificSnippet(snippetCreatorId: string): boolean {
    if (!authStore.user) return false

    // 管理员可以删除任何代码片段
    if (isAdmin.value) return true

    // 创建者可以删除自己的代码片段
    if (authStore.user.id === snippetCreatorId) return true

    return false
  }

  /**
   * 检查是否可以查看特定代码片段
   * @param snippet 代码片段信息
   * @returns 是否可以查看
   */
  function canViewSnippet(snippet: { isPublic: boolean; createdBy: string }): boolean {
    if (!authStore.user) return false

    // 公开的代码片段所有人都可以查看
    if (snippet.isPublic) return true

    // 管理员可以查看任何代码片段
    if (isAdmin.value) return true

    // 创建者可以查看自己的代码片段
    if (authStore.user.id === snippet.createdBy) return true

    return false
  }

  /**
   * 获取用户角色显示名称
   * @param role 用户角色
   * @returns 角色显示名称
   */
  function getRoleDisplayName(role: UserRole): string {
    switch (role) {
      case UserRole.Admin:
        return '管理员'
      case UserRole.Editor:
        return '编辑者'
      case UserRole.Viewer:
        return '查看者'
      default:
        return '未知'
    }
  }

  return {
    // 角色检查
    isAdmin,
    isEditor,
    isViewer,

    // 权限检查
    canCreateSnippet,
    canEditSnippet,
    canDeleteSnippet,
    canManageUsers,
    canManageTags,

    // 特定权限检查函数
    canEditSpecificSnippet,
    canDeleteSpecificSnippet,
    canViewSnippet,

    // 工具函数
    getRoleDisplayName
  }
}
