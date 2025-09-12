# CodeShare 系统设置功能 API 文档

## 概述

CodeShare 系统设置功能提供了一套完整的 RESTful API，用于管理系统的各种配置设置。本文档详细描述了所有可用的 API 端点、请求格式、响应格式和使用示例。

## 基础信息

- **基础 URL**: `/api/settings`
- **认证方式**: Bearer Token (JWT)
- **内容类型**: `application/json`
- **响应格式**: JSON

## 认证

所有 API 请求都需要在请求头中包含有效的 JWT token：

```
Authorization: Bearer <your-jwt-token>
```

## 响应格式

### 成功响应
```json
{
  "success": true,
  "message": "操作成功",
  "data": {
    // 响应数据
  }
}
```

### 错误响应
```json
{
  "success": false,
  "message": "错误描述",
  "error": "错误代码",
  "details": {
    // 详细错误信息
  }
}
```

## 端点列表

### 1. 系统设置管理

#### 1.1 获取所有设置
- **URL**: `GET /api/settings`
- **描述**: 获取所有系统设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X GET "https://api.codeshare.com/api/settings" \
  -H "Authorization: Bearer <token>"
```

**响应示例**:
```json
{
  "success": true,
  "message": "获取设置成功",
  "data": {
    "siteSettings": {
      "siteName": "CodeShare",
      "siteDescription": "代码片段分享平台",
      "siteKeywords": "代码,片段,分享,编程",
      "logoUrl": "",
      "faviconUrl": "",
      "footerText": "© 2025 CodeShare",
      "defaultLanguage": "zh-CN",
      "theme": "light",
      "maxUploadSize": 10,
      "maxSnippetLength": 50000,
      "enableComments": true,
      "enableRatings": true,
      "enableSharing": true,
      "enableClipboard": true,
      "enableSearch": true,
      "customCss": "",
      "customJs": "",
      "seoTitle": "",
      "seoDescription": "",
      "seoKeywords": ""
    },
    "securitySettings": {
      "minPasswordLength": 8,
      "maxPasswordLength": 128,
      "maxLoginAttempts": 5,
      "accountLockoutDuration": 15,
      "sessionTimeout": 30,
      "enableRememberMe": true,
      "enable2FA": false,
      "enableSecurityLogging": true,
      "enablePasswordStrength": true,
      "requireEmailVerification": false,
      "allowedLoginAttempts": 3,
      "lockoutDurationMinutes": 15,
      "sessionTimeoutMinutes": 30,
      "passwordMinLength": 8,
      "passwordMaxLength": 128,
      "passwordRequireUppercase": true,
      "passwordRequireLowercase": true,
      "passwordRequireNumbers": true,
      "passwordRequireSpecialChars": true,
      "enableAccountLockout": true,
      "maxFailedLoginAttempts": 5,
      "lockoutDurationMinutes": 15,
      "enableSessionTimeout": true,
      "sessionTimeoutMinutes": 30,
      "enableRememberMe": true,
      "rememberMeDays": 30,
      "enable2FA": false,
      "enableSecurityLogging": true,
      "logFailedLoginAttempts": true,
      "logSuccessfulLogins": true,
      "logPasswordChanges": true,
      "logProfileChanges": true,
      "enablePasswordStrengthMeter": true,
      "enablePasswordExpiration": false,
      "passwordExpirationDays": 90,
      "enableSso": false,
      "ssoProviders": [],
      "enableCaptcha": false,
      "captchaType": "recaptcha",
      "recaptchaSiteKey": "",
      "recaptchaSecretKey": ""
    },
    "featureSettings": {
      "enableCodeSnippets": true,
      "enableSharing": true,
      "enableClipboard": true,
      "enableFileUpload": true,
      "enableSearch": true,
      "enableApi": true,
      "enablePublicSnippets": true,
      "enablePrivateSnippets": true,
      "enableCategories": true,
      "enableTags": true,
      "enableComments": true,
      "enableRatings": true,
      "enableFavorites": true,
      "enableReports": true,
      "enableAnalytics": true,
      "enableNotifications": true,
      "enableEmailNotifications": true,
      "enablePushNotifications": false,
      "enableWebhooks": false,
      "maxSnippetLength": 50000,
      "maxUploadSize": 10,
      "maxFileCount": 5,
      "maxFileSize": 5,
      "allowedFileTypes": [".txt", ".json", ".xml", ".csv", ".log", ".md"],
      "enableSyntaxHighlighting": true,
      "enableLineNumbers": true,
      "enableCopyButton": true,
      "enableFullScreen": true,
      "enableThemeSwitching": true,
      "enableLanguageDetection": true,
      "enableAutoSave": true,
      "autoSaveInterval": 30,
      "enableVersioning": true,
      "maxVersions": 10,
      "enableExport": true,
      "exportFormats": ["json", "xml", "csv", "txt"],
      "enableImport": true,
      "importFormats": ["json", "xml", "csv", "txt"],
      "enableApiRateLimiting": true,
      "apiRateLimit": 100,
      "apiRateLimitWindow": 60,
      "enableApiAuthentication": true,
      "apiKeyExpiration": 365,
      "enableWebhooks": false,
      "webhookEvents": [],
      "webhookUrl": "",
      "webhookSecret": ""
    },
    "emailSettings": {
      "smtpHost": "",
      "smtpPort": 587,
      "smtpUsername": "",
      "smtpPassword": "",
      "enableSsl": true,
      "enableTls": false,
      "fromEmail": "noreply@codeshare.com",
      "fromName": "CodeShare",
      "enableEmailNotifications": true,
      "enableUserRegistrationEmail": true,
      "enablePasswordResetEmail": true,
      "enableWelcomeEmail": true,
      "enableNotificationEmail": true,
      "enableQueue": true,
      "queueMaxRetries": 3,
      "queueRetryDelay": 5,
      "enableBcc": false,
      "bccEmail": "",
      "enableTracking": true,
      "trackingDomain": "",
      "enableAnalytics": true,
      "analyticsProvider": "default",
      "testEmail": ""
    }
  }
}
```

#### 1.2 更新站点设置
- **URL**: `PUT /api/settings/site`
- **描述**: 更新站点设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X PUT "https://api.codeshare.com/api/settings/site" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "siteName": "新的站点名称",
    "siteDescription": "这是一个新的站点描述",
    "siteKeywords": "代码,编程,分享",
    "theme": "dark",
    "enableComments": true,
    "enableRatings": true,
    "enableSharing": true,
    "enableClipboard": true,
    "enableSearch": true
  }'
```

