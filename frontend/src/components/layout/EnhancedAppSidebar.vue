<template>
  <aside 
    class="enhanced-app-sidebar" 
    :class="sidebarClasses"
    :style="sidebarStyles"
    :aria-label="ariaLabel"
  >
    <!-- 侧边栏头部 -->
    <transition :name="headerAnimation">
      <div v-if="showHeader" class="sidebar-header" :class="headerClasses">
        <div class="header-content">
          <slot name="sidebar-header">
            <!-- Logo区域 -->
            <div v-if="showLogo" class="logo-section">
              <router-link to="/" class="logo-link">
                <div class="logo-icon">
                  <slot name="logo-icon">
                    <svg viewBox="0 0 24 24" fill="currentColor">
                      <path d="M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0L19.2 12l-4.6-4.6L16 6l6 6-6 6-1.4-1.4z"/>
                    </svg>
                  </slot>
                </div>
                <transition :name="logoTextAnimation">
                  <span v-if="!collapsed && showLogoText" class="logo-text">
                    <slot name="logo-text">SeekCode</slot>
                  </span>
                </transition>
              </router-link>
            </div>
          </slot>
        </div>

        <!-- 折叠按钮 -->
        <button
          v-if="collapsible"
          @click="handleToggleCollapse"
          class="collapse-btn"
          :class="collapseBtnClasses"
          :title="collapsed ? expandTooltip : collapseTooltip"
          :aria-label="collapsed ? expandTooltip : collapseTooltip"
          :aria-expanded="!collapsed"
        >
          <transition :name="collapseIconAnimation" mode="out-in">
            <svg 
              :key="collapsed ? 'expand' : 'collapse'"
              class="collapse-icon"
              viewBox="0 0 24 24" 
              fill="currentColor"
            >
              <path v-if="collapsed" d="M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z"/>
              <path v-else d="M15.41,16.58L10.83,12L15.41,7.41L14,6L8,12L14,18L15.41,16.58Z"/>
            </svg>
          </transition>
        </button>
      </div>
    </transition>

    <!-- 搜索栏 -->
    <transition :name="searchAnimation">
      <div v-if="showSearch && !collapsed" class="search-section" :class="searchClasses">
        <div class="search-container">
          <slot name="search">
            <div class="search-input-wrapper">
              <svg class="search-icon" viewBox="0 0 24 24" fill="currentColor">
                <path d="M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z"/>
              </svg>
              <input
                type="text"
                :placeholder="searchPlaceholder"
                class="search-input"
                :value="searchQuery"
                @input="handleSearchInput"
                @keyup.enter="handleSearchSubmit"
                :aria-label="searchPlaceholder"
              />
            </div>
          </slot>
        </div>
      </div>
    </transition>

    <!-- 导航菜单 -->
    <nav class="sidebar-nav" :class="navClasses" :aria-label="navAriaLabel">
      <ul class="nav-list" :class="navListClasses">
        <!-- 导航项目 -->
        <template v-for="(section, sectionIndex) in navigationSections" :key="sectionIndex">
          <!-- 分组标题 -->
          <transition :name="sectionTitleAnimation">
            <li 
              v-if="section.title && !collapsed && shouldShowSection(section)" 
              class="nav-section"
              :key="`section-${sectionIndex}`"
            >
              <h3 class="section-title" :class="sectionTitleClasses">
                {{ section.title }}
              </h3>
            </li>
          </transition>

          <!-- 导航项目 -->
          <li 
            v-for="(item, itemIndex) in section.items" 
            :key="`item-${sectionIndex}-${itemIndex}`"
            class="nav-item"
            :class="navItemClasses"
          >
            <component
              :is="item.to ? 'router-link' : 'button'"
              :to="item.to"
              :href="item.href"
              :target="item.target"
              class="nav-link"
              :class="[
                navLinkClasses,
                {
                  'active': isActive(item),
                  'disabled': item.disabled,
                  'external': item.external
                }
              ]"
              :title="collapsed ? item.title : undefined"
              :aria-current="isActive(item) ? 'page' : undefined"
              :disabled="item.disabled"
              @click="handleItemClick(item, $event)"
            >
              <!-- 图标 -->
              <div class="nav-icon-wrapper">
                <component 
                  :is="item.icon || 'div'" 
                  class="nav-icon"
                  :class="item.iconClass"
                />
                
                <!-- 激活状态指示器 -->
                <transition :name="activeIndicatorAnimation">
                  <div v-if="isActive(item) && !collapsed" class="active-indicator"></div>
                </transition>
              </div>

              <!-- 文本 -->
              <transition :name="navTextAnimation">
                <span v-if="!collapsed" class="nav-text">
                  {{ item.title }}
                </span>
              </transition>

              <!-- 徽章 -->
              <transition :name="badgeAnimation">
                <div v-if="item.badge && !collapsed" class="nav-badge" :class="item.badgeClass">
                  {{ item.badge }}
                </div>
              </transition>

              <!-- 子菜单指示器 -->
              <transition :name="submenuIndicatorAnimation">
                <div v-if="item.children && !collapsed" class="submenu-indicator">
                  <svg class="submenu-icon" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z"/>
                  </svg>
                </div>
              </transition>
            </component>

            <!-- 子菜单 -->
            <transition :name="submenuAnimation">
              <div 
                v-if="item.children && !collapsed && isSubmenuOpen(item)" 
                class="submenu"
                :class="submenuClasses"
              >
                <ul class="submenu-list">
                  <li 
                    v-for="(child, childIndex) in item.children" 
                    :key="`child-${sectionIndex}-${itemIndex}-${childIndex}`"
                    class="submenu-item"
                  >
                    <component
                      :is="child.to ? 'router-link' : 'button'"
                      :to="child.to"
                      :href="child.href"
                      :target="child.target"
                      class="submenu-link"
                      :class="[
                        submenuLinkClasses,
                        {
                          'active': isActive(child),
                          'disabled': child.disabled
                        }
                      ]"
                      :disabled="child.disabled"
                      @click="handleItemClick(child, $event)"
                    >
                      <span class="submenu-text">{{ child.title }}</span>
                      
                      <!-- 子菜单徽章 -->
                      <div v-if="child.badge" class="submenu-badge" :class="child.badgeClass">
                        {{ child.badge }}
                      </div>
                    </component>
                  </li>
                </ul>
              </div>
            </transition>
          </li>
        </template>
      </ul>
    </nav>

    <!-- 侧边栏底部 -->
    <transition :name="footerAnimation">
      <div v-if="showFooter && !collapsed" class="sidebar-footer" :class="footerClasses">
        <slot name="sidebar-footer">
          <!-- 用户信息 -->
          <div v-if="showUserInfo" class="user-info" :class="userInfoClasses">
            <div class="user-avatar" :class="userAvatarClasses">
              <img 
                v-if="userAvatar" 
                :src="userAvatar" 
                :alt="userName"
                class="avatar-image"
              />
              <span v-else class="avatar-initials">
                {{ userInitials }}
              </span>
            </div>
            <div class="user-details">
              <span class="user-name" :class="userNameClasses">{{ userName }}</span>
              <span class="user-role" :class="userRoleClasses">{{ userRole }}</span>
            </div>
          </div>

          <!-- 底部操作 -->
          <div v-if="showFooterActions" class="footer-actions">
            <slot name="footer-actions">
              <button 
                v-if="showThemeToggle"
                @click="handleThemeToggle"
                class="footer-action-btn"
                :title="themeToggleTooltip"
              >
                <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12,18C11.11,18 10.26,17.8 9.5,17.45C11.56,16.5 13,14.42 13,12C13,9.58 11.56,7.5 9.5,6.55C10.26,6.2 11.11,6 12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18M20,8.69V4H15.31L12,0.69L8.69,4H4V8.69L0.69,12L4,15.31V20H8.69L12,23.31L15.31,20H20V15.31L23.31,12L20,8.69Z"/>
                </svg>
              </button>

              <button 
                v-if="showSettingsBtn"
                @click="handleSettingsClick"
                class="footer-action-btn"
                :title="settingsTooltip"
              >
                <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z"/>
                </svg>
              </button>

              <button 
                v-if="showLogoutBtn"
                @click="handleLogout"
                class="footer-action-btn logout-btn"
                :title="logoutTooltip"
              >
                <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M16,17V14H9V10H16V7L21,12L16,17M14,2A2,2 0 0,1 16,4V6H14V4H5V20H14V18H16V20A2,2 0 0,1 14,22H5A2,2 0 0,1 3,20V4A2,2 0 0,1 5,2H14Z"/>
                </svg>
              </button>
            </slot>
          </div>
        </slot>
      </div>
    </transition>

    <!-- 移动端关闭按钮 -->
    <button
      v-if="responsive && isMobile"
      @click="handleCloseMobile"
      class="mobile-close-btn"
      :class="mobileCloseBtnClasses"
      :title="mobileCloseTooltip"
      :aria-label="mobileCloseTooltip"
    >
      <svg class="close-icon" viewBox="0 0 24 24" fill="currentColor">
        <path d="M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"/>
      </svg>
    </button>

    <!-- 工具提示 -->
    <transition :name="tooltipAnimation">
      <div 
        v-if="showTooltip && hoveredItem && collapsed"
        class="nav-tooltip"
        :class="tooltipClasses"
        :style="tooltipStyles"
      >
        {{ hoveredItem.title }}
        <div class="tooltip-arrow"></div>
      </div>
    </transition>
  </aside>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useBreakpoints } from '@/composables/useBreakpoints'

