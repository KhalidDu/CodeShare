# 标签管理 API 文档

## 概述

标签管理API提供了完整的标签CRUD操作、自动补全搜索和使用统计功能。所有API端点都需要用户认证，部分操作需要特定角色权限。

## 基础信息

- **基础URL**: `/api/tags`
- **认证方式**: JWT Bearer Token
- **内容类型**: `application/json`

## API 端点

### 1. 获取所有标签

获取系统中所有可用的标签列表。

**请求**
```http
GET /api/tags
Authorization: Bearer {token}
```

**响应**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "JavaScript",
    "color": "#f7df1e",
    "createdBy": "550e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

### 2. 获取标签详情

根据ID获取特定标签的详细信息。

**请求**
```http
GET /api/tags/{id}
Authorization: Bearer {token}
```

**路径参数**
- `id` (GUID): 标签ID

**响应**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "JavaScript",
  "color": "#f7df1e",
  "createdBy": "550e8400-e29b-41d4-a716-446655440001",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

**错误响应**
- `404 Not Found`: 标签不存在

### 3. 创建标签

创建新的标签。需要编辑者或管理员权限。

**请求**
```http
POST /api/tags
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "React",
  "color": "#61dafb"
}
```

**请求体参数**
- `name` (string, 必需): 标签名称，最大长度50字符
- `color` (string, 可选): 十六进制颜色代码，默认为 "#007bff"

**响应**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "name": "React",
  "color": "#61dafb",
  "createdBy": "550e8400-e29b-41d4-a716-446655440001",
  "createdAt": "2024-01-15T11:00:00Z"
}
```

**错误响应**
- `400 Bad Request`: 输入验证失败
- `409 Conflict`: 标签名称已存在
- `403 Forbidden`: 权限不足

### 4. 更新标签

更新现有标签的信息。需要编辑者或管理员权限。

**请求**
```http
PUT /api/tags/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "React.js",
  "color": "#61dafb"
}
```

**路径参数**
- `id` (GUID): 标签ID

**请求体参数**
- `name` (string, 可选): 新的标签名称
- `color` (string, 可选): 新的颜色代码

**响应**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "name": "React.js",
  "color": "#61dafb",
  "createdBy": "550e8400-e29b-41d4-a716-446655440001",
  "createdAt": "2024-01-15T11:00:00Z"
}
```

**错误响应**
- `400 Bad Request`: 输入验证失败
- `404 Not Found`: 标签不存在
- `409 Conflict`: 标签名称已存在
- `403 Forbidden`: 权限不足

### 5. 删除标签

删除指定的标签。需要管理员权限。

**请求**
```http
DELETE /api/tags/{id}
Authorization: Bearer {token}
```

**路径参数**
- `id` (GUID): 标签ID

**响应**
- `204 No Content`: 删除成功

**错误响应**
- `400 Bad Request`: 标签正在被使用，无法删除
- `404 Not Found`: 标签不存在
- `403 Forbidden`: 权限不足

### 6. 搜索标签（自动补全）

根据名称前缀搜索标签，用于自动补全功能。

**请求**
```http
GET /api/tags/search?prefix=java&limit=10
Authorization: Bearer {token}
```

**查询参数**
- `prefix` (string, 必需): 搜索前缀
- `limit` (int, 可选): 结果数量限制，默认10，最大50

