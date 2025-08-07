import { useAuthStore } from '@/stores/auth'
import { usePermissions } from './usePermissions'
import { UserRole } from '@/types'

/**
 * 路由守卫组合式函数 - 提供高级路由权限检查
 */
export function useRouteGuards() {
  const authStore = useAuthStore()
  const permissions = usePermissions()

  /**
   * 检查用户是否有权限访问需要特定角色的路由
   * @param requiredRoles 需要的角色数组
   * @returns 是否有权限
   */
  function hasRequiredRole(requiredRoles: UserRole[]): boolean {
    if (!authStore.isAuthenticated || !authStore.user) {
      return false
    }

    return requiredRoles.includes(authStore.user.role)
  }

  /**
   * 检查是否可以访问管理员页面
   * @returns 是否可以访问
   */
  function canAccessAdminPages(): boolean {
    return permissions.isAdmin.value
  }

  /**
   * 检查是否可以访问编辑功能
   * @returns 是否可以访问
   */
  function canAccessEditorFeatures(): boolean {
    return permissions.isEditor.value
  }

  /**
   * 重定向到适当的页面
   * @param router Vue Router 实例
   */
  function redirectToAppropriate(router: { push: (path: string) => void }) {
    if (!authStore.isAuthenticated) {
      router.push('/login')
      return
    }

    // 根据用户角色重定向到合适的页面
    if (permissions.isAdmin.value) {
      router.push('/admin/dashboard')
    } else if (permissions.isEditor.value) {
      router.push('/snippets')
    } else {
      router.push('/snippets')
    }
  }

  return {
    hasRequiredRole,
    canAccessAdminPages,
    canAccessEditorFeatures,
    redirectToAppropriate
  }
}