// 导航项目接口
interface NavigationItem {
  title: string
  to?: string
  href?: string
  target?: '_blank' | '_self' | '_parent' | '_top'
  icon?: string | object
  iconClass?: string
  badge?: string | number
  badgeClass?: string
  disabled?: boolean
  external?: boolean
  children?: NavigationItem[]
  permission?: string | string[]
  visible?: boolean
  onClick?: (item: NavigationItem, event: Event) => void
}

// 导航分组接口
interface NavigationSection {
  title?: string
  items: NavigationItem[]
  visible?: boolean
  permission?: string | string[]
}

// Props 接口
interface Props {
  // 基础配置
  variant?: 'default' | 'minimal' | 'compact' | 'overlay'
  collapsible?: boolean
  collapsed?: boolean
  animation?: 'slide' | 'fade' | 'scale' | 'bounce' | 'elastic'
  responsive?: boolean
  sticky?: boolean
  
  // 头部配置
  showHeader?: boolean
  showLogo?: boolean
  showLogoText?: boolean
  
  // 搜索配置
  showSearch?: boolean
  searchPlaceholder?: string
  searchQuery?: string
  
  // 导航配置
  navigationSections?: NavigationSection[]
  navAriaLabel?: string
  
  // 底部配置
  showFooter?: boolean
  showUserInfo?: boolean
  showFooterActions?: boolean
  showThemeToggle?: boolean
  showSettingsBtn?: boolean
  showLogoutBtn?: boolean
  
