<template>
  <footer class="app-footer">
    <div class="footer-container">
      <!-- 左侧信息 -->
      <div class="footer-left">
        <div class="footer-brand">
          <svg class="brand-icon" viewBox="0 0 24 24" fill="currentColor">
            <path d="M9.4 16.6L4.8 12l4.6-4.6L8 6l-6 6 6 6 1.4-1.4zm5.2 0L19.2 12l-4.6-4.6L16 6l6 6-6 6-1.4-1.4z"/>
          </svg>
          <span class="brand-text">代码片段管理工具</span>
        </div>
        <p class="footer-description">
          高效的团队代码片段管理和分享平台
        </p>
      </div>

      <!-- 中间链接 -->
      <div class="footer-center">
        <div class="footer-links">
          <div class="link-group">
            <h4 class="link-title">功能</h4>
            <ul class="link-list">
              <li><router-link to="/snippets" class="footer-link">代码片段</router-link></li>
              <li><router-link to="/snippets/create" class="footer-link" v-if="canCreateSnippet">创建片段</router-link></li>
              <li><router-link to="/clipboard-history" class="footer-link">剪贴板历史</router-link></li>
            </ul>
          </div>

          <div class="link-group" v-if="isAdmin">
            <h4 class="link-title">管理</h4>
            <ul class="link-list">
              <li><router-link to="/admin/users" class="footer-link">用户管理</router-link></li>
              <li><router-link to="/admin/tags" class="footer-link">标签管理</router-link></li>
            </ul>
          </div>

          <div class="link-group">
            <h4 class="link-title">帮助</h4>
            <ul class="link-list">
              <li><a href="#" class="footer-link" @click.prevent="showHelp">使用指南</a></li>
              <li><a href="#" class="footer-link" @click.prevent="showAbout">关于我们</a></li>
              <li><a href="#" class="footer-link" @click.prevent="showContact">联系支持</a></li>
            </ul>
          </div>
        </div>
      </div>

      <!-- 右侧统计信息 -->
      <div class="footer-right">
        <div class="stats-info" v-if="stats">
          <div class="stat-item">
            <span class="stat-number">{{ stats.totalSnippets }}</span>
            <span class="stat-label">代码片段</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">{{ stats.totalUsers }}</span>
            <span class="stat-label">用户</span>
          </div>
          <div class="stat-item">
            <span class="stat-number">{{ stats.totalCopies }}</span>
            <span class="stat-label">复制次数</span>
          </div>
        </div>
      </div>
    </div>

    <!-- 底部版权信息 -->
    <div class="footer-bottom">
      <div class="footer-container">
        <div class="copyright">
          <p>&copy; {{ currentYear }} 代码片段管理工具. 保留所有权利.</p>
        </div>
        <div class="footer-meta">
          <span class="version">版本 {{ version }}</span>
          <span class="separator">|</span>
          <span class="build-info">构建时间: {{ buildTime }}</span>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const authStore = useAuthStore()

// 响应式数据
const stats = ref<{
  totalSnippets: number
  totalUsers: number
  totalCopies: number
} | null>(null)

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
 * 获取当前年份
 */
const currentYear = computed(() => {
  return new Date().getFullYear()
})

/**
 * 获取应用版本
 */
const version = computed(() => {
  return import.meta.env.VITE_APP_VERSION || '1.0.0'
})

/**
 * 获取构建时间
 */
const buildTime = computed(() => {
  return import.meta.env.VITE_BUILD_TIME || new Date().toLocaleDateString('zh-CN')
})

/**
 * 加载统计信息
 */
async function loadStats() {
  try {
    // 这里应该调用 API 获取统计信息
    // const response = await api.get('/api/stats')
    // stats.value = response.data

    // 临时模拟数据
    stats.value = {
      totalSnippets: 1234,
      totalUsers: 56,
      totalCopies: 7890
    }
  } catch (error) {
    console.error('加载统计信息失败:', error)
  }
}

/**
 * 显示帮助信息
 */
function showHelp() {
  // 这里可以打开帮助模态框或跳转到帮助页面
  console.log('显示帮助信息')
}

/**
 * 显示关于信息
 */
function showAbout() {
  // 这里可以打开关于模态框
  console.log('显示关于信息')
}

/**
 * 显示联系信息
 */
function showContact() {
  // 这里可以打开联系模态框
  console.log('显示联系信息')
}

// 组件挂载时加载统计信息
onMounted(() => {
  if (authStore.isAuthenticated) {
    loadStats()
  }
})
</script>

<style scoped>
.app-footer {
  background-color: #343a40;
  color: #adb5bd;
  margin-top: auto;
}

.footer-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 1rem;
}

/* 主要内容区域 */
.footer-container:first-child {
  display: grid;
  grid-template-columns: 1fr 2fr 1fr;
  gap: 2rem;
  padding: 2rem 1rem;
}

/* 左侧品牌信息 */
.footer-left {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.footer-brand {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: white;
  font-size: 1.125rem;
  font-weight: 600;
}

.brand-icon {
  width: 24px;
  height: 24px;
}

.footer-description {
  margin: 0;
  font-size: 0.875rem;
  line-height: 1.5;
  color: #6c757d;
}

/* 中间链接区域 */
.footer-center {
  display: flex;
  justify-content: center;
}

.footer-links {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: 2rem;
  width: 100%;
  max-width: 400px;
}

.link-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.link-title {
  color: white;
  font-size: 0.875rem;
  font-weight: 600;
  margin: 0;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.link-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.footer-link {
  color: #adb5bd;
  text-decoration: none;
  font-size: 0.875rem;
  transition: color 0.3s ease;
}

.footer-link:hover {
  color: white;
}

/* 右侧统计信息 */
.footer-right {
  display: flex;
  justify-content: flex-end;
}

.stats-info {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  text-align: right;
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.stat-number {
  color: white;
  font-size: 1.5rem;
  font-weight: 700;
  line-height: 1;
}

.stat-label {
  color: #6c757d;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

/* 底部版权区域 */
.footer-bottom {
  border-top: 1px solid #495057;
  background-color: #212529;
}

.footer-bottom .footer-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
}

.copyright p {
  margin: 0;
  font-size: 0.875rem;
  color: #6c757d;
}

.footer-meta {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.75rem;
  color: #6c757d;
}

.separator {
  color: #495057;
}

.version,
.build-info {
  white-space: nowrap;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .footer-container:first-child {
    grid-template-columns: 1fr;
    gap: 2rem;
    text-align: center;
  }

  .footer-right {
    justify-content: center;
  }

  .stats-info {
    flex-direction: row;
    justify-content: center;
    text-align: center;
  }

  .stat-item {
    text-align: center;
  }
}

@media (max-width: 768px) {
  .footer-container:first-child {
    padding: 1.5rem 1rem;
  }

  .footer-links {
    grid-template-columns: repeat(2, 1fr);
    gap: 1.5rem;
  }

  .footer-bottom .footer-container {
    flex-direction: column;
    gap: 0.5rem;
    text-align: center;
  }

  .footer-meta {
    flex-wrap: wrap;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .footer-links {
    grid-template-columns: 1fr;
    gap: 1rem;
  }

  .stats-info {
    flex-direction: column;
    gap: 0.75rem;
  }

  .stat-number {
    font-size: 1.25rem;
  }

  .brand-text {
    display: none;
  }
}
</style>
