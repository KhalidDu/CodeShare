<template>
  <div class="version-comparison">
    <div class="comparison-header">
      <div class="version-info">
        <div class="old-version">
          <h4>版本 {{ comparison.oldVersion.versionNumber }}</h4>
          <div class="version-meta">
            <span>{{ comparison.oldVersion.creatorName }}</span>
            <span class="separator">•</span>
            <span>{{ formatDate(comparison.oldVersion.createdAt) }}</span>
          </div>
        </div>
        <div class="comparison-arrow">
          <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <path d="M5 12h14M12 5l7 7-7 7"/>
          </svg>
        </div>
        <div class="new-version">
          <h4>版本 {{ comparison.newVersion.versionNumber }}</h4>
          <div class="version-meta">
            <span>{{ comparison.newVersion.creatorName }}</span>
            <span class="separator">•</span>
            <span>{{ formatDate(comparison.newVersion.createdAt) }}</span>
          </div>
        </div>
      </div>
      
      <div class="comparison-stats">
        <div class="stat-item added">
          <span class="stat-count">{{ addedLines }}</span>
          <span class="stat-label">新增</span>
        </div>
        <div class="stat-item removed">
          <span class="stat-count">{{ removedLines }}</span>
          <span class="stat-label">删除</span>
        </div>
        <div class="stat-item modified">
          <span class="stat-count">{{ modifiedLines }}</span>
          <span class="stat-label">修改</span>
        </div>
      </div>
    </div>

    <div class="comparison-controls">
      <div class="view-mode-toggle">
        <button 
          @click="viewMode = 'split'"
          :class="{ active: viewMode === 'split' }"
          class="btn btn-sm"
        >
          分屏对比
        </button>
        <button 
          @click="viewMode = 'unified'"
          :class="{ active: viewMode === 'unified' }"
          class="btn btn-sm"
        >
          统一视图
        </button>
      </div>
      
      <div class="diff-options">
        <label class="checkbox-label">
          <input 
            type="checkbox" 
            v-model="showWhitespace"
            @change="updateDiff"
          />
          显示空白字符
        </label>
        <label class="checkbox-label">
          <input 
            type="checkbox" 
            v-model="ignoreWhitespace"
            @change="updateDiff"
          />
          忽略空白差异
        </label>
      </div>
    </div>

    <!-- 分屏对比视图 -->
    <div v-if="viewMode === 'split'" class="split-view">
      <div class="split-pane old-pane">
        <div class="pane-header">
          <span class="version-label">版本 {{ comparison.oldVersion.versionNumber }}</span>
          <span class="language-badge">{{ comparison.oldVersion.language }}</span>
        </div>
        <div class="code-container">
          <pre class="code-content"><code 
            v-for="(line, index) in oldVersionLines" 
            :key="`old-${index}`"
            :class="getLineClass(line)"
            class="code-line"
          ><span class="line-number">{{ line.number || '' }}</span><span class="line-content">{{ line.content }}</span></code></pre>
        </div>
      </div>
      
      <div class="split-pane new-pane">
        <div class="pane-header">
          <span class="version-label">版本 {{ comparison.newVersion.versionNumber }}</span>
          <span class="language-badge">{{ comparison.newVersion.language }}</span>
        </div>
        <div class="code-container">
          <pre class="code-content"><code 
            v-for="(line, index) in newVersionLines" 
            :key="`new-${index}`"
            :class="getLineClass(line)"
            class="code-line"
          ><span class="line-number">{{ line.number || '' }}</span><span class="line-content">{{ line.content }}</span></code></pre>
        </div>
      </div>
    </div>

    <!-- 统一视图 -->
    <div v-else class="unified-view">
      <div class="unified-header">
        <span class="file-info">
          {{ comparison.oldVersion.title }} 
          ({{ comparison.oldVersion.language }})
        </span>
      </div>
      <div class="code-container">
        <pre class="code-content"><code 
          v-for="(line, index) in unifiedLines" 
          :key="`unified-${index}`"
          :class="getLineClass(line)"
          class="code-line"
        ><span class="line-numbers">
          <span class="old-line-number">{{ line.oldNumber || '' }}</span>
          <span class="new-line-number">{{ line.newNumber || '' }}</span>
        </span><span class="line-prefix">{{ line.prefix }}</span><span class="line-content">{{ line.content }}</span></code></pre>
      </div>
    </div>

    <!-- 差异摘要 -->
    <div v-if="comparison.differences.length > 0" class="diff-summary">
      <h5>变更摘要</h5>
      <div class="diff-list">
        <div 
          v-for="(diff, index) in comparison.differences" 
          :key="index"
          class="diff-item"
          :class="diff.type"
        >
          <div class="diff-type-icon">
            <span v-if="diff.type === 'added'">+</span>
            <span v-else-if="diff.type === 'removed'">-</span>
            <span v-else>~</span>
          </div>
          <div class="diff-content">
            <div class="diff-line">第 {{ diff.lineNumber }} 行</div>
            <div v-if="diff.oldContent" class="diff-old">
              <span class="diff-label">原内容:</span>
              <code>{{ diff.oldContent }}</code>
            </div>
            <div v-if="diff.newContent" class="diff-new">
              <span class="diff-label">新内容:</span>
              <code>{{ diff.newContent }}</code>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import type { VersionComparison } from '@/types'

