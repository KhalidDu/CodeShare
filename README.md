# 代码片段管理工具

一个支持团队协作的代码片段存储、管理和分享平台。

## 技术栈

### 后端
- .NET 8 Web API
- Dapper ORM
- MySQL 8.0+
- JWT 认证
- 三层架构设计
- 遵循六大设计原则（单一职责、开闭原则、里氏替换、接口隔离、依赖倒置、迪米特法则）

### 前端
- Vue 3 + Composition API
- TypeScript
- Vite
- Vue Router
- Pinia (状态管理)
- Axios (HTTP 客户端)

## 项目结构

```
├── backend/                    # .NET 8 Web API 后端
│   ├── Controllers/           # API 控制器
│   ├── Services/             # 业务逻辑层
│   ├── Repositories/         # 数据访问层
│   ├── Models/               # 数据模型
│   ├── DTOs/                 # 数据传输对象
│   ├── Interfaces/           # 接口定义
│   └── Data/                 # 数据库连接工厂
├── frontend/                  # Vue 3 前端
│   ├── src/
│   │   ├── components/       # Vue 组件
│   │   ├── views/           # 页面视图
│   │   ├── stores/          # Pinia 状态管理
│   │   ├── services/        # API 服务
│   │   ├── types/           # TypeScript 类型定义
│   │   └── router/          # 路由配置
└── database/                 # 数据库脚本
    └── init.sql             # 数据库初始化脚本
```

## 快速开始

### 1. 环境要求

- .NET 8 SDK
- Node.js 18+
- MySQL 8.0+

### 2. 数据库设置

1. 创建 MySQL 数据库：
```sql
CREATE DATABASE CodeSnippetManager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

2. 执行初始化脚本：
```bash
mysql -u root -p CodeSnippetManager < database/init.sql
```

### 3. 后端设置

1. 进入后端目录：
```bash
cd backend
```

2. 还原 NuGet 包：
```bash
dotnet restore
```

3. 更新数据库连接字符串（appsettings.json）：
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CodeSnippetManager;Uid=root;Pwd=your_password;CharSet=utf8mb4;"
  }
}
```

4. 运行后端：
```bash
dotnet run
```

后端将在 `http://localhost:5000` 启动，Swagger 文档在 `http://localhost:5000/swagger`

### 4. 前端设置

1. 进入前端目录：
```bash
cd frontend
```

2. 安装依赖：
```bash
npm install
```

3. 启动开发服务器：
```bash
npm run dev
```

前端将在 `http://localhost:5173` 启动

### 5. 默认账户

- 用户名：`admin`
- 密码：`admin123`
- 角色：管理员

## 功能特性

### 已实现（任务1）
- ✅ 项目基础架构搭建
- ✅ .NET 8 Web API 项目结构
- ✅ Vue 3 前端项目配置
- ✅ MySQL 数据库连接和 Dapper ORM
- ✅ 三层架构基础结构
- ✅ 核心接口定义（面向接口编程）
- ✅ 依赖注入容器配置
- ✅ JWT 认证基础配置
- ✅ 前端路由和状态管理
- ✅ 基础 UI 组件和页面

### 待实现
- 🔄 数据库实体模型和 Dapper 映射
- 🔄 用户认证和权限管理系统
- 🔄 代码片段核心功能
- 🔄 版本管理功能
- 🔄 标签管理系统
- 🔄 一键复制和剪贴板历史
- 🔄 前端核心组件开发
- 🔄 代码编辑器集成
- 🔄 搜索和筛选功能
- 🔄 用户管理界面
- 🔄 性能优化和缓存
- 🔄 安全加固和测试

## 开发指南

### 后端开发原则

1. **三层架构**：严格分离表现层、业务逻辑层和数据访问层
2. **面向接口编程**：所有业务逻辑都通过接口定义契约
3. **依赖注入**：使用 .NET 内置 DI 容器管理对象生命周期
4. **六大设计原则**：遵循 SOLID 原则和迪米特法则

### 前端开发原则

1. **组件化开发**：使用 Vue 3 Composition API
2. **类型安全**：使用 TypeScript 确保类型安全
3. **状态管理**：使用 Pinia 进行集中状态管理
4. **路由守卫**：实现认证和权限控制

## API 文档

启动后端后，访问 `http://localhost:5000/swagger` 查看完整的 API 文档。

## 许可证

MIT License