**响应**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "JavaScript",
    "color": "#f7df1e",
    "createdBy": "550e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z"
  },
  {
    "id": "550e8400-e29b-41d4-a716-446655440003",
    "name": "Java",
    "color": "#ed8b00",
    "createdBy": "550e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:45:00Z"
  }
]
```

**错误响应**
- `400 Bad Request`: 搜索前缀为空

### 7. 获取标签使用统计

获取所有标签的使用统计信息。需要管理员权限。

**请求**
```http
GET /api/tags/statistics
Authorization: Bearer {token}
```

**响应**
```json
[
  {
    "tagId": "550e8400-e29b-41d4-a716-446655440000",
    "tagName": "JavaScript",
    "tagColor": "#f7df1e",
    "usageCount": 25,
    "createdAt": "2024-01-15T10:30:00Z"
  },
  {
    "tagId": "550e8400-e29b-41d4-a716-446655440003",
    "tagName": "Java",
    "tagColor": "#ed8b00",
    "usageCount": 18,
    "createdAt": "2024-01-15T10:45:00Z"
  }
]
```

**错误响应**
- `403 Forbidden`: 权限不足

### 8. 获取最常用标签

获取使用次数最多的标签列表。

**请求**
```http
GET /api/tags/most-used?limit=20
Authorization: Bearer {token}
```

**查询参数**
- `limit` (int, 可选): 结果数量限制，默认20，最大100

**响应**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "JavaScript",
    "color": "#f7df1e",
    "createdBy": "550e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

### 9. 检查标签是否可删除

检查指定标签是否可以安全删除（未被任何代码片段使用）。需要管理员权限。

**请求**
```http
GET /api/tags/{id}/can-delete
Authorization: Bearer {token}
```

**路径参数**
- `id` (GUID): 标签ID

**响应**
```json
{
  "canDelete": true
}
```

**错误响应**
- `403 Forbidden`: 权限不足

### 10. 获取代码片段关联的标签

获取指定代码片段关联的所有标签。

**请求**
```http
GET /api/tags/by-snippet/{snippetId}
Authorization: Bearer {token}
```

**路径参数**
- `snippetId` (GUID): 代码片段ID

**响应**
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "JavaScript",
    "color": "#f7df1e",
    "createdBy": "550e8400-e29b-41d4-a716-446655440001",
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

## 数据模型

### TagDto
```json
{
  "id": "GUID",
  "name": "string",
  "color": "string (hex color)",
  "createdBy": "GUID",
  "createdAt": "datetime"
}
```

### CreateTagDto
```json
{
  "name": "string (required, max 50 chars)",
  "color": "string (hex color, optional, default: #007bff)"
}
```

### UpdateTagDto
```json
{
  "name": "string (optional, max 50 chars)",
  "color": "string (hex color, optional)"
}
```

### TagUsageDto
```json
{
  "tagId": "GUID",
  "tagName": "string",
  "tagColor": "string",
  "usageCount": "int",
  "createdAt": "datetime"
}
```

## 权限要求

- **查看操作**: 所有认证用户
- **创建/更新操作**: 编辑者或管理员
- **删除操作**: 仅管理员
- **统计查看**: 仅管理员

## 错误处理

所有API端点都遵循统一的错误响应格式：

```json
{
  "message": "错误描述信息"
}
```

常见HTTP状态码：
- `200 OK`: 请求成功
- `201 Created`: 资源创建成功
- `204 No Content`: 删除成功
- `400 Bad Request`: 请求参数错误
- `401 Unauthorized`: 未认证
- `403 Forbidden`: 权限不足
- `404 Not Found`: 资源不存在
- `409 Conflict`: 资源冲突
- `500 Internal Server Error`: 服务器内部错误

## 使用示例

### 创建标签并搜索
```javascript
// 创建标签
const createResponse = await fetch('/api/tags', {
  method: 'POST',
  headers: {
    'Authorization': 'Bearer ' + token,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    name: 'Vue.js',
    color: '#4fc08d'
  })
});

// 搜索标签用于自动补全
const searchResponse = await fetch('/api/tags/search?prefix=vue&limit=5', {
  headers: {
    'Authorization': 'Bearer ' + token
  }
});
```

### 获取使用统计
```javascript
// 获取标签使用统计（管理员）
const statsResponse = await fetch('/api/tags/statistics', {
  headers: {
    'Authorization': 'Bearer ' + adminToken
  }
});

const statistics = await statsResponse.json();
console.log('标签使用统计:', statistics);
```