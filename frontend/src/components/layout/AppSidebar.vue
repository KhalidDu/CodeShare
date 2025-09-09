<template>
  <aside class="app-sidebar" :class="{ collapsed: isCollapsed }">
    <!-- 侧边栏头部 -->
    <div class="sidebar-header">
      <button
        @click="toggleCollapse"
        class="collapse-btn"
        :title="isCollapsed ? '展开侧边栏' : '收起侧边栏'"
      >
        <svg class="collapse-icon" viewBox="0 0 24 24" fill="currentColor">
          <path v-if="isCollapsed" d="M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z"/>
          <path v-else d="M15.41,16.58L10.83,12L15.41,7.41L14,6L8,12L14,18L15.41,16.58Z"/>
        </svg>
      </button>
    </div>

    <!-- 导航菜单 -->
    <nav class="sidebar-nav">
      <ul class="nav-list">
        <!-- 主要功能 -->
        <li class="nav-section">
          <h3 class="section-title" v-if="!isCollapsed">主要功能</h3>
        </li>

        <li class="nav-item">
          <router-link to="/" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">首页</span>
          </router-link>
        </li>

        <li class="nav-item">
          <router-link to="/snippets" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">代码片段</span>
          </router-link>
        </li>

        <li class="nav-item" v-if="canCreateSnippet">
          <router-link to="/snippets/create" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">创建片段</span>
          </router-link>
        </li>

        <!-- 管理功能 -->
        <li class="nav-section" v-if="isAdmin">
          <h3 class="section-title" v-if="!isCollapsed">管理功能</h3>
        </li>

        <li class="nav-item" v-if="isAdmin">
          <router-link to="/admin/users" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A2.996 2.996 0 0 0 16.96 6H15c-.8 0-1.54.37-2.01 1l-2.54 7.63H13V21h7zM12.5 11.5c.83 0 1.5-.67 1.5-1.5s-.67-1.5-1.5-1.5S11 9.17 11 10s.67 1.5 1.5 1.5zm1.5 1h-4c-.83 0-1.5.67-1.5 1.5v6h2v7h3v-7h2v-6c0-.83-.67-1.5-1.5-1.5z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">用户管理</span>
          </router-link>
        </li>

        <li class="nav-item" v-if="isAdmin">
          <router-link to="/admin/tags" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M5.5,7A1.5,1.5 0 0,1 4,5.5A1.5,1.5 0 0,1 5.5,4A1.5,1.5 0 0,1 7,5.5A1.5,1.5 0 0,1 5.5,7M21.41,11.58L12.41,2.58C12.05,2.22 11.55,2 11,2H4C2.89,2 2,2.89 2,4V11C2,11.55 2.22,12.05 2.59,12.41L11.58,21.41C11.95,21.78 12.45,22 13,22C13.55,22 14.05,21.78 14.41,21.41L21.41,14.41C21.78,14.05 22,13.55 22,13C22,12.45 21.78,11.95 21.41,11.58Z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">标签管理</span>
          </router-link>
        </li>

        <!-- 个人功能 -->
        <li class="nav-section">
          <h3 class="section-title" v-if="!isCollapsed">个人功能</h3>
        </li>

        <li class="nav-item" v-if="isLoggedIn">
          <router-link to="/shares" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M18,16.08C17.24,16.08 16.56,16.38 16.04,16.85L8.91,12.7C8.96,12.47 9,12.24 9,12C9,11.76 8.96,11.53 8.91,11.3L15.96,7.19C16.5,7.69 17.21,8 18,8A3,3 0 0,0 21,5A3,3 0 0,0 18,2A3,3 0 0,0 15,5C15,5.24 15.04,5.47 15.09,5.7L8.04,9.81C7.5,9.31 6.79,9 6,9A3,3 0 0,0 3,12A3,3 0 0,0 6,15C6.79,15 7.5,14.69 8.04,14.19L15.16,18.34C15.11,18.55 15.08,18.77 15.08,19C15.08,20.61 16.39,21.91 18,21.91C19.61,21.91 20.92,20.61 20.92,19A2.92,2.92 0 0,0 18,16.08Z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">分享管理</span>
          </router-link>
        </li>

        <li class="nav-item" v-if="isLoggedIn">
          <router-link to="/clipboard/history" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">剪贴板历史</span>
          </router-link>
        </li>

        <li class="nav-item" v-if="isLoggedIn">
          <router-link to="/settings" class="nav-link">
            <svg class="nav-icon" viewBox="0 0 24 24" fill="currentColor">
              <path d="M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z"/>
            </svg>
            <span class="nav-text" v-if="!isCollapsed">设置</span>
          </router-link>
        </li>
      </ul>
    </nav>

    <!-- 侧边栏底部 -->
    <div class="sidebar-footer" v-if="!isCollapsed">
      <div class="user-info">
        <div class="user-avatar">
          {{ userInitials }}
        </div>
        <div class="user-details">
          <span class="user-name">{{ user?.username }}</span>
          <span class="user-role">{{ userRoleText }}</span>
        </div>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const authStore = useAuthStore()
const isCollapsed = ref(false)

// 计算属性
const user = computed(() => authStore.user)

