# API 参考文档

## 概述

代码片段管理工具提供 RESTful API，支持代码片段的创建、查询、更新和删除操作。所有 API 都需要 JWT 认证（除了登录和注册接口）。

**基础 URL**: `https://your-domain.com/api`

**认证方式**: Bearer Token (JWT)

**内容类型**: `application/json`

## 认证

### 登录

获取访问令牌。

**端点**: `POST /auth/login`

**请求体**:
```json
{
  "username": "string",
  "password": "string"
}
```

**响应**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": "uuid",
    "username": "string",
    "email": "string",
    "role": "Admin|Editor|Viewer"
  }
}
```

**状态码**:
- `200 OK`: 登录成功
- `400 Bad Request`: 请求参数错误
- `401 Unauthorized`: 用户名或密码错误

### 注册

创建新用户账户。

**端点**: `POST /auth/register`

**请求体**:
```json
{
  "username": "string",
  "email": "string",
  "password": "string"
}
```

**响应**:
```json
{
  "id": "uuid",
  "username": "string",
  "email": "string",
  "role": "Viewer"
}
```

### 刷新令牌

刷新访问令牌。

**端点**: `POST /auth/refresh`

**请求头**: `Authorization: Bearer <token>`

**响应**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-01-01T12:00:00Z"
}
```

## 代码片段

### 获取代码片段列表

获取分页的代码片段列表。

**端点**: `GET /codesnippets`

**查询参数**:
- `page` (int, 可选): 页码，默认为 1
- `pageSize` (int, 可选): 每页数量，默认为 20，最大 100
- `search` (string, 可选): 搜索关键词
- `language` (string, 可选): 编程语言筛选
- `tags` (string[], 可选): 标签筛选
- `createdBy` (string, 可选): 创建者筛选
- `isPublic` (bool, 可选): 是否公开

**示例请求**:
```
GET /api/codesnippets?page=1&pageSize=10&search=javascript&language=javascript&isPublic=true
```

**响应**:
```json
{
  "items": [
    {
      "id": "uuid",
      "title": "JavaScript Array Helper",
      "description": "Utility functions for array manipulation",
      "code": "const unique = arr => [...new Set(arr)];",
      "language": "javascript",
      "tags": ["utility", "array"],
      "isPublic": true,
      "createdBy": "uuid",
      "createdByName": "John Doe",
      "createdAt": "2024-01-01T12:00:00Z",
      "updatedAt": "2024-01-01T12:00:00Z",
      "viewCount": 42,
      "copyCount": 15
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

### 获取代码片段详情

获取单个代码片段的详细信息。

**端点**: `GET /codesnippets/{id}`

**路径参数**:
- `id` (uuid): 代码片段 ID

**响应**:
```json
{
  "id": "uuid",
  "title": "JavaScript Array Helper",
  "description": "Utility functions for array manipulation",
  "code": "const unique = arr => [...new Set(arr)];\nconst flatten = arr => arr.flat(Infinity);",
  "language": "javascript",
  "tags": ["utility", "array"],
  "isPublic": true,
  "createdBy": "uuid",
  "createdByName": "John Doe",
  "createdAt": "2024-01-01T12:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z",
  "viewCount": 42,
  "copyCount": 15
}
```

**状态码**:
- `200 OK`: 成功
- `404 Not Found`: 代码片段不存在
- `403 Forbidden`: 无权限访问私有片段

### 创建代码片段

创建新的代码片段。

**端点**: `POST /codesnippets`

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "title": "JavaScript Array Helper",
  "description": "Utility functions for array manipulation",
  "code": "const unique = arr => [...new Set(arr)];",
  "language": "javascript",
  "tags": ["utility", "array"],
  "isPublic": true
}
```

**响应**:
```json
{
  "id": "uuid",
  "title": "JavaScript Array Helper",
  "description": "Utility functions for array manipulation",
  "code": "const unique = arr => [...new Set(arr)];",
  "language": "javascript",
  "tags": ["utility", "array"],
  "isPublic": true,
  "createdBy": "uuid",
  "createdByName": "John Doe",
  "createdAt": "2024-01-01T12:00:00Z",
  "updatedAt": "2024-01-01T12:00:00Z",
  "viewCount": 0,
  "copyCount": 0
}
```

**状态码**:
- `201 Created`: 创建成功
- `400 Bad Request`: 请求参数错误
- `401 Unauthorized`: 未认证

### 更新代码片段

更新现有的代码片段。

**端点**: `PUT /codesnippets/{id}`