**请求参数**:
```json
{
  "siteName": "string (required, max: 100)",
  "siteDescription": "string (max: 500)",
  "siteKeywords": "string (max: 200)",
  "logoUrl": "string (url)",
  "faviconUrl": "string (url)",
  "footerText": "string (max: 200)",
  "defaultLanguage": "string (max: 10)",
  "theme": "string (light|dark)",
  "maxUploadSize": "number (1-100)",
  "maxSnippetLength": "number (1000-100000)",
  "enableComments": "boolean",
  "enableRatings": "boolean",
  "enableSharing": "boolean",
  "enableClipboard": "boolean",
  "enableSearch": "boolean",
  "customCss": "string",
  "customJs": "string",
  "seoTitle": "string (max: 100)",
  "seoDescription": "string (max: 200)",
  "seoKeywords": "string (max: 200)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "站点设置更新成功",
  "data": {
    "siteSettings": {
      "siteName": "新的站点名称",
      "siteDescription": "这是一个新的站点描述",
      "siteKeywords": "代码,编程,分享",
      "theme": "dark",
      // ... 其他设置
    }
  }
}
```

#### 1.3 更新安全设置
- **URL**: `PUT /api/settings/security`
- **描述**: 更新安全设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X PUT "https://api.codeshare.com/api/settings/security" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "minPasswordLength": 10,
    "maxPasswordLength": 128,
    "maxLoginAttempts": 5,
    "accountLockoutDuration": 15,
    "sessionTimeout": 30,
    "enableRememberMe": true,
    "enable2FA": false,
    "enableSecurityLogging": true,
    "enablePasswordStrength": true
  }'
