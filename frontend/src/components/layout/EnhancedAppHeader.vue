<template>
  <header 
    class="enhanced-app-header"
    :class="headerClasses"
    :style="headerStyles"
    :aria-label="ariaLabel"
  >
    <!-- 背景装饰 -->
    <div v-if="showBackgroundDecoration" class="header-decoration" :class="decorationClasses">
      <div class="decoration-pattern"></div>
    </div>

    <!-- 主容器 -->
    <div class="header-container" :class="containerClasses">
      <div class="header-content" :class="contentClasses">
        <!-- 左侧区域 -->
        <div class="header-start" :class="startClasses">
          <slot name="header-start">
            <!-- 移动端菜单按钮 -->
            <button
              v-if="showMobileMenuBtn && isMobile"
              @click="handleMobileMenuToggle"
              class="mobile-menu-btn"
              :class="mobileMenuBtnClasses"
              :title="mobileMenuTooltip"
              :aria-label="mobileMenuTooltip"
              :aria-expanded="isMobileMenuOpen"
            >
              <transition :name="mobileMenuIconAnimation" mode="out-in">
                <svg 
                  :key="isMobileMenuOpen ? 'close' : 'menu'"
                  class="menu-icon"
                  viewBox="0 0 24 24" 
                  fill="currentColor"
                >
                  <path v-if="!isMobileMenuOpen" d="M3,6H21V8H3V6M3,11H21V13H3V11M3,16H21V18H3V16Z"/>
                  <path v-else d="M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"/>
                </svg>
              </transition>
            </button>

            <!-- 侧边栏切换按钮 -->
            <button
              v-if="showSidebarToggle && !isMobile"
              @click="handleSidebarToggle"
              class="sidebar-toggle-btn"
              :class="sidebarToggleBtnClasses"
              :title="sidebarToggleTooltip"
              :aria-label="sidebarToggleTooltip"
              :aria-expanded="isSidebarOpen"
            >
              <transition :name="sidebarToggleIconAnimation" mode="out-in">
                <svg 
                  :key="isSidebarOpen ? 'close' : 'menu'"
                  class="toggle-icon"
                  viewBox="0 0 24 24" 
                  fill="currentColor"
                >
                  <path v-if="!isSidebarOpen" d="M3,6H21V8H3V6M3,11H21V13H3V11M3,16H21V18H3V16Z"/>
                  <path v-else d="M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"/>
                </svg>
              </transition>
            </button>

            <!-- Logo区域 -->
            <div v-if="showLogo" class="logo-section" :class="logoSectionClasses">
              <component 
                :is="logoTo ? 'router-link' : 'div'"
                :to="logoTo"
                :href="logoHref"
                :target="logoTarget"
                class="logo-link"
                :class="logoLinkClasses"
              >
                <div class="logo-container" :class="logoContainerClasses">
                  <div class="logo-icon" :class="logoIconClasses">
                    <slot name="logo-icon">
                      <svg viewBox="0 0 24 24" fill="currentColor">
                        <path d="M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0L19.2 12l-4.6-4.6L16 6l6 6-6 6-1.4-1.4z"/>
                      </svg>
                    </slot>
                  </div>
                  
                  <transition :name="logoTextAnimation">
                    <div v-if="showLogoText && (!isMobile || mobileShowLogoText)" class="logo-text-container">
                      <div class="logo-text" :class="logoTextClasses">
                        <slot name="logo-text">{{ logoText }}</slot>
                      </div>
                      <div v-if="showLogoSubtitle && (!isMobile || mobileShowLogoText)" class="logo-subtitle" :class="logoSubtitleClasses">
                        <slot name="logo-subtitle">{{ logoSubtitle }}</slot>
                      </div>
                    </div>
                  </transition>
                </div>
              </component>
            </div>
          </slot>
        </div>

        <!-- 中间区域 -->
        <div class="header-center" :class="centerClasses">
          <slot name="header-center">
            <!-- 导航菜单 -->
            <nav 
              v-if="showNavigation && !isMobile" 
              class="nav-menu"
              :class="navMenuClasses"
              :aria-label="navAriaLabel"
            >
              <ul class="nav-list" :class="navListClasses">
                <li 
                  v-for="(item, index) in navigationItems" 
                  :key="`nav-${index}`"
                  class="nav-item"
                  :class="navItemClasses"
                >
                  <component
                    :is="item.to ? 'router-link' : item.href ? 'a' : 'button'"
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
                    :title="item.title"
                    :aria-current="isActive(item) ? 'page' : undefined"
                    :disabled="item.disabled"
                    @click="handleNavClick(item, $event)"
                  >
                    <span class="nav-text">{{ item.title }}</span>
                    
                    <!-- 激活指示器 -->
                    <transition :name="navIndicatorAnimation">
                      <div v-if="isActive(item) && showNavIndicator" class="nav-indicator"></div>
                    </transition>
                    
                    <!-- 徽章 -->
                    <transition :name="navBadgeAnimation">
                      <div v-if="item.badge" class="nav-badge" :class="item.badgeClass">
                        {{ item.badge }}
                      </div>
                    </transition>
                  </component>
                </li>
              </ul>
            </nav>

            <!-- 搜索栏 -->
            <div v-if="showSearch && !isMobile" class="search-section" :class="searchSectionClasses">
              <div class="search-container" :class="searchContainerClasses">
                <slot name="search">
                  <div class="search-input-wrapper">
                    <svg class="search-icon" viewBox="0 0 24 24" fill="currentColor">
                      <path d="M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z"/>
                    </svg>
                    <input
                      type="text"
                      :placeholder="searchPlaceholder"
                      class="search-input"
                      :class="searchInputClasses"
                      :value="searchQuery"
                      @input="handleSearchInput"
                      @keyup.enter="handleSearchSubmit"
                      :aria-label="searchPlaceholder"
                    />
                    <button
                      v-if="searchQuery"
                      @click="handleSearchClear"
                      class="search-clear-btn"
                      :class="searchClearBtnClasses"
                      :title="searchClearTooltip"
                      :aria-label="searchClearTooltip"
                    >
                      <svg class="clear-icon" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M19,6.41L17.59,5L12,10.59L6.41,5L5,6.41L10.59,12L5,17.59L6.41,19L12,13.41L17.59,19L19,17.59L13.41,12L19,6.41Z"/>
                      </svg>
                    </button>
                  </div>
                </slot>
              </div>
            </div>
          </slot>
        </div>

        <!-- 右侧区域 -->
        <div class="header-end" :class="endClasses">
          <slot name="header-end">
            <!-- 操作按钮组 -->
            <div v-if="showActions" class="actions-section" :class="actionsSectionClasses">
              <!-- 通知按钮 -->
              <button
                v-if="showNotifications"
                @click="handleNotificationsClick"
                class="action-btn notification-btn"
                :class="actionBtnClasses"
                :title="notificationsTooltip"
                :aria-label="notificationsTooltip"
              >
                <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M21,19V20H3V19L5,17V11C5,7.9 7.03,5.17 9.6,3.59C10.57,2.61 11.73,2.06 13,2V1H15V2C16.27,2.06 17.43,2.61 18.4,3.59C20.97,5.17 23,7.9 23,11V17L25,19M18,20A2,2 0 0,1 16,22A2,2 0 0,1 14,20M12,2A9,9 0 0,1 21,11V17L23,19H20V20A4,4 0 0,1 16,24H8A4,4 0 0,1 4,20V19H1L3,17V11A9,9 0 0,1 12,2Z"/>
                </svg>
                
                <!-- 通知徽章 -->
                <transition :name="notificationBadgeAnimation">
                  <div v-if="notificationCount > 0" class="notification-badge" :class="notificationBadgeClasses">
                    {{ notificationCount > 99 ? '99+' : notificationCount }}
                  </div>
                </transition>
              </button>

              <!-- 主题切换按钮 -->
              <button
                v-if="showThemeToggle"
                @click="handleThemeToggle"
                class="action-btn theme-toggle-btn"
                :class="actionBtnClasses"
                :title="themeToggleTooltip"
                :aria-label="themeToggleTooltip"
              >
                <transition :name="themeToggleIconAnimation" mode="out-in">
                  <svg 
                    :key="currentTheme"
                    class="action-icon"
                    viewBox="0 0 24 24" 
                    fill="currentColor"
                  >
                    <path v-if="currentTheme === 'light'" d="M12,18C11.11,18 10.26,17.8 9.5,17.45C11.56,16.5 13,14.42 13,12C13,9.58 11.56,7.5 9.5,6.55C10.26,6.2 11.11,6 12,6A6,6 0 0,1 18,12A6,6 0 0,1 12,18M20,8.69V4H15.31L12,0.69L8.69,4H4V8.69L0.69,12L4,15.31V20H8.69L12,23.31L15.31,20H20V15.31L23.31,12L20,8.69Z"/>
                    <path v-else d="M12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,2L14.39,5.42C13.65,5.15 12.84,5 12,5C11.16,5 10.35,5.15 9.61,5.42L12,2M3.34,7L7.5,6.65C6.9,7.16 6.36,7.78 5.94,8.5C5.5,9.24 5.25,10 5.11,10.79L3.34,7M3.36,17L5.12,13.23C5.26,14 5.53,14.78 5.95,15.5C6.37,16.24 6.91,16.86 7.5,17.37L3.36,17M20.65,7L18.88,10.79C18.74,10 18.47,9.23 18.05,8.5C17.63,7.78 17.1,7.15 16.5,6.64L20.65,7M20.64,17L16.5,17.36C17.09,16.85 17.62,16.22 18.04,15.5C18.46,14.77 18.73,14 18.87,13.21L20.64,17Z"/>
                  </svg>
                </transition>
              </button>

              <!-- 设置按钮 -->
              <button
                v-if="showSettings"
                @click="handleSettingsClick"
                class="action-btn settings-btn"
                :class="actionBtnClasses"
                :title="settingsTooltip"
                :aria-label="settingsTooltip"
              >
                <svg class="action-icon" viewBox="0 0 24 24" fill="currentColor">
                  <path d="M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z"/>
                </svg>
              </button>

              <!-- 用户菜单 -->
              <div v-if="showUserMenu && isAuthenticated" class="user-menu-section" :class="userMenuSectionClasses">
                <button
                  @click="handleUserMenuToggle"
                  class="user-menu-btn"
                  :class="userMenuBtnClasses"
                  :title="userMenuTooltip"
                  :aria-label="userMenuTooltip"
                  :aria-expanded="isUserMenuOpen"
                  :aria-haspopup="true"
                >
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
                  
                  <div class="user-info" :class="userInfoClasses">
                    <span class="user-name" :class="userNameClasses">{{ userName }}</span>
                    <span class="user-role" :class="userRoleClasses">{{ userRole }}</span>
                  </div>
                  
                  <svg class="user-menu-arrow" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z"/>
                  </svg>
                </button>

                <!-- 用户下拉菜单 -->
                <transition :name="userMenuDropdownAnimation">
                  <div 
                    v-if="isUserMenuOpen"
                    class="user-menu-dropdown"
                    :class="userMenuDropdownClasses"
                  >
                    <div class="dropdown-header" :class="dropdownHeaderClasses">
                      <div class="dropdown-user-info">
                        <div class="dropdown-user-name">{{ userName }}</div>
                        <div class="dropdown-user-role">{{ userRole }}</div>
                      </div>
                    </div>
                    
                    <div class="dropdown-divider"></div>
                    
                    <div class="dropdown-menu-items">
                      <button
                        v-for="(item, index) in userMenuItems"
                        :key="`user-menu-${index}`"
                        @click="handleUserMenuItemClick(item)"
                        class="dropdown-item"
                        :class="[
                          dropdownItemClasses,
                          { 'danger': item.danger }
                        ]"
                        :disabled="item.disabled"
                      >
                        <component 
                          :is="item.icon || 'div'"
                          class="dropdown-item-icon"
                          :class="item.iconClass"
                        />
                        <span class="dropdown-item-text">{{ item.title }}</span>
                      </button>
                    </div>
                  </div>
                </transition>
              </div>

              <!-- 登录注册按钮 -->
              <div v-else-if="showAuthButtons && !isAuthenticated" class="auth-buttons" :class="authButtonsClasses">
                <router-link
                  v-if="showLoginBtn"
                  :to="loginBtnTo"
                  class="auth-btn login-btn"
                  :class="authBtnClasses"
                >
                  {{ loginBtnText }}
                </router-link>
                
                <router-link
                  v-if="showRegisterBtn"
                  :to="registerBtnTo"
                  class="auth-btn register-btn"
                  :class="[authBtnClasses, registerBtnClasses]"
                >
                  {{ registerBtnText }}
                </router-link>
              </div>
            </div>
          </slot>
        </div>
      </div>
    </div>

    <!-- 移动端下拉菜单 -->
    <transition :name="mobileMenuAnimation">
      <div 
        v-if="isMobileMenuOpen && isMobile"
        class="mobile-menu"
        :class="mobileMenuClasses"
      >
        <div class="mobile-menu-container" :class="mobileMenuContainerClasses">
          <!-- 移动端搜索 -->
          <div v-if="showSearch && isMobile" class="mobile-search-section">
            <div class="mobile-search-container">
              <slot name="mobile-search">
                <div class="mobile-search-input-wrapper">
                  <svg class="mobile-search-icon" viewBox="0 0 24 24" fill="currentColor">
                    <path d="M9.5,3A6.5,6.5 0 0,1 16,9.5C16,11.11 15.41,12.59 14.44,13.73L14.71,14H15.5L20.5,19L19,20.5L14,15.5V14.71L13.73,14.44C12.59,15.41 11.11,16 9.5,16A6.5,6.5 0 0,1 3,9.5A6.5,6.5 0 0,1 9.5,3M9.5,5C7,5 5,7 5,9.5C5,12 7,14 9.5,14C12,14 14,12 14,9.5C14,7 12,5 9.5,5Z"/>
                  </svg>
                  <input
                    type="text"
                    :placeholder="searchPlaceholder"
                    class="mobile-search-input"
                    :value="searchQuery"
                    @input="handleSearchInput"
                    @keyup.enter="handleSearchSubmit"
                  />
                </div>
              </slot>
            </div>
          </div>

          <!-- 移动端导航 -->
          <nav class="mobile-nav" :aria-label="mobileNavAriaLabel">
            <ul class="mobile-nav-list">
              <li 
                v-for="(item, index) in navigationItems" 
                :key="`mobile-nav-${index}`"
                class="mobile-nav-item"
              >
                <component
                  :is="item.to ? 'router-link' : item.href ? 'a' : 'button'"
                  :to="item.to"
                  :href="item.href"
                  :target="item.target"
                  class="mobile-nav-link"
                  :class="[
                    mobileNavLinkClasses,
                    {
                      'active': isActive(item),
                      'disabled': item.disabled
                    }
                  ]"
                  @click="handleMobileNavClick(item, $event)"
                >
                  <component 
                    :is="item.icon || 'div'"
                    class="mobile-nav-icon"
                    :class="item.iconClass"
                  />
                  <span class="mobile-nav-text">{{ item.title }}</span>
                  
                  <div v-if="item.badge" class="mobile-nav-badge" :class="item.badgeClass">
                    {{ item.badge }}
                  </div>
                </component>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </transition>
  </header>
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
  permission?: string | string[]
  visible?: boolean
  onClick?: (item: NavigationItem, event: Event) => void
}