**路径参数**:
- `id` (uuid): 代码片段 ID

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "title": "Updated JavaScript Array Helper",
  "description": "Enhanced utility functions for array manipulation",
  "code": "const unique = arr => [...new Set(arr)];\nconst flatten = arr => arr.flat(Infinity);",
  "language": "javascript",
  "tags": ["utility", "array", "enhanced"],
  "isPublic": true
}
```

**响应**: 与创建代码片段相同

**状态码**:
- `200 OK`: 更新成功
- `400 Bad Request`: 请求参数错误
- `401 Unauthorized`: 未认证
- `403 Forbidden`: 无权限修改
- `404 Not Found`: 代码片段不存在

### 删除代码片段

删除代码片段。

**端点**: `DELETE /codesnippets/{id}`

**路径参数**:
- `id` (uuid): 代码片段 ID

**请求头**: `Authorization: Bearer <token>`

**响应**: 无内容

**状态码**:
- `204 No Content`: 删除成功
- `401 Unauthorized`: 未认证
- `403 Forbidden`: 无权限删除
- `404 Not Found`: 代码片段不存在

## 版本管理

### 获取版本历史

获取代码片段的版本历史。

**端点**: `GET /codesnippets/{id}/versions`

**路径参数**:
- `id` (uuid): 代码片段 ID

**响应**:
```json
[
  {
    "id": "uuid",
    "snippetId": "uuid",
    "versionNumber": 2,
    "title": "Updated JavaScript Array Helper",
    "description": "Enhanced utility functions",
    "code": "const unique = arr => [...new Set(arr)];\nconst flatten = arr => arr.flat(Infinity);",
    "language": "javascript",
    "createdBy": "uuid",
    "createdByName": "John Doe",
    "createdAt": "2024-01-02T12:00:00Z",
    "changeDescription": "Added flatten function"
  },
  {
    "id": "uuid",
    "snippetId": "uuid",
    "versionNumber": 1,
    "title": "JavaScript Array Helper",
    "description": "Utility functions for array manipulation",
    "code": "const unique = arr => [...new Set(arr)];",
    "language": "javascript",
    "createdBy": "uuid",
    "createdByName": "John Doe",
    "createdAt": "2024-01-01T12:00:00Z",
    "changeDescription": "Initial version"
  }
]
```

### 获取特定版本

获取代码片段的特定版本。

**端点**: `GET /codesnippets/{id}/versions/{versionNumber}`

**路径参数**:
- `id` (uuid): 代码片段 ID
- `versionNumber` (int): 版本号

**响应**: 与获取版本历史中的单个版本对象相同

### 恢复版本

将代码片段恢复到指定版本。

**端点**: `POST /codesnippets/{id}/versions/{versionNumber}/restore`

**路径参数**:
- `id` (uuid): 代码片段 ID
- `versionNumber` (int): 要恢复的版本号

**请求头**: `Authorization: Bearer <token>`

**响应**: 恢复后的代码片段对象

**状态码**:
- `200 OK`: 恢复成功
- `401 Unauthorized`: 未认证
- `403 Forbidden`: 无权限操作
- `404 Not Found`: 代码片段或版本不存在

## 标签管理

### 获取标签列表

获取所有标签。

**端点**: `GET /tags`

**查询参数**:
- `search` (string, 可选): 搜索标签名称

**响应**:
```json
[
  {
    "id": "uuid",
    "name": "javascript",
    "color": "#f7df1e",
    "usageCount": 25,
    "createdBy": "uuid",
    "createdAt": "2024-01-01T12:00:00Z"
  },
  {
    "id": "uuid",
    "name": "utility",
    "color": "#007bff",
    "usageCount": 15,
    "createdBy": "uuid",
    "createdAt": "2024-01-01T12:00:00Z"
  }
]
```

### 创建标签

创建新标签。

**端点**: `POST /tags`

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "name": "react",
  "color": "#61dafb"
}
```

**响应**:
```json
{
  "id": "uuid",
  "name": "react",
  "color": "#61dafb",
  "usageCount": 0,
  "createdBy": "uuid",
  "createdAt": "2024-01-01T12:00:00Z"
}
```

### 更新标签

更新标签信息。

**端点**: `PUT /tags/{id}`

**路径参数**:
- `id` (uuid): 标签 ID

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "name": "reactjs",
  "color": "#61dafb"
}
```

### 删除标签

删除标签。

**端点**: `DELETE /tags/{id}`

**路径参数**:
- `id` (uuid): 标签 ID

**请求头**: `Authorization: Bearer <token>`

**状态码**:
- `204 No Content`: 删除成功
- `400 Bad Request`: 标签正在使用中
- `401 Unauthorized`: 未认证
- `403 Forbidden`: 无权限删除

## 剪贴板功能

### 记录复制操作

记录代码片段复制操作。

**端点**: `POST /clipboard/copy`

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "snippetId": "uuid"
}
```

**响应**:
```json
{
  "id": "uuid",
  "snippetId": "uuid",
  "userId": "uuid",
  "copiedAt": "2024-01-01T12:00:00Z"
}
```

### 获取剪贴板历史

获取用户的剪贴板历史。

**端点**: `GET /clipboard/history`

**请求头**: `Authorization: Bearer <token>`

**查询参数**:
- `page` (int, 可选): 页码，默认为 1
- `pageSize` (int, 可选): 每页数量，默认为 50

