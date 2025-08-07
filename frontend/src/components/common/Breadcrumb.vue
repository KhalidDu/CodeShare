<template>
  <nav class="breadcrumb" aria-label="面包屑导航">
    <ol class="breadcrumb-list">
      <li
        v-for="(item, index) in breadcrumbItems"
        :key="item.path || index"
        class="breadcrumb-item"
        :class="{ active: index === breadcrumbItems.length - 1 }"
      >
        <!-- 非最后一项显示为链接 -->
        <router-link
          v-if="index < breadcrumbItems.length - 1 && item.path"
          :to="item.path"
          class="breadcrumb-link"
        >
          <svg v-if="item.icon" class="breadcrumb-icon" viewBox="0 0 24 24" fill="currentColor">
            <path :d="item.icon"/>
          </svg>
          {{ item.title }}
        </router-link>

        <!-- 最后一项显示为普通文本 -->
        <span v-else class="breadcrumb-text">
          <svg v-if="item.icon" class="breadcrumb-icon" viewBox="0 0 24 24" fill="currentColor">
            <path :d="item.icon"/>
          </svg>
          {{ item.title }}
        </span>

        <!-- 分隔符 -->
        <svg
          v-if="index < breadcrumbItems.length - 1"
          class="breadcrumb-separator"
          viewBox="0 0 24 24"
          fill="currentColor"
        >
          <path d="M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z"/>
        </svg>
      </li>
    </ol>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

interface BreadcrumbItem {
  title: string
  path?: string
  icon?: string
}

interface Props {
  items?: BreadcrumbItem[]
  showHome?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  items: () => [],
  showHome: true
})

const route = useRoute()

/**
 * 根据当前路由自动生成面包屑项目
 */
const autoBreadcrumbItems = computed((): BreadcrumbItem[] => {
  const items: BreadcrumbItem[] = []

  // 添加首页
  if (props.showHome) {
    items.push({
      title: '首页',
      path: '/',
      icon: 'M10 20v-6h4v6h5v-8h3L12 3 2 12h3v8z'
    })
  }

  // 根据路由路径生成面包屑
  const pathSegments = route.path.split('/').filter(segment => segment)
  let currentPath = ''

  pathSegments.forEach((segment, index) => {
    currentPath += `/${segment}`

    // 根据路径段生成标题和图标
    const item = getBreadcrumbItemByPath(currentPath, segment, route.params)
    if (item) {
      items.push(item)
    }
  })

  return items
})

/**
 * 最终的面包屑项目（优先使用传入的 items，否则使用自动生成的）
 */
const breadcrumbItems = computed(() => {
  return props.items.length > 0 ? props.items : autoBreadcrumbItems.value
})

/**
 * 根据路径生成面包屑项目
 */