/**
 * 检查用户是否可以创建代码片段
 */
const canCreateSnippet = computed(() => {
  if (!user.value) return false
  return user.value.role === UserRole.Admin || user.value.role === UserRole.Editor
})

/**
 * 检查用户是否为管理员
 */
const isAdmin = computed(() => {
  return user.value?.role === UserRole.Admin
})

/**
 * 检查用户是否已登录（用于个人功能区域的权限控制）
 */
const isLoggedIn = computed(() => {
  return !!user.value
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
 * 切换侧边栏折叠状态
 */
function toggleCollapse() {
  isCollapsed.value = !isCollapsed.value
}
</script>

<style scoped>
.app-sidebar {
  width: 280px;
  background: linear-gradient(180deg, #ffffff 0%, #f8f9fa 100%);
  border-right: 1px solid rgba(0, 0, 0, 0.06);
  height: 100vh;
  position: fixed;
  left: 0;
  top: 64px;
  z-index: 999;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 2px 0 20px rgba(0, 0, 0, 0.05);
}

.app-sidebar.collapsed {
  width: 64px;
}

/* 侧边栏头部 */
.sidebar-header {
  padding: 1rem;
  border-bottom: 1px solid #dee2e6;
  display: flex;
  justify-content: flex-end;
}

.collapse-btn {
  background: none;
  border: none;
  color: #6c757d;
  padding: 0.5rem;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.3s ease;
  display: flex;
  align-items: center;
  justify-content: center;
}

.collapse-btn:hover {
  background-color: #e9ecef;
  color: #495057;
}

.collapse-icon {
  width: 20px;
  height: 20px;
}

/* 导航菜单 */
.sidebar-nav {
  flex: 1;
  overflow-y: auto;
  padding: 1rem 0;
}

.nav-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.nav-section {
  margin: 1.5rem 0 0.5rem 0;
}

.section-title {
  font-size: 0.75rem;
  font-weight: 600;
  color: #6c757d;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  margin: 0;
  padding: 0 1rem;
}

.nav-item {
  margin: 0.25rem 0;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  color: #6c757d;
  text-decoration: none;
  padding: 0.875rem 1.25rem;
  margin: 0 0.75rem;
  border-radius: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  font-weight: 500;
  overflow: hidden;
}

.nav-link::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, rgba(0, 123, 255, 0.08) 0%, rgba(0, 86, 179, 0.08) 100%);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.nav-link:hover {
  background: rgba(0, 123, 255, 0.05);
  color: #007bff;
  transform: translateX(4px);
}

.nav-link:hover::before {
  opacity: 1;
}

.nav-link.router-link-active {
  background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(0, 123, 255, 0.25);
  transform: translateX(4px);
}

.nav-link.router-link-active::before {
  opacity: 0;
}

.nav-link.router-link-active::after {
  content: '';
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 4px;
  height: 24px;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.8) 0%, rgba(255, 255, 255, 0.4) 100%);
  border-radius: 0 2px 2px 0;
}

.nav-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
}

.nav-text {
  font-weight: 500;
  white-space: nowrap;
}

/* 侧边栏底部 */
.sidebar-footer {
  padding: 1rem;
  border-top: 1px solid #dee2e6;
  background-color: #ffffff;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.user-avatar {
  width: 40px;
  height: 40px;
  background-color: #007bff;
  color: white;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: 0.875rem;
  flex-shrink: 0;
}

.user-details {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.user-name {
  font-weight: 500;
  font-size: 0.875rem;
  color: #212529;
  line-height: 1.2;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-role {
  font-size: 0.75rem;
  color: #6c757d;
  line-height: 1.2;
}

/* 折叠状态样式 */
.app-sidebar.collapsed .nav-link {
  justify-content: center;
  padding: 0.75rem;
  margin: 0.25rem 0.5rem;
}

.app-sidebar.collapsed .nav-link.router-link-active::before {
  display: none;
}

.app-sidebar.collapsed .nav-link::after {
  content: attr(title);
  position: absolute;
  left: 100%;
  top: 50%;
  transform: translateY(-50%);
  background-color: #212529;
  color: white;
  padding: 0.5rem 0.75rem;
  border-radius: 6px;
  font-size: 0.75rem;
  white-space: nowrap;
  opacity: 0;
  visibility: hidden;
  transition: all 0.3s ease;
  margin-left: 0.5rem;
  z-index: 1000;
}

.app-sidebar.collapsed .nav-link:hover::after {
  opacity: 1;
  visibility: visible;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .app-sidebar {
    transform: translateX(-100%);
    transition: transform 0.3s ease;
  }

  .app-sidebar.show {
    transform: translateX(0);
  }
}

@media (max-width: 768px) {
  .app-sidebar {
    width: 100%;
    max-width: 280px;
  }
}

/* 滚动条样式 */
.sidebar-nav::-webkit-scrollbar {
  width: 4px;
}

.sidebar-nav::-webkit-scrollbar-track {
  background: transparent;
}

.sidebar-nav::-webkit-scrollbar-thumb {
  background-color: #dee2e6;
  border-radius: 2px;
}

.sidebar-nav::-webkit-scrollbar-thumb:hover {
  background-color: #adb5bd;
}
</style>