// 用户菜单项目接口
interface UserMenuItem {
  title: string
  icon?: string | object
  iconClass?: string
  danger?: boolean
  disabled?: boolean
  permission?: string | string[]
  onClick?: (item: UserMenuItem) => void
}

// Props 接口
interface Props {
  // 基础配置
  variant?: 'default' | 'minimal' | 'compact' | 'expanded' | 'overlay'
  sticky?: boolean
  transparent?: boolean
  shadow?: boolean
  
  // Logo配置
  showLogo?: boolean
  showLogoText?: boolean
  showLogoSubtitle?: boolean
  logoText?: string
  logoSubtitle?: string
  logoTo?: string
  logoHref?: string
  logoTarget?: '_blank' | '_self' | '_parent' | '_top'
  
  // 移动端配置
  mobileShowLogoText?: boolean
  showMobileMenuBtn?: boolean
  mobileMenuTooltip?: string
  
  // 侧边栏配置
  showSidebarToggle?: boolean
  sidebarToggleTooltip?: string
  
  // 导航配置
  showNavigation?: boolean
  navigationItems?: NavigationItem[]
  showNavIndicator?: boolean
  navAriaLabel?: string
  
  // 搜索配置
  showSearch?: boolean
  searchPlaceholder?: string
  searchQuery?: string
  searchClearTooltip?: string
  