  // 用户信息
  userName?: string
  userRole?: string
  userAvatar?: string
  
  // 工具提示配置
  showTooltip?: boolean
  tooltipDelay?: number
  
  // 动画配置
  headerAnimation?: 'fade' | 'slide' | 'scale'
  logoTextAnimation?: 'fade' | 'slide' | 'scale'
  collapseIconAnimation?: 'rotate' | 'fade' | 'scale'
  searchAnimation?: 'fade' | 'slide' | 'scale'
  sectionTitleAnimation?: 'fade' | 'slide' | 'scale'
  navTextAnimation?: 'fade' | 'slide' | 'scale'
  badgeAnimation?: 'fade' | 'slide' | 'scale'
  submenuIndicatorAnimation?: 'rotate' | 'fade' | 'scale'
  submenuAnimation?: 'slide' | 'scale' | 'fade'
  activeIndicatorAnimation?: 'fade' | 'slide' | 'scale'
  footerAnimation?: 'fade' | 'slide' | 'scale'
  tooltipAnimation?: 'fade' | 'slide' | 'scale'
  
  // 响应式配置
  mobileBreakpoint?: 'xs' | 'sm' | 'md' | 'lg'
  
  // 主题配置
  theme?: 'light' | 'dark' | 'auto'
  
  // 无障碍配置
  ariaLabel?: string
  
  // 工具提示配置
  expandTooltip?: string
  collapseTooltip?: string
  mobileCloseTooltip?: string
  themeToggleTooltip?: string
  settingsTooltip?: string
  logoutTooltip?: string
}

// Emits 接口
interface Emits {
  (e: 'update:collapsed', collapsed: boolean): void
  (e: 'toggle-collapse', collapsed: boolean): void
  (e: 'close-mobile'): void
  (e: 'item-click', item: NavigationItem, event: Event): void
  (e: 'search', query: string): void
  (e: 'search-submit', query: string): void
  (e: 'theme-toggle'): void
  (e: 'settings-click'): void
  (e: 'logout'): void
  (e: 'submenu-toggle', item: NavigationItem, open: boolean): void
}

const props = withDefaults(defineProps<Props>(), {
  // 基础配置
  variant: 'default',
  collapsible: true,
  collapsed: false,
  animation: 'slide',
  responsive: true,
  sticky: true,
  
  // 头部配置
  showHeader: true,
  showLogo: true,
  showLogoText: true,
  
  // 搜索配置
  showSearch: false,
  searchPlaceholder: '搜索...',
  searchQuery: '',
  
  // 导航配置
  navAriaLabel: '主导航菜单',
  
  // 底部配置
  showFooter: true,
  showUserInfo: true,
  showFooterActions: true,
  showThemeToggle: true,
  showSettingsBtn: true,
  showLogoutBtn: true,
  
  // 工具提示配置
  showTooltip: true,
  tooltipDelay: 300,
  
  // 动画配置
  headerAnimation: 'fade',
  logoTextAnimation: 'fade',
  collapseIconAnimation: 'rotate',
  searchAnimation: 'fade',
  sectionTitleAnimation: 'fade',
  navTextAnimation: 'fade',
  badgeAnimation: 'fade',
  submenuIndicatorAnimation: 'rotate',
  submenuAnimation: 'slide',
  activeIndicatorAnimation: 'fade',
  footerAnimation: 'fade',
  tooltipAnimation: 'fade',
  
  // 响应式配置
  mobileBreakpoint: 'md',
  
  // 主题配置
  theme: 'auto',
  
  // 无障碍配置
  ariaLabel: '侧边栏导航',
  
  // 工具提示配置
  expandTooltip: '展开侧边栏',
  collapseTooltip: '收起侧边栏',
  mobileCloseTooltip: '关闭菜单',
  themeToggleTooltip: '切换主题',
  settingsTooltip: '设置',
  logoutTooltip: '退出登录'
})

