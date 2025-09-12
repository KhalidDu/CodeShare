/**
 * 代码片段增强功能路由配置
 * 
 * 此文件定义了增强的代码片段详情页面的路由配置
 * 包括代码查看器、编辑器、设置等功能的路由
 */

import type { RouteRecordRaw } from 'vue-router'

// 代码片段增强功能路由配置
export const snippetEnhancedRoutes: RouteRecordRaw[] = [
  // 增强的代码片段详情页面
  {
    path: '/snippets/:id/enhanced',
    name: 'snippet-detail-enhanced',
    component: () => import('@/views/SnippetDetailEnhancedView.vue'),
    meta: {
      title: '代码片段详情',
      keepAlive: true,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id,
      view: route.query.view as string || 'view',
      theme: route.query.theme as string || 'vs-dark',
      language: route.query.language as string
    })
  },

  // 代码片段编辑器页面
  {
    path: '/snippets/:id/edit/enhanced',
    name: 'snippet-edit-enhanced',
    component: () => import('@/views/SnippetEditEnhancedView.vue'),
    meta: {
      title: '编辑代码片段',
      requiresAuth: true,
      keepAlive: false,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id,
      mode: route.query.mode as string || 'edit',
      theme: route.query.theme as string || 'vs-dark'
    })
  },

  // 代码片段版本比较页面
  {
    path: '/snippets/:id/compare/enhanced',
    name: 'snippet-compare-enhanced',
    component: () => import('@/views/SnippetCompareEnhancedView.vue'),
    meta: {
      title: '版本比较',
      requiresAuth: true,
      keepAlive: true,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id,
      version1: route.query.version1 as string,
      version2: route.query.version2 as string
    })
  },

  // 代码片段设置页面
  {
    path: '/snippets/:id/settings',
    name: 'snippet-settings',
    component: () => import('@/views/SnippetSettingsView.vue'),
    meta: {
      title: '代码片段设置',
      requiresAuth: true,
      keepAlive: false,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id
    })
  },

  // 增强的分享视图页面
  {
    path: '/share/:token/enhanced',
    name: 'share-view-enhanced',
    component: () => import('@/views/ShareViewEnhanced.vue'),
    meta: {
      title: '增强分享视图',
      public: true,
      keepAlive: true,
      layout: 'default'
    },
    props: (route) => ({
      token: route.params.token,
      view: route.query.view as string || 'view',
      theme: route.query.theme as string || 'vs-dark'
    })
  },

  // 代码片段分析页面
  {
    path: '/snippets/:id/analysis',
    name: 'snippet-analysis',
    component: () => import('@/views/SnippetAnalysisView.vue'),
    meta: {
      title: '代码分析',
      requiresAuth: true,
      keepAlive: true,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id
    })
  },

  // 代码片段性能测试页面
  {
    path: '/snippets/:id/performance',
    name: 'snippet-performance',
    component: () => import('@/views/SnippetPerformanceView.vue'),
    meta: {
      title: '性能测试',
      requiresAuth: true,
      keepAlive: false,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id
    })
  },

  // 代码片段分享管理页面
  {
    path: '/snippets/:id/shares',
    name: 'snippet-shares',
    component: () => import('@/views/SnippetSharesView.vue'),
    meta: {
      title: '分享管理',
      requiresAuth: true,
      keepAlive: true,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id
    })
  },

  // 代码片段协作页面
  {
    path: '/snippets/:id/collaboration',
    name: 'snippet-collaboration',
    component: () => import('@/views/SnippetCollaborationView.vue'),
    meta: {
      title: '协作编辑',
      requiresAuth: true,
      keepAlive: false,
      layout: 'default'
    },
    props: (route) => ({
      id: route.params.id,
      sessionId: route.query.sessionId as string
    })
  }
]

// 路由重定向配置
export const snippetEnhancedRedirects: RouteRecordRaw[] = [
  // 旧的重定向
  {
    path: '/snippet/:id',
    redirect: '/snippets/:id/enhanced'
  },
  {
    path: '/snippet/:id/edit',
    redirect: '/snippets/:id/edit/enhanced'
  },
  {
    path: '/snippet/:id/view',
    redirect: '/snippets/:id/enhanced'
  },
  
  // 新的重定向 - 默认路由
  {
    path: '/snippets/:id',
    redirect: '/snippets/:id/enhanced'
  },
  {
    path: '/snippets/:id/view',
    redirect: '/snippets/:id/enhanced'
  },
  {
    path: '/snippets/:id/read',
    redirect: '/snippets/:id/enhanced'
  }
]

