# 评论API服务使用指南

## 概述

本评论API服务提供了完整的评论系统功能，包括评论的CRUD操作、点赞、举报、审核、统计等功能。服务基于Vue 3 + Composition API开发，使用TypeScript确保类型安全。

## 文件结构

```
frontend/src/
├── services/
│   ├── commentService.ts          # 主要评论API服务
│   ├── commentAdminService.ts     # 管理员功能API服务
│   └── index.ts                    # 统一导出文件
├── composables/
│   └── useComments.ts             # 组合式API
├── utils/
│   └── commentUtils.ts            # 工具函数和错误处理
└── examples/
    └── CommentExample.vue          # 使用示例
```

## 主要功能

### 1. 评论CRUD操作

```typescript
import { commentService } from '@/services'

// 获取评论列表
const comments = await commentService.getComments({
  snippetId: 'snippet-id',
  page: 1,
  pageSize: 20,
  sortBy: 0 // CommentSort.CREATED_AT_DESC
})

// 创建评论
const newComment = await commentService.createComment({
  content: '这是一个很好的代码片段',
  snippetId: 'snippet-id'
})

// 更新评论
const updatedComment = await commentService.updateComment('comment-id', {
  content: '更新后的评论内容'
})

// 删除评论
await commentService.deleteComment('comment-id')
```

### 2. 评论点赞功能

```typescript
// 点赞评论
const result = await commentService.likeComment('comment-id')
console.log(result.isLiked, result.likeCount)

// 获取点赞列表
const likes = await commentService.getCommentLikes('comment-id', {
  page: 1,
  pageSize: 20
})

// 检查点赞状态
const status = await commentService.getCommentLikeStatus('comment-id')
```

### 3. 评论举报功能

```typescript
// 举报评论
const report = await commentService.reportComment('comment-id', {
  reason: 1, // ReportReason.INAPPROPRIATE
  description: '该评论包含不当内容'
})

// 获取用户举报列表
const reports = await commentService.getMyCommentReports({
  page: 1,
  pageSize: 20
})
```

### 4. 评论审核功能（管理员）

```typescript
import { commentAdminService } from '@/services'

// 获取待审核评论
const pendingComments = await commentAdminService.getPendingComments({
  page: 1,
  pageSize: 20
})

// 审核评论
const moderatedComment = await commentAdminService.moderateComment('comment-id', {
  status: 0, // CommentStatus.NORMAL
  reason: '评论内容合规'
})

// 批量操作
const result = await commentAdminService.batchOperationComments({
  commentIds: ['comment-id-1', 'comment-id-2'],
  operation: 0, // CommentBatchOperationType.DELETE
  reason: '批量删除违规评论'
})
```

### 5. 评论统计功能

```typescript
// 获取评论统计
const stats = await commentService.getCommentStats('snippet-id')

// 获取详细统计
const detailStats = await commentAdminService.getCommentStatsDetail({
  snippetIds: ['snippet-id-1', 'snippet-id-2'],
  startDate: '2024-01-01',
  endDate: '2024-12-31'
})
```

## 组合式API使用

### 使用评论管理

```typescript
import { useComments } from '@/services'

const {
  comments,
  loading,
  error,
  totalCount,
  filter,
  loadComments,
  refreshComments
} = useComments()

// 加载评论
await loadComments({
  snippetId: 'snippet-id',
  page: 1,
  pageSize: 20
})

// 刷新评论
await refreshComments()
```

### 使用评论详情

```typescript
import { useComment } from '@/services'

const {
  comment,
  loading,
  updateComment,
  deleteComment,
  likeComment
} = useComment('comment-id')

// 更新评论
await updateComment('comment-id', {
  content: '更新后的内容'
})
```

## 错误处理

### 自动错误处理

服务内置了错误处理机制，会自动处理网络错误、认证错误、验证错误等：

