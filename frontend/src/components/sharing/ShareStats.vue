<!--
  分享统计组件
  显示分享链接的访问统计、趋势图表和安全提醒
-->
<template>
  <div class="share-stats">
    <!-- 统计概览卡片 -->
    <div class="stats-overview">
      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-eye"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ stats?.totalAccessCount || 0 }}</div>
          <div class="stat-label">总访问次数</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-users"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ stats?.uniqueAccessCount || 0 }}</div>
          <div class="stat-label">独立访客</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-clock"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ stats?.lastAccessedAt ? formatDate(stats.lastAccessedAt) : '无' }}</div>
          <div class="stat-label">最后访问</div>
        </div>
      </div>
      
      <div class="stat-card">
        <div class="stat-icon">
          <i class="fas fa-calendar"></i>
        </div>
        <div class="stat-content">
          <div class="stat-value">{{ stats?.firstAccessedAt ? formatDate(stats.firstAccessedAt) : '无' }}</div>
          <div class="stat-label">首次访问</div>
        </div>
      </div>
    </div>

    <!-- 安全提醒 -->
    <div v-if="showSecurityWarning" class="security-warning">
      <div class="warning-icon">
        <i class="fas fa-exclamation-triangle"></i>
      </div>
      <div class="warning-content">
        <h3 class="warning-title">安全提醒</h3>
        <p class="warning-message">
          该分享链接访问量异常，可能存在安全风险。建议检查访问记录或撤销分享。
        </p>
        <div class="warning-actions">
          <button @click="viewAccessRecords" class="btn-view-records">
            <i class="fas fa-list"></i>
            查看访问记录
          </button>
          <button @click="revokeShare" class="btn-revoke">
            <i class="fas fa-ban"></i>
            撤销分享
          </button>
        </div>
      </div>
    </div>

    <!-- 访问趋势图表 -->
    <div class="chart-section">
      <div class="chart-header">
        <h3 class="chart-title">访问趋势</h3>
        <div class="chart-controls">
          <select v-model="chartPeriod" @change="updateChart" class="period-select">
            <option value="7">最近7天</option>
            <option value="14">最近14天</option>
            <option value="30">最近30天</option>
          </select>
        </div>
      </div>
      
      <div class="chart-container">
        <div v-if="isLoading" class="chart-loading">
          <LoadingSpinner size="lg" />
        </div>
        
        <div v-else-if="hasChartData" class="chart">
          <svg :viewBox="`0 0 ${chartWidth} ${chartHeight}`" class="chart-svg">
            <!-- 网格线 -->
            <g class="grid-lines">
              <line
                v-for="(line, index) in gridLines"
                :key="index"
                :x1="line.x1"
                :y1="line.y1"
                :x2="line.x2"
                :y2="line.y2"
                :stroke="line.color"
                :stroke-width="line.width"
                :stroke-dasharray="line.dash"
              />
            </g>
            
            <!-- 数据线 -->
            <polyline
              :points="chartPoints"
              fill="none"
              stroke="#3b82f6"
              stroke-width="2"
              stroke-linejoin="round"
              stroke-linecap="round"
            />
            
            <!-- 数据点 -->
            <g class="data-points">
              <circle
                v-for="(point, index) in chartDataPoints"
                :key="index"
                :cx="point.x"
                :cy="point.y"
                r="4"
                fill="#3b82f6"
                stroke="white"
                stroke-width="2"
                class="data-point"
                @mouseover="showTooltip(point, $event)"
                @mouseout="hideTooltip"
              />
            </g>
            
            <!-- Y轴标签 -->
            <g class="y-labels">
              <text
                v-for="(label, index) in yAxisLabels"
                :key="index"
                :x="label.x"
                :y="label.y"
                text-anchor="end"
                class="axis-label"
              >
                {{ label.text }}
              </text>
            </g>
            
            <!-- X轴标签 -->
            <g class="x-labels">
              <text
                v-for="(label, index) in xAxisLabels"
                :key="index"
                :x="label.x"
                :y="label.y"
                text-anchor="middle"
                class="axis-label"
              >
                {{ label.text }}
              </text>
            </g>
          </svg>
        </div>
        
        <div v-else class="chart-empty">
          <div class="empty-icon">
            <i class="fas fa-chart-line"></i>
          </div>
          <div class="empty-text">暂无访问数据</div>
        </div>
      </div>
    </div>

    <!-- 详细统计 -->
    <div class="detailed-stats">
      <div class="stats-tabs">
        <button
          v-for="tab in statsTabs"
          :key="tab.key"
          @click="activeTab = tab.key"
          :class="['tab-btn', { active: activeTab === tab.key }]"
        >
          <i :class="tab.icon"></i>
          {{ tab.label }}
        </button>
      </div>
      
      <div class="stats-content">
        <!-- 浏览器统计 -->
        <div v-if="activeTab === 'browser'" class="browser-stats">
          <div v-if="stats?.browserStats?.length" class="stat-list">
            <div
              v-for="browser in stats.browserStats"
              :key="browser.browser"
              class="stat-item"
            >
              <div class="stat-info">
                <div class="stat-name">{{ browser.browser }}</div>
                <div class="stat-percentage">{{ browser.percentage }}%</div>
              </div>
              <div class="stat-bar">
                <div
                  class="stat-bar-fill"
                  :style="{ width: `${browser.percentage}%` }"
                ></div>
              </div>
              <div class="stat-count">{{ browser.accessCount }} 次</div>
            </div>
          </div>
          <div v-else class="empty-state">
            <div class="empty-text">暂无浏览器统计</div>
          </div>
        </div>
        
        <!-- 操作系统统计 -->
        <div v-if="activeTab === 'os'" class="os-stats">
          <div v-if="stats?.osStats?.length" class="stat-list">
            <div
              v-for="os in stats.osStats"
              :key="os.os"
              class="stat-item"
            >
              <div class="stat-info">
                <div class="stat-name">{{ os.os }}</div>
                <div class="stat-percentage">{{ os.percentage }}%</div>
              </div>
              <div class="stat-bar">
                <div
                  class="stat-bar-fill"
                  :style="{ width: `${os.percentage}%` }"
                ></div>
              </div>
              <div class="stat-count">{{ os.accessCount }} 次</div>
            </div>
          </div>
          <div v-else class="empty-state">
            <div class="empty-text">暂无操作系统统计</div>
          </div>
        </div>
        
        <!-- 地理位置统计 -->
        <div v-if="activeTab === 'location'" class="location-stats">
          <div v-if="stats?.locationStats?.length" class="stat-list">
            <div
              v-for="location in stats.locationStats"
              :key="location.country"
              class="stat-item"
            >
              <div class="stat-info">
                <div class="stat-name">
                  {{ location.country }}
                  <span v-if="location.city" class="stat-subname">({{ location.city }})</span>
                </div>
                <div class="stat-percentage">{{ location.percentage }}%</div>
              </div>
              <div class="stat-bar">
                <div
                  class="stat-bar-fill"
                  :style="{ width: `${location.percentage}%` }"
                ></div>
              </div>
              <div class="stat-count">{{ location.accessCount }} 次</div>
            </div>
          </div>
          <div v-else class="empty-state">
            <div class="empty-text">暂无地理位置统计</div>
          </div>
        </div>
      </div>
    </div>

    <!-- 工具提示 -->
    <div
      v-if="tooltip.visible"
      class="tooltip"
      :style="{
        left: `${tooltip.x}px`,
        top: `${tooltip.y}px`
      }"
    >
      <div class="tooltip-content">
        <div class="tooltip-date">{{ tooltip.date }}</div>
        <div class="tooltip-count">访问次数: {{ tooltip.count }}</div>
        <div class="tooltip-unique">独立访客: {{ tooltip.unique }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useShareStore } from '@/stores/share'
