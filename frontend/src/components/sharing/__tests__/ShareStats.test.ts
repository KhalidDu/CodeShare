import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import ShareStats from '../ShareStats.vue'

// Mock the stores
vi.mock('@/stores/share', () => ({
  useShareStore: vi.fn(() => ({
    fetchShareStats: vi.fn(),
    isLoading: false,
    error: null
  }))
}))

vi.mock('@/stores/toast', () => ({
  useToastStore: vi.fn(() => ({
    error: vi.fn()
  }))
}))

// Mock the LoadingSpinner component
vi.mock('@/components/common/LoadingSpinner.vue', () => ({
  default: {
    name: 'LoadingSpinner',
    template: '<div class="loading-spinner">Loading...</div>'
  }
}))

describe('ShareStats', () => {
  let wrapper: any
  let shareStore: any
  let toastStore: any

  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    
    // 获取store实例
    const { useShareStore } = require('@/stores/share')
    const { useToastStore } = require('@/stores/toast')
    
    shareStore = useShareStore()
    toastStore = useToastStore()
  })

  afterEach(() => {
    if (wrapper) {
      wrapper.unmount()
    }
  })

  describe('基本渲染测试', () => {
    it('应该能够挂载组件', () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(wrapper.exists()).toBe(true)
      expect(wrapper.find('.share-stats').exists()).toBe(true)
    })

    it('应该显示统计概览卡片', () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(wrapper.find('.stats-overview').exists()).toBe(true)
      expect(wrapper.find('.stat-card').exists()).toBe(true)
    })

    it('应该显示图表区域', () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(wrapper.find('.chart-section').exists()).toBe(true)
      expect(wrapper.find('.chart-header').exists()).toBe(true)
    })

    it('应该显示详细统计', () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(wrapper.find('.detailed-stats').exists()).toBe(true)
      expect(wrapper.find('.stats-tabs').exists()).toBe(true)
    })
  })

  describe('统计数据显示测试', () => {
    beforeEach(() => {
      // Mock统计数据
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 150,
        uniqueAccessCount: 45,
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-10T12:00:00Z',
        dailyStats: [
          {
            date: '2024-01-10',
            accessCount: 25,
            uniqueAccessCount: 8
          },
          {
            date: '2024-01-09',
            accessCount: 18,
            uniqueAccessCount: 6
          }
        ],
        browserStats: [
          {
            browser: 'Chrome',
            accessCount: 80,
            percentage: 53.3
          },
          {
            browser: 'Firefox',
            accessCount: 45,
            percentage: 30.0
          },
          {
            browser: 'Safari',
            accessCount: 25,
            percentage: 16.7
          }
        ],
        osStats: [
          {
            os: 'Windows',
            accessCount: 90,
            percentage: 60.0
          },
          {
            os: 'macOS',
            accessCount: 40,
            percentage: 26.7
          },
          {
            os: 'Linux',
            accessCount: 20,
            percentage: 13.3
          }
        ],
        locationStats: [
          {
            country: '中国',
            city: '北京',
            accessCount: 100,
            percentage: 66.7
          },
          {
            country: '美国',
            city: '纽约',
            accessCount: 30,
            percentage: 20.0
          },
          {
            country: '日本',
            city: '东京',
            accessCount: 20,
            percentage: 13.3
          }
        ]
      })
    })

    it('应该显示总访问次数', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const statCards = wrapper.findAll('.stat-card')
      expect(statCards[0].find('.stat-value').text()).toBe('150')
      expect(statCards[0].find('.stat-label').text()).toBe('总访问次数')
    })

    it('应该显示独立访客数', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const statCards = wrapper.findAll('.stat-card')
      expect(statCards[1].find('.stat-value').text()).toBe('45')
      expect(statCards[1].find('.stat-label').text()).toBe('独立访客')
    })

    it('应该显示最后访问时间', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const statCards = wrapper.findAll('.stat-card')
      expect(statCards[2].find('.stat-value').text()).not.toBe('无')
    })

    it('应该显示首次访问时间', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const statCards = wrapper.findAll('.stat-card')
      expect(statCards[3].find('.stat-value').text()).not.toBe('无')
    })
  })

  describe('安全提醒测试', () => {
    it('应该显示安全提醒当访问异常', async () => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 150,
        uniqueAccessCount: 30, // 独立访客较少
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-10T12:00:00Z',
        dailyStats: [],
        browserStats: [],
        osStats: [],
        locationStats: []
      })

      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      expect(wrapper.vm.showSecurityWarning).toBe(true)
      expect(wrapper.find('.security-warning').exists()).toBe(true)
    })

    it('不应该显示安全提醒当访问正常', async () => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 150,
        uniqueAccessCount: 120, // 独立访客较多
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-10T12:00:00Z',
        dailyStats: [],
        browserStats: [],
        osStats: [],
        locationStats: []
      })

      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      expect(wrapper.vm.showSecurityWarning).toBe(false)
      expect(wrapper.find('.security-warning').exists()).toBe(false)
    })

    it('应该能够查看访问记录', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.setData({ showSecurityWarning: true })
      
      const viewRecordsButton = wrapper.find('.btn-view-records')
      await viewRecordsButton.trigger('click')
      
      expect(wrapper.emitted('viewAccessRecords')).toBeTruthy()
      expect(wrapper.emitted('viewAccessRecords')[0][0]).toBe('test-token-id')
    })

    it('应该能够撤销分享', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.setData({ showSecurityWarning: true })
      
      const revokeButton = wrapper.find('.btn-revoke')
      await revokeButton.trigger('click')
      
      expect(wrapper.emitted('revokeShare')).toBeTruthy()
      expect(wrapper.emitted('revokeShare')[0][0]).toBe('test-token-id')
    })
  })

  describe('图表功能测试', () => {
    beforeEach(() => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 150,
        uniqueAccessCount: 45,
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-10T12:00:00Z',
        dailyStats: [
          {
            date: '2024-01-10',
            accessCount: 25,
            uniqueAccessCount: 8
          },
          {
            date: '2024-01-09',
            accessCount: 18,
            uniqueAccessCount: 6
          },
          {
            date: '2024-01-08',
            accessCount: 32,
            uniqueAccessCount: 12
          },
          {
            date: '2024-01-07',
            accessCount: 15,
            uniqueAccessCount: 5
          },
          {
            date: '2024-01-06',
            accessCount: 22,
            uniqueAccessCount: 7
          },
          {
            date: '2024-01-05',
            accessCount: 28,
            uniqueAccessCount: 9
          },
          {
            date: '2024-01-04',
            accessCount: 10,
            uniqueAccessCount: 4
          }
        ],
        browserStats: [],
        osStats: [],
        locationStats: []
      })
    })

    it('应该显示访问趋势图表', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      expect(wrapper.find('.chart').exists()).toBe(true)
      expect(wrapper.find('.chart-svg').exists()).toBe(true)
    })

    it('应该能够更改图表时间范围', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const periodSelect = wrapper.find('.period-select')
      await periodSelect.setValue('14')
      
      expect(wrapper.vm.chartPeriod).toBe('14')
    })

    it('应该显示图表数据点', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const dataPoints = wrapper.findAll('.data-point')
      expect(dataPoints.length).toBeGreaterThan(0)
    })

    it('应该显示网格线', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const gridLines = wrapper.findAll('.grid-lines line')
      expect(gridLines.length).toBeGreaterThan(0)
    })

    it('应该显示坐标轴标签', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const yAxisLabels = wrapper.findAll('.y-labels text')
      const xAxisLabels = wrapper.findAll('.x-labels text')
      
      expect(yAxisLabels.length).toBeGreaterThan(0)
      expect(xAxisLabels.length).toBeGreaterThan(0)
    })

    it('应该显示工具提示', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const dataPoint = wrapper.find('.data-point')
      await dataPoint.trigger('mouseover')
      
      expect(wrapper.vm.tooltip.visible).toBe(true)
      expect(wrapper.find('.tooltip').exists()).toBe(true)
    })

    it('应该隐藏工具提示', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const dataPoint = wrapper.find('.data-point')
      await dataPoint.trigger('mouseover')
      await dataPoint.trigger('mouseout')
      
      expect(wrapper.vm.tooltip.visible).toBe(false)
      expect(wrapper.find('.tooltip').exists()).toBe(false)
    })
  })

  describe('详细统计测试', () => {
    beforeEach(() => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 150,
        uniqueAccessCount: 45,
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-10T12:00:00Z',
        dailyStats: [],
        browserStats: [
          {
            browser: 'Chrome',
            accessCount: 80,
            percentage: 53.3
          },
          {
            browser: 'Firefox',
            accessCount: 45,
            percentage: 30.0
          },
          {
            browser: 'Safari',
            accessCount: 25,
            percentage: 16.7
          }
        ],
        osStats: [
          {
            os: 'Windows',
            accessCount: 90,
            percentage: 60.0
          },
          {
            os: 'macOS',
            accessCount: 40,
            percentage: 26.7
          },
          {
            os: 'Linux',
            accessCount: 20,
            percentage: 13.3
          }
        ],
        locationStats: [
          {
            country: '中国',
            city: '北京',
            accessCount: 100,
            percentage: 66.7
          },
          {
            country: '美国',
            city: '纽约',
            accessCount: 30,
            percentage: 20.0
          },
          {
            country: '日本',
            city: '东京',
            accessCount: 20,
            percentage: 13.3
          }
        ]
      })
    })

    it('应该显示浏览器统计', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      // 切换到浏览器标签
      await wrapper.setData({ activeTab: 'browser' })
      
      const browserStats = wrapper.find('.browser-stats')
      expect(browserStats.exists()).toBe(true)
      
      const statItems = browserStats.findAll('.stat-item')
      expect(statItems.length).toBe(3)
      
      const firstItem = statItems[0]
      expect(firstItem.find('.stat-name').text()).toBe('Chrome')
      expect(firstItem.find('.stat-percentage').text()).toBe('53.3%')
      expect(firstItem.find('.stat-count').text()).toBe('80 次')
    })

    it('应该显示操作系统统计', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      // 切换到操作系统标签
      await wrapper.setData({ activeTab: 'os' })
      
      const osStats = wrapper.find('.os-stats')
      expect(osStats.exists()).toBe(true)
      
      const statItems = osStats.findAll('.stat-item')
      expect(statItems.length).toBe(3)
      
      const firstItem = statItems[0]
      expect(firstItem.find('.stat-name').text()).toBe('Windows')
      expect(firstItem.find('.stat-percentage').text()).toBe('60.0%')
      expect(firstItem.find('.stat-count').text()).toBe('90 次')
    })

    it('应该显示地理位置统计', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      // 切换到地理位置标签
      await wrapper.setData({ activeTab: 'location' })
      
      const locationStats = wrapper.find('.location-stats')
      expect(locationStats.exists()).toBe(true)
      
      const statItems = locationStats.findAll('.stat-item')
      expect(statItems.length).toBe(3)
      
      const firstItem = statItems[0]
      expect(firstItem.find('.stat-name').text()).toContain('中国')
      expect(firstItem.find('.stat-name').text()).toContain('北京')
      expect(firstItem.find('.stat-percentage').text()).toBe('66.7%')
      expect(firstItem.find('.stat-count').text()).toBe('100 次')
    })

    it('应该能够切换统计标签', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const tabs = wrapper.findAll('.tab-btn')
      expect(tabs.length).toBe(3)
      
      // 点击操作系统标签
      await tabs[1].trigger('click')
      expect(wrapper.vm.activeTab).toBe('os')
      
      // 点击地理位置标签
      await tabs[2].trigger('click')
      expect(wrapper.vm.activeTab).toBe('location')
    })

    it('应该显示统计进度条', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      // 切换到浏览器标签
      await wrapper.setData({ activeTab: 'browser' })
      
      const statBars = wrapper.findAll('.stat-bar-fill')
      expect(statBars.length).toBe(3)
      
      const firstBar = statBars[0]
      expect(firstBar.attributes('style')).toContain('width: 53.3%')
    })
  })

  describe('空状态测试', () => {
    it('应该显示图表空状态', async () => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 0,
        uniqueAccessCount: 0,
        firstAccessedAt: null,
        lastAccessedAt: null,
        dailyStats: [],
        browserStats: [],
        osStats: [],
        locationStats: []
      })

      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      expect(wrapper.find('.chart-empty').exists()).toBe(true)
      expect(wrapper.find('.empty-text').text()).toBe('暂无访问数据')
    })

    it('应该显示统计空状态', async () => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 0,
        uniqueAccessCount: 0,
        firstAccessedAt: null,
        lastAccessedAt: null,
        dailyStats: [],
        browserStats: [],
        osStats: [],
        locationStats: []
      })

      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      // 切换到浏览器标签
      await wrapper.setData({ activeTab: 'browser' })
      
      expect(wrapper.find('.empty-state').exists()).toBe(true)
      expect(wrapper.find('.empty-text').text()).toBe('暂无浏览器统计')
    })
  })

  describe('加载状态测试', () => {
    it('应该显示加载状态', () => {
      shareStore.isLoading = true
      
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })
      
      expect(wrapper.find('.chart-loading').exists()).toBe(true)
      expect(wrapper.find('.loading-spinner').exists()).toBe(true)
    })
  })

  describe('错误处理测试', () => {
    it('应该处理加载失败', async () => {
      shareStore.fetchShareStats.mockRejectedValue(new Error('加载统计数据失败'))
      
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      expect(toastStore.error).toHaveBeenCalledWith('加载统计数据失败')
    })
  })

  describe('格式化功能测试', () => {
    it('应该格式化相对时间', () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      // 测试"刚刚"
      const now = new Date()
      const recentDate = new Date(now.getTime() - 30 * 60 * 1000) // 30分钟前
      expect(wrapper.vm.formatDate(recentDate.toISOString())).toBe('30分钟前')
      
      // 测试"小时前"
      const hoursAgo = new Date(now.getTime() - 2 * 60 * 60 * 1000) // 2小时前
      expect(wrapper.vm.formatDate(hoursAgo.toISOString())).toBe('2小时前')
      
      // 测试"昨天"
      const yesterday = new Date(now.getTime() - 25 * 60 * 60 * 1000) // 25小时前
      expect(wrapper.vm.formatDate(yesterday.toISOString())).toBe('昨天')
    })

    it('应该格式化绝对时间', () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      // 测试很久以前的日期
      const oldDate = new Date('2024-01-01T00:00:00Z')
      const formatted = wrapper.vm.formatDate(oldDate.toISOString())
      
      expect(formatted).toMatch(/\d{4}\/\d{1,2}\/\d{1,2}/)
    })
  })

  describe('图表数据计算测试', () => {
    beforeEach(() => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 100,
        uniqueAccessCount: 30,
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-07T12:00:00Z',
        dailyStats: [
          {
            date: '2024-01-07',
            accessCount: 20,
            uniqueAccessCount: 8
          },
          {
            date: '2024-01-06',
            accessCount: 15,
            uniqueAccessCount: 5
          },
          {
            date: '2024-01-05',
            accessCount: 25,
            uniqueAccessCount: 9
          }
        ],
        browserStats: [],
        osStats: [],
        locationStats: []
      })
    })

    it('应该计算最大访问次数', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      expect(wrapper.vm.maxAccessCount).toBe(25)
    })

    it('应该生成图表数据点', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const chartPoints = wrapper.vm.chartPoints
      expect(chartPoints).toBeTruthy()
      expect(typeof chartPoints).toBe('string')
    })

    it('应该生成图表数据点对象', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      const dataPoints = wrapper.vm.chartDataPoints
      expect(dataPoints).toBeInstanceOf(Array)
      expect(dataPoints.length).toBeGreaterThan(0)
      
      const firstPoint = dataPoints[0]
      expect(firstPoint).toHaveProperty('x')
      expect(firstPoint).toHaveProperty('y')
      expect(firstPoint).toHaveProperty('date')
      expect(firstPoint).toHaveProperty('count')
      expect(firstPoint).toHaveProperty('unique')
    })
  })

  describe('响应式设计测试', () => {
    beforeEach(() => {
      shareStore.fetchShareStats.mockResolvedValue({
        totalAccessCount: 150,
        uniqueAccessCount: 45,
        firstAccessedAt: '2024-01-01T00:00:00Z',
        lastAccessedAt: '2024-01-10T12:00:00Z',
        dailyStats: [],
        browserStats: [
          {
            browser: 'Chrome',
            accessCount: 80,
            percentage: 53.3
          },
          {
            browser: 'Firefox',
            accessCount: 45,
            percentage: 30.0
          },
          {
            browser: 'Safari',
            accessCount: 25,
            percentage: 16.7
          }
        ],
        osStats: [],
        locationStats: []
      })
    })

    it('应该在移动端调整布局', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      await wrapper.vm.loadStats()
      
      // 模拟移动端视图
      global.innerWidth = 500
      global.dispatchEvent(new Event('resize'))
      
      // 检查是否应用了移动端样式
      expect(wrapper.find('.share-stats').classes()).toContain('mobile-optimized')
    })
  })

  describe('无障碍性测试', () => {
    it('应该支持减少动画', async () => {
      // 模拟减少动画偏好
      Object.defineProperty(window, 'matchMedia', {
        writable: true,
        value: vi.fn().mockImplementation(query => ({
          matches: query.includes('prefers-reduced-motion: reduce'),
          media: query,
          onchange: null,
          addListener: vi.fn(),
          removeListener: vi.fn(),
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
          dispatchEvent: vi.fn(),
        })),
      })

      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(wrapper.find('.share-stats').classes()).toContain('reduced-motion')
    })

    it('应该支持高对比度模式', async () => {
      // 模拟高对比度偏好
      Object.defineProperty(window, 'matchMedia', {
        writable: true,
        value: vi.fn().mockImplementation(query => ({
          matches: query.includes('prefers-contrast: high'),
          media: query,
          onchange: null,
          addListener: vi.fn(),
          removeListener: vi.fn(),
          addEventListener: vi.fn(),
          removeEventListener: vi.fn(),
          dispatchEvent: vi.fn(),
        })),
      })

      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(wrapper.find('.share-stats').classes()).toContain('high-contrast')
    })
  })

  describe('生命周期测试', () => {
    it('应该在组件挂载时加载数据', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      expect(shareStore.fetchShareStats).toHaveBeenCalledWith('test-token-id')
    })

    it('应该在visible属性变化时重新加载数据', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: false
        }
      })

      // 显示组件
      await wrapper.setProps({ visible: true })
      
      expect(shareStore.fetchShareStats).toHaveBeenCalledWith('test-token-id')
    })

    it('应该在tokenId变化时重新加载数据', async () => {
      wrapper = mount(ShareStats, {
        props: {
          tokenId: 'test-token-id',
          visible: true
        }
      })

      // 改变tokenId
      await wrapper.setProps({ tokenId: 'new-token-id' })
      
      expect(shareStore.fetchShareStats).toHaveBeenCalledWith('new-token-id')
    })
  })
})