```

**请求参数**:
```json
{
  "minPasswordLength": "number (8-128)",
  "maxPasswordLength": "number (8-128)",
  "maxLoginAttempts": "number (1-10)",
  "accountLockoutDuration": "number (1-1440)",
  "sessionTimeout": "number (5-1440)",
  "enableRememberMe": "boolean",
  "enable2FA": "boolean",
  "enableSecurityLogging": "boolean",
  "enablePasswordStrength": "boolean",
  "requireEmailVerification": "boolean",
  "allowedLoginAttempts": "number (1-10)",
  "lockoutDurationMinutes": "number (1-1440)",
  "sessionTimeoutMinutes": "number (5-1440)",
  "passwordMinLength": "number (8-128)",
  "passwordMaxLength": "number (8-128)",
  "passwordRequireUppercase": "boolean",
  "passwordRequireLowercase": "boolean",
  "passwordRequireNumbers": "boolean",
  "passwordRequireSpecialChars": "boolean",
  "enableAccountLockout": "boolean",
  "maxFailedLoginAttempts": "number (1-10)",
  "lockoutDurationMinutes": "number (1-1440)",
  "enableSessionTimeout": "boolean",
  "sessionTimeoutMinutes": "number (5-1440)",
  "enableRememberMe": "boolean",
  "rememberMeDays": "number (1-90)",
  "enable2FA": "boolean",
  "enableSecurityLogging": "boolean",
  "logFailedLoginAttempts": "boolean",
  "logSuccessfulLogins": "boolean",
  "logPasswordChanges": "boolean",
  "logProfileChanges": "boolean",
  "enablePasswordStrengthMeter": "boolean",
  "enablePasswordExpiration": "boolean",
  "passwordExpirationDays": "number (7-365)",
  "enableSso": "boolean",
  "ssoProviders": "array",
  "enableCaptcha": "boolean",
  "captchaType": "string",
  "recaptchaSiteKey": "string",
  "recaptchaSecretKey": "string"
}
```

#### 1.4 更新功能设置
- **URL**: `PUT /api/settings/features`
- **描述**: 更新功能设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X PUT "https://api.codeshare.com/api/settings/features" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "enableCodeSnippets": true,
    "enableSharing": true,
    "enableClipboard": true,
    "enableFileUpload": true,
    "enableSearch": true,
    "enableApi": true,
    "maxSnippetLength": 50000,
    "maxUploadSize": 10,
    "maxFileCount": 5,
    "maxFileSize": 5,
    "allowedFileTypes": [".txt", ".json", ".xml", ".csv", ".log", ".md"]
  }'
```

**请求参数**:
```json
{
  "enableCodeSnippets": "boolean",
  "enableSharing": "boolean",
  "enableClipboard": "boolean",
  "enableFileUpload": "boolean",
  "enableSearch": "boolean",
  "enableApi": "boolean",
  "enablePublicSnippets": "boolean",
  "enablePrivateSnippets": "boolean",
  "enableCategories": "boolean",
  "enableTags": "boolean",
  "enableComments": "boolean",
  "enableRatings": "boolean",
  "enableFavorites": "boolean",
  "enableReports": "boolean",
  "enableAnalytics": "boolean",
  "enableNotifications": "boolean",
  "enableEmailNotifications": "boolean",
  "enablePushNotifications": "boolean",
  "enableWebhooks": "boolean",
  "maxSnippetLength": "number (1000-100000)",
  "maxUploadSize": "number (1-100)",
  "maxFileCount": "number (1-20)",
  "maxFileSize": "number (1-50)",
  "allowedFileTypes": "array",
  "enableSyntaxHighlighting": "boolean",
  "enableLineNumbers": "boolean",
  "enableCopyButton": "boolean",
  "enableFullScreen": "boolean",
  "enableThemeSwitching": "boolean",
  "enableLanguageDetection": "boolean",
  "enableAutoSave": "boolean",
  "autoSaveInterval": "number (10-300)",
  "enableVersioning": "boolean",
  "maxVersions": "number (1-50)",
  "enableExport": "boolean",
  "exportFormats": "array",
  "enableImport": "boolean",
  "importFormats": "array",
  "enableApiRateLimiting": "boolean",
  "apiRateLimit": "number (10-1000)",
  "apiRateLimitWindow": "number (60-3600)",
  "enableApiAuthentication": "boolean",
  "apiKeyExpiration": "number (30-365)",
  "enableWebhooks": "boolean",
  "webhookEvents": "array",
  "webhookUrl": "string (url)",
  "webhookSecret": "string"
}
```