const emit = defineEmits<Emits>()

defineSlots<{
  'sidebar-header'(): any
  'sidebar-content'(): any
  'sidebar-footer'(): any
  'logo-icon'(): any
  'logo-text'(): any
  'search'(): any
  'footer-actions'(): any
}>()

// 组合式 API
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const breakpoints = useBreakpoints()

// 响应式状态
const internalCollapsed = ref(props.collapsed)
const hoveredItem = ref<NavigationItem | null>(null)
const tooltipTimeout = ref<number | null>(null)
const openSubmenus = ref<Set<string>>(new Set())

// 计算属性
const isMobile = computed(() => {
  if (!props.responsive) return false
  return breakpoints.isLessThan(props.mobileBreakpoint)
})

const collapsed = computed({
  get: () => props.collapsed !== undefined ? props.collapsed : internalCollapsed.value,
  set: (value) => {
    if (props.collapsed === undefined) {
      internalCollapsed.value = value
    }
    emit('update:collapsed', value)
  }
})

// 默认导航配置
const defaultNavigationSections: NavigationSection[] = [
  {
    title: '主要功能',
    items: [
      {
        title: '首页',
        to: '/',
        icon: 'HomeIcon',
        permission: 'view:dashboard'
      },
      {
        title: '代码片段',
        to: '/snippets',
        icon: 'CodeIcon',
        permission: 'view:snippets'
      },
      {
        title: '创建片段',
        to: '/snippets/create',
        icon: 'PlusIcon',
        permission: 'create:snippet'
      }
    ]
  },
  {
    title: '管理功能',
    items: [
      {
        title: '用户管理',
        to: '/admin/users',
        icon: 'UsersIcon',
        permission: 'admin:users'
      },
      {
        title: '标签管理',
        to: '/admin/tags',
        icon: 'TagsIcon',
        permission: 'admin:tags'
      }
    ]
  },
  {
    title: '个人功能',
    items: [
      {
        title: '分享管理',
        to: '/shares',
        icon: 'ShareIcon',
        permission: 'view:shares'
      },
      {
        title: '剪贴板历史',
        to: '/clipboard/history',
        icon: 'ClipboardIcon',
        permission: 'view:clipboard'
      },
      {
        title: '设置',
        to: '/settings',
        icon: 'SettingsIcon',
        permission: 'view:settings'
      }
    ]
  }
]

// 过滤后的导航配置
const navigationSections = computed(() => {
  const sections = props.navigationSections || defaultNavigationSections
  return sections.filter(section => shouldShowSection(section))
})

// 样式类计算
const sidebarClasses = computed(() => [
  `sidebar-${props.variant}`,
  `animation-${props.animation}`,
  `theme-${props.theme}`,
  {
    'collapsed': collapsed.value,
    'sticky': props.sticky,
    'responsive': props.responsive,
    'mobile': isMobile.value,
    'has-header': props.showHeader,
    'has-footer': props.showFooter,
    'has-search': props.showSearch,
    'collapsible': props.collapsible
  }
])

const sidebarStyles = computed(() => ({
  '--sidebar-width': getSidebarWidth(),
  '--sidebar-collapsed-width': getSidebarCollapsedWidth(),
  '--animation-duration': getAnimationDuration()
}))

const headerClasses = computed(() => [
  `header-${props.variant}`,
  {
    'collapsed': collapsed.value
  }
])

const collapseBtnClasses = computed(() => [
  `collapse-btn-${props.variant}`,
  {
    'collapsed': collapsed.value
  }
])

const searchClasses = computed(() => [
  `search-${props.variant}`
])

const navClasses = computed(() => [
  `nav-${props.variant}`
])

const navListClasses = computed(() => [
  `nav-list-${props.variant}`
])

const navItemClasses = computed(() => [
  `nav-item-${props.variant}`
])

const navLinkClasses = computed(() => [
  `nav-link-${props.variant}`
])

const submenuClasses = computed(() => [
  `submenu-${props.variant}`
])

const submenuLinkClasses = computed(() => [
  `submenu-link-${props.variant}`
])

const sectionTitleClasses = computed(() => [
  `section-title-${props.variant}`
])

