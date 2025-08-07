# 复制功能 API 文档

## 概述

复制功能 API 提供了完整的代码片段复制记录、剪贴板历史管理和复制统计功能。该 API 遵循 RESTful 设计原则，支持已登录用户和匿名用户的不同使用场景。

## API 端点

### 1. 记录代码片段复制操作

**端点**: `POST /api/clipboard/copy/{snippetId}`

**描述**: 记录代码片段的复制操作，增加复制计数，并为已登录用户创建剪贴板历史记录。

**参数**:
- `snippetId` (路径参数): 代码片段的唯一标识符

**响应**:
- **已登录用户**: 返回剪贴板历史记录
- **匿名用户**: 返回复制成功消息

**示例请求**:
```http
POST /api/clipboard/copy/123e4567-e89b-12d3-a456-426614174000
Authorization: Bearer <token>
```

**示例响应** (已登录用户):
```json
{
  "id": "987fcdeb-51a2-43d1-9f12-123456789abc",
  "userId": "456e7890-e89b-12d3-a456-426614174000",
  "snippetId": "123e4567-e89b-12d3-a456-426614174000",
  "copiedAt": "2024-01-15T10:30:00Z"
}
```

**示例响应** (匿名用户):
```json
{
  "message": "复制成功",
  "snippetId": "123e4567-e89b-12d3-a456-426614174000"
}
```

### 2. 获取剪贴板历史

**端点**: `GET /api/clipboard/history`

**描述**: 获取当前用户的剪贴板历史记录，包含相关的代码片段信息。

**权限**: 需要登录

**查询参数**:
- `limit` (可选): 返回记录数限制，默认50条，最大100条

**示例请求**:
```http
GET /api/clipboard/history?limit=20
Authorization: Bearer <token>
```

**示例响应**:
```json
[
  {
    "id": "987fcdeb-51a2-43d1-9f12-123456789abc",
    "userId": "456e7890-e89b-12d3-a456-426614174000",
    "snippetId": "123e4567-e89b-12d3-a456-426614174000",
    "copiedAt": "2024-01-15T10:30:00Z",
    "snippet": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "title": "JavaScript 数组去重",
      "description": "使用 Set 实现数组去重",
      "code": "const unique = [...new Set(array)];",
      "language": "javascript",
      "createdAt": "2024-01-10T08:00:00Z",
      "copyCount": 15,
      "viewCount": 45
    }
  }
]
```

### 3. 获取剪贴板历史记录数量

**端点**: `GET /api/clipboard/history/count`

**描述**: 获取当前用户的剪贴板历史记录总数。

**权限**: 需要登录

**示例请求**:
```http
GET /api/clipboard/history/count
Authorization: Bearer <token>
```

**示例响应**:
```json
25
```

### 4. 清空剪贴板历史

**端点**: `DELETE /api/clipboard/history`

**描述**: 清空当前用户的所有剪贴板历史记录。

**权限**: 需要登录

**示例请求**:
```http
DELETE /api/clipboard/history
Authorization: Bearer <token>
```

**示例响应**:
```json
{
  "message": "剪贴板历史已清空"
}
```

### 5. 重新复制历史记录

**端点**: `POST /api/clipboard/history/{historyId}/recopy`

**描述**: 从历史记录中重新复制代码片段，创建新的复制记录。

**权限**: 需要登录

**参数**:
- `historyId` (路径参数): 历史记录的唯一标识符

**示例请求**:
```http
POST /api/clipboard/history/987fcdeb-51a2-43d1-9f12-123456789abc/recopy
Authorization: Bearer <token>
```

**示例响应**:
```json
{
  "id": "111fcdeb-51a2-43d1-9f12-123456789def",
  "userId": "456e7890-e89b-12d3-a456-426614174000",
  "snippetId": "123e4567-e89b-12d3-a456-426614174000",
  "copiedAt": "2024-01-15T11:00:00Z"
}
```

