# 项目结构与组织

## 根目录布局

```
├── backend/                    # .NET 8 Web API 后端
├── frontend/                   # Vue 3 + TypeScript 前端  
├── database/                   # 数据库脚本和迁移
├── .kiro/                      # Kiro IDE 配置
├── README.md                   # 项目文档
└── LICENSE                     # MIT 许可证
```

## 后端结构 (`backend/`)

### 核心架构（三层模式）

```
backend/
├── Controllers/                # API 端点（表现层）
├── Services/                   # 业务逻辑（服务层）
├── Repositories/               # 数据访问（数据层）
├── Interfaces/                 # 契约定义
├── Models/                     # 领域实体
├── DTOs/                       # 数据传输对象
├── Data/                       # 数据库连接工厂
├── Properties/                 # 程序集信息和启动设置
├── bin/                        # 构建输出
├── obj/                        # 构建中间文件
├── Program.cs                  # 应用程序入口点
├── appsettings.json           # 生产环境配置
├── appsettings.Development.json # 开发环境配置
└── CodeSnippetManager.Api.csproj # 项目文件
```

### 命名约定
- **控制器**：`{实体}Controller.cs`（如：`CodeSnippetController.cs`）
- **服务**：`{实体}Service.cs` 配合接口 `I{实体}Service.cs`
- **仓储**：`{实体}Repository.cs` 配合接口 `I{实体}Repository.cs`
- **模型**：单数实体名称（如：`CodeSnippet.cs`、`User.cs`）
- **DTOs**：按用途命名（如：`CreateSnippetDto.cs`、`UpdateSnippetDto.cs`）

### 依赖流向
```
控制器 → 服务 → 仓储 → 数据库
  ↓      ↓      ↓
DTOs → 模型 → 模型
```

## 前端结构 (`frontend/`)

### Vue 3 + TypeScript 组织

```
frontend/
├── src/
│   ├── components/             # 可复用的 Vue 组件
│   ├── views/                  # 页面级组件
│   ├── stores/                 # Pinia 状态管理
│   ├── services/               # API 服务层
│   ├── types/                  # TypeScript 类型定义
│   ├── router/                 # Vue Router 配置
│   ├── assets/                 # 静态资源（图片、样式）
│   ├── App.vue                 # 根组件
│   └── main.ts                 # 应用程序入口点
├── public/                     # 公共静态文件
├── dist/                       # 构建输出
├── node_modules/               # 依赖包
├── e2e/                        # Playwright 测试
├── package.json                # 依赖和脚本
├── vite.config.ts             # Vite 配置
├── tsconfig.json              # TypeScript 配置
├── eslint.config.ts           # ESLint 配置
└── playwright.config.ts       # E2E 测试配置
```

### 命名约定
- **组件**：PascalCase（如：`CodeEditor.vue`、`SnippetCard.vue`）
- **视图**：PascalCase 加 "View" 后缀（如：`SnippetsView.vue`）
- **状态管理**：camelCase（如：`codeSnippets.ts`、`auth.ts`）
- **服务**：camelCase 加 "Service" 后缀（如：`codeSnippetService.ts`）
- **类型**：PascalCase 接口（如：`CodeSnippet`、`User`）

### 组件结构
```vue
<script setup lang="ts">
// 组合式 API 配合 TypeScript
</script>

<template>
  <!-- 模板，注意无障碍性 -->
</template>

<style scoped>
/* 组件特定样式 */
</style>
```

## 数据库结构 (`database/`)

```
database/
└── init.sql                    # 完整模式初始化
```

### 数据库模式组织
- **Users**：身份验证和授权
- **CodeSnippets**：核心代码片段存储
- **SnippetVersions**：版本历史跟踪
- **Tags**：分类系统
- **SnippetTags**：多对多关系
- **ClipboardHistory**：使用情况跟踪

### 关键模式
- **UUID 主键**：所有实体使用 CHAR(36)
- **审计字段**：CreatedAt、UpdatedAt 时间戳
- **软删除**：适用时使用 IsActive 布尔标志
- **适当索引**：性能优化查询
- **外键约束**：数据完整性保障

## 配置文件

### 后端配置
- `appsettings.json`：生产环境设置
- `appsettings.Development.json`：开发环境覆盖
- 连接字符串、JWT 设置、CORS 策略

### 前端配置
- `package.json`：依赖和 npm 脚本
- `vite.config.ts`：构建工具配置
- `tsconfig.json`：TypeScript 编译器选项
- `eslint.config.ts`：代码质量规则

## 开发工作流

### 文件组织原则
1. **关注点分离**：清晰的层次边界
2. **单一职责**：每个文件一个类/组件
3. **一致命名**：遵循既定约定
4. **接口隔离**：小而专注的接口
5. **依赖方向**：始终依赖抽象

### 代码组织规则
- 将相关文件放在一起（基于功能的分组）
- 使用桶导出实现清洁导入
- 保持一致的文件和文件夹命名
- 遵循既定的三层架构
- 在每一层实现适当的错误处理