  // 操作配置
  showActions?: boolean
  showNotifications?: boolean
  showThemeToggle?: boolean
  showSettings?: boolean
  showUserMenu?: boolean
  showAuthButtons?: boolean
  showLoginBtn?: boolean
  showRegisterBtn?: boolean
  
  // 通知配置
  notificationCount?: number
  notificationsTooltip?: string
  
  // 主题配置
  themeToggleTooltip?: string
  settingsTooltip?: string
  
  // 用户菜单配置
  userName?: string
  userRole?: string
  userAvatar?: string
  userMenuTooltip?: string
  userMenuItems?: UserMenuItem[]
  
  // 认证按钮配置
  loginBtnText?: string
  loginBtnTo?: string
  registerBtnText?: string
  registerBtnTo?: string
  
  // 移动端配置
  mobileNavAriaLabel?: string
  
  // 背景装饰配置
  showBackgroundDecoration?: boolean
  
  // 响应式配置
  responsive?: boolean
  mobileBreakpoint?: 'xs' | 'sm' | 'md' | 'lg'
  
  // 动画配置
  animation?: 'none' | 'fade' | 'slide' | 'scale' | 'bounce'
  mobileMenuAnimation?: 'slide' | 'fade' | 'scale'
  mobileMenuIconAnimation?: 'rotate' | 'fade' | 'scale'
  sidebarToggleIconAnimation?: 'rotate' | 'fade' | 'scale'
  logoTextAnimation?: 'fade' | 'slide' | 'scale'
  navIndicatorAnimation?: 'fade' | 'slide' | 'scale'
  navBadgeAnimation?: 'fade' | 'slide' | 'scale'
  notificationBadgeAnimation?: 'fade' | 'slide' | 'scale'
  themeToggleIconAnimation?: 'rotate' | 'fade' | 'scale'
  userMenuDropdownAnimation?: 'fade' | 'slide' | 'scale'
  