const footerClasses = computed(() => [
  `footer-${props.variant}`
])

const userInfoClasses = computed(() => [
  `user-info-${props.variant}`
])

const userAvatarClasses = computed(() => [
  `user-avatar-${props.variant}`
])

const userNameClasses = computed(() => [
  `user-name-${props.variant}`
])

const userRoleClasses = computed(() => [
  `user-role-${props.variant}`
])

const mobileCloseBtnClasses = computed(() => [
  `mobile-close-btn-${props.variant}`
])

const tooltipClasses = computed(() => [
  `nav-tooltip-${props.variant}`
])

const tooltipStyles = computed(() => ({
  '--tooltip-delay': `${props.tooltipDelay}ms`
}))

// 用户信息计算
const userName = computed(() => props.userName || authStore.user?.username || '用户')
const userRole = computed(() => props.userRole || getUserRoleText())
const userAvatar = computed(() => props.userAvatar || authStore.user?.avatar)
const userInitials = computed(() => {
  const name = userName.value
  return name.charAt(0).toUpperCase()
})

// 工具方法
function getSidebarWidth(): string {
  switch (props.variant) {
    case 'minimal': return '200px'
    case 'compact': return '240px'
    case 'overlay': return '280px'
    default: return '256px'
  }
}

function getSidebarCollapsedWidth(): string {
  switch (props.variant) {
    case 'minimal': return '48px'
    case 'compact': return '56px'
    case 'overlay': return '64px'
    default: return '64px'
  }
}

function getAnimationDuration(): string {
  switch (props.animation) {
    case 'bounce': return '0.6s'
    case 'elastic': return '0.5s'
    default: return '0.3s'
  }
}

function getUserRoleText(): string {
  const user = authStore.user
  if (!user) return ''
  
  switch (user.role) {
    case 'Admin': return '管理员'
    case 'Editor': return '编辑者'
    case 'Viewer': return '查看者'
    default: return ''
  }
}

function shouldShowSection(section: NavigationSection): boolean {
  if (section.visible === false) return false
  if (section.permission && !hasPermission(section.permission)) return false
  return section.items.some(item => shouldShowItem(item))
}

function shouldShowItem(item: NavigationItem): boolean {
  if (item.visible === false) return false
  if (item.disabled) return false
  if (item.permission && !hasPermission(item.permission)) return false
  return true
}

function hasPermission(permission: string | string[]): boolean {
  // 这里可以根据实际的权限系统进行判断
  return true
}

function isActive(item: NavigationItem): boolean {
  if (item.to) {
    return route.path === item.to || route.path.startsWith(item.to + '/')
  }
  return false
}

function isSubmenuOpen(item: NavigationItem): boolean {
  return openSubmenus.value.has(item.title)
}

// 事件处理函数
function handleToggleCollapse() {
  collapsed.value = !collapsed.value
  emit('toggle-collapse', collapsed.value)
}

function handleCloseMobile() {
  emit('close-mobile')
}

function handleItemClick(item: NavigationItem, event: Event) {
  emit('item-click', item, event)
  
  if (item.children) {
    const isOpen = isSubmenuOpen(item)
    if (isOpen) {
      openSubmenus.value.delete(item.title)
    } else {
      openSubmenus.value.add(item.title)
    }
    emit('submenu-toggle', item, !isOpen)
  }
  
  if (item.onClick) {
    item.onClick(item, event)
  }
  
  // 如果是外部链接或移动端，关闭侧边栏
  if ((item.external || isMobile.value) && !item.children) {
    handleCloseMobile()
  }
}

function handleSearchInput(event: Event) {
  const query = (event.target as HTMLInputElement).value
  emit('search', query)
}

function handleSearchSubmit(event: Event) {
  const query = (event.target as HTMLInputElement).value
  emit('search-submit', query)
}

function handleThemeToggle() {
  emit('theme-toggle')
}

function handleSettingsClick() {
  emit('settings-click')
  router.push('/settings')
  handleCloseMobile()
}

function handleLogout() {
  emit('logout')
  handleCloseMobile()
}

function handleMouseEnter(item: NavigationItem) {
  if (!collapsed.value || !props.showTooltip) return
  
  hoveredItem.value = item
  
  if (tooltipTimeout.value) {
    clearTimeout(tooltipTimeout.value)
  }
  
  tooltipTimeout.value = window.setTimeout(() => {
    // 工具提示显示逻辑
  }, props.tooltipDelay)
}

function handleMouseLeave() {
  hoveredItem.value = null
  
  if (tooltipTimeout.value) {
    clearTimeout(tooltipTimeout.value)
    tooltipTimeout.value = null
  }
}

// 监听器
watch(() => props.collapsed, (newValue) => {
  internalCollapsed.value = newValue
})

