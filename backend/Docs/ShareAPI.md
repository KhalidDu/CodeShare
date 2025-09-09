# 分享功能 API 文档

## 概述

分享功能 API 提供了完整的代码片段分享功能，包括分享链接生成、访问控制、权限管理、统计跟踪等功能。用户可以为其代码片段生成公开分享链接，未登录用户也能通过分享链接访问代码片段。

## API 端点

### 1. 生成分享链接

**端点**: `POST /api/codesnippets/{id}/share`

**描述**: 为指定的代码片段生成分享链接。

**参数**:
- `id` (Guid): 代码片段ID

**请求体**: `CreateShareDto`
```json
{
  "permission": 1,
  "expiresAt": "2024-12-31T23:59:59Z",
  "description": "分享给团队的代码片段"
}
```

**响应**: `ShareTokenDto`

**示例**:
```http
POST /api/codesnippets/12345678-1234-1234-1234-123456789012/share
Authorization: Bearer {token}
Content-Type: application/json

{
  "permission": 1,
  "expiresAt": "2024-12-31T23:59:59Z",
  "description": "分享给团队的代码片段"
}
```

**响应示例**:
```json
{
  "id": "87654321-4321-4321-4321-210987654321",
  "token": "abc123def456ghi789",
  "snippetId": "12345678-1234-1234-1234-123456789012",
  "createdBy": "11111111-1111-1111-1111-111111111111",
  "createdAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2024-12-31T23:59:59Z",
  "permission": 1,
  "isActive": true,
  "viewCount": 0,
  "copyCount": 0,
  "lastAccessedAt": null,
  "shareUrl": "https://codeshare.example.com/share/abc123def456ghi789"
}
```

### 2. 通过分享链接访问代码片段

**端点**: `GET /api/share/{token}`

**描述**: 通过分享令牌访问代码片段内容，无需认证。

**参数**:
- `token` (string): 分享令牌

**响应**: `SharedSnippetDto`

**示例**:
```http
GET /api/share/abc123def456ghi789
```

**响应示例**:
```json
{
  "id": "12345678-1234-1234-1234-123456789012",
  "title": "防抖函数",
  "description": "防止函数频繁调用的工具函数",
  "code": "function debounce(func, wait) {\n  let timeout;\n  return function executedFunction(...args) {\n    const later = () => {\n      clearTimeout(timeout);\n      func(...args);\n    };\n    clearTimeout(timeout);\n    timeout = setTimeout(later, wait);\n  };\n}",
  "language": "javascript",
  "createdBy": "11111111-1111-1111-1111-111111111111",
  "createdAt": "2024-01-15T10:30:00Z",
  "permission": 1,
  "allowCopy": true
}
```

### 3. 获取分享统计信息

**端点**: `GET /api/share/{token}/stats`

**描述**: 获取分享链接的详细统计信息。

**参数**:
- `token` (string): 分享令牌

**响应**: `ShareStatsDto`

**示例**:
```http
GET /api/share/abc123def456ghi789/stats
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "shareTokenId": "87654321-4321-4321-4321-210987654321",
  "snippetTitle": "防抖函数",
  "snippetLanguage": "javascript",
  "creatorUsername": "admin",
  "createdAt": "2024-01-15T10:30:00Z",
  "expiresAt": "2024-12-31T23:59:59Z",
  "permission": 1,
  "isActive": true,
  "viewCount": 150,
  "copyCount": 25,
  "lastAccessedAt": "2024-01-20T15:30:00Z",
  "uniqueVisitors": 45,
  "totalViews": 150,
  "totalCopies": 25,
  "firstAccessDate": "2024-01-15T11:00:00Z",
  "lastAccessDate": "2024-01-20T15:30:00Z"
}
```

### 4. 撤销分享链接

**端点**: `DELETE /api/share/{tokenId}`

**描述**: 撤销指定的分享链接，使其失效。

**参数**:
- `tokenId` (Guid): 分享令牌ID

**响应**: 成功消息

**示例**:
```http
DELETE /api/share/87654321-4321-4321-4321-210987654321
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "message": "分享链接已成功撤销"
}
```

### 5. 获取用户的分享链接列表