  // 主题配置
  theme?: 'light' | 'dark' | 'auto'
  
  // 无障碍配置
  ariaLabel?: string
}

// Emits 接口
interface Emits {
  (e: 'mobile-menu-toggle', open: boolean): void
  (e: 'sidebar-toggle', open: boolean): void
  (e: 'nav-click', item: NavigationItem, event: Event): void
  (e: 'search', query: string): void
  (e: 'search-submit', query: string): void
  (e: 'search-clear'): void
  (e: 'notifications-click'): void
  (e: 'theme-toggle'): void
  (e: 'settings-click'): void
  (e: 'user-menu-toggle', open: boolean): void
  (e: 'user-menu-item-click', item: UserMenuItem): void
}

const props = withDefaults(defineProps<Props>(), {
  // 基础配置
  variant: 'default',
  sticky: true,
  transparent: false,
  shadow: true,
  
  // Logo配置
  showLogo: true,
  showLogoText: true,
  showLogoSubtitle: false,
  logoText: 'SeekCode',
  logoSubtitle: '代码片段管理',
  
  // 移动端配置
  mobileShowLogoText: false,
  showMobileMenuBtn: true,
  mobileMenuTooltip: '切换菜单',
  
  // 侧边栏配置
  showSidebarToggle: false,
  sidebarToggleTooltip: '切换侧边栏',
  
  // 导航配置
  showNavigation: true,
  showNavIndicator: true,
  navAriaLabel: '主导航',
  
  // 搜索配置
  showSearch: false,
  searchPlaceholder: '搜索...',
  searchQuery: '',
  searchClearTooltip: '清除搜索',
  
  // 操作配置
  showActions: true,
  showNotifications: false,
  showThemeToggle: true,
  showSettings: true,
  showUserMenu: true,
  showAuthButtons: true,
  showLoginBtn: true,
  showRegisterBtn: true,
  
  // 通知配置
  notificationCount: 0,
  notificationsTooltip: '通知',
  
  // 主题配置
  themeToggleTooltip: '切换主题',
  settingsTooltip: '设置',
  
  // 用户菜单配置
  userMenuTooltip: '用户菜单',
  
  // 认证按钮配置
  loginBtnText: '登录',
  loginBtnTo: '/login',
  registerBtnText: '注册',
  registerBtnTo: '/register',
  
  // 移动端配置
  mobileNavAriaLabel: '移动端导航',
  
  // 背景装饰配置
  showBackgroundDecoration: false,
  
  // 响应式配置
  responsive: true,
  mobileBreakpoint: 'md',
  
  // 动画配置
  animation: 'fade',
  mobileMenuAnimation: 'slide',
  mobileMenuIconAnimation: 'rotate',
  sidebarToggleIconAnimation: 'rotate',
  logoTextAnimation: 'fade',
  navIndicatorAnimation: 'fade',
  navBadgeAnimation: 'fade',
  notificationBadgeAnimation: 'fade',
  themeToggleIconAnimation: 'rotate',
  userMenuDropdownAnimation: 'fade',
  
  // 主题配置
  theme: 'auto',
  
  // 无障碍配置
  ariaLabel: '应用头部'
})

const emit = defineEmits<Emits>()

defineSlots<{
  'header-start'(): any
  'header-center'(): any
  'header-end'(): any
  'logo-icon'(): any
  'logo-text'(): any
  'logo-subtitle'(): any
  'search'(): any
  'mobile-search'(): any
}>()

// 组合式 API
const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const breakpoints = useBreakpoints()