// 生命周期钩子
onMounted(() => {
  // 恢复折叠状态
  const savedCollapsed = localStorage.getItem('sidebar-collapsed')
  if (savedCollapsed && props.collapsed === undefined) {
    internalCollapsed.value = savedCollapsed === 'true'
  }
})

onUnmounted(() => {
  if (tooltipTimeout.value) {
    clearTimeout(tooltipTimeout.value)
  }
})
</script>

<style scoped>
/* 基础样式 */
.enhanced-app-sidebar {
  @apply flex flex-col;
  width: var(--sidebar-width);
  height: 100vh;
  background: var(--gradient-surface);
  border-right: 1px solid rgba(0, 0, 0, 0.06);
  position: fixed;
  left: 0;
  top: 0;
  z-index: var(--z-dropdown);
  transition: all var(--animation-duration) var(--animation-smooth);
  overflow: hidden;
  box-shadow: var(--shadow-lg);
}

.enhanced-app-sidebar.collapsed {
  width: var(--sidebar-collapsed-width);
}

.enhanced-app-sidebar.sticky {
  position: sticky;
}

.enhanced-app-sidebar.responsive.mobile {
  transform: translateX(-100%);
}

.enhanced-app-sidebar.responsive.mobile.show {
  transform: translateX(0);
}

/* 变体样式 */
.sidebar-default {
  /* 默认样式 */
}

.sidebar-minimal {
  --sidebar-width: 200px;
  --sidebar-collapsed-width: 48px;
}

.sidebar-compact {
  --sidebar-width: 240px;
  --sidebar-collapsed-width: 56px;
}

.sidebar-overlay {
  --sidebar-width: 280px;
  --sidebar-collapsed-width: 64px;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-backdrop);
  border: 1px solid var(--glass-border);
}

/* 动画样式 */
.animation-slide {
  transition: transform var(--animation-duration) var(--animation-smooth);
}

.animation-fade {
  transition: opacity var(--animation-duration) var(--animation-smooth);
}

.animation-scale {
  transition: transform var(--animation-duration) var(--animation-elastic);
}

.animation-bounce {
  transition: all var(--animation-duration) var(--animation-bounce);
}

.animation-elastic {
  transition: all var(--animation-duration) var(--animation-elastic);
}

/* 头部样式 */
.sidebar-header {
  @apply flex items-center justify-between p-4 border-b;
  background: var(--gradient-surface);
  border-color: rgba(0, 0, 0, 0.06);
  min-height: 64px;
}

.header-minimal {
  min-height: 56px;
  padding: var(--space-3);
}

.header-compact {
  min-height: 60px;
  padding: var(--space-3);
}

.header-overlay {
  background: transparent;
  border-color: transparent;
}

.header-content {
  @apply flex items-center flex-1;
}

/* Logo样式 */
.logo-section {
  @apply flex items-center;
}

.logo-link {
  @apply flex items-center gap-2 text-decoration-none;
  color: var(--gray-800);
}

.logo-icon {
  @apply w-8 h-8;
  color: var(--primary-500);
}

.logo-text {
  @apply font-semibold text-lg;
  transition: opacity var(--transition-normal);
}

/* 折叠按钮 */
.collapse-btn {
  @apply p-2 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: var(--gray-600);
  cursor: pointer;
}

.collapse-btn:hover {
  background: var(--gray-100);
  color: var(--gray-800);
}

.collapse-btn-minimal {
  @apply p-1;
}

.collapse-btn-compact {
  @apply p-1.5;
}

.collapse-btn-overlay {
  background: var(--glass-bg);
  backdrop-filter: var(--glass-backdrop);
}

.collapse-icon {
  @apply w-5 h-5;
  transition: transform var(--transition-normal);
}

.collapse-btn.collapsed .collapse-icon {
  transform: rotate(180deg);
}

/* 搜索样式 */
.search-section {
  @apply p-4 border-b;
  border-color: rgba(0, 0, 0, 0.06);
}

.search-container {
  @apply relative;
}

.search-input-wrapper {
  @apply relative flex items-center;
}

.search-icon {
  @apply absolute left-3 w-5 h-5;
  color: var(--gray-400);
}

.search-input {
  @apply w-full pl-10 pr-4 py-2 rounded-lg border;
  background: var(--gray-50);
  border-color: var(--gray-200);
  color: var(--gray-800);
  font-size: var(--text-sm);
  transition: all var(--transition-normal);
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-500);
  box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
  background: white;
}

/* 导航样式 */
.sidebar-nav {
  @apply flex-1 overflow-y-auto;
  padding: var(--space-2);
}

.nav-list {
  @apply space-y-1;
}

.nav-section {
  @apply mb-2;
}