import { useToastStore } from '@/stores/toast'
import LoadingSpinner from '@/components/common/LoadingSpinner.vue'
import type { ShareStats, DailyShareStats } from '@/types/share'

// 组件属性
interface Props {
  tokenId: string
  visible?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  visible: true
})

// 组件事件
const emit = defineEmits<{
  close: []
  viewAccessRecords: [tokenId: string]
  revokeShare: [tokenId: string]
}>()

// Store注入
const shareStore = useShareStore()
const toastStore = useToastStore()

// 响应式状态
const stats = ref<ShareStats | null>(null)
const isLoading = ref(false)
const chartPeriod = ref('7')
const activeTab = ref('browser')
const tooltip = ref({
  visible: false,
  x: 0,
  y: 0,
  date: '',
  count: 0,
  unique: 0
})

// 图表配置
const chartWidth = 800
const chartHeight = 300
const chartPadding = 60

// 统计标签页
const statsTabs = [
  { key: 'browser', label: '浏览器', icon: 'fas fa-globe' },
  { key: 'os', label: '操作系统', icon: 'fas fa-desktop' },
  { key: 'location', label: '地理位置', icon: 'fas fa-map-marker-alt' }
]

// 计算属性
const showSecurityWarning = computed(() => {
  if (!stats.value) return false
  // 如果总访问次数超过100次且独立访客较少，显示安全提醒
  return stats.value.totalAccessCount > 100 && 
         stats.value.uniqueAccessCount < stats.value.totalAccessCount * 0.3
})

