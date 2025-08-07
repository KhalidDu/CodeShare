<template>
  <div class="home">
    <h1>代码片段管理工具</h1>
    <p>欢迎使用代码片段管理工具！</p>

    <div class="quick-actions">
      <router-link to="/snippets" class="btn btn-primary">
        浏览代码片段
      </router-link>
      <router-link to="/snippets/create" class="btn btn-secondary">
        创建新片段
      </router-link>
    </div>

    <div class="stats" v-if="user">
      <h2>欢迎回来，{{ user.username }}！</h2>
      <p>角色：{{ getRoleName(user.role) }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { UserRole } from '@/types'

const authStore = useAuthStore()
const user = computed(() => authStore.user)

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
</script>

<style scoped>
.home {
  text-align: center;
  padding: 2rem;
}

.quick-actions {
  margin: 2rem 0;
}

.btn {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  margin: 0 0.5rem;
  text-decoration: none;
  border-radius: 4px;
  font-weight: 500;
  transition: background-color 0.3s;
}

.btn-primary {
  background-color: #007bff;
  color: white;
}

.btn-primary:hover {
  background-color: #0056b3;
}

.btn-secondary {
  background-color: #6c757d;
  color: white;
}

.btn-secondary:hover {
  background-color: #545b62;
}

.stats {
  margin-top: 2rem;
  padding: 1rem;
  background-color: #f8f9fa;
  border-radius: 4px;
}
</style>