function getBreadcrumbItemByPath(path: string, segment: string, params: any): BreadcrumbItem | null {
  const pathMappings: Record<string, BreadcrumbItem> = {
    '/snippets': {
      title: '代码片段',
      path: '/snippets',
      icon: 'M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z'
    },
    '/snippets/create': {
      title: '创建片段',
      icon: 'M19,13H13V19H11V13H5V11H11V5H13V11H19V13Z'
    },
    '/clipboard-history': {
      title: '剪贴板历史',
      path: '/clipboard-history',
      icon: 'M19,3H14.82C14.4,1.84 13.3,1 12,1C10.7,1 9.6,1.84 9.18,3H5A2,2 0 0,0 3,5V19A2,2 0 0,0 5,21H19A2,2 0 0,0 21,19V5A2,2 0 0,0 19,3M12,3A1,1 0 0,1 13,4A1,1 0 0,1 12,5A1,1 0 0,1 11,4A1,1 0 0,1 12,3'
    },
    '/admin/users': {
      title: '用户管理',
      path: '/admin/users',
      icon: 'M16 4c0-1.11.89-2 2-2s2 .89 2 2-.89 2-2 2-2-.89-2-2zm4 18v-6h2.5l-2.54-7.63A2.996 2.996 0 0 0 16.96 6H15c-.8 0-1.54.37-2.01 1l-2.54 7.63H13V21h7z'
    },
    '/admin/tags': {
      title: '标签管理',
      path: '/admin/tags',
      icon: 'M5.5,7A1.5,1.5 0 0,1 4,5.5A1.5,1.5 0 0,1 5.5,4A1.5,1.5 0 0,1 7,5.5A1.5,1.5 0 0,1 5.5,7M21.41,11.58L12.41,2.58C12.05,2.22 11.55,2 11,2H4C2.89,2 2,2.89 2,4V11C2,11.55 2.22,12.05 2.59,12.41L11.58,21.41C11.95,21.78 12.45,22 13,22C13.55,22 14.05,21.78 14.41,21.41L21.41,14.41C21.78,14.05 22,13.55 22,13C22,12.45 21.78,11.95 21.41,11.58Z'
    },
    '/settings': {
      title: '设置',
      path: '/settings',
      icon: 'M12,15.5A3.5,3.5 0 0,1 8.5,12A3.5,3.5 0 0,1 12,8.5A3.5,3.5 0 0,1 15.5,12A3.5,3.5 0 0,1 12,15.5M19.43,12.97C19.47,12.65 19.5,12.33 19.5,12C19.5,11.67 19.47,11.34 19.43,11L21.54,9.37C21.73,9.22 21.78,8.95 21.66,8.73L19.66,5.27C19.54,5.05 19.27,4.96 19.05,5.05L16.56,6.05C16.04,5.66 15.5,5.32 14.87,5.07L14.5,2.42C14.46,2.18 14.25,2 14,2H10C9.75,2 9.54,2.18 9.5,2.42L9.13,5.07C8.5,5.32 7.96,5.66 7.44,6.05L4.95,5.05C4.73,4.96 4.46,5.05 4.34,5.27L2.34,8.73C2.22,8.95 2.27,9.22 2.46,9.37L4.57,11C4.53,11.34 4.5,11.67 4.5,12C4.5,12.33 4.53,12.65 4.57,12.97L2.46,14.63C2.27,14.78 2.22,15.05 2.34,15.27L4.34,18.73C4.46,18.95 4.73,19.03 4.95,18.95L7.44,17.94C7.96,18.34 8.5,18.68 9.13,18.93L9.5,21.58C9.54,21.82 9.75,22 10,22H14C14.25,22 14.46,21.82 14.5,21.58L14.87,18.93C15.5,18.68 16.04,18.34 16.56,17.94L19.05,18.95C19.27,19.03 19.54,18.95 19.66,18.73L21.66,15.27C21.78,15.05 21.73,14.78 21.54,14.63L19.43,12.97Z'
    }
  }

  // 处理动态路由参数
  if (path.includes('/snippets/') && params.id) {
    if (path.endsWith('/edit')) {
      return {
        title: '编辑片段',
        icon: 'M20.71,7.04C21.1,6.65 21.1,6 20.71,5.63L18.37,3.29C18,2.9 17.35,2.9 16.96,3.29L15.12,5.12L18.87,8.87M3,17.25V21H6.75L17.81,9.93L14.06,6.18L3,17.25Z'
      }
    } else {
      return {
        title: '片段详情',
        icon: 'M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2M18,20H6V4H13V9H18V20Z'
      }
    }
  }

  return pathMappings[path] || null
}
</script>

<style scoped>
.breadcrumb {
  background-color: #f8f9fa;
  border-bottom: 1px solid #dee2e6;
  padding: 0.75rem 0;
}

.breadcrumb-list {
  display: flex;
  align-items: center;
  list-style: none;
  margin: 0;
  padding: 0;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.breadcrumb-link {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  color: #007bff;
  text-decoration: none;
  font-size: 0.875rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  transition: all 0.3s ease;
}

.breadcrumb-link:hover {
  background-color: #e7f3ff;
  color: #0056b3;
}

.breadcrumb-text {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  color: #6c757d;
  font-size: 0.875rem;
  padding: 0.25rem 0.5rem;
}

.breadcrumb-item.active .breadcrumb-text {
  color: #495057;
  font-weight: 500;
}

.breadcrumb-icon {
  width: 16px;
  height: 16px;
  flex-shrink: 0;
}

.breadcrumb-separator {
  width: 16px;
  height: 16px;
  color: #adb5bd;
  flex-shrink: 0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .breadcrumb {
    padding: 0.5rem 0;
  }

  .breadcrumb-link,
  .breadcrumb-text {
    font-size: 0.8125rem;
    padding: 0.1875rem 0.375rem;
  }

  .breadcrumb-icon {
    width: 14px;
    height: 14px;
  }

  .breadcrumb-separator {
    width: 14px;
    height: 14px;
  }
}

@media (max-width: 480px) {
  .breadcrumb-list {
    gap: 0.125rem;
  }

  .breadcrumb-item {
    gap: 0.25rem;
  }

  .breadcrumb-link,
  .breadcrumb-text {
    gap: 0.25rem;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .breadcrumb-link {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .breadcrumb {
    border-bottom-width: 2px;
  }

  .breadcrumb-link {
    border: 1px solid transparent;
  }

  .breadcrumb-link:hover,
  .breadcrumb-link:focus {
    border-color: currentColor;
  }
}
</style>