### 6. 获取代码片段复制统计

**端点**: `GET /api/clipboard/stats/{snippetId}`

**描述**: 获取指定代码片段的复制统计信息。

**参数**:
- `snippetId` (路径参数): 代码片段的唯一标识符

**示例请求**:
```http
GET /api/clipboard/stats/123e4567-e89b-12d3-a456-426614174000
```

**示例响应**:
```json
{
  "snippetId": "123e4567-e89b-12d3-a456-426614174000",
  "totalCopyCount": 15,
  "viewCount": 45,
  "lastCopiedAt": "2024-01-15T10:30:00Z"
}
```

### 7. 批量获取复制统计

**端点**: `POST /api/clipboard/stats/batch`

**描述**: 批量获取多个代码片段的复制统计信息。

**请求体**:
```json
{
  "snippetIds": [
    "123e4567-e89b-12d3-a456-426614174000",
    "456e7890-e89b-12d3-a456-426614174001"
  ]
}
```

**示例响应**:
```json
{
  "123e4567-e89b-12d3-a456-426614174000": 15,
  "456e7890-e89b-12d3-a456-426614174001": 8
}
```

## 错误响应

所有端点都可能返回以下错误响应：

### 400 Bad Request
```json
{
  "message": "请求参数无效"
}
```

### 401 Unauthorized
```json
{
  "message": "用户未登录"
}
```

### 403 Forbidden
```json
{
  "message": "无权限访问"
}
```

### 404 Not Found
```json
{
  "message": "资源不存在"
}
```

### 500 Internal Server Error
```json
{
  "message": "服务器内部错误"
}
```

## 业务规则

### 剪贴板历史管理
1. **历史记录限制**: 每个用户最多保存100条剪贴板历史记录（可配置）
2. **自动清理**: 当达到限制时，自动删除最旧的记录
3. **权限验证**: 用户只能访问自己的剪贴板历史

### 复制统计
1. **匿名用户**: 可以复制公开的代码片段，但不记录历史
2. **已登录用户**: 可以复制有权限的代码片段，并记录历史
3. **统计更新**: 每次复制操作都会增加代码片段的复制计数

### 权限控制
1. **公开代码片段**: 所有用户都可以复制
2. **私有代码片段**: 只有创建者和有权限的用户可以复制
3. **历史记录**: 只有已登录用户才能查看和管理自己的历史

## 配置选项

在 `appsettings.json` 中可以配置以下选项：

```json
{
  "ClipboardSettings": {
    "MaxHistoryCount": 100,
    "HistoryRetentionDays": 30
  }
}
```

- `MaxHistoryCount`: 每个用户的最大历史记录数
- `HistoryRetentionDays`: 历史记录保留天数（用于定期清理）

## 使用场景

### 1. 前端一键复制功能
```javascript
// 复制代码片段
async function copySnippet(snippetId) {
  try {
    const response = await fetch(`/api/clipboard/copy/${snippetId}`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    if (response.ok) {
      showMessage('复制成功');
      // 更新UI显示复制计数
    }
  } catch (error) {
    showError('复制失败');
  }
}
```

### 2. 剪贴板历史页面
```javascript
// 获取剪贴板历史
async function loadClipboardHistory() {
  const response = await fetch('/api/clipboard/history?limit=50', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  const history = await response.json();
  renderHistoryList(history);
}
```

### 3. 统计信息显示
```javascript
// 获取复制统计
async function loadCopyStats(snippetIds) {
  const response = await fetch('/api/clipboard/stats/batch', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ snippetIds })
  });
  
  const stats = await response.json();
  updateStatsDisplay(stats);
}
```

## 注意事项

1. **性能考虑**: 批量统计查询限制最多100个代码片段
2. **数据一致性**: 复制操作使用事务确保数据一致性
3. **缓存策略**: 统计数据可以考虑缓存以提高性能
4. **日志记录**: 所有操作都有详细的日志记录用于监控和调试