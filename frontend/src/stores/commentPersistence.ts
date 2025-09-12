/**
 * 评论Store持久化存储插件
 * 
 * 本文件提供评论Pinia Store的持久化存储功能，包括：
 * - 评论筛选条件持久化
 * - 用户偏好设置持久化
 * - 评论缓存持久化
 * - 性能优化配置持久化
 * 
 * 遵循Vue 3 + Pinia插件开发模式，使用TypeScript确保类型安全
 */

import { PiniaPluginContext } from 'pinia'
import type { CommentFilter, CommentSearchFilter, CommentNotificationSettings } from '@/types/comment'
import { CommentSort } from '@/types/comment'

/**
 * 持久化存储接口
 */
interface PersistedCommentState {
  filter: CommentFilter
  searchFilter: CommentSearchFilter
  likeStats: { [commentId: string]: { count: number; isLiked: boolean } }
  notificationSettings: CommentNotificationSettings | null
  lastAccessed: string
  preferences: {
    defaultSort: number
    defaultPageSize: number
    autoRefresh: boolean
    showReplies: boolean
    compactView: boolean
  }
}

/**
 * 默认持久化配置
 */
const DEFAULT_PERSISTED_STATE: PersistedCommentState = {
  filter: {
    sortBy: CommentSort.CREATED_AT_DESC,
    page: 1,
    pageSize: 20,
    includeUser: true,
    includeReplies: true,
    includeLikes: true
  },
  searchFilter: {
    keyword: '',
    searchScope: 0, // CommentSearchScope.ALL
    sortBy: 0, // CommentSearchSort.RELEVANCE
    page: 1,
    pageSize: 20
  },
  likeStats: {},
  notificationSettings: null,
  lastAccessed: new Date().toISOString(),
  preferences: {
    defaultSort: CommentSort.CREATED_AT_DESC,
    defaultPageSize: 20,
    autoRefresh: false,
    showReplies: true,
    compactView: false
  }
}

/**
 * 持久化存储插件
 */
