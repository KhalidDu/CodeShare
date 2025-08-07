<template>
  <div class="snippets-view">
    <div class="header">
      <h1>代码片段</h1>
      <router-link to="/snippets/create" class="btn btn-primary">
        创建新片段
      </router-link>
    </div>

    <div class="filters">
      <input
        v-model="searchTerm"
        type="text"
        placeholder="搜索代码片段..."
        @input="handleSearch"
        class="search-input"
      />

      <select v-model="selectedLanguage" @change="handleLanguageFilter" class="filter-select">
        <option value="">所有语言</option>
        <option value="javascript">JavaScript</option>
        <option value="typescript">TypeScript</option>
        <option value="python">Python</option>
        <option value="java">Java</option>
        <option value="csharp">C#</option>
        <option value="html">HTML</option>
        <option value="css">CSS</option>
      </select>
    </div>

    <div v-if="isLoading" class="loading">
      加载中...
    </div>

    <div v-else-if="snippets.length === 0" class="empty-state">
      <p>暂无代码片段</p>
      <router-link to="/snippets/create" class="btn btn-secondary">
        创建第一个片段
      </router-link>
    </div>

    <div v-else class="snippets-grid">
      <div
        v-for="snippet in snippets"
        :key="snippet.id"
        class="snippet-card"
        @click="viewSnippet(snippet.id)"
      >
        <div class="snippet-header">
          <h3>{{ snippet.title }}</h3>
          <span class="language-badge">{{ snippet.language }}</span>
        </div>

        <p class="snippet-description">{{ snippet.description }}</p>

        <div class="snippet-meta">
          <span>作者：{{ snippet.creatorName }}</span>
          <span>{{ formatDate(snippet.createdAt) }}</span>
        </div>

        <div class="snippet-stats">
          <span>👁️ {{ snippet.viewCount }}</span>
          <span>📋 {{ snippet.copyCount }}</span>
        </div>
      </div>
    </div>

    <div v-if="totalPages > 1" class="pagination">
      <button
        v-for="page in totalPages"
        :key="page"
        @click="changePage(page)"
        :class="{ active: page === currentPage }"
        class="page-btn"
      >
        {{ page }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCodeSnippetsStore } from '@/stores/codeSnippets'

const router = useRouter()
const snippetsStore = useCodeSnippetsStore()

const searchTerm = ref('')
const selectedLanguage = ref('')

const snippets = computed(() => snippetsStore.snippets)
const isLoading = computed(() => snippetsStore.isLoading)
const currentPage = computed(() => snippetsStore.currentPage)
const totalPages = computed(() => Math.ceil(snippetsStore.totalCount / snippetsStore.pageSize))

onMounted(() => {
  snippetsStore.fetchSnippets()
})

function handleSearch() {
  snippetsStore.searchSnippets(searchTerm.value)
}

function handleLanguageFilter() {
  if (selectedLanguage.value) {
    snippetsStore.filterByLanguage(selectedLanguage.value)
  } else {
    snippetsStore.clearFilters()
  }
}

function viewSnippet(id: string) {
  router.push(`/snippets/${id}`)
}

function changePage(page: number) {
  snippetsStore.fetchSnippets({ page })
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString('zh-CN')
}
</script>

<style scoped>
.snippets-view {
  padding: 2rem;
  max-width: 1200px;
  margin: 0 auto;
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 2rem;
}

.filters {
  display: flex;
  gap: 1rem;
  margin-bottom: 2rem;
}

.search-input,
.filter-select {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 4px;
  font-size: 1rem;
}

.search-input {
  flex: 1;
  max-width: 300px;
}

.loading,
.empty-state {
  text-align: center;
  padding: 2rem;
}

.snippets-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 1.5rem;
}

.snippet-card {
  background: white;
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 1.5rem;
  cursor: pointer;
  transition: box-shadow 0.3s, transform 0.2s;
}

.snippet-card:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  transform: translateY(-2px);
}

.snippet-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 1rem;
}

.snippet-header h3 {
  margin: 0;
  font-size: 1.2rem;
  color: #333;
}

.language-badge {
  background-color: #007bff;
  color: white;
  padding: 0.25rem 0.5rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 500;
}

.snippet-description {
  color: #666;
  margin-bottom: 1rem;
  line-height: 1.4;
}

.snippet-meta {
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
  color: #888;
  margin-bottom: 0.5rem;
}

.snippet-stats {
  display: flex;
  gap: 1rem;
  font-size: 0.9rem;
  color: #666;
}

.pagination {
  display: flex;
  justify-content: center;
  gap: 0.5rem;
  margin-top: 2rem;
}

.page-btn {
  padding: 0.5rem 1rem;
  border: 1px solid #ddd;
  background: white;
  cursor: pointer;
  border-radius: 4px;
}

.page-btn:hover {
  background-color: #f8f9fa;
}

.page-btn.active {
  background-color: #007bff;
  color: white;
  border-color: #007bff;
}

.btn {
  display: inline-block;
  padding: 0.75rem 1.5rem;
  text-decoration: none;
  border-radius: 4px;
  font-weight: 500;
  transition: background-color 0.3s;
  border: none;
  cursor: pointer;
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
</style>