.section-title {
  @apply px-3 py-1 text-xs font-semibold;
  color: var(--gray-600);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.nav-item {
  @apply relative;
}

.nav-link {
  @apply flex items-center gap-3 px-3 py-2 rounded-lg transition-all duration-200;
  color: var(--gray-700);
  text-decoration: none;
  position: relative;
  overflow: hidden;
}

.nav-link::before {
  content: '';
  @apply absolute inset-0;
  background: var(--gradient-primary);
  opacity: 0;
  transition: opacity var(--transition-normal);
}

.nav-link:hover {
  background: rgba(0, 123, 255, 0.05);
  color: var(--primary-600);
  transform: translateX(2px);
}

.nav-link:hover::before {
  opacity: 0.1;
}

.nav-link.active {
  background: var(--gradient-primary);
  color: white;
  transform: translateX(2px);
  box-shadow: var(--shadow-primary);
}

.nav-link.active::before {
  opacity: 1;
}

.nav-link.disabled {
  @apply opacity-50 cursor-not-allowed;
}

.nav-link.disabled:hover {
  transform: none;
  background: transparent;
}

.nav-icon-wrapper {
  @apply relative flex items-center justify-center;
}

.nav-icon {
  @apply w-5 h-5;
  flex-shrink: 0;
}

.nav-text {
  @apply font-medium text-sm;
  white-space: nowrap;
  transition: opacity var(--transition-normal);
}

.nav-badge {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2;
  @apply px-2 py-1 text-xs font-semibold rounded-full;
  background: var(--error-500);
  color: white;
  font-size: 0.625rem;
}

.submenu-indicator {
  @apply absolute right-3 top-1/2 transform -translate-y-1/2;
  @apply transition-transform duration-200;
}

.submenu-indicator svg {
  @apply w-4 h-4;
}

.nav-link.active .submenu-indicator {
  transform: translateX(-2px) translateY(-50%) rotate(90deg);
}

.active-indicator {
  @apply absolute left-0 top-1/2 transform -translate-y-1/2;
  width: 3px;
  height: 20px;
  background: white;
  border-radius: 0 2px 2px 0;
}

/* 子菜单样式 */
.submenu {
  @apply ml-4 mt-1 space-y-1;
}

.submenu-list {
  @apply space-y-1;
}

.submenu-item {
  @apply relative;
}

.submenu-link {
  @apply flex items-center justify-between px-3 py-2 rounded-lg transition-all duration-200;
  color: var(--gray-600);
  text-decoration: none;
  font-size: var(--text-sm);
}

.submenu-link:hover {
  background: var(--gray-100);
  color: var(--gray-800);
}

.submenu-link.active {
  background: var(--primary-50);
  color: var(--primary-600);
}

.submenu-text {
  @apply flex-1;
}

.submenu-badge {
  @apply px-2 py-1 text-xs font-semibold rounded-full;
  background: var(--warning-500);
  color: white;
  font-size: 0.625rem;
}

/* 底部样式 */
.sidebar-footer {
  @apply p-4 border-t;
  background: var(--gradient-surface);
  border-color: rgba(0, 0, 0, 0.06);
}

.footer-minimal {
  @apply p-3;
}

.footer-compact {
  @apply p-3;
}

.footer-overlay {
  background: transparent;
  border-color: transparent;
}

.user-info {
  @apply flex items-center gap-3 mb-3;
}

.user-avatar {
  @apply w-10 h-10 rounded-full overflow-hidden;
  background: var(--gradient-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: var(--font-semibold);
}

.avatar-image {
  @apply w-full h-full object-cover;
}

.avatar-initials {
  @apply text-sm font-semibold;
}

.user-details {
  @apply flex-1 min-w-0;
}

.user-name {
  @apply font-medium text-sm text-gray-800 truncate;
}

.user-role {
  @apply text-xs text-gray-600;
}

.footer-actions {
  @apply flex items-center justify-around;
}

.footer-action-btn {
  @apply p-2 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: var(--gray-600);
  cursor: pointer;
}

.footer-action-btn:hover {
  background: var(--gray-100);
  color: var(--gray-800);
}

.action-icon {
  @apply w-5 h-5;
}

.logout-btn:hover {
  background: var(--error-50);
  color: var(--error-600);
}

/* 移动端关闭按钮 */
.mobile-close-btn {
  @apply absolute top-4 right-4 p-2 rounded-lg;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-backdrop);
  border: 1px solid var(--glass-border);
  color: var(--gray-600);
  cursor: pointer;
  z-index: 10;
}

.mobile-close-btn:hover {
  background: var(--gray-100);
  color: var(--gray-800);
}

.close-icon {
  @apply w-5 h-5;
}

/* 工具提示样式 */
.nav-tooltip {
  @apply absolute left-full ml-2 px-3 py-2 rounded-lg text-sm font-medium;
  background: var(--gray-800);
  color: white;
  white-space: nowrap;
  z-index: var(--z-tooltip);
  opacity: 0;
  pointer-events: none;
  transition: opacity var(--transition-normal);
}

.nav-tooltip.show {
  opacity: 1;
}

.tooltip-arrow {
  @apply absolute right-full top-1/2 transform -translate-y-1/2;
  width: 0;
  height: 0;
  border-top: 4px solid transparent;
  border-bottom: 4px solid transparent;
  border-right: 4px solid var(--gray-800);
}

/* 折叠状态样式 */
.enhanced-app-sidebar.collapsed .logo-text,
.enhanced-app-sidebar.collapsed .nav-text,
.enhanced-app-sidebar.collapsed .nav-badge,
.enhanced-app-sidebar.collapsed .submenu-indicator,
.enhanced-app-sidebar.collapsed .section-title,
.enhanced-app-sidebar.collapsed .search-section,
.enhanced-app-sidebar.collapsed .sidebar-footer {
  @apply hidden;
}

.enhanced-app-sidebar.collapsed .nav-link {
  @apply justify-center;
}

.enhanced-app-sidebar.collapsed .nav-link:hover::after {
  content: attr(title);
  @apply absolute left-full ml-2 px-3 py-2 rounded-lg text-sm font-medium;
  background: var(--gray-800);
  color: white;
  white-space: nowrap;
  z-index: var(--z-tooltip);
}

/* 动画类 */
.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--transition-normal);
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-enter-active,
.slide-leave-active {
  transition: all var(--transition-normal);
}

