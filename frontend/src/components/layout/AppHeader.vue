<template>
  <header class="app-header">
    <div class="header-container">
      <!-- 品牌标识 -->
      <div class="header-brand">
        <router-link to="/" class="brand-link">
          <svg class="brand-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0L19.2 12l-4.6-4.6L16 6l6 6-6 6-1.4-1.4z"/>
          </svg>
          <span class="brand-text">代码片段管理</span>
        </router-link>
      </div>

      <!-- 主导航菜单 -->
      <nav class="header-nav" v-if="isAuthenticated">
        <ul class="nav-list">
          <li class="nav-item">
            <router-link to="/" class="nav-link">
              <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
                <path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z"/>
              </svg>
              首页
            </router-link>
          </li>
          <li class="nav-item">
            <router-link to="/snippets" class="nav-link">
              <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
                <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z"/>
              </svg>
              代码片段
            </router-link>
          </li>
          <li class="nav-item" v-if="canCreateSnippet">
            <router-link to="/snippets/create" class="nav-link">
              <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
                <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
              </svg>
              创建片段
            </router-link>
          </li>
        </ul>
      </nav>

      <!-- 用户信息和操作 -->
      <div class="header-user" v-if="isAuthenticated">
        <div class="user-info">
          <div class="user-avatar">
            {{ userInitials }}
          </div>
          <div class="user-details">
            <span class="user-name">{{ user?.username }}</span>
            <span class="user-role">{{ userRoleText }}</span>
          </div>
        </div>

        <div class="user-actions">
          <button @click="handleLogout" class="logout-btn" title="登出">
            <svg class="logout-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M16,17V14H9V10H16V7L21,12L16,17M14,2A2,2 0 0,1 16,4V6H14V4H5V20H14V18H16V20A2,2 0 0,1 14,22H5A2,2 0 0,1 3,20V4A2,2 0 0,1 5,2H14Z"/>
            </svg>
          </button>
        </div>
      </div>

      <!-- 移动端菜单按钮 -->
      <button
        class="mobile-menu-btn"
        @click="toggleMobileMenu"
        v-if="isAuthenticated"
        :class="{ active: isMobileMenuOpen }"
      >
        <span></span>
        <span></span>
        <span></span>
      </button>
    </div>

    <!-- 移动端导航菜单 -->
    <nav class="mobile-nav" v-if="isAuthenticated && isMobileMenuOpen">
      <ul class="mobile-nav-list">
        <li class="mobile-nav-item">
          <router-link to="/" class="mobile-nav-link" @click="closeMobileMenu">
            首页
          </router-link>
        </li>
        <li class="mobile-nav-item">
          <router-link to="/snippets" class="mobile-nav-link" @click="closeMobileMenu">
            代码片段
          </router-link>
        </li>
        <li class="mobile-nav-item" v-if="canCreateSnippet">
          <router-link to="/snippets/create" class="mobile-nav-link" @click="closeMobileMenu">
            创建片段
          </router-link>
        </li>
      </ul>
    </nav>
  </header>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const router = useRouter()
const authStore = useAuthStore()
const isMobileMenuOpen = ref(false)

// 计算属性
const isAuthenticated = computed(() => authStore.isAuthenticated)
const user = computed(() => authStore.user)

/**
 * 检查用户是否可以创建代码片段
 */
const canCreateSnippet = computed(() => {
  if (!user.value) return false
  return user.value.role === UserRole.Admin || user.value.role === UserRole.Editor
})

/**
 * 获取用户姓名首字母
 */
const userInitials = computed(() => {
  if (!user.value?.username) return 'U'
  return user.value.username.charAt(0).toUpperCase()
})

/**
 * 获取用户角色文本
 */
const userRoleText = computed(() => {
  if (!user.value) return ''
  switch (user.value.role) {
    case UserRole.Admin:
      return '管理员'
    case UserRole.Editor:
      return '编辑者'
    case UserRole.Viewer:
      return '查看者'
    default:
      return ''
  }
})

/**
 * 处理用户登出
 */