```typescript
try {
  await commentService.createComment({
    content: '',
    snippetId: 'snippet-id'
  })
} catch (error) {
  // 错误会自动转换为用户友好的消息
  console.error('创建评论失败:', error.message)
}
```

### 自定义错误处理

```typescript
import { CommentErrorHandler } from '@/services'

const error = CommentErrorHandler.handleApiError(apiError)
const userMessage = CommentErrorHandler.getUserFriendlyMessage(error)
```

## 数据验证

### 评论内容验证

```typescript
import { CommentValidator } from '@/services'

const validation = CommentValidator.validateContent('评论内容')
if (!validation.isValid) {
  console.error('验证失败:', validation.errors)
}
```

### 分页参数验证

```typescript
const validation = CommentValidator.validatePagination(1, 20)
if (!validation.isValid) {
  console.error('分页参数无效:', validation.errors)
}
```

## 工具函数

### 时间格式化

```typescript
import { CommentUtils } from '@/services'

const formattedTime = CommentUtils.formatTime('2024-01-01T10:00:00Z')
console.log(formattedTime) // "2小时前"
```

### 文本截断

```typescript
const truncated = CommentUtils.truncateText('长文本内容', 50)
console.log(truncated) // "长文本内容..."
```

## 配置选项

### 评论状态配置

```typescript
import { COMMENT_STATUS_CONFIG } from '@/services'

const status = COMMENT_STATUS_CONFIG[CommentStatus.NORMAL]
console.log(status.label) // "正常"
console.log(status.color) // "success"
```

### 举报原因配置

```typescript
import { REPORT_REASON_CONFIG } from '@/services'

const reason = REPORT_REASON_CONFIG[ReportReason.SPAM]
console.log(reason.label) // "垃圾信息"
console.log(reason.severity) // "low"
```

## 性能优化

### 防抖和节流

```typescript
import { CommentUtils } from '@/services'

// 防抖搜索
const debouncedSearch = CommentUtils.debounce(async (keyword: string) => {
  const results = await commentService.searchComments({
    keyword,
    page: 1,
    pageSize: 20
  })
  return results
}, 300)

// 节流点赞
const throttledLike = CommentUtils.throttle(async (commentId: string) => {
  await commentService.likeComment(commentId)
}, 1000)
```

## 最佳实践

1. **使用组合式API**：优先使用组合式API，它们提供了响应式状态管理和自动错误处理

2. **错误处理**：始终使用try-catch包装API调用，并向用户显示友好的错误消息

3. **加载状态**：利用服务提供的loading状态，为用户提供良好的反馈

4. **数据验证**：在发送请求前使用验证工具验证数据

5. **性能优化**：使用防抖和节流处理高频操作，如搜索和点赞

6. **权限检查**：在执行操作前检查用户权限

## 类型安全

所有服务都使用TypeScript提供完整的类型安全支持：

```typescript
import type {
  Comment,
  CreateCommentRequest,
  CommentFilter,
  CommentStats
} from '@/services'

// 所有参数和返回值都有完整的类型提示
const comment: Comment = await commentService.getComment('id')
```

## 迁移指南

如果您从其他评论系统迁移，可以：

1. 保持现有的组件结构
2. 逐步替换API调用为新的服务
3. 使用组合式API重构状态管理
4. 添加错误处理和数据验证

## 扩展功能

您可以通过以下方式扩展评论服务：

1. 继承现有服务类
2. 添加新的API方法
3. 扩展类型定义
4. 创建自定义的组合式API

## 故障排除

### 常见问题

1. **认证错误**：检查用户是否已登录，token是否有效
2. **权限错误**：确认用户有执行操作的权限
3. **网络错误**：检查网络连接和API服务状态
4. **验证错误**：检查输入数据是否符合要求

### 调试技巧

1. 使用浏览器开发者工具检查网络请求
2. 查看控制台错误信息
3. 使用Vue DevTools检查组件状态
4. 检查API响应数据格式