.slide-enter-from {
  opacity: 0;
  transform: translateX(-10px);
}

.slide-leave-to {
  opacity: 0;
  transform: translateX(10px);
}

.scale-enter-active,
.scale-leave-active {
  transition: all var(--transition-normal);
}

.scale-enter-from,
.scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

.rotate-enter-active,
.rotate-leave-active {
  transition: transform var(--transition-normal);
}

.rotate-enter-from,
.rotate-leave-to {
  transform: rotate(-180deg);
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .enhanced-app-sidebar.responsive {
    @apply shadow-xl;
  }
}

@media (max-width: 768px) {
  .enhanced-app-sidebar.responsive {
    width: 100%;
    max-width: 320px;
  }
  
  .sidebar-header {
    min-height: 56px;
  }
}

@media (max-width: 480px) {
  .enhanced-app-sidebar.responsive {
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
  background: var(--gray-300);
  border-radius: var(--radius-full);
}

.sidebar-nav::-webkit-scrollbar-thumb:hover {
  background: var(--gray-400);
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .enhanced-app-sidebar,
  .sidebar-header,
  .collapse-btn,
  .nav-link,
  .submenu-link,
  .footer-action-btn,
  .mobile-close-btn,
  .fade-enter-active,
  .fade-leave-active,
  .slide-enter-active,
  .slide-leave-active,
  .scale-enter-active,
  .scale-leave-active,
  .rotate-enter-active,
  .rotate-leave-active {
    transition: none;
    animation: none;
  }
  
  .nav-link:hover,
  .nav-link.active {
    transform: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .enhanced-app-sidebar {
    border-width: 2px;
  }
  
  .nav-link {
    border-width: 1px;
  }
  
  .footer-action-btn {
    border-width: 1px;
  }
}

/* 焦点样式 */
.nav-link:focus-visible,
.footer-action-btn:focus-visible,
.mobile-close-btn:focus-visible,
.collapse-btn:focus-visible {
  @apply outline-none ring-2 ring-primary-500 ring-offset-2;
}

/* 深色模式适配 */
@media (prefers-color-scheme: dark) {
  .theme-auto {
    background: linear-gradient(180deg, #2d2d2d 0%, #1a1a1a 100%);
    border-color: rgba(255, 255, 255, 0.1);
  }
  
  .theme-auto .sidebar-header,
  .theme-auto .sidebar-footer {
    background: transparent;
    border-color: rgba(255, 255, 255, 0.1);
  }
  
  .theme-auto .nav-link {
    color: var(--gray-300);
  }
  
  .theme-auto .nav-link:hover {
    background: rgba(255, 255, 255, 0.05);
    color: var(--gray-200);
  }
  
  .theme-auto .nav-link.active {
    background: var(--gradient-primary);
    color: white;
  }
  
  .theme-auto .section-title {
    color: var(--gray-400);
  }
  
  .theme-auto .user-name {
    color: var(--gray-200);
  }
  
  .theme-auto .user-role {
    color: var(--gray-400);
  }
  
  .theme-auto .footer-action-btn {
    color: var(--gray-400);
  }
  
  .theme-auto .footer-action-btn:hover {
    background: rgba(255, 255, 255, 0.1);
    color: var(--gray-200);
  }
}

/* 打印样式 */
@media print {
  .enhanced-app-sidebar {
    @apply hidden;
  }
}
</style>