const hasChartData = computed(() => {
  return stats.value?.dailyStats && stats.value.dailyStats.length > 0
})

const chartData = computed(() => {
  if (!stats.value?.dailyStats) return []
  
  const days = parseInt(chartPeriod.value)
  const endDate = new Date()
  const startDate = new Date()
  startDate.setDate(endDate.getDate() - days + 1)
  
  // 生成日期范围内的数据
  const dateMap = new Map<string, DailyShareStats>()
  stats.value.dailyStats.forEach(stat => {
    dateMap.set(stat.date, stat)
  })
  
  const chartData: DailyShareStats[] = []
  const currentDate = new Date(startDate)
  
  while (currentDate <= endDate) {
    const dateStr = currentDate.toISOString().split('T')[0]
    const stat = dateMap.get(dateStr)
    
    chartData.push({
      date: dateStr,
      accessCount: stat?.accessCount || 0,
      uniqueAccessCount: stat?.uniqueAccessCount || 0
    })
    
    currentDate.setDate(currentDate.getDate() + 1)
  }
  
  return chartData
})

const maxAccessCount = computed(() => {
  return Math.max(...chartData.value.map(d => d.accessCount), 1)
})

const chartPoints = computed(() => {
  if (!hasChartData.value) return ''
  
  const xStep = (chartWidth - chartPadding * 2) / (chartData.value.length - 1)
  const yScale = (chartHeight - chartPadding * 2) / maxAccessCount.value
  
  return chartData.value.map((data, index) => {
    const x = chartPadding + index * xStep
    const y = chartHeight - chartPadding - data.accessCount * yScale
    return `${x},${y}`
  }).join(' ')
})

const chartDataPoints = computed(() => {
  if (!hasChartData.value) return []
  
  const xStep = (chartWidth - chartPadding * 2) / (chartData.value.length - 1)
  const yScale = (chartHeight - chartPadding * 2) / maxAccessCount.value
  
  return chartData.value.map((data, index) => ({
    x: chartPadding + index * xStep,
    y: chartHeight - chartPadding - data.accessCount * yScale,
    date: data.date,
    count: data.accessCount,
    unique: data.uniqueAccessCount
  }))
})

const gridLines = computed(() => {
  const lines = []
  
  // 水平网格线
  for (let i = 0; i <= 5; i++) {
    const y = chartPadding + (chartHeight - chartPadding * 2) * i / 5
    lines.push({
      x1: chartPadding,
      y1: y,
      x2: chartWidth - chartPadding,
      y2: y,
      color: '#e5e7eb',
      width: 1,
      dash: '2,2'
    })
  }
  
  return lines
})