// 路由导航守卫配置
export const snippetEnhancedGuards = {
  // 代码片段访问权限检查
  beforeEnter: (to: any, from: any, next: any) => {
    // 这里可以添加特定的代码片段访问权限检查
    // 例如：检查代码片段是否存在、是否私有、是否有访问权限等
    next()
  },

  // 代码片段编辑权限检查
  beforeEnterEdit: (to: any, from: any, next: any) => {
    // 动态导入auth store以避免循环依赖
    import('@/stores/auth').then(({ useAuthStore }) => {
      const authStore = useAuthStore()
      
      if (!authStore.isAuthenticated) {
        next('/login')
        return
      }
      
      // 这里可以添加编辑权限检查
      // 例如：检查用户是否是代码片段的创建者、是否有编辑权限等
      next()
    }).catch(() => {
      next('/login')
    })
  },

  // 代码片段设置权限检查
  beforeEnterSettings: (to: any, from: any, next: any) => {
    // 动态导入auth store以避免循环依赖
    import('@/stores/auth').then(({ useAuthStore }) => {
      const authStore = useAuthStore()
      
      if (!authStore.isAuthenticated) {
        next('/login')
        return
      }
      
      // 这里可以添加设置权限检查
      next()
    }).catch(() => {
      next('/login')
    })
  }
}

// 路由元信息类型定义
export interface SnippetRouteMeta {
  title?: string
  requiresAuth?: boolean
  requiredRoles?: string[]
  keepAlive?: boolean
  layout?: 'default' | 'minimal' | 'fullscreen'
  public?: boolean
  hideHeader?: boolean
  hideSidebar?: boolean
  hideFooter?: boolean
  fullscreen?: boolean
  cacheable?: boolean
}

// 路由参数类型定义
export interface SnippetRouteParams {
  id: string
  view?: 'view' | 'edit' | 'compare' | 'analysis'
  theme?: string
  language?: string
  version1?: string
  version2?: string
  sessionId?: string
}

// 路由查询参数类型定义
export interface SnippetRouteQuery {
  view?: 'view' | 'edit' | 'compare' | 'analysis'
  theme?: string
  language?: string
  version1?: string
  version2?: string
  sessionId?: string
  line?: string
  highlight?: string
  share?: boolean
  readonly?: boolean
}

// 工具函数
export const snippetRouteUtils = {
  // 生成代码片段详情页链接
  getSnippetDetailUrl: (id: string, options: Partial<SnippetRouteQuery> = {}) => {
    const query = new URLSearchParams()
    Object.entries(options).forEach(([key, value]) => {
      if (value !== undefined) {
        query.append(key, String(value))
      }
    })
    
    const queryString = query.toString()
    return `/snippets/${id}/enhanced${queryString ? `?${queryString}` : ''}`
  },

  // 生成代码片段编辑页链接
  getSnippetEditUrl: (id: string, options: Partial<SnippetRouteQuery> = {}) => {
    const query = new URLSearchParams()
    Object.entries(options).forEach(([key, value]) => {
      if (value !== undefined) {
        query.append(key, String(value))
      }
    })
    
    const queryString = query.toString()
    return `/snippets/${id}/edit/enhanced${queryString ? `?${queryString}` : ''}`
  },

  // 生成代码片段分享链接
  getSnippetShareUrl: (token: string, options: Partial<SnippetRouteQuery> = {}) => {
    const query = new URLSearchParams()
    Object.entries(options).forEach(([key, value]) => {
      if (value !== undefined) {
        query.append(key, String(value))
      }
    })
    
    const queryString = query.toString()
    return `/share/${token}/enhanced${queryString ? `?${queryString}` : ''}`
  },

  // 解析路由参数
  parseRouteParams: (route: any): SnippetRouteParams => {
    return {
      id: route.params.id as string,
      view: route.query.view as SnippetRouteParams['view'],
      theme: route.query.theme as string,
      language: route.query.language as string,
      version1: route.query.version1 as string,
      version2: route.query.version2 as string,
      sessionId: route.query.sessionId as string
    }
  },

  // 解析查询参数
  parseRouteQuery: (route: any): SnippetRouteQuery => {
    return {
      view: route.query.view as SnippetRouteQuery['view'],
      theme: route.query.theme as string,
      language: route.query.language as string,
      version1: route.query.version1 as string,
      version2: route.query.version2 as string,
      sessionId: route.query.sessionId as string,
      line: route.query.line as string,
      highlight: route.query.highlight as string,
      share: route.query.share === 'true',
      readonly: route.query.readonly === 'true'
    }
  }
}

// 路由配置默认选项
export const snippetRouteDefaults = {
  // 默认主题
  defaultTheme: 'vs-dark',
  
  // 默认视图模式
  defaultView: 'view',
  
  // 默认语言
  defaultLanguage: 'javascript',
  
  // 是否启用缓存
  enableCache: true,
  
  // 是否启用预加载
  enablePreload: true,
  
  // 路由切换动画
  enableTransition: true,
  
  // 路由切换动画时长
  transitionDuration: 300,
  
  // 是否启用滚动恢复
  enableScrollRestoration: true,
  
  // 是否启用路由守卫
  enableGuards: true
}

// 导出所有路由配置
export const snippetEnhancedRouteConfig = {
  routes: snippetEnhancedRoutes,
  redirects: snippetEnhancedRedirects,
  guards: snippetEnhancedGuards,
  utils: snippetRouteUtils,
  defaults: snippetRouteDefaults,
  types: {
    SnippetRouteMeta,
    SnippetRouteParams,
    SnippetRouteQuery
  }
}