#### 1.5 更新邮件设置
- **URL**: `PUT /api/settings/email`
- **描述**: 更新邮件设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X PUT "https://api.codeshare.com/api/settings/email" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "smtpHost": "smtp.gmail.com",
    "smtpPort": 587,
    "smtpUsername": "your-email@gmail.com",
    "smtpPassword": "your-password",
    "enableSsl": true,
    "enableTls": false,
    "fromEmail": "noreply@codeshare.com",
    "fromName": "CodeShare",
    "enableEmailNotifications": true,
    "enableUserRegistrationEmail": true,
    "enablePasswordResetEmail": true
  }'
```

**请求参数**:
```json
{
  "smtpHost": "string (max: 100)",
  "smtpPort": "number (1-65535)",
  "smtpUsername": "string (max: 100)",
  "smtpPassword": "string (max: 100)",
  "enableSsl": "boolean",
  "enableTls": "boolean",
  "fromEmail": "string (email, max: 100)",
  "fromName": "string (max: 100)",
  "enableEmailNotifications": "boolean",
  "enableUserRegistrationEmail": "boolean",
  "enablePasswordResetEmail": "boolean",
  "enableWelcomeEmail": "boolean",
  "enableNotificationEmail": "boolean",
  "enableQueue": "boolean",
  "queueMaxRetries": "number (1-10)",
  "queueRetryDelay": "number (1-300)",
  "enableBcc": "boolean",
  "bccEmail": "string (email, max: 100)",
  "enableTracking": "boolean",
  "trackingDomain": "string (url, max: 100)",
  "enableAnalytics": "boolean",
  "analyticsProvider": "string (max: 50)",
  "testEmail": "string (email, max: 100)"
}
```

### 2. 设置历史管理

#### 2.1 获取设置历史
- **URL**: `GET /api/settings/history`
- **描述**: 获取设置变更历史
- **权限**: 管理员

**请求参数**:
- `pageNumber`: 页码 (默认: 1)
- `pageSize`: 每页数量 (默认: 20, 最大: 100)
- `settingType`: 设置类型 (Site, Security, Feature, Email)
- `startDate`: 开始日期 (YYYY-MM-DD)
- `endDate`: 结束日期 (YYYY-MM-DD)
- `changedBy`: 操作人
- `keyword`: 搜索关键词

**请求示例**:
```bash
curl -X GET "https://api.codeshare.com/api/settings/history?pageNumber=1&pageSize=20&settingType=Site&startDate=2025-01-01&endDate=2025-01-31" \
  -H "Authorization: Bearer <token>"