const yAxisLabels = computed(() => {
  const labels: Array<{x: number, y: number, text: string}> = []
  
  for (let i = 0; i <= 5; i++) {
    const value = Math.round(maxAccessCount.value * (5 - i) / 5)
    const y = chartPadding + (chartHeight - chartPadding * 2) * i / 5
    labels.push({
      x: chartPadding - 10,
      y: y + 5,
      text: value.toString()
    })
  }
  
  return labels
})

const xAxisLabels = computed(() => {
  const labels: Array<{x: number, y: number, text: string}> = []
  const xStep = (chartWidth - chartPadding * 2) / (chartData.value.length - 1)
  
  chartData.value.forEach((data, index) => {
    const x = chartPadding + index * xStep
    const date = new Date(data.date)
    const label = `${date.getMonth() + 1}/${date.getDate()}`
    
    labels.push({
      x: x,
      y: chartHeight - chartPadding + 20,
      text: label
    })
  })
  
  return labels
})

// 生命周期
onMounted(async () => {
  if (props.visible) {
    await loadStats()
  }
})

watch(() => props.visible, async (newVisible) => {
  if (newVisible) {
    await loadStats()
  }
})

watch(() => props.tokenId, async () => {
  if (props.visible) {
    await loadStats()
  }
})

// 方法
async function loadStats() {
  if (!props.tokenId) return
  
  isLoading.value = true
  try {
    const result = await shareStore.fetchShareStats(props.tokenId)
    stats.value = result || null
  } catch (error) {
    toastStore.error('加载统计数据失败')
  } finally {
    isLoading.value = false
  }
}

function updateChart() {
  // 图表数据会通过计算属性自动更新
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  const now = new Date()
  const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60))
  
  if (diffInHours < 1) {
    return '刚刚'
  } else if (diffInHours < 24) {
    return `${diffInHours}小时前`
  } else if (diffInHours < 48) {
    return '昨天'
  } else {
    return date.toLocaleDateString('zh-CN')
  }
}

function showTooltip(point: any, event: MouseEvent) {
  tooltip.value = {
    visible: true,
    x: event.clientX + 10,
    y: event.clientY - 10,
    date: formatDate(point.date),
    count: point.count,
    unique: point.unique
  }
}

function hideTooltip() {
  tooltip.value.visible = false
}

function viewAccessRecords() {
  emit('viewAccessRecords', props.tokenId)
}

function revokeShare() {
  emit('revokeShare', props.tokenId)
}
</script>

<style scoped>
.share-stats {
  max-width: 1000px;
  margin: 0 auto;
  padding: 2rem;
}

/* 统计概览 */
.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
  margin-bottom: 2rem;
}

.stat-card {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  display: flex;
  align-items: center;
  gap: 1rem;
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: #3b82f6;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.25rem;
}

.stat-value {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
}

.stat-label {
  font-size: 0.875rem;
  color: #6b7280;
}

/* 安全提醒 */
.security-warning {
  background: #fef3c7;
  border: 1px solid #f59e0b;
  border-radius: 12px;
  padding: 1.5rem;
  margin-bottom: 2rem;
  display: flex;
  gap: 1rem;
  align-items: flex-start;
}

.warning-icon {
  width: 40px;
  height: 40px;
  background: #f59e0b;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-size: 1.25rem;
  flex-shrink: 0;
}

.warning-content {
  flex: 1;
}

.warning-title {
  font-size: 1.125rem;
  font-weight: 600;
  color: #92400e;
  margin-bottom: 0.5rem;
}

.warning-message {
  color: #92400e;
  margin-bottom: 1rem;
}

.warning-actions {
  display: flex;
  gap: 0.5rem;
}

.btn-view-records {
  padding: 0.5rem 1rem;
  background: #3b82f6;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-view-records:hover {
  background: #2563eb;
}

