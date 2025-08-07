# 版本管理 API 文档

## 概述

版本管理 API 提供了完整的代码片段版本控制功能，包括版本历史查询、版本详情查看、版本恢复和版本比较等功能。

## API 端点

### 1. 获取代码片段的版本历史

**端点**: `GET /api/versions/snippet/{snippetId}`

**描述**: 获取指定代码片段的所有版本历史，按版本号降序排列。

**参数**:
- `snippetId` (Guid): 代码片段ID

**响应**: `SnippetVersionDto[]`

**示例**:
```http
GET /api/versions/snippet/12345678-1234-1234-1234-123456789012
Authorization: Bearer {token}
```

**响应示例**:
```json
[
  {
    "id": "87654321-4321-4321-4321-210987654321",
    "snippetId": "12345678-1234-1234-1234-123456789012",
    "versionNumber": 3,
    "title": "更新的代码片段",
    "description": "最新版本的描述",
    "code": "console.log('version 3');",
    "language": "javascript",
    "createdBy": "11111111-1111-1111-1111-111111111111",
    "createdAt": "2024-01-15T10:30:00Z",
    "changeDescription": "添加了新功能"
  },
  {
    "id": "76543210-3210-3210-3210-109876543210",
    "snippetId": "12345678-1234-1234-1234-123456789012",
    "versionNumber": 2,
    "title": "代码片段",
    "description": "第二版本",
    "code": "console.log('version 2');",
    "language": "javascript",
    "createdBy": "11111111-1111-1111-1111-111111111111",
    "createdAt": "2024-01-14T15:20:00Z",
    "changeDescription": "修复了bug"
  }
]
```

### 2. 获取特定版本详情

**端点**: `GET /api/versions/{versionId}`

**描述**: 获取指定版本的详细信息。

**参数**:
- `versionId` (Guid): 版本ID

**响应**: `SnippetVersionDto`

**示例**:
```http
GET /api/versions/87654321-4321-4321-4321-210987654321
Authorization: Bearer {token}
```

### 3. 手动创建版本

**端点**: `POST /api/versions/snippet/{snippetId}`

**描述**: 为指定代码片段手动创建一个新版本（用于重要的变更点）。

**参数**:
- `snippetId` (Guid): 代码片段ID

**请求体**: `CreateVersionRequest`
```json
{
  "changeDescription": "重要功能更新"
}
```

**响应**: `SnippetVersionDto`

**示例**:
```http
POST /api/versions/snippet/12345678-1234-1234-1234-123456789012
Authorization: Bearer {token}
Content-Type: application/json

{
  "changeDescription": "添加了错误处理机制"
}
```

### 4. 恢复到指定版本

**端点**: `POST /api/versions/snippet/{snippetId}/restore/{versionId}`

**描述**: 将代码片段恢复到指定的历史版本。此操作会：
1. 创建当前版本的备份
2. 将代码片段内容恢复到指定版本
3. 创建恢复操作的版本记录

**参数**:
- `snippetId` (Guid): 代码片段ID
- `versionId` (Guid): 要恢复的版本ID

**响应**: 成功消息

**示例**:
```http
POST /api/versions/snippet/12345678-1234-1234-1234-123456789012/restore/76543210-3210-3210-3210-109876543210
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "message": "版本恢复成功"
}
```

### 5. 比较两个版本

**端点**: `GET /api/versions/compare/{fromVersionId}/{toVersionId}`

**描述**: 比较两个版本之间的差异，包括标题、描述、代码和语言的变化，以及详细的代码行级差异。

**参数**:
- `fromVersionId` (Guid): 源版本ID
- `toVersionId` (Guid): 目标版本ID

**响应**: `VersionComparisonDto`

**示例**:
```http
GET /api/versions/compare/76543210-3210-3210-3210-109876543210/87654321-4321-4321-4321-210987654321
Authorization: Bearer {token}
```

**响应示例**:
```json
{
  "fromVersion": {
    "id": "76543210-3210-3210-3210-109876543210",
    "versionNumber": 2,
    "title": "原始标题",
    "code": "console.log('version 2');"
  },
  "toVersion": {
    "id": "87654321-4321-4321-4321-210987654321",
    "versionNumber": 3,
    "title": "更新的标题",
    "code": "console.log('version 3');"
  },
  "titleChanged": true,
  "descriptionChanged": false,
  "codeChanged": true,
  "languageChanged": false,
  "codeDifferences": [
    {
      "lineNumber": 1,
      "diffType": "Modified",
      "fromContent": "console.log('version 2');",
      "toContent": "console.log('version 3');"
    }
  ]
}
```

## 权限要求

- **查看版本历史和详情**: 需要对代码片段的读取权限
- **创建版本**: 需要对代码片段的编辑权限
- **恢复版本**: 需要对代码片段的编辑权限
- **比较版本**: 需要对代码片段的读取权限

## 错误处理

所有 API 端点都包含适当的错误处理：

- `400 Bad Request`: 请求参数无效或操作失败
- `401 Unauthorized`: 用户未认证
- `403 Forbidden`: 用户权限不足
- `404 Not Found`: 版本或代码片段不存在

## 数据模型

### SnippetVersionDto
```csharp
public class SnippetVersionDto
{
    public Guid Id { get; set; }
    public Guid SnippetId { get; set; }
    public int VersionNumber { get; set; }
    public string Title { get; set; }
    public string? Description { get; set; }
    public string Code { get; set; }
    public string Language { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ChangeDescription { get; set; }
}
```

### VersionComparisonDto
```csharp
public class VersionComparisonDto
{
    public SnippetVersionDto FromVersion { get; set; }
    public SnippetVersionDto ToVersion { get; set; }
    public bool TitleChanged { get; set; }
    public bool DescriptionChanged { get; set; }
    public bool CodeChanged { get; set; }
    public bool LanguageChanged { get; set; }
    public List<CodeDiffLine> CodeDifferences { get; set; }
}
```

### CodeDiffLine
```csharp
public class CodeDiffLine
{
    public int LineNumber { get; set; }
    public string DiffType { get; set; } // "Added", "Removed", "Modified", "Unchanged"
    public string? FromContent { get; set; }
    public string? ToContent { get; set; }
}
```

## 使用场景

1. **查看历史**: 开发者可以查看代码片段的完整变更历史
2. **版本恢复**: 当新版本出现问题时，可以快速恢复到之前的稳定版本
3. **变更追踪**: 通过版本比较功能，可以清楚地看到每次变更的具体内容
4. **协作开发**: 团队成员可以了解代码片段的演进过程和变更原因

## 最佳实践

1. **描述性的变更说明**: 创建版本时提供清晰的变更描述
2. **定期创建版本**: 在重要变更前手动创建版本作为检查点
3. **谨慎恢复**: 恢复版本前先查看版本比较，确认变更内容
4. **权限管理**: 确保只有有权限的用户才能进行版本恢复操作