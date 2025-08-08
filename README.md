# 代码片段管理工具

[![CI/CD Pipeline](https://github.com/your-org/codesnippet-manager/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/your-org/codesnippet-manager/actions/workflows/ci-cd.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![Vue.js](https://img.shields.io/badge/Vue.js-3.0-green.svg)](https://vuejs.org/)

一个支持团队协作的代码片段存储、管理和分享平台，专为开发团队设计。

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

后端将在 `http://localhost:6676` 启动，Swagger 文档在 `http://localhost:6676/swagger`

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

前端将在 `http://localhost:6677` 启动

### 5. 默认账户

- 用户名：`admin`
- 密码：`admin123`
- 角色：管理员

## 功能特性

### 核心功能
- ✅ **用户权限管理** - 基于角色的访问控制（管理员、编辑者、查看者）
- ✅ **代码片段管理** - 创建、编辑、删除和查看代码片段
- ✅ **版本控制** - 自动版本管理和历史记录跟踪
- ✅ **标签系统** - 使用标签对代码片段进行分类和组织
- ✅ **搜索和筛选** - 按关键词、语言、标签快速查找代码片段
- ✅ **一键复制** - 快速复制代码到剪贴板
- ✅ **剪贴板历史** - 跟踪复制历史和使用统计
- ✅ **语法高亮** - 支持多种编程语言的语法高亮显示
- ✅ **响应式设计** - 适配桌面和移动设备

### 技术特性
- ✅ **高性能** - 使用 Dapper ORM 和优化的数据库查询
- ✅ **安全性** - JWT 认证、XSS 防护、CSRF 保护、请求频率限制
- ✅ **可扩展性** - 遵循 SOLID 原则的三层架构设计
- ✅ **容器化** - Docker 支持，便于部署和扩展
- ✅ **CI/CD** - 自动化构建、测试和部署流水线

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

启动后端后，访问 `http://localhost:6676/swagger` 查看完整的 API 文档。

## 许可证

MIT License
## 部署指南


### Docker 部署（推荐）

1. 克隆项目：
```bash
git clone https://github.com/your-org/codesnippet-manager.git
cd codesnippet-manager
```

2. 复制环境变量文件：
```bash
cp .env.example .env.production
```

3. 编辑环境变量：
```bash
# 编辑 .env.production 文件，设置数据库连接、JWT 密钥等
```

4. 使用部署脚本：
```bash
./scripts/deploy.sh production
```

### 手动部署

详细的手动部署步骤请参考 [部署文档](docs/deployment.md)。

### Kubernetes 部署

Kubernetes 配置文件位于 `k8s/` 目录，详细说明请参考 [Kubernetes 部署指南](docs/kubernetes.md)。

## 文档

- [用户手册](docs/user-guide.md) - 如何使用代码片段管理工具
- [管理员指南](docs/admin-guide.md) - 系统管理和配置
- [开发者文档](docs/developer-guide.md) - 开发环境设置和代码贡献指南
- [API 文档](docs/api-reference.md) - 完整的 API 参考
- [部署文档](docs/deployment.md) - 生产环境部署指南

## 贡献

我们欢迎社区贡献！请阅读 [贡献指南](CONTRIBUTING.md) 了解如何参与项目开发。

### 开发流程

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 支持

如果您遇到问题或有建议，请：

1. 查看 [常见问题](docs/faq.md)
2. 搜索现有的 [Issues](https://github.com/your-org/codesnippet-manager/issues)
3. 创建新的 Issue

## 更新日志

查看 [CHANGELOG.md](CHANGELOG.md) 了解版本更新历史。

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 致谢

感谢所有为这个项目做出贡献的开发者和用户。