// 响应式状态
const isMobileMenuOpen = ref(false)
const isUserMenuOpen = ref(false)
const isSidebarOpen = ref(true)
const currentTheme = ref('light')

// 计算属性
const isMobile = computed(() => {
  if (!props.responsive) return false
  return breakpoints.isLessThan(props.mobileBreakpoint)
})

const isAuthenticated = computed(() => authStore.isAuthenticated)

// 默认导航配置
const defaultNavigationItems: NavigationItem[] = [
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

// 过滤后的导航配置
const navigationItems = computed(() => {
  const items = props.navigationItems || defaultNavigationItems
  return items.filter(item => shouldShowNavItem(item))
})

// 默认用户菜单配置
const defaultUserMenuItems: UserMenuItem[] = [
  {
    title: '个人资料',
    icon: 'UserIcon',
    onClick: () => router.push('/profile')
  },
  {
    title: '账户设置',
    icon: 'SettingsIcon',
    onClick: () => router.push('/settings')
  },
  {
    title: '退出登录',
    icon: 'LogoutIcon',
    danger: true,
    onClick: () => handleLogout()
  }
]

// 用户菜单配置
const userMenuItems = computed(() => {
  return props.userMenuItems || defaultUserMenuItems
})

// 用户信息计算
const userName = computed(() => props.userName || authStore.user?.username || '用户')
const userRole = computed(() => props.userRole || getUserRoleText())
const userAvatar = computed(() => props.userAvatar || authStore.user?.avatar)
const userInitials = computed(() => {
  const name = userName.value
  return name.charAt(0).toUpperCase()
})

// 样式类计算
const headerClasses = computed(() => [
  `header-${props.variant}`,
  `animation-${props.animation}`,
  `theme-${props.theme}`,
  {
    'sticky': props.sticky,
    'transparent': props.transparent,
    'shadow': props.shadow,
    'mobile': isMobile.value,
    'mobile-menu-open': isMobileMenuOpen.value,
    'has-decoration': props.showBackgroundDecoration
  }
])

const headerStyles = computed(() => ({
  '--header-height': getHeaderHeight(),
  '--animation-duration': getAnimationDuration()
}))

const containerClasses = computed(() => [
  `container-${props.variant}`
])

const contentClasses = computed(() => [
  `content-${props.variant}`
])

const startClasses = computed(() => [
  `start-${props.variant}`
])

const centerClasses = computed(() => [
  `center-${props.variant}`
])

const endClasses = computed(() => [
  `end-${props.variant}`
])

const decorationClasses = computed(() => [
  `decoration-${props.variant}`
])

const logoSectionClasses = computed(() => [
  `logo-section-${props.variant}`
])

const logoLinkClasses = computed(() => [
  `logo-link-${props.variant}`
])

const logoContainerClasses = computed(() => [
  `logo-container-${props.variant}`
])

const logoIconClasses = computed(() => [
  `logo-icon-${props.variant}`
])

const logoTextClasses = computed(() => [
  `logo-text-${props.variant}`
])

const logoSubtitleClasses = computed(() => [
  `logo-subtitle-${props.variant}`
])

const mobileMenuBtnClasses = computed(() => [
  `mobile-menu-btn-${props.variant}`
])

const sidebarToggleBtnClasses = computed(() => [
  `sidebar-toggle-btn-${props.variant}`
])

const navMenuClasses = computed(() => [
  `nav-menu-${props.variant}`
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

const searchSectionClasses = computed(() => [
  `search-section-${props.variant}`
])

const searchContainerClasses = computed(() => [
  `search-container-${props.variant}`
])

const searchInputClasses = computed(() => [
  `search-input-${props.variant}`
])

const searchClearBtnClasses = computed(() => [
  `search-clear-btn-${props.variant}`
])

const actionsSectionClasses = computed(() => [
  `actions-section-${props.variant}`
])

const actionBtnClasses = computed(() => [
  `action-btn-${props.variant}`
])

const notificationBadgeClasses = computed(() => [
  `notification-badge-${props.variant}`
])

const userMenuSectionClasses = computed(() => [
  `user-menu-section-${props.variant}`
])

const userMenuBtnClasses = computed(() => [
  `user-menu-btn-${props.variant}`
])

const userAvatarClasses = computed(() => [
  `user-avatar-${props.variant}`
])

const userInfoClasses = computed(() => [
  `user-info-${props.variant}`
])

const userNameClasses = computed(() => [
  `user-name-${props.variant}`
])

const userRoleClasses = computed(() => [
  `user-role-${props.variant}`
])

const userMenuDropdownClasses = computed(() => [
  `user-menu-dropdown-${props.variant}`
])

const dropdownHeaderClasses = computed(() => [
  `dropdown-header-${props.variant}`
])

const dropdownItemClasses = computed(() => [
  `dropdown-item-${props.variant}`
])

const authButtonsClasses = computed(() => [
  `auth-buttons-${props.variant}`
])

const registerBtnClasses = computed(() => [
  `register-btn-${props.variant}`
])

const mobileMenuClasses = computed(() => [
  `mobile-menu-${props.variant}`
])

const mobileMenuContainerClasses = computed(() => [
  `mobile-menu-container-${props.variant}`
])

const mobileNavLinkClasses = computed(() => [
  `mobile-nav-link-${props.variant}`
])

// 工具方法
function getHeaderHeight(): string {
  switch (props.variant) {
    case 'minimal': return '48px'
    case 'compact': return '56px'
    case 'expanded': return '72px'
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

function shouldShowNavItem(item: NavigationItem): boolean {
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

// 事件处理函数
function handleMobileMenuToggle() {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
  emit('mobile-menu-toggle', isMobileMenuOpen.value)
}

function handleSidebarToggle() {
  isSidebarOpen.value = !isSidebarOpen.value
  emit('sidebar-toggle', isSidebarOpen.value)
}

function handleNavClick(item: NavigationItem, event: Event) {
  emit('nav-click', item, event)
  
  if (item.onClick) {
    item.onClick(item, event)
  }
  
  // 移动端点击导航项后关闭菜单
  if (isMobile.value) {
    isMobileMenuOpen.value = false
  }
}

function handleMobileNavClick(item: NavigationItem, event: Event) {
  handleNavClick(item, event)
}

function handleSearchInput(event: Event) {
  const query = (event.target as HTMLInputElement).value
  emit('search', query)
}

function handleSearchSubmit(event: Event) {
  const query = (event.target as HTMLInputElement).value
  emit('search-submit', query)
}

function handleSearchClear() {
  emit('search-clear')
}

function handleNotificationsClick() {
  emit('notifications-click')
}

function handleThemeToggle() {
  // 切换主题逻辑
  const html = document.documentElement
  if (html.classList.contains('dark')) {
    html.classList.remove('dark')
    localStorage.setItem('theme', 'light')
    currentTheme.value = 'light'
  } else {
    html.classList.add('dark')
    localStorage.setItem('theme', 'dark')
    currentTheme.value = 'dark'
  }
  emit('theme-toggle')
}

function handleSettingsClick() {
  emit('settings-click')
  router.push('/settings')
}

function handleUserMenuToggle() {
  isUserMenuOpen.value = !isUserMenuOpen.value
  emit('user-menu-toggle', isUserMenuOpen.value)
}

function handleUserMenuItemClick(item: UserMenuItem) {
  emit('user-menu-item-click', item)
  
  if (item.onClick) {
    item.onClick(item)
  }
  
  isUserMenuOpen.value = false
}

async function handleLogout() {
  try {
    await authStore.logout()
    router.push('/login')
  } catch (error) {
    console.error('登出失败:', error)
  }
}

// 监听器
watch(() => props.theme, (newTheme) => {
  if (newTheme === 'auto') {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    currentTheme.value = prefersDark ? 'dark' : 'light'
  } else {
    currentTheme.value = newTheme
  }
})

// 点击外部关闭用户菜单
function handleClickOutside(event: MouseEvent) {
  const target = event.target as Element
  const userMenuSection = document.querySelector('.user-menu-section')
  
  if (userMenuSection && !userMenuSection.contains(target)) {
    isUserMenuOpen.value = false
  }
}

// 生命周期钩子
onMounted(() => {
  // 初始化主题
  const savedTheme = localStorage.getItem('theme')
  if (savedTheme) {
    currentTheme.value = savedTheme
  } else if (props.theme === 'auto') {
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches
    currentTheme.value = prefersDark ? 'dark' : 'light'
  } else {
    currentTheme.value = props.theme
  }

  // 监听系统主题变化
  const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
  const handleThemeChange = (e: MediaQueryListEvent) => {
    if (props.theme === 'auto') {
      currentTheme.value = e.matches ? 'dark' : 'light'
    }
  }

  mediaQuery.addEventListener('change', handleThemeChange)

  // 添加点击外部监听器
  document.addEventListener('click', handleClickOutside)

  // 清理函数
  onUnmounted(() => {
    mediaQuery.removeEventListener('change', handleThemeChange)
    document.removeEventListener('click', handleClickOutside)
  })
})
</script>

<style scoped>
/* 基础样式 */
.enhanced-app-header {
  @apply relative;
  height: var(--header-height);
  background: var(--gradient-surface);
  border-bottom: 1px solid rgba(0, 0, 0, 0.06);
  transition: all var(--animation-duration) var(--animation-smooth);
  z-index: var(--z-fixed-header);
}

.enhanced-app-header.sticky {
  @apply sticky top-0;
}

.enhanced-app-header.transparent {
  background: transparent;
  border-color: transparent;
}

.enhanced-app-header.shadow {
  box-shadow: var(--shadow-md);
}

.enhanced-app-header.overlay {
  background: var(--glass-bg);
  backdrop-filter: var(--glass-backdrop);
  border: 1px solid var(--glass-border);
}

/* 变体样式 */
.header-default {
  /* 默认样式 */
}

.header-minimal {
  background: white;
  border-color: rgba(0, 0, 0, 0.1);
}

.header-compact {
  background: linear-gradient(180deg, #ffffff 0%, #f8f9fa 100%);
}

.header-expanded {
  background: linear-gradient(180deg, var(--gradient-primary) 0%, var(--primary-600) 100%);
  color: white;
}

.header-expanded * {
  color: white;
}

/* 动画样式 */
.animation-none {
  transition: none;
}

.animation-fade {
  transition: opacity var(--animation-duration) var(--animation-smooth);
}

.animation-slide {
  transition: transform var(--animation-duration) var(--animation-smooth);
}

.animation-scale {
  transition: transform var(--animation-duration) var(--animation-elastic);
}

.animation-bounce {
  transition: all var(--animation-duration) var(--animation-bounce);
}

/* 背景装饰 */
.header-decoration {
  @apply absolute inset-0 pointer-events-none;
  overflow: hidden;
}

.decoration-pattern {
  @apply absolute inset-0;
  background-image: 
    radial-gradient(circle at 20% 50%, rgba(59, 130, 246, 0.1) 0%, transparent 50%),
    radial-gradient(circle at 80% 80%, rgba(139, 92, 246, 0.1) 0%, transparent 50%);
}

/* 容器样式 */
.header-container {
  @apply relative h-full mx-auto px-4 sm:px-6 lg:px-8;
  max-width: var(--max-w-7xl);
}

.header-content {
  @apply flex items-center justify-between h-full;
}

/* 区域样式 */
.header-start,
.header-center,
.header-end {
  @apply flex items-center;
}

.header-start {
  @apply flex-1;
}

.header-center {
  @apply flex-1 justify-center;
}

.header-end {
  @apply flex-1 justify-end;
}

/* Logo样式 */
.logo-section {
  @apply flex items-center;
}

.logo-link {
  @apply flex items-center text-decoration-none;
  color: inherit;
}

.logo-container {
  @apply flex items-center gap-2;
}

.logo-icon {
  @apply w-8 h-8;
  color: var(--primary-500);
}

.logo-text-container {
  @apply flex flex-col;
}

.logo-text {
  @apply font-semibold text-lg;
}

.logo-subtitle {
  @apply text-xs opacity-70;
}

/* 移动端菜单按钮 */
.mobile-menu-btn {
  @apply p-2 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
}

.mobile-menu-btn:hover {
  background: rgba(0, 0, 0, 0.05);
}

.menu-icon {
  @apply w-6 h-6;
}

/* 侧边栏切换按钮 */
.sidebar-toggle-btn {
  @apply p-2 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
}

.sidebar-toggle-btn:hover {
  background: rgba(0, 0, 0, 0.05);
}

.toggle-icon {
  @apply w-6 h-6;
}

/* 导航样式 */
.nav-menu {
  @apply hidden md:flex items-center space-x-1;
}

.nav-list {
  @apply flex items-center space-x-1;
}

.nav-item {
  @apply relative;
}

.nav-link {
  @apply px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200;
  color: inherit;
  text-decoration: none;
  position: relative;
}

.nav-link:hover {
  background: rgba(0, 0, 0, 0.05);
}

.nav-link.active {
  background: rgba(59, 130, 246, 0.1);
  color: var(--primary-600);
}

.nav-indicator {
  @apply absolute bottom-0 left-1/2 transform -translate-x-1/2;
  width: 20px;
  height: 2px;
  background: var(--primary-500);
  border-radius: var(--radius-full);
}

.nav-badge {
  @apply absolute -top-1 -right-1 px-1.5 py-0.5 text-xs font-semibold rounded-full;
  background: var(--error-500);
  color: white;
  min-width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 搜索样式 */
.search-section {
  @apply hidden md:block;
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
  @apply w-full pl-10 pr-8 py-2 rounded-lg border;
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

.search-clear-btn {
  @apply absolute right-2 p-1 rounded transition-all duration-200;
  background: transparent;
  border: none;
  color: var(--gray-400);
  cursor: pointer;
}

.search-clear-btn:hover {
  background: var(--gray-100);
  color: var(--gray-600);
}

.clear-icon {
  @apply w-4 h-4;
}

/* 操作按钮样式 */
.actions-section {
  @apply flex items-center space-x-2;
}

.action-btn {
  @apply p-2 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
  position: relative;
}

.action-btn:hover {
  background: rgba(0, 0, 0, 0.05);
}

.action-icon {
  @apply w-5 h-5;
}

.notification-badge {
  @apply absolute -top-1 -right-1 px-1.5 py-0.5 text-xs font-semibold rounded-full;
  background: var(--error-500);
  color: white;
  min-width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
}

/* 用户菜单样式 */
.user-menu-section {
  @apply relative;
}

.user-menu-btn {
  @apply flex items-center gap-2 p-2 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: inherit;
  cursor: pointer;
}

.user-menu-btn:hover {
  background: rgba(0, 0, 0, 0.05);
}

.user-avatar {
  @apply w-8 h-8 rounded-full overflow-hidden;
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

.user-info {
  @apply hidden md:block text-left;
}

.user-name {
  @apply text-sm font-medium;
}

.user-role {
  @apply text-xs opacity-70;
}

.user-menu-arrow {
  @apply w-4 h-4 transition-transform duration-200;
}

.user-menu-btn[aria-expanded="true"] .user-menu-arrow {
  transform: rotate(180deg);
}

/* 用户下拉菜单 */
.user-menu-dropdown {
  @apply absolute right-0 mt-2 w-56 rounded-lg shadow-lg border;
  background: white;
  border-color: var(--gray-200);
  z-index: var(--z-dropdown);
}

.dropdown-header {
  @apply p-4 border-b;
  border-color: var(--gray-200);
}

.dropdown-user-info {
  @apply text-center;
}

.dropdown-user-name {
  @apply font-medium text-gray-900;
}

.dropdown-user-role {
  @apply text-sm text-gray-500;
}

.dropdown-divider {
  @apply border-t;
  border-color: var(--gray-200);
}

.dropdown-menu-items {
  @apply py-1;
}

.dropdown-item {
  @apply w-full flex items-center gap-3 px-4 py-2 text-left transition-all duration-200;
  background: transparent;
  border: none;
  color: var(--gray-700);
  cursor: pointer;
}

.dropdown-item:hover {
  background: var(--gray-50);
  color: var(--gray-900);
}

.dropdown-item.danger {
  color: var(--error-600);
}

.dropdown-item.danger:hover {
  background: var(--error-50);
  color: var(--error-700);
}

.dropdown-item-icon {
  @apply w-4 h-4;
}

.dropdown-item-text {
  @apply flex-1;
}

/* 认证按钮样式 */
.auth-buttons {
  @apply flex items-center space-x-2;
}

.auth-btn {
  @apply px-4 py-2 rounded-lg text-sm font-medium transition-all duration-200;
  text-decoration: none;
}

.login-btn {
  background: var(--gray-100);
  color: var(--gray-700);
  border: 1px solid var(--gray-200);
}

.login-btn:hover {
  background: var(--gray-200);
  color: var(--gray-800);
}

.register-btn {
  background: var(--gradient-primary);
  color: white;
  border: 1px solid transparent;
}

.register-btn:hover {
  background: var(--gradient-primary-dark);
  transform: translateY(-1px);
  box-shadow: var(--shadow-primary);
}

/* 移动端菜单 */
.mobile-menu {
  @apply absolute top-full left-0 right-0 bg-white border-b;
  border-color: rgba(0, 0, 0, 0.06);
  z-index: var(--z-dropdown);
}

.mobile-menu-container {
  @apply p-4 space-y-4;
}

.mobile-search-section {
  @apply block md:hidden;
}

.mobile-search-container {
  @apply relative;
}

.mobile-search-input-wrapper {
  @apply relative flex items-center;
}

.mobile-search-icon {
  @apply absolute left-3 w-5 h-5;
  color: var(--gray-400);
}

.mobile-search-input {
  @apply w-full pl-10 pr-4 py-2 rounded-lg border;
  background: var(--gray-50);
  border-color: var(--gray-200);
  color: var(--gray-800);
}

.mobile-nav {
  @apply block md:hidden;
}

.mobile-nav-list {
  @apply space-y-1;
}

.mobile-nav-item {
  @apply relative;
}

.mobile-nav-link {
  @apply flex items-center gap-3 px-4 py-3 rounded-lg transition-all duration-200;
  background: transparent;
  border: none;
  color: var(--gray-700);
  text-decoration: none;
  text-align: left;
  width: 100%;
}

.mobile-nav-link:hover {
  background: var(--gray-50);
  color: var(--gray-900);
}

.mobile-nav-link.active {
  background: var(--primary-50);
  color: var(--primary-600);
}

.mobile-nav-icon {
  @apply w-5 h-5;
}

.mobile-nav-text {
  @apply flex-1;
}

.mobile-nav-badge {
  @apply px-2 py-1 text-xs font-semibold rounded-full;
  background: var(--error-500);
  color: white;
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
  transform: translateY(-10px);
}

.slide-leave-to {
  opacity: 0;
  transform: translateY(-10px);
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
@media (max-width: 768px) {
  .header-container {
    @apply px-4;
  }

  .header-center {
    @apply hidden;
  }

  .header-start {
    @apply flex-none;
  }

  .header-end {
    @apply flex-none;
  }

  .nav-menu {
    @apply hidden;
  }

  .search-section {
    @apply hidden;
  }

  .user-info {
    @apply hidden;
  }
}

@media (max-width: 480px) {
  .enhanced-app-header {
    height: 56px;
  }

  .header-container {
    @apply px-3;
  }

  .logo-text {
    @apply hidden;
  }

  .actions-section {
    @apply space-x-1;
  }

  .action-btn {
    @apply p-1.5;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .enhanced-app-header,
  .mobile-menu-btn,
  .sidebar-toggle-btn,
  .nav-link,
  .action-btn,
  .user-menu-btn,
  .auth-btn,
  .mobile-nav-link,
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
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .enhanced-app-header {
    border-width: 2px;
  }

  .nav-link,
  .action-btn,
  .user-menu-btn,
  .auth-btn,
  .mobile-nav-link {
    border-width: 1px;
  }
}

/* 焦点样式 */
.mobile-menu-btn:focus-visible,
.sidebar-toggle-btn:focus-visible,
.nav-link:focus-visible,
.action-btn:focus-visible,
.user-menu-btn:focus-visible,
.auth-btn:focus-visible,
.mobile-nav-link:focus-visible,
.search-input:focus-visible {
  @apply outline-none ring-2 ring-primary-500 ring-offset-2;
}

/* 深色模式适配 */
@media (prefers-color-scheme: dark) {
  .theme-auto {
    background: linear-gradient(180deg, #1a1a1a 0%, #2d2d2d 100%);
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
    background: rgba(59, 130, 246, 0.2);
    color: var(--primary-400);
  }
  
  .theme-auto .search-input {
    background: rgba(255, 255, 255, 0.1);
    border-color: rgba(255, 255, 255, 0.2);
    color: var(--gray-200);
  }
  
  .theme-auto .search-input:focus {
    background: rgba(255, 255, 255, 0.15);
  }
  
  .theme-auto .user-menu-dropdown {
    background: #2d2d2d;
    border-color: rgba(255, 255, 255, 0.2);
  }
  
  .theme-auto .dropdown-header {
    border-color: rgba(255, 255, 255, 0.2);
  }
  
  .theme-auto .dropdown-divider {
    border-color: rgba(255, 255, 255, 0.2);
  }
  
  .theme-auto .dropdown-item {
    color: var(--gray-300);
  }
  
  .theme-auto .dropdown-item:hover {
    background: rgba(255, 255, 255, 0.1);
    color: var(--gray-200);
  }
  
  .theme-auto .mobile-menu {
    background: #2d2d2d;
    border-color: rgba(255, 255, 255, 0.2);
  }
  
  .theme-auto .mobile-nav-link {
    color: var(--gray-300);
  }
  
  .theme-auto .mobile-nav-link:hover {
    background: rgba(255, 255, 255, 0.1);
    color: var(--gray-200);
  }
  
  .theme-auto .mobile-nav-link.active {
    background: rgba(59, 130, 246, 0.2);
    color: var(--primary-400);
  }
}

/* 打印样式 */
@media print {
  .enhanced-app-header {
    @apply hidden;
  }
}
</style>