**响应**:
```json
{
  "items": [
    {
      "id": "uuid",
      "snippetId": "uuid",
      "snippetTitle": "JavaScript Array Helper",
      "snippetLanguage": "javascript",
      "copiedAt": "2024-01-01T12:00:00Z"
    }
  ],
  "totalCount": 100,
  "page": 1,
  "pageSize": 50,
  "totalPages": 2
}
```

### 清理剪贴板历史

清理用户的剪贴板历史。

**端点**: `DELETE /clipboard/history`

**请求头**: `Authorization: Bearer <token>`

**查询参数**:
- `olderThan` (int, 可选): 删除多少天前的记录，默认删除全部

**状态码**:
- `204 No Content`: 清理成功

## 用户管理

### 获取用户列表

获取用户列表（仅管理员）。

**端点**: `GET /users`

**请求头**: `Authorization: Bearer <token>`

**查询参数**:
- `page` (int, 可选): 页码
- `pageSize` (int, 可选): 每页数量
- `search` (string, 可选): 搜索用户名或邮箱
- `role` (string, 可选): 角色筛选

**响应**:
```json
{
  "items": [
    {
      "id": "uuid",
      "username": "john_doe",
      "email": "john@example.com",
      "role": "Editor",
      "isActive": true,
      "createdAt": "2024-01-01T12:00:00Z",
      "lastLoginAt": "2024-01-01T12:00:00Z"
    }
  ],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3
}
```

### 创建用户

创建新用户（仅管理员）。

**端点**: `POST /users`

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "username": "new_user",
  "email": "newuser@example.com",
  "password": "secure_password",
  "role": "Editor"
}
```

### 更新用户

更新用户信息。

**端点**: `PUT /users/{id}`

**路径参数**:
- `id` (uuid): 用户 ID

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "username": "updated_user",
  "email": "updated@example.com",
  "role": "Editor",
  "isActive": true
}
```

### 重置用户密码

重置用户密码（仅管理员）。

**端点**: `POST /users/{id}/reset-password`

**路径参数**:
- `id` (uuid): 用户 ID

**请求头**: `Authorization: Bearer <token>`

**请求体**:
```json
{
  "newPassword": "new_secure_password"
}
```

## 错误响应

所有错误响应都遵循统一格式：

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human readable error message",
    "details": {
      "field": "Additional error details"
    }
  }
}
```

### 常见错误码

- `VALIDATION_ERROR`: 请求参数验证失败
- `UNAUTHORIZED`: 未认证或认证失败
- `FORBIDDEN`: 权限不足
- `NOT_FOUND`: 资源不存在
- `CONFLICT`: 资源冲突（如用户名已存在）
- `RATE_LIMITED`: 请求频率超限
- `INTERNAL_ERROR`: 服务器内部错误

## 速率限制

API 实施速率限制以防止滥用：

- **默认限制**: 每分钟 1000 次请求
- **认证端点**: 每分钟 50 次请求
- **写操作**: 每分钟 200 次请求

超出限制时返回 `429 Too Many Requests` 状态码。

## 分页

支持分页的端点使用统一的分页格式：

**查询参数**:
- `page`: 页码（从 1 开始）
- `pageSize`: 每页数量

**响应格式**:
```json
{
  "items": [],
  "totalCount": 100,
  "page": 1,
  "pageSize": 20,
  "totalPages": 5
}
```

## SDK 和客户端库

### JavaScript/TypeScript

```typescript
import { CodeSnippetClient } from '@codesnippet/client';

const client = new CodeSnippetClient({
  baseUrl: 'https://your-domain.com/api',
  token: 'your-jwt-token'
});

// 获取代码片段列表
const snippets = await client.snippets.list({
  page: 1,
  pageSize: 10,
  search: 'javascript'
});

// 创建代码片段
const newSnippet = await client.snippets.create({
  title: 'My Snippet',
  code: 'console.log("Hello World");',
  language: 'javascript',
  isPublic: true
});
```

### C#

```csharp
using CodeSnippet.Client;

var client = new CodeSnippetClient("https://your-domain.com/api", "your-jwt-token");

// 获取代码片段列表
var snippets = await client.Snippets.ListAsync(new SnippetFilter
{
    Page = 1,
    PageSize = 10,
    Search = "javascript"
});

// 创建代码片段
var newSnippet = await client.Snippets.CreateAsync(new CreateSnippetRequest
{
    Title = "My Snippet",
    Code = "Console.WriteLine(\"Hello World\");",
    Language = "csharp",
    IsPublic = true
});
```

## 更新日志

### v1.0.0 (2024-01-01)
- 初始 API 版本
- 支持代码片段 CRUD 操作
- 用户认证和权限管理
- 版本控制功能
- 标签管理
- 剪贴板历史

### 即将推出
- 批量操作 API
- 代码片段导入/导出
- Webhook 支持
- GraphQL API

## 支持

如需 API 支持，请：

1. 查看本文档和 [FAQ](faq.md)
2. 访问 [GitHub Issues](https://github.com/your-org/codesnippet-manager/issues)
3. 联系技术支持：api-support@your-domain.com