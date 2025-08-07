<template>
  <div id="app">
    <nav v-if="isAuthenticated" class="navbar">
      <div class="nav-brand">
        <router-link to="/">代码片段管理</router-link>
      </div>

      <div class="nav-links">
        <router-link to="/">首页</router-link>
        <router-link to="/snippets">代码片段</router-link>
        <router-link to="/snippets/create">创建片段</router-link>
      </div>

      <div class="nav-user">
        <span v-if="user">{{ user.username }}</span>
        <button @click="handleLogout" class="logout-btn">登出</button>
      </div>
    </nav>

    <main class="main-content">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const isAuthenticated = computed(() => authStore.isAuthenticated)
const user = computed(() => authStore.user)

async function handleLogout() {
  await authStore.logout()
  router.push('/login')
}
</script>

<style scoped>
#app {
  min-height: 100vh;
  background-color: #f8f9fa;
}

.navbar {
  background-color: #343a40;
  color: white;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.nav-brand a {
  color: white;
  text-decoration: none;
  font-size: 1.25rem;
  font-weight: bold;
}

.nav-links {
  display: flex;
  gap: 1rem;
}

.nav-links a {
  color: white;
  text-decoration: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  transition: background-color 0.3s;
}

.nav-links a:hover,
.nav-links a.router-link-active {
  background-color: #495057;
}

.nav-user {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.logout-btn {
  background-color: #dc3545;
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: 4px;
  cursor: pointer;
  transition: background-color 0.3s;
}

.logout-btn:hover {
  background-color: #c82333;
}

.main-content {
  min-height: calc(100vh - 80px);
}
</style>