```

**响应示例**:
```json
{
  "success": true,
  "message": "获取历史记录成功",
  "data": {
    "items": [
      {
        "id": "uuid",
        "createdAt": "2025-01-15T10:30:00Z",
        "settingType": "Site",
        "settingKey": "siteName",
        "oldValue": "CodeShare",
        "newValue": "新的站点名称",
        "changedBy": "admin",
        "changedById": "uuid",
        "changeReason": "更新站点名称",
        "changeCategory": "Configuration",
        "clientIp": "192.168.1.100",
        "userAgent": "Mozilla/5.0...",
        "isImportant": false,
        "status": "Success",
        "errorMessage": null,
        "metadata": null
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 2
  }
}
```

#### 2.2 获取设置统计
- **URL**: `GET /api/settings/history/statistics`
- **描述**: 获取设置变更统计
- **权限**: 管理员

**请求示例**:
```bash
curl -X GET "https://api.codeshare.com/api/settings/history/statistics" \
  -H "Authorization: Bearer <token>"
```

**响应示例**:
```json
{
  "success": true,
  "message": "获取统计信息成功",
  "data": {
    "totalChanges": 150,
    "todayChanges": 5,
    "thisWeekChanges": 25,
    "thisMonthChanges": 85,
    "mostActiveUser": "admin",
    "mostActiveUserChanges": 45,
    "mostChangedSetting": "siteName",
    "mostChangedSettingCount": 12,
    "changesByType": {
      "Site": 45,
      "Security": 35,
      "Feature": 40,
      "Email": 30
    },
    "changesByCategory": {
      "Configuration": 80,
      "Security": 35,
      "Feature": 25,
      "Other": 10
    },
    "changesByMonth": [
      {
        "month": "2025-01",
        "count": 85
      },
      {
        "month": "2024-12",
        "count": 65
      }
    ]
  }
}
```

#### 2.3 导出设置历史
- **URL**: `GET /api/settings/history/export`
- **描述**: 导出设置历史
- **权限**: 管理员

**请求参数**:
- `format`: 导出格式 (json, csv, excel)
- `startDate`: 开始日期 (YYYY-MM-DD)
- `endDate`: 结束日期 (YYYY-MM-DD)
- `settingType`: 设置类型 (Site, Security, Feature, Email)
- `changedBy`: 操作人

**请求示例**:
```bash
curl -X GET "https://api.codeshare.com/api/settings/history/export?format=json&startDate=2025-01-01&endDate=2025-01-31" \
  -H "Authorization: Bearer <token>" \
  -o settings-history.json
```

### 3. 导入导出管理

#### 3.1 导出设置
- **URL**: `GET /api/settings/export`
- **描述**: 导出系统设置
- **权限**: 管理员

**请求参数**:
- `format`: 导出格式 (json, csv, excel)
- `includeSiteSettings`: 包含站点设置 (true/false)
- `includeSecuritySettings`: 包含安全设置 (true/false)
- `includeFeatureSettings`: 包含功能设置 (true/false)
- `includeEmailSettings`: 包含邮件设置 (true/false)

**请求示例**:
```bash
curl -X GET "https://api.codeshare.com/api/settings/export?format=json&includeSiteSettings=true&includeSecuritySettings=true&includeFeatureSettings=true&includeEmailSettings=true" \
  -H "Authorization: Bearer <token>" \
  -o settings-export.json
```

**响应示例**:
```json
{
  "success": true,
  "message": "导出成功",
  "data": {
    "exportData": {
      "siteSettings": {
        "siteName": "CodeShare",
        "siteDescription": "代码片段分享平台",
        // ... 其他站点设置
      },
      "securitySettings": {
        "minPasswordLength": 8,
        "maxPasswordLength": 128,
        // ... 其他安全设置
      },
      "featureSettings": {
        "enableCodeSnippets": true,
        "enableSharing": true,
        // ... 其他功能设置
      },
      "emailSettings": {
        "smtpHost": "smtp.gmail.com",
        "smtpPort": 587,
        // ... 其他邮件设置
      }
    },
    "exportInfo": {
      "exportedAt": "2025-01-15T10:30:00Z",
      "exportedBy": "admin",
      "format": "json",
      "includedSections": ["siteSettings", "securitySettings", "featureSettings", "emailSettings"]
    }
  }
}
```

#### 3.2 导入设置
- **URL**: `POST /api/settings/import`
- **描述**: 导入系统设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X POST "https://api.codeshare.com/api/settings/import" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonData": "{\"siteSettings\":{\"siteName\":\"导入的站点\"}}",
    "format": "json",
    "mode": "merge",
    "overwriteExisting": false
  }'
```

**请求参数**:
```json
{
  "jsonData": "string (required, JSON格式的设置数据)",
  "format": "string (required, json|csv|excel)",
  "mode": "string (required, merge|overwrite)",
  "overwriteExisting": "boolean (default: false)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "导入成功",
  "data": {
    "importResult": {
      "totalSettings": 15,
      "importedSettings": 12,
      "skippedSettings": 3,
      "errors": [],
      "warnings": []
    },
    "importInfo": {
      "importedAt": "2025-01-15T10:30:00Z",
      "importedBy": "admin",
      "format": "json",
      "mode": "merge"
    }
  }
}
```

#### 3.3 验证导入数据
- **URL**: `POST /api/settings/import/validate`
- **描述**: 验证导入数据
- **权限**: 管理员

**请求示例**:
```bash
curl -X POST "https://api.codeshare.com/api/settings/import/validate" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "jsonData": "{\"siteSettings\":{\"siteName\":\"测试站点\"}}",
    "format": "json"
  }'
```

**请求参数**:
```json
{
  "jsonData": "string (required, JSON格式的设置数据)",
  "format": "string (required, json|csv|excel)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "验证完成",
  "data": {
    "validationResult": {
      "valid": true,
      "errors": [],
      "warnings": [
        {
          "field": "siteSettings.siteDescription",
          "message": "站点描述为空，建议添加描述"
        }
      ]
    },
    "validationInfo": {
      "validatedAt": "2025-01-15T10:30:00Z",
      "format": "json",
      "totalFields": 25,
      "validFields": 24,
      "warningFields": 1
    }
  }
}
```

### 4. 邮件测试

#### 4.1 测试邮件连接
- **URL**: `POST /api/settings/email/test-connection`
- **描述**: 测试邮件服务器连接
- **权限**: 管理员

**请求示例**:
```bash
curl -X POST "https://api.codeshare.com/api/settings/email/test-connection" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "smtpHost": "smtp.gmail.com",
    "smtpPort": 587,
    "smtpUsername": "your-email@gmail.com",
    "smtpPassword": "your-password",
    "enableSsl": true,
    "enableTls": false
  }'
```

**请求参数**:
```json
{
  "smtpHost": "string (required, max: 100)",
  "smtpPort": "number (required, 1-65535)",
  "smtpUsername": "string (required, max: 100)",
  "smtpPassword": "string (required, max: 100)",
  "enableSsl": "boolean (required)",
  "enableTls": "boolean (required)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "连接测试成功",
  "data": {
    "connectionResult": {
      "success": true,
      "message": "成功连接到邮件服务器",
      "server": "smtp.gmail.com",
      "port": 587,
      "sslEnabled": true,
      "responseTime": 250
    }
  }
}
```

#### 4.2 发送测试邮件
- **URL**: `POST /api/settings/email/send-test`
- **描述**: 发送测试邮件
- **权限**: 管理员

**请求示例**:
```bash
curl -X POST "https://api.codeshare.com/api/settings/email/send-test" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "toEmail": "test@example.com",
    "subject": "CodeShare 测试邮件",
    "body": "这是一封来自 CodeShare 的测试邮件。"
  }'
```

**请求参数**:
```json
{
  "toEmail": "string (required, email, max: 100)",
  "subject": "string (required, max: 200)",
  "body": "string (required, max: 5000)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "测试邮件发送成功",
  "data": {
    "emailResult": {
      "success": true,
      "message": "邮件已成功发送",
      "toEmail": "test@example.com",
      "sentAt": "2025-01-15T10:30:00Z",
      "messageId": "1234567890@example.com"
    }
  }
}
```

### 5. 备份恢复

#### 5.1 创建备份
- **URL**: `POST /api/settings/backup`
- **描述**: 创建系统设置备份
- **权限**: 管理员

**请求示例**:
```bash
curl -X POST "https://api.codeshare.com/api/settings/backup" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "description": "常规备份"
  }'
```

**请求参数**:
```json
{
  "description": "string (optional, max: 200)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "备份创建成功",
  "data": {
    "backupResult": {
      "backupId": "uuid",
      "backupName": "settings-backup-2025-01-15",
      "description": "常规备份",
      "createdAt": "2025-01-15T10:30:00Z",
      "createdBy": "admin",
      "fileSize": 45678,
      "checksum": "sha256-abcdef1234567890"
    }
  }
}
```

#### 5.2 恢复备份
- **URL**: `POST /api/settings/backup/restore`
- **描述**: 从备份恢复系统设置
- **权限**: 管理员

**请求示例**:
```bash
curl -X POST "https://api.codeshare.com/api/settings/backup/restore" \
  -H "Authorization: Bearer <token>" \
  -H "Content-Type: application/json" \
  -d '{
    "backupId": "uuid",
    "createRestorePoint": true
  }'
```

**请求参数**:
```json
{
  "backupId": "string (required)",
  "createRestorePoint": "boolean (default: true)"
}
```

**响应示例**:
```json
{
  "success": true,
  "message": "备份恢复成功",
  "data": {
    "restoreResult": {
      "backupId": "uuid",
      "restorePointId": "uuid",
      "restoredAt": "2025-01-15T10:30:00Z",
      "restoredBy": "admin",
      "settingsRestored": ["siteSettings", "securitySettings", "featureSettings", "emailSettings"]
    }
  }
}
```

## 错误代码

| 错误代码 | 描述 | HTTP 状态码 |
|----------|------|-------------|
| `UNAUTHORIZED` | 未授权访问 | 401 |
| `FORBIDDEN` | 权限不足 | 403 |
| `NOT_FOUND` | 资源不存在 | 404 |
| `VALIDATION_ERROR` | 数据验证失败 | 400 |
| `INVALID_INPUT` | 无效的输入数据 | 400 |
| `FILE_TOO_LARGE` | 文件过大 | 413 |
| `UNSUPPORTED_FORMAT` | 不支持的格式 | 400 |
| `IMPORT_ERROR` | 导入失败 | 400 |
| `EXPORT_ERROR` | 导出失败 | 500 |
| `EMAIL_ERROR` | 邮件发送失败 | 500 |
| `BACKUP_ERROR` | 备份失败 | 500 |
| `RESTORE_ERROR` | 恢复失败 | 500 |
| `INTERNAL_ERROR` | 内部服务器错误 | 500 |

## 使用示例

### JavaScript 示例

```javascript
// 获取系统设置
async function getSettings() {
  try {
    const response = await fetch('/api/settings', {
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });
    
    const data = await response.json();
    if (data.success) {
      console.log('系统设置:', data.data);
    } else {
      console.error('获取设置失败:', data.message);
    }
  } catch (error) {
    console.error('请求失败:', error);
  }
}

// 更新站点设置
async function updateSiteSettings(settings) {
  try {
    const response = await fetch('/api/settings/site', {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(settings)
    });
    
    const data = await response.json();
    if (data.success) {
      console.log('站点设置更新成功');
    } else {
      console.error('更新失败:', data.message);
    }
  } catch (error) {
    console.error('请求失败:', error);
  }
}
```

### Python 示例

```python
import requests
import json

# 配置
base_url = 'https://api.codeshare.com'
token = 'your-jwt-token'
headers = {
    'Authorization': f'Bearer {token}',
    'Content-Type': 'application/json'
}

# 获取系统设置
def get_settings():
    response = requests.get(f'{base_url}/api/settings', headers=headers)
    if response.status_code == 200:
        data = response.json()
        if data['success']:
            return data['data']
    return None

# 更新站点设置
def update_site_settings(settings):
    response = requests.put(f'{base_url}/api/settings/site', 
                          headers=headers, 
                          json=settings)
    if response.status_code == 200:
        data = response.json()
        return data['success']
    return False

# 导出设置
def export_settings(format='json'):
    params = {
        'format': format,
        'includeSiteSettings': True,
        'includeSecuritySettings': True,
        'includeFeatureSettings': True,
        'includeEmailSettings': True
    }
    response = requests.get(f'{base_url}/api/settings/export', 
                          headers=headers, 
                          params=params)
    if response.status_code == 200:
        return response.json()
    return None
```

## 速率限制

API 端点有速率限制：

- **常规端点**: 1000 请求/小时
- **导入导出端点**: 100 请求/小时
- **邮件测试端点**: 50 请求/小时

超过限制时，返回 `429 Too Many Requests` 状态码。

## 版本信息

- **API 版本**: v1.0.0
- **文档版本**: v1.0.0
- **最后更新**: 2025-01-01

## 联系支持

如有 API 使用问题，请联系技术支持团队。

---

© 2025 CodeShare. 保留所有权利。