.btn-revoke {
  padding: 0.5rem 1rem;
  background: #ef4444;
  color: white;
  border: none;
  border-radius: 6px;
  font-size: 0.875rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.btn-revoke:hover {
  background: #dc2626;
}

/* 图表区域 */
.chart-section {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
  margin-bottom: 2rem;
}

.chart-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.chart-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #1f2937;
}

.chart-controls {
  display: flex;
  gap: 1rem;
  align-items: center;
}

.period-select {
  padding: 0.5rem;
  border: 1px solid #d1d5db;
  border-radius: 6px;
  font-size: 0.875rem;
  background: white;
  cursor: pointer;
}

.chart-container {
  position: relative;
  height: 300px;
}

.chart-loading {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}

.chart {
  width: 100%;
  height: 100%;
}

.chart-svg {
  width: 100%;
  height: 100%;
}

.chart-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  color: #6b7280;
}

.empty-icon {
  font-size: 3rem;
  margin-bottom: 1rem;
  opacity: 0.5;
}

.empty-text {
  font-size: 1rem;
}

.data-point {
  cursor: pointer;
  transition: r 0.2s;
}

.data-point:hover {
  r: 6;
}

.axis-label {
  font-size: 0.75rem;
  fill: #6b7280;
}

/* 详细统计 */
.detailed-stats {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.stats-tabs {
  display: flex;
  gap: 0.5rem;
  margin-bottom: 1.5rem;
  border-bottom: 1px solid #e5e7eb;
}

.tab-btn {
  padding: 0.75rem 1rem;
  background: none;
  border: none;
  border-radius: 6px 6px 0 0;
  font-size: 0.875rem;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: #6b7280;
  transition: all 0.2s;
}

.tab-btn:hover {
  background: #f3f4f6;
  color: #374151;
}

.tab-btn.active {
  background: #3b82f6;
  color: white;
}

.stats-content {
  min-height: 200px;
}

.stat-list {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.stat-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.stat-name {
  font-weight: 500;
  color: #1f2937;
}

.stat-subname {
  font-size: 0.875rem;
  color: #6b7280;
  font-weight: normal;
}

.stat-percentage {
  font-weight: 600;
  color: #3b82f6;
}

.stat-bar {
  height: 8px;
  background: #f3f4f6;
  border-radius: 4px;
  overflow: hidden;
}

.stat-bar-fill {
  height: 100%;
  background: #3b82f6;
  transition: width 0.3s ease;
}

.stat-count {
  font-size: 0.875rem;
  color: #6b7280;
  text-align: right;
}

.empty-state {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: #6b7280;
}

/* 工具提示 */
.tooltip {
  position: fixed;
  background: #1f2937;
  color: white;
  padding: 0.75rem;
  border-radius: 6px;
  font-size: 0.875rem;
  z-index: 1000;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.tooltip-content {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.tooltip-date {
  font-weight: 600;
}

.tooltip-count,
.tooltip-unique {
  font-size: 0.75rem;
  opacity: 0.9;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .share-stats {
    padding: 1rem;
  }
  
  .stats-overview {
    grid-template-columns: repeat(2, 1fr);
  }
  
  .security-warning {
    flex-direction: column;
    text-align: center;
  }
  
  .warning-actions {
    justify-content: center;
  }
  
  .chart-header {
    flex-direction: column;
    gap: 1rem;
    align-items: stretch;
  }
  
  .stats-tabs {
    flex-wrap: wrap;
  }
  
  .stat-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.25rem;
  }
}

@media (max-width: 480px) {
  .stats-overview {
    grid-template-columns: 1fr;
  }
}

/* 无障碍性增强 */
@media (prefers-reduced-motion: reduce) {
  .stat-bar-fill,
  .data-point {
    transition: none;
  }
}

/* 高对比度模式支持 */
@media (prefers-contrast: high) {
  .stat-card,
  .chart-section,
  .detailed-stats {
    border: 2px solid #000;
  }
  
  .security-warning {
    border-width: 2px;
  }
}
</style>