async function handleLogout() {
  try {
    await authStore.logout()
    router.push('/login')
  } catch (error) {
    console.error('登出失败:', error)
  }
}

/**
 * 切换移动端菜单显示状态
 */
function toggleMobileMenu() {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
}

/**
 * 关闭移动端菜单
 */
function closeMobileMenu() {
  isMobileMenuOpen.value = false
}
</script>

<style scoped>
.app-header {
  background-color: #343a40;
  color: white;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  position: sticky;
  top: 0;
  z-index: 1000;
}

.header-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  height: 64px;
}

/* 品牌标识 */
.header-brand {
  flex-shrink: 0;
}

.brand-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: white;
  text-decoration: none;
  font-size: 1.25rem;
  font-weight: 600;
  transition: color 0.3s ease;
}

.brand-link:hover {
  color: #adb5bd;
}

.brand-icon {
  width: 24px;
  height: 24px;
}

.brand-text {
  white-space: nowrap;
}

/* 主导航 */
.header-nav {
  flex: 1;
  margin: 0 2rem;
}

.nav-list {
  display: flex;
  list-style: none;
  margin: 0;
  padding: 0;
  gap: 0.5rem;
}

.nav-item {
  position: relative;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: white;
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 6px;
  transition: all 0.3s ease;
  white-space: nowrap;
}

.nav-link:hover {
  background-color: #495057;
  color: #f8f9fa;
}

.nav-link.router-link-active {
  background-color: #007bff;
  color: white;
}

.nav-icon {
  width: 18px;
  height: 18px;
}

/* 用户信息区域 */
.header-user {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-shrink: 0;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.user-avatar {
  width: 36px;
  height: 36px;
  background-color: #007bff;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.875rem;
}

.user-details {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}

.user-name {
  font-weight: 500;
  font-size: 0.875rem;
  line-height: 1.2;
}

.user-role {
  font-size: 0.75rem;
  color: #adb5bd;
  line-height: 1.2;
}

.user-actions {
  display: flex;
  align-items: center;
}

.logout-btn {
  background: none;
  border: none;
  color: white;
  padding: 0.5rem;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.logout-btn:hover {
  background-color: #dc3545;
  color: white;
}

.logout-icon {
  width: 18px;
  height: 18px;
}

/* 移动端菜单按钮 */
.mobile-menu-btn {
  display: none;
  flex-direction: column;
  justify-content: space-around;
  width: 24px;
  height: 24px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 0;
}

.mobile-menu-btn span {
  width: 100%;
  height: 2px;
  background-color: white;
  transition: all 0.3s ease;
}

.mobile-menu-btn.active span:nth-child(1) {
  transform: rotate(45deg) translate(5px, 5px);
}

.mobile-menu-btn.active span:nth-child(2) {
  opacity: 0;
}

.mobile-menu-btn.active span:nth-child(3) {
  transform: rotate(-45deg) translate(7px, -6px);
}

/* 移动端导航 */
.mobile-nav {
  display: none;
  background-color: #495057;
  border-top: 1px solid #6c757d;
}

.mobile-nav-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.mobile-nav-item {
  border-bottom: 1px solid #6c757d;
}

.mobile-nav-item:last-child {
  border-bottom: none;
}

.mobile-nav-link {
  display: block;
  color: white;
  text-decoration: none;
  padding: 1rem;
  transition: background-color 0.3s ease;
}

.mobile-nav-link:hover,
.mobile-nav-link.router-link-active {
  background-color: #007bff;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .header-nav {
    display: none;
  }

  .mobile-menu-btn {
    display: flex;
  }

  .mobile-nav {
    display: block;
  }

  .user-details {
    display: none;
  }

  .header-container {
    padding: 0 0.75rem;
  }

  .brand-text {
    display: none;
  }
}

@media (max-width: 480px) {
  .header-container {
    height: 56px;
  }

  .brand-link {
    font-size: 1.125rem;
  }

  .user-avatar {
    width: 32px;
    height: 32px;
    font-size: 0.75rem;
  }
}
</style>