**端点**: `GET /api/users/me/shares`

**描述**: 获取当前用户创建的所有分享链接。

**查询参数**:
- `page` (int, 可选): 页码，默认为1
- `pageSize` (int, 可选): 每页大小，默认为10
- `isActive` (bool, 可选): 是否只返回活跃的分享链接
- `search` (string, 可选): 搜索关键词

**响应**: `PaginatedResult<ShareTokenDto>`

**示例**:
```http
GET /api/users/me/shares?page=1&pageSize=10&isActive=true&search=javascript
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "items": [
    {
      "id": "87654321-4321-4321-4321-210987654321",
      "token": "abc123def456ghi789",
      "snippetId": "12345678-1234-1234-1234-123456789012",
      "snippetTitle": "防抖函数",
      "createdBy": "11111111-1111-1111-1111-111111111111",
      "createdAt": "2024-01-15T10:30:00Z",
      "expiresAt": "2024-12-31T23:59:59Z",
      "permission": 1,
      "isActive": true,
      "viewCount": 150,
      "copyCount": 25,
      "lastAccessedAt": "2024-01-20T15:30:00Z",
      "shareUrl": "https://codeshare.example.com/share/abc123def456ghi789"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### 6. 管理员获取所有分享链接

**端点**: `GET /api/admin/shares`

**描述**: 管理员获取系统中的所有分享链接。

**查询参数**:
- `page` (int, 可选): 页码，默认为1
- `pageSize` (int, 可选): 每页大小，默认为10
- `username` (string, 可选): 按用户名筛选
- `snippetTitle` (string, 可选): 按代码片段标题筛选
- `isActive` (bool, 可选): 按活跃状态筛选
- `createdFrom` (datetime, 可选): 创建时间起始范围
- `createdTo` (datetime, 可选): 创建时间结束范围

**响应**: `PaginatedResult<AdminShareDto>`

**示例**:
```http
GET /api/admin/shares?page=1&pageSize=10&username=admin&isActive=true
Authorization: Bearer {admin_token}
```

**响应示例**:
```json
{
  "items": [
    {
      "id": "87654321-4321-4321-4321-210987654321",
      "token": "abc123def456ghi789",
      "snippetId": "12345678-1234-1234-1234-123456789012",
      "snippetTitle": "防抖函数",
      "snippetLanguage": "javascript",
      "createdBy": "11111111-1111-1111-1111-111111111111",
      "creatorUsername": "admin",
      "createdAt": "2024-01-15T10:30:00Z",
      "expiresAt": "2024-12-31T23:59:59Z",
      "permission": 1,
      "isActive": true,
      "viewCount": 150,
      "copyCount": 25,
      "lastAccessedAt": "2024-01-20T15:30:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

### 7. 管理员强制撤销分享链接

**端点**: `DELETE /api/admin/shares/{tokenId}`

**描述**: 管理员强制撤销分享链接。

**参数**:
- `tokenId` (Guid): 分享令牌ID

**请求体**: `AdminRevokeShareDto`
```json
{
  "reason": "违反使用政策"
}
```

**响应**: 成功消息

**示例**:
```http
DELETE /api/admin/shares/87654321-4321-4321-4321-210987654321
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "reason": "违反使用政策"
}
```

**响应示例**:
```json
{
  "message": "分享链接已强制撤销",
  "notificationSent": true
}
```

### 8. 管理员获取分享访问日志

**端点**: `GET /api/admin/shares/{tokenId}/logs`

**描述**: 管理员获取指定分享链接的访问日志。

**参数**:
- `tokenId` (Guid): 分享令牌ID

**查询参数**:
- `page` (int, 可选): 页码，默认为1
- `pageSize` (int, 可选): 每页大小，默认为10
- `accessType` (int, 可选): 访问类型筛选 (0:查看, 1:复制)
- `fromDate` (datetime, 可选): 访问时间起始范围
- `toDate` (datetime, 可选): 访问时间结束范围

**响应**: `PaginatedResult<ShareAccessLogDto>`

**示例**:
```http
GET /api/admin/shares/87654321-4321-4321-4321-210987654321/logs?page=1&pageSize=10
Authorization: Bearer {admin_token}
```

**响应示例**:
```json
{
  "items": [
    {
      "id": "98765432-1098-7654-3210-987654321098",
      "shareTokenId": "87654321-4321-4321-4321-210987654321",
      "accessTime": "2024-01-20T15:30:00Z",
      "ipAddress": "192.168.1.100",
      "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
      "accessType": 0,
      "sessionId": "session_abc_123",
      "referrer": "https://google.com"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

## 权限要求

- **生成分享链接**: 需要对代码片段的编辑权限
- **撤销分享链接**: 需要是分享链接的创建者或管理员
- **查看分享统计**: 需要是分享链接的创建者或管理员
- **访问分享链接**: 无需认证，但需要有效的分享令牌
- **管理员操作**: 需要管理员权限

## 错误处理

所有 API 端点都包含适当的错误处理：

- `400 Bad Request`: 请求参数无效或操作失败
- `401 Unauthorized`: 用户未认证
- `403 Forbidden`: 用户权限不足
- `404 Not Found`: 代码片段、分享令牌或用户不存在
- `410 Gone`: 分享链接已过期或已撤销
- `429 Too Many Requests`: 请求频率过高

## 数据模型

### CreateShareDto
```csharp
public class CreateShareDto
{
    public int Permission { get; set; } // 0:只读, 1:允许复制
    public DateTime? ExpiresAt { get; set; }
    public string? Description { get; set; }
}
```

### ShareTokenDto
```csharp
public class ShareTokenDto
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public Guid SnippetId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int Permission { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int CopyCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public string ShareUrl { get; set; }
}
```

### SharedSnippetDto
```csharp
public class SharedSnippetDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Code { get; set; }
    public string Language { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Permission { get; set; }
    public bool AllowCopy { get; set; }
}
```

### ShareStatsDto
```csharp
public class ShareStatsDto
{
    public Guid ShareTokenId { get; set; }
    public string SnippetTitle { get; set; }
    public string SnippetLanguage { get; set; }
    public string CreatorUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int Permission { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int CopyCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public int UniqueVisitors { get; set; }
    public int TotalViews { get; set; }
    public int TotalCopies { get; set; }
    public DateTime? FirstAccessDate { get; set; }
    public DateTime? LastAccessDate { get; set; }
}
```

### AdminShareDto
```csharp
public class AdminShareDto
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public Guid SnippetId { get; set; }
    public string SnippetTitle { get; set; }
    public string SnippetLanguage { get; set; }
    public Guid CreatedBy { get; set; }
    public string CreatorUsername { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int Permission { get; set; }
    public bool IsActive { get; set; }
    public int ViewCount { get; set; }
    public int CopyCount { get; set; }
    public DateTime? LastAccessedAt { get; set; }
}
```

### ShareAccessLogDto
```csharp
public class ShareAccessLogDto
{
    public Guid Id { get; set; }
    public Guid ShareTokenId { get; set; }
    public DateTime AccessTime { get; set; }
    public string IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public int AccessType { get; set; } // 0:查看, 1:复制
    public string? SessionId { get; set; }
    public string? Referrer { get; set; }
}
```

### PaginatedResult<T>
```csharp
public class PaginatedResult<T>
{
    public List<T> Items { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
```

## 使用场景

1. **团队协作**: 开发者可以分享代码片段给团队成员，无需注册账号即可访问
2. **技术分享**: 在社交媒体或技术博客中分享代码片段
3. **面试准备**: 分享代码示例给面试官
4. **教学演示**: 教师可以分享代码示例给学生
5. **开源项目**: 分享项目中的关键代码片段

## 使用示例

### 基本分享流程

1. **创建分享链接**:
   ```bash
   curl -X POST "https://api.example.com/api/share" \
   -H "Authorization: Bearer YOUR_JWT_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{
     "codeSnippetId": "12345678-1234-1234-1234-123456789012",
     "permission": 1,
     "expiresAt": "2024-12-31T23:59:59Z",
     "description": "分享给团队的代码片段"
   }'
   ```

2. **通过分享链接访问**:
   ```bash
   curl -X GET "https://api.example.com/api/share/abc123def456ghi789"
   ```

3. **获取分享统计**:
   ```bash
   curl -X GET "https://api.example.com/api/share/87654321-4321-4321-4321-210987654321/stats" \
   -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

4. **撤销分享链接**:
   ```bash
   curl -X DELETE "https://api.example.com/api/share/87654321-4321-4321-4321-210987654321/revoke" \
   -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

### 高级功能示例

1. **延长分享有效期**:
   ```bash
   curl -X POST "https://api.example.com/api/share/87654321-4321-4321-4321-210987654321/extend" \
   -H "Authorization: Bearer YOUR_JWT_TOKEN" \
   -H "Content-Type: application/json" \
   -d '{
     "extendHours": 24
   }'
   ```

2. **获取用户的分享列表**:
   ```bash
   curl -X GET "https://api.example.com/api/share/my-shares?page=1&pageSize=10" \
   -H "Authorization: Bearer YOUR_JWT_TOKEN"
   ```

3. **管理员获取所有分享**:
   ```bash
   curl -X GET "https://api.example.com/api/share/admin/all?page=1&pageSize=10&isActive=true" \
   -H "Authorization: Bearer ADMIN_JWT_TOKEN"
   ```

## 错误处理示例

### 常见错误响应

1. **认证失败**:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
     "title": "Unauthorized",
     "status": 401,
     "detail": "用户未认证或认证令牌无效"
   }
   ```

2. **权限不足**:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
     "title": "Forbidden",
     "status": 403,
     "detail": "用户权限不足，无法执行此操作"
   }
   ```

3. **分享链接不存在**:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
     "title": "Not Found",
     "status": 404,
     "detail": "分享链接不存在或已失效"
   }
   ```

4. **分享链接已过期**:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.6",
     "title": "Gone",
     "status": 410,
     "detail": "分享链接已过期，请创建新的分享链接"
   }
   ```

5. **请求参数错误**:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
     "title": "Bad Request",
     "status": 400,
     "detail": "请求参数无效",
     "errors": {
       "codeSnippetId": ["代码片段ID不能为空"],
       "expiresAt": ["过期时间必须在当前时间之后"]
     }
   }
   ```

6. **频率限制**:
   ```json
   {
     "type": "https://tools.ietf.org/html/rfc6585#section-4",
     "title": "Too Many Requests",
     "status": 429,
     "detail": "请求频率过高，请稍后再试",
     "retryAfter": 60
   }
   ```

### 错误处理最佳实践

1. **检查HTTP状态码**: 首先检查响应的状态码，确定请求的成功或失败
2. **解析错误信息**: 从响应体中提取详细的错误信息，帮助用户理解问题
3. **实现重试机制**: 对于网络错误或服务器错误，实现适当的重试逻辑
4. **处理令牌过期**: 当收到401错误时，引导用户重新登录获取新的令牌
5. **显示用户友好的错误**: 将技术错误转换为用户友好的错误消息

## 最佳实践

1. **权限控制**: 根据分享内容设置适当的权限级别
2. **有效期设置**: 为临时分享设置合理的有效期
3. **监控统计**: 定期查看分享统计，了解代码片段的使用情况
4. **安全意识**: 不要分享包含敏感信息的代码
5. **定期清理**: 及时撤销不再需要的分享链接
6. **使用HTTPS**: 确保所有API请求都通过HTTPS进行
7. **缓存分享链接**: 在前端缓存分享链接，减少重复请求
8. **错误处理**: 实现完善的错误处理机制，提供良好的用户体验

## 限制说明

- 分享令牌长度为64位，使用加密算法生成
- 每个用户最多可以同时拥有100个活跃的分享链接
- 分享链接的最短有效期为1小时，最长为1年
- 系统会自动清理过期超过30天的分享链接
- 访问频率限制：每个IP地址每分钟最多访问100次

## 安全注意事项

- 分享令牌使用加密算法生成，具有足够的随机性
- 系统会记录所有访问日志，用于安全审计
- 管理员可以随时撤销任何分享链接
- 分享链接的访问会受到频率限制和异常检测
- 不要在分享链接中包含敏感信息或密钥