interface Props {
  comparison: VersionComparison
}

const props = defineProps<Props>()

// 响应式数据
const viewMode = ref<'split' | 'unified'>('split')
const showWhitespace = ref(false)
const ignoreWhitespace = ref(false)

// 代码行数据结构
interface CodeLine {
  number?: number
  content: string
  type?: 'added' | 'removed' | 'modified' | 'unchanged'
  oldNumber?: number
  newNumber?: number
  prefix?: string
}

// 计算属性
const addedLines = computed(() => 
  props.comparison.differences.filter(d => d.type === 'added').length
)

const removedLines = computed(() => 
  props.comparison.differences.filter(d => d.type === 'removed').length
)

const modifiedLines = computed(() => 
  props.comparison.differences.filter(d => d.type === 'modified').length
)

const oldVersionLines = computed(() => {
  return processCodeLines(props.comparison.oldVersion.code, 'old')
})

const newVersionLines = computed(() => {
  return processCodeLines(props.comparison.newVersion.code, 'new')
})

const unifiedLines = computed(() => {
  return generateUnifiedDiff()
})

/**
 * 处理代码行，添加行号和差异信息
 */
const processCodeLines = (code: string, version: 'old' | 'new'): CodeLine[] => {
  const lines = code.split('\n')
  const differences = props.comparison.differences
  
  return lines.map((content, index) => {
    const lineNumber = index + 1
    const diff = differences.find(d => d.lineNumber === lineNumber)
    
    let type: CodeLine['type'] = 'unchanged'
    if (diff) {
      if (version === 'old' && (diff.type === 'removed' || diff.type === 'modified')) {
        type = diff.type
      } else if (version === 'new' && (diff.type === 'added' || diff.type === 'modified')) {
        type = diff.type
      }
    }
    
    return {
      number: lineNumber,
      content: showWhitespace.value ? content.replace(/ /g, '·').replace(/\t/g, '→') : content,
      type
    }
  })
}

/**
 * 生成统一差异视图
 */
const generateUnifiedDiff = (): CodeLine[] => {
  const oldLines = props.comparison.oldVersion.code.split('\n')
  const newLines = props.comparison.newVersion.code.split('\n')
  const differences = props.comparison.differences
  const result: CodeLine[] = []
  
  // 创建差异映射以便快速查找
  const diffMap = new Map<number, typeof differences[0]>()
  differences.forEach(diff => {
    diffMap.set(diff.lineNumber, diff)
  })
  
  let oldIndex = 0
  let newIndex = 0
  const maxLines = Math.max(oldLines.length, newLines.length)
  
  for (let i = 0; i < maxLines; i++) {
    const oldLine = oldLines[oldIndex]
    const newLine = newLines[newIndex]
    const diff = diffMap.get(i + 1)
    
    if (diff) {
      if (diff.type === 'removed' && oldLine !== undefined) {
        result.push({
          oldNumber: oldIndex + 1,
          content: showWhitespace.value ? oldLine.replace(/ /g, '·').replace(/\t/g, '→') : oldLine,
          type: 'removed',
          prefix: '-'
        })
        oldIndex++
      } else if (diff.type === 'added' && newLine !== undefined) {
        result.push({
          newNumber: newIndex + 1,
          content: showWhitespace.value ? newLine.replace(/ /g, '·').replace(/\t/g, '→') : newLine,
          type: 'added',
          prefix: '+'
        })
        newIndex++
      } else if (diff.type === 'modified') {
        if (oldLine !== undefined) {
          result.push({
            oldNumber: oldIndex + 1,
            content: showWhitespace.value ? oldLine.replace(/ /g, '·').replace(/\t/g, '→') : oldLine,
            type: 'removed',
            prefix: '-'
          })
          oldIndex++
        }
        if (newLine !== undefined) {
          result.push({
            newNumber: newIndex + 1,
            content: showWhitespace.value ? newLine.replace(/ /g, '·').replace(/\t/g, '→') : newLine,
            type: 'added',
            prefix: '+'
          })
          newIndex++
        }
      }
    } else {
      // 未更改的行
      if (oldLine !== undefined && newLine !== undefined && oldLine === newLine) {
        result.push({
          oldNumber: oldIndex + 1,
          newNumber: newIndex + 1,
          content: showWhitespace.value ? oldLine.replace(/ /g, '·').replace(/\t/g, '→') : oldLine,
          type: 'unchanged',
          prefix: ' '
        })
        oldIndex++
        newIndex++
      } else {
        // 处理行数不匹配的情况
        if (oldLine !== undefined) {
          result.push({
            oldNumber: oldIndex + 1,
            newNumber: newIndex + 1,
            content: showWhitespace.value ? oldLine.replace(/ /g, '·').replace(/\t/g, '→') : oldLine,
            type: 'unchanged',
            prefix: ' '
          })
          oldIndex++
          newIndex++
        }
      }
    }
  }
  
  return result
}

/**
 * 获取代码行的CSS类
 */
const getLineClass = (line: CodeLine) => {
  const classes = []
  
  if (line.type) {
    classes.push(`line-${line.type}`)
  }
  
  return classes.join(' ')
}

/**
 * 更新差异显示
 */
const updateDiff = () => {
  // 这里可以重新计算差异，考虑空白字符选项
  // 实际实现中可能需要调用后端API重新计算差异
}

/**
 * 格式化日期
 */
const formatDate = (dateString: string) => {
  const date = new Date(dateString)
  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

onMounted(() => {
  // 组件挂载后的初始化逻辑
})
</script>

<style scoped>
.version-comparison {
  background: white;
  min-height: 500px;
}

.comparison-header {
  padding: 1rem;
  border-bottom: 1px solid #e1e5e9;
  background: #f6f8fa;
}

.version-info {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 2rem;
  margin-bottom: 1rem;
}

.old-version, .new-version {
  text-align: center;
}

.old-version h4, .new-version h4 {
  margin: 0 0 0.5rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #24292f;
}

.version-meta {
  font-size: 0.875rem;
  color: #656d76;
  display: flex;
  align-items: center;
  gap: 0.5rem;
  justify-content: center;
}

.separator {
  color: #d0d7de;
}

.comparison-arrow {
  color: #656d76;
  display: flex;
  align-items: center;
}

.comparison-stats {
  display: flex;
  justify-content: center;
  gap: 2rem;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.25rem;
}

.stat-count {
  font-size: 1.5rem;
  font-weight: 700;
}

.stat-label {
  font-size: 0.75rem;
  text-transform: uppercase;
  font-weight: 600;
}

.stat-item.added .stat-count {
  color: #1a7f37;
}

.stat-item.removed .stat-count {
  color: #d1242f;
}

.stat-item.modified .stat-count {
  color: #bf8700;
}

.comparison-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  border-bottom: 1px solid #e1e5e9;
  background: white;
}

.view-mode-toggle {
  display: flex;
  border: 1px solid #d0d7de;
  border-radius: 6px;
  overflow: hidden;
}

.view-mode-toggle .btn {
  border: none;
  border-radius: 0;
  background: white;
  color: #24292f;
}

.view-mode-toggle .btn.active {
  background: #0969da;
  color: white;
}

.diff-options {
  display: flex;
  gap: 1rem;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: #24292f;
  cursor: pointer;
}

.checkbox-label input[type="checkbox"] {
  margin: 0;
}

/* 分屏视图 */
.split-view {
  display: flex;
  height: 600px;
}

.split-pane {
  flex: 1;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #e1e5e9;
}

.split-pane:last-child {
  border-right: none;
}

.pane-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0.5rem 1rem;
  background: #f1f3f4;
  border-bottom: 1px solid #d0d7de;
  font-size: 0.875rem;
}

.version-label {
  font-weight: 600;
  color: #24292f;
}

.language-badge {
  background: #0969da;
  color: white;
  font-size: 0.75rem;
  padding: 0.25rem 0.5rem;
  border-radius: 4px;
  font-weight: 500;
}

/* 统一视图 */
.unified-view {
  display: flex;
  flex-direction: column;
  height: 600px;
}

.unified-header {
  padding: 0.5rem 1rem;
  background: #f1f3f4;
  border-bottom: 1px solid #d0d7de;
  font-size: 0.875rem;
  font-weight: 600;
  color: #24292f;
}

/* 代码容器 */
.code-container {
  flex: 1;
  overflow: auto;
  background: #ffffff;
}

.code-content {
  margin: 0;
  padding: 0;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  font-size: 0.875rem;
  line-height: 1.45;
}

.code-line {
  display: block;
  padding: 0;
  margin: 0;
  white-space: pre;
  border: none;
  background: none;
}

.line-number {
  display: inline-block;
  width: 3rem;
  padding: 0 0.5rem;
  text-align: right;
  color: #656d76;
  background: #f6f8fa;
  border-right: 1px solid #e1e5e9;
  user-select: none;
}

.line-numbers {
  display: inline-block;
  background: #f6f8fa;
  border-right: 1px solid #e1e5e9;
  user-select: none;
}

.old-line-number, .new-line-number {
  display: inline-block;
  width: 2.5rem;
  padding: 0 0.25rem;
  text-align: right;
  color: #656d76;
}

.line-prefix {
  display: inline-block;
  width: 1rem;
  text-align: center;
  font-weight: bold;
  user-select: none;
}

.line-content {
  padding-left: 0.5rem;
}

/* 差异行样式 */
.line-added {
  background-color: #d1f4d1;
}

.line-added .line-number,
.line-added .line-numbers {
  background-color: #a2d2a2;
}

.line-added .line-prefix {
  color: #1a7f37;
}

.line-removed {
  background-color: #ffd7d5;
}

.line-removed .line-number,
.line-removed .line-numbers {
  background-color: #f1aeb5;
}

.line-removed .line-prefix {
  color: #d1242f;
}

.line-modified {
  background-color: #fff8c5;
}

.line-modified .line-number,
.line-modified .line-numbers {
  background-color: #f7e98e;
}

/* 差异摘要 */
.diff-summary {
  padding: 1rem;
  border-top: 1px solid #e1e5e9;
  background: #f6f8fa;
}

.diff-summary h5 {
  margin: 0 0 1rem 0;
  font-size: 1rem;
  font-weight: 600;
  color: #24292f;
}

.diff-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.diff-item {
  display: flex;
  gap: 0.75rem;
  padding: 0.75rem;
  background: white;
  border-radius: 6px;
  border-left: 3px solid #e1e5e9;
}

.diff-item.added {
  border-left-color: #1a7f37;
}

.diff-item.removed {
  border-left-color: #d1242f;
}

.diff-item.modified {
  border-left-color: #bf8700;
}

.diff-type-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 1.5rem;
  height: 1.5rem;
  border-radius: 50%;
  font-weight: bold;
  font-size: 0.875rem;
}

.diff-item.added .diff-type-icon {
  background: #1a7f37;
  color: white;
}

.diff-item.removed .diff-type-icon {
  background: #d1242f;
  color: white;
}

.diff-item.modified .diff-type-icon {
  background: #bf8700;
  color: white;
}

.diff-content {
  flex: 1;
}

.diff-line {
  font-weight: 600;
  color: #24292f;
  margin-bottom: 0.5rem;
}

.diff-old, .diff-new {
  margin: 0.25rem 0;
  font-size: 0.875rem;
}

.diff-label {
  font-weight: 500;
  color: #656d76;
  margin-right: 0.5rem;
}

.diff-old code, .diff-new code {
  background: #f6f8fa;
  padding: 0.125rem 0.25rem;
  border-radius: 3px;
  font-family: 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
}

/* 按钮样式 */
.btn {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.375rem 0.75rem;
  font-size: 0.875rem;
  font-weight: 500;
  line-height: 1.5;
  border-radius: 6px;
  border: 1px solid transparent;
  text-decoration: none;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-sm {
  padding: 0.25rem 0.5rem;
  font-size: 0.75rem;
}
</style>