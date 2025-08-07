# 技术栈与构建系统

## 后端 (.NET 8 Web API)

### 核心技术
- **.NET 8**：最新 LTS 版本，支持最小化 API 和性能优化
- **Dapper ORM**：轻量级微型 ORM，用于数据库操作
- **MySQL 8.0+**：主数据库，使用 UTF8MB4 字符集
- **JWT 认证**：基于令牌的身份验证，使用 BCrypt 密码哈希
- **Swagger/OpenAPI**：API 文档和测试接口

### 关键依赖包
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Dapper" Version="2.1.66" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.11" />
<PackageReference Include="MySql.Data" Version="9.4.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.6.2" />
```

### 架构模式
- **三层架构**：控制器 → 服务 → 仓储
- **依赖注入**：使用内置 .NET DI 容器，作用域生命周期
- **面向接口设计**：所有服务都实现契约接口，便于测试
- **SOLID 原则**：严格遵循设计原则

## 前端 (Vue 3 + TypeScript)

### 核心技术
- **Vue 3**：组合式 API，使用 `<script setup>` 语法
- **TypeScript**：完整类型安全，严格配置
- **Vite**：快速构建工具和开发服务器
- **Pinia**：状态管理，使用组合式 API 模式
- **Vue Router**：客户端路由，带有路由守卫
- **Axios**：HTTP 客户端，用于 API 通信

### 开发工具
- **ESLint**：代码检查，使用 Vue 和 TypeScript 规则
- **Prettier**：代码格式化，保持一致的代码风格
- **Vitest**：单元测试框架
- **Playwright**：端到端测试
- **Vue DevTools**：开发调试工具

### Node.js 要求
- **Node.js**：^20.19.0 || >=22.12.0
- **包管理器**：npm（锁定文件：package-lock.json）

## 数据库

### MySQL 配置
- **版本**：8.0+
- **字符集**：utf8mb4_unicode_ci
- **连接**：MySql.Data 提供程序配合 Dapper
- **模式**：UUID 主键，适当索引，外键约束

## 常用命令

### 后端开发
```bash
# 进入后端目录
cd backend

# 还原包
dotnet restore

# 运行开发服务器
dotnet run

# 生产环境构建
dotnet build --configuration Release

# 运行测试
dotnet test
```

### 前端开发
```bash
# 进入前端目录
cd frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev

# 生产环境构建
npm run build

# 运行单元测试
npm run test:unit

# 运行端到端测试
npm run test:e2e

# 代码检查和修复
npm run lint

# 格式化代码
npm run format

# 类型检查
npm run type-check
```

### 数据库设置
```bash
# 创建数据库
mysql -u root -p -e "CREATE DATABASE CodeSnippetManager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

# 初始化模式
mysql -u root -p CodeSnippetManager < database/init.sql
```

## 开发端口
- **后端 API**：http://localhost:5000
- **Swagger UI**：http://localhost:5000/swagger
- **前端开发**：http://localhost:5173
- **MySQL**：localhost:3306

## 环境配置
- **后端**：appsettings.json / appsettings.Development.json
- **前端**：通过 Vite 的环境变量
- **CORS**：配置为允许 localhost:5173 和 localhost:3000