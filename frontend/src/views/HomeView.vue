<template>
  <div class="home">
    <h1>代码片段管理工具</h1>
    <p>欢迎使用代码片段管理工具！一个支持团队协作的代码片段存储、管理和分享平台。</p>

    <!-- 未登录用户显示的内容 -->
    <div v-if="!user" class="guest-actions">
      <div class="feature-highlights">
        <h2>主要功能</h2>
        <ul>
          <li>📝 存储和管理代码片段</li>
          <li>🔍 强大的搜索和筛选功能</li>
          <li>🏷️ 标签分类系统</li>
          <li>👥 团队协作和分享</li>
          <li>📋 一键复制到剪贴板</li>
          <li>📈 版本控制和历史记录</li>
        </ul>
      </div>

      <div class="auth-actions">
        <router-link to="/login" class="btn btn-primary">
          立即登录
        </router-link>
        <router-link to="/register" class="btn btn-secondary">
          免费注册
        </router-link>
      </div>

      <div class="guest-browse">
        <router-link to="/snippets" class="btn btn-outline">
          浏览公开代码片段
        </router-link>
      </div>
    </div>

    <!-- 已登录用户显示的内容 -->
    <div v-else class="user-content">
      <div class="stats">
        <h2>欢迎回来，{{ user.username }}！</h2>
        <p>角色：{{ getRoleName(user.role) }}</p>
      </div>

      <div class="quick-actions">
        <router-link to="/snippets" class="btn btn-primary">
          浏览代码片段
        </router-link>
        <router-link
          v-if="canCreateSnippet"
          to="/snippets/create"
          class="btn btn-secondary"
        >
          创建新片段
        </router-link>
        <router-link to="/clipboard/history" class="btn btn-outline">
          剪贴板历史
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const authStore = useAuthStore()
const user = computed(() => authStore.user)

/**
 * 获取用户角色的中文名称
 * @param role 用户角色枚举
 * @returns 角色的中文名称
 */
function getRoleName(role: UserRole): string {
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

/**
 * 检查当前用户是否可以创建代码片段
 */
const canCreateSnippet = computed(() => {
  if (!user.value) return false
  return user.value.role === UserRole.Admin || user.value.role === UserRole.Editor
})
</script>

<style scoped>
.home {
  text-align: center;
  padding: 2rem;
  max-width: 800px;
  margin: 0 auto;
}

.guest-actions {
  margin-top: 2rem;
}

.feature-highlights {
  margin: 2rem 0;
  padding: 1.5rem;
  background-color: #f8f9fa;
  border-radius: 8px;
  text-align: left;
}

.feature-highlights h2 {
  text-align: center;
  margin-bottom: 1rem;
  color: #333;
}

.feature-highlights ul {
  list-style: none;
  padding: 0;
  max-width: 400px;
  margin: 0 auto;
}

.feature-highlights li {
  padding: 0.5rem 0;
  font-size: 1.1rem;
  color: #555;
}

.auth-actions {
  margin: 2rem 0;
}

.guest-browse {
  margin-top: 1.5rem;
  padding-top: 1.5rem;
  border-top: 1px solid #e9ecef;
}

.user-content {
  margin-top: 2rem;
}

.quick-actions {
  margin: 2rem 0;
}

.btn {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  margin: 0 0.5rem;
  text-decoration: none;
  border-radius: 6px;
  font-weight: 500;
  transition: all 0.3s ease;
  border: 2px solid transparent;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover {
  background-color: #0056b3;
  transform: translateY(-1px);
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background-color: #545b62;
  transform: translateY(-1px);
}

.btn-outline {
  background-color: transparent;
  color: #007bff;
  border-color: #007bff;
}

.btn-outline:hover {
  background-color: #007bff;
  color: white;
  transform: translateY(-1px);
}

.stats {
  margin: 2rem 0;
  padding: 1.5rem;
  background-color: #f8f9fa;
  border-radius: 8px;
  border-left: 4px solid #007bff;
}

.stats h2 {
  margin-bottom: 0.5rem;
  color: #333;
}

.stats p {
  margin: 0;
  color: #666;
  font-size: 1.1rem;
}

@media (max-width: 768px) {
  .home {
    padding: 1rem;
  }

  .btn {
    display: block;
    margin: 0.5rem 0;
    width: 100%;
    max-width: 300px;
    margin-left: auto;
    margin-right: auto;
  }

  .feature-highlights ul {
    max-width: 100%;
  }
}
</style>