export function createCommentPersistencePlugin() {
  return (context: PiniaPluginContext) => {
    const { store } = context
    
    // 只对评论Store应用持久化
    if (store.$id !== 'comment') {
      return
    }

    // 存储键名
    const STORAGE_KEY = 'comment-store-state'
    
    // 加载持久化状态
    const loadPersistedState = (): PersistedCommentState => {
      try {
        const stored = localStorage.getItem(STORAGE_KEY)
        if (stored) {
          const parsed = JSON.parse(stored)
          
          // 验证数据结构
          if (isValidPersistedState(parsed)) {
            return parsed
          }
        }
      } catch (error) {
        console.warn('Failed to load persisted comment state:', error)
      }
      
      return DEFAULT_PERSISTED_STATE
    }

    // 保存持久化状态
    const savePersistedState = (state: PersistedCommentState) => {
      try {
        state.lastAccessed = new Date().toISOString()
        localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
      } catch (error) {
        console.warn('Failed to save persisted comment state:', error)
      }
    }

    // 验证持久化状态数据结构
    const isValidPersistedState = (data: any): data is PersistedCommentState => {
      return (
        data &&
        typeof data === 'object' &&
        typeof data.filter === 'object' &&
        typeof data.searchFilter === 'object' &&
        typeof data.likeStats === 'object' &&
        typeof data.preferences === 'object' &&
        Array.isArray(Object.keys(data.likeStats))
      )
    }

    // 清理过期的持久化数据
    const cleanupExpiredData = (state: PersistedCommentState) => {
      const now = Date.now()
      const thirtyDaysAgo = now - 30 * 24 * 60 * 60 * 1000 // 30天
      
      // 清理过期的点赞统计
      Object.keys(state.likeStats).forEach(commentId => {
        // 可以添加更复杂的过期逻辑
        // 暂时保留所有点赞数据
      })
      
      // 如果数据太旧，重置到默认状态
      if (state.lastAccessed) {
        const lastAccessed = new Date(state.lastAccessed).getTime()
        if (lastAccessed < thirtyDaysAgo) {
          return DEFAULT_PERSISTED_STATE
        }
      }
      
      return state
    }

    // 合并持久化状态到Store
    const mergePersistedState = (persisted: PersistedCommentState) => {
      // 合并筛选条件
      Object.assign(store.filter, persisted.filter)
      
      // 合并搜索筛选条件
      Object.assign(store.searchFilter, persisted.searchFilter)
      
      // 合并点赞统计
      store.likeStats = { ...persisted.likeStats, ...store.likeStats }
      
      // 合并通知设置
      if (persisted.notificationSettings) {
        store.notificationSettings = persisted.notificationSettings
      }
      
      // 应用用户偏好
      if (persisted.preferences) {
        store.filter.sortBy = persisted.preferences.defaultSort
        store.filter.pageSize = persisted.preferences.defaultPageSize
        store.filter.includeReplies = persisted.preferences.showReplies
      }
    }

    // 监听状态变化并保存
    const setupStateWatchers = () => {
      // 监听筛选条件变化
      store.$onAction(({ name, after }) => {
        if (name === 'updateFilter') {
          after(() => {
            saveCurrentState()
          })
        }
      })

      // 监听搜索筛选条件变化
      store.$onAction(({ name, after }) => {
        if (name === 'updateSearchFilter') {
          after(() => {
            saveCurrentState()
          })
        }
      })

      // 监听点赞统计变化
      store.$subscribe((mutation, state) => {
        if (mutation.type === 'direct' && mutation.storeId === 'comment') {
          // 检查是否是点赞统计的变化
          const likeStatsChanged = JSON.stringify(state.likeStats) !== 
            JSON.stringify(store.likeStats)
          
          if (likeStatsChanged) {
            saveCurrentState()
          }
        }
      })
    }

    // 保存当前状态
    const saveCurrentState = () => {
      const state: PersistedCommentState = {
        filter: { ...store.filter },
        searchFilter: { ...store.searchFilter },
        likeStats: { ...store.likeStats },
        notificationSettings: store.notificationSettings,
        lastAccessed: new Date().toISOString(),
        preferences: {
          defaultSort: store.filter.sortBy,
          defaultPageSize: store.filter.pageSize,
          autoRefresh: false, // 可以从Store中获取
          showReplies: store.filter.includeReplies || true,
          compactView: false // 可以从Store中获取
        }
      }
      
      savePersistedState(state)
    }

    // 重置持久化数据
    const resetPersistedData = () => {
      localStorage.removeItem(STORAGE_KEY)
      const defaultState = DEFAULT_PERSISTED_STATE
      mergePersistedState(defaultState)
      savePersistedState(defaultState)
    }

    // 获取持久化统计信息
    const getPersistenceStats = () => {
      try {
        const stored = localStorage.getItem(STORAGE_KEY)
        if (stored) {
          const parsed = JSON.parse(stored)
          return {
            hasData: true,
            dataSize: new Blob([stored]).size,
            lastAccessed: parsed.lastAccessed,
            likeStatsCount: Object.keys(parsed.likeStats || {}).length,
            hasFilter: !!parsed.filter,
            hasSearchFilter: !!parsed.searchFilter,
            hasNotificationSettings: !!parsed.notificationSettings
          }
        }
      } catch (error) {
        console.warn('Failed to get persistence stats:', error)
      }
      
      return {
        hasData: false,
        dataSize: 0,
        lastAccessed: null,
        likeStatsCount: 0,
        hasFilter: false,
        hasSearchFilter: false,
        hasNotificationSettings: false
      }
    }

    // 清理持久化数据（释放存储空间）
    const cleanupPersistedData = () => {
      try {
        const stored = localStorage.getItem(STORAGE_KEY)
        if (stored) {
          const parsed = JSON.parse(stored)
          
          // 保留必要的数据，清理大量数据
          const cleaned = {
            filter: parsed.filter || DEFAULT_PERSISTED_STATE.filter,
            searchFilter: parsed.searchFilter || DEFAULT_PERSISTED_STATE.searchFilter,
            likeStats: {}, // 清空点赞统计，可以重新获取
            notificationSettings: parsed.notificationSettings,
            lastAccessed: new Date().toISOString(),
            preferences: parsed.preferences || DEFAULT_PERSISTED_STATE.preferences
          }
          
          localStorage.setItem(STORAGE_KEY, JSON.stringify(cleaned))
          return true
        }
      } catch (error) {
        console.warn('Failed to cleanup persisted data:', error)
      }
      
      return false
    }

    // 初始化持久化功能
    const init = () => {
      // 加载持久化状态
      const persistedState = loadPersistedState()
      
      // 清理过期数据
      const cleanedState = cleanupExpiredData(persistedState)
      
      // 合并到Store
      mergePersistedState(cleanedState)
      
      // 设置状态监听
      setupStateWatchers()
      
      // 添加持久化方法到Store
      store.resetPersistedData = resetPersistedData
      store.getPersistenceStats = getPersistenceStats
      store.cleanupPersistedData = cleanupPersistedData
      store.saveCurrentState = saveCurrentState
    }

    // 执行初始化
    init()
  }
}

// 导出持久化插件工厂函数
export const commentPersistencePlugin = createCommentPersistencePlugin()

// 导出类型定义
export type { PersistedCommentState }