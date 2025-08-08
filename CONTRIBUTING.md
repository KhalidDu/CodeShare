# 贡献指南

感谢您对代码片段管理工具项目的关注！我们欢迎所有形式的贡献，包括但不限于代码、文档、问题报告和功能建议。

## 目录

1. [行为准则](#行为准则)
2. [如何贡献](#如何贡献)
3. [开发环境设置](#开发环境设置)
4. [提交指南](#提交指南)
5. [代码规范](#代码规范)
6. [测试要求](#测试要求)
7. [文档贡献](#文档贡献)
8. [问题报告](#问题报告)
9. [功能请求](#功能请求)
10. [代码审查流程](#代码审查流程)

## 行为准则

### 我们的承诺

为了营造一个开放和友好的环境，我们作为贡献者和维护者承诺，无论年龄、体型、残疾、种族、性别认同和表达、经验水平、国籍、个人形象、种族、宗教或性取向如何，参与我们项目和社区的每个人都能获得无骚扰的体验。

### 我们的标准

有助于创造积极环境的行为包括：

- 使用友好和包容的语言
- 尊重不同的观点和经验
- 优雅地接受建设性批评
- 关注对社区最有利的事情
- 对其他社区成员表示同理心

不可接受的行为包括：

- 使用性化的语言或图像，以及不受欢迎的性关注或性骚扰
- 恶意评论、侮辱/贬损评论，以及个人或政治攻击
- 公开或私下骚扰
- 未经明确许可，发布他人的私人信息，如物理或电子地址
- 在专业环境中可能被合理认为不适当的其他行为

### 执行

如果您遇到不当行为，请联系项目团队：conduct@your-domain.com。所有投诉都将被审查和调查，并将产生被认为必要和适当的回应。

## 如何贡献

### 贡献类型

我们欢迎以下类型的贡献：

1. **代码贡献**
   - 新功能开发
   - Bug 修复
   - 性能优化
   - 代码重构

2. **文档贡献**
   - API 文档改进
   - 用户指南更新
   - 代码注释完善
   - 翻译工作

3. **测试贡献**
   - 单元测试
   - 集成测试
   - 端到端测试
   - 性能测试

4. **设计贡献**
   - UI/UX 改进
   - 图标设计
   - 用户体验优化

5. **社区贡献**
   - 问题报告
   - 功能建议
   - 社区支持
   - 推广宣传

### 贡献流程

1. **Fork 项目**
   ```bash
   # 在 GitHub 上 Fork 项目
   # 然后克隆到本地
   git clone https://github.com/your-username/codesnippet-manager.git
   cd codesnippet-manager
   ```

2. **创建分支**
   ```bash
   # 从 main 分支创建新分支
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix
   ```

3. **进行更改**
   - 编写代码
   - 添加测试
   - 更新文档

4. **提交更改**
   ```bash
   git add .
   git commit -m "feat: add new feature description"
   ```

5. **推送分支**
   ```bash
   git push origin feature/your-feature-name
   ```

6. **创建 Pull Request**
   - 在 GitHub 上创建 Pull Request
   - 填写详细的描述
   - 等待代码审查

## 开发环境设置

### 系统要求

- **.NET 8 SDK**
- **Node.js 20+**
- **MySQL 8.0+**
- **Git**

### 环境配置

1. **克隆项目**
   ```bash
   git clone https://github.com/your-org/codesnippet-manager.git
   cd codesnippet-manager
   ```

2. **设置数据库**
   ```sql
   CREATE DATABASE CodeSnippetManager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```
   ```bash
   mysql -u root -p CodeSnippetManager < database/init.sql
   ```

3. **配置后端**
   ```bash
   cd backend
   dotnet restore
   # 更新 appsettings.Development.json 中的数据库连接字符串
   dotnet run
   ```

4. **配置前端**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

### 开发工具推荐

- **IDE**: Visual Studio Code, Visual Studio 2022, JetBrains Rider
- **数据库工具**: MySQL Workbench, DBeaver
- **API 测试**: Postman, Insomnia
- **Git 客户端**: GitHub Desktop, SourceTree

## 提交指南

### 提交消息格式

我们使用 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

### 提交类型

- **feat**: 新功能
- **fix**: Bug 修复
- **docs**: 文档更新
- **style**: 代码格式化（不影响代码运行的变动）
- **refactor**: 重构（既不是新增功能，也不是修改 bug 的代码变动）
- **perf**: 性能优化
- **test**: 增加测试
- **chore**: 构建过程或辅助工具的变动
- **ci**: CI 配置文件和脚本的变动
- **revert**: 回滚之前的提交

### 提交示例

```bash
# 新功能
git commit -m "feat(auth): add JWT token refresh mechanism"

# Bug 修复
git commit -m "fix(api): resolve null reference exception in snippet service"

# 文档更新
git commit -m "docs: update API documentation for user endpoints"

# 重构
git commit -m "refactor(frontend): extract common validation logic to composable"

# 性能优化
git commit -m "perf(database): add indexes to improve query performance"
```

### 提交最佳实践

1. **保持提交原子性**：每个提交只做一件事
2. **编写清晰的提交消息**：描述做了什么和为什么
3. **避免大型提交**：将大的更改拆分为多个小提交
4. **测试后再提交**：确保代码能够正常运行
5. **遵循代码规范**：使用项目的代码风格

## 代码规范

### C# 代码规范

#### 命名约定

```csharp
// 类名：PascalCase
public class CodeSnippetService { }

// 方法名：PascalCase
public async Task<CodeSnippet> GetSnippetAsync(Guid id) { }

// 属性名：PascalCase
public string Title { get; set; }

// 字段名：camelCase with underscore prefix
private readonly IRepository _repository;

// 参数名：camelCase
public void UpdateSnippet(Guid snippetId, string newTitle) { }

// 常量：PascalCase
public const int MaxRetryCount = 3;
```

#### 代码风格

```csharp
// 使用 var 当类型明显时
var snippet = new CodeSnippet();
var snippets = await _repository.GetAllAsync();

// 显式类型当不明显时
IEnumerable<CodeSnippet> filteredSnippets = snippets.Where(s => s.IsPublic);

// 方法参数验证
public async Task<CodeSnippet> GetSnippetAsync(Guid id)
{
    if (id == Guid.Empty)
        throw new ArgumentException("ID cannot be empty", nameof(id));
    
    // 方法实现...
}

// 使用 using 语句
using var connection = _connectionFactory.CreateConnection();
var result = await connection.QueryAsync<CodeSnippet>(sql);

// 异步方法命名
public async Task<List<CodeSnippet>> GetSnippetsAsync() { }
public async Task<CodeSnippet> CreateSnippetAsync(CreateSnippetDto dto) { }
```

### TypeScript 代码规范

#### 命名约定

```typescript
// 接口：PascalCase
interface CodeSnippet {
  id: string;
  title: string;
}

// 类型：PascalCase
type UserRole = 'Admin' | 'Editor' | 'Viewer';

// 变量：camelCase
const snippetList: CodeSnippet[] = [];
let currentUser: User | null = null;

// 函数：camelCase
function fetchSnippets(): Promise<CodeSnippet[]> { }
const createSnippet = async (data: CreateSnippetRequest) => { };

// 常量：UPPER_SNAKE_CASE
const API_BASE_URL = 'https://api.example.com';
const MAX_RETRY_ATTEMPTS = 3;

// 组件：PascalCase
const SnippetCard = defineComponent({ });
```

#### 代码风格

```typescript
// 使用类型注解
const fetchSnippet = async (id: string): Promise<CodeSnippet> => {
  const response = await api.get<CodeSnippet>(`/snippets/${id}`);
  return response.data;
};

// 使用可选链
const title = snippet?.title ?? 'Untitled';

// 使用解构赋值
const { title, description, code } = snippet;

// 使用模板字符串
const message = `Snippet "${title}" created successfully`;

// 错误处理
try {
  const snippet = await fetchSnippet(id);
  return snippet;
} catch (error) {
  console.error('Failed to fetch snippet:', error);
  throw new Error('Failed to fetch snippet');
}
```

### Vue 组件规范

```vue
<script setup lang="ts">
// 导入顺序：Vue API -> 第三方库 -> 本地模块
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useSnippetStore } from '@/stores/snippet';

// Props 定义
interface Props {
  snippetId: string;
  readonly?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  readonly: false,
});

// Emits 定义
interface Emits {
  (e: 'update', snippet: CodeSnippet): void;
  (e: 'delete', id: string): void;
}

const emit = defineEmits<Emits>();

// 响应式数据
const loading = ref(false);
const error = ref<string | null>(null);

// 计算属性
const isEditable = computed(() => !props.readonly && !loading.value);

// 方法
const handleUpdate = (snippet: CodeSnippet) => {
  emit('update', snippet);
};
</script>

<template>
  <div class="snippet-card">
    <!-- 模板内容 -->
  </div>
</template>

<style scoped>
.snippet-card {
  /* 样式定义 */
}
</style>
```

## 测试要求

### 测试覆盖率

- **最低要求**：80% 代码覆盖率
- **推荐目标**：90% 代码覆盖率
- **关键路径**：100% 覆盖率

### 后端测试

#### 单元测试

```csharp
[TestClass]
public class CodeSnippetServiceTests
{
    private Mock<ICodeSnippetRepository> _mockRepository;
    private CodeSnippetService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ICodeSnippetRepository>();
        _service = new CodeSnippetService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task GetSnippetAsync_ValidId_ReturnsSnippet()
    {
        // Arrange
        var snippetId = Guid.NewGuid();
        var expectedSnippet = new CodeSnippet { Id = snippetId, Title = "Test" };
        _mockRepository.Setup(r => r.GetByIdAsync(snippetId))
                      .ReturnsAsync(expectedSnippet);

        // Act
        var result = await _service.GetSnippetAsync(snippetId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSnippet.Title, result.Title);
    }

    [TestMethod]
    public async Task GetSnippetAsync_InvalidId_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.GetSnippetAsync(Guid.Empty)
        );
    }
}
```

#### 集成测试

```csharp
[TestClass]
public class CodeSnippetsControllerIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetSnippets_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/codesnippets");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}
```

### 前端测试

#### 组件测试

```typescript
import { describe, it, expect, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import SnippetCard from '@/components/SnippetCard.vue';

describe('SnippetCard', () => {
  it('renders snippet information correctly', () => {
    const snippet = {
      id: '1',
      title: 'Test Snippet',
      language: 'javascript',
    };

    const wrapper = mount(SnippetCard, {
      props: { snippet },
    });

    expect(wrapper.find('.snippet-title').text()).toBe('Test Snippet');
    expect(wrapper.find('.snippet-language').text()).toBe('javascript');
  });

  it('emits copy event when copy button is clicked', async () => {
    const snippet = { id: '1', title: 'Test', code: 'test code' };
    const wrapper = mount(SnippetCard, { props: { snippet } });

    await wrapper.find('.copy-button').trigger('click');

    expect(wrapper.emitted('copy')).toBeTruthy();
  });
});
```

#### E2E 测试

```typescript
import { test, expect } from '@playwright/test';

test('user can create a new snippet', async ({ page }) => {
  // 登录
  await page.goto('/login');
  await page.fill('[data-testid="username"]', 'testuser');
  await page.fill('[data-testid="password"]', 'password');
  await page.click('[data-testid="login-button"]');

  // 创建代码片段
  await page.click('[data-testid="create-snippet-button"]');
  await page.fill('[data-testid="snippet-title"]', 'Test Snippet');
  await page.fill('[data-testid="snippet-code"]', 'console.log("test");');
  await page.selectOption('[data-testid="snippet-language"]', 'javascript');
  await page.click('[data-testid="save-button"]');

  // 验证创建成功
  await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
});
```

### 运行测试

```bash
# 后端测试
cd backend
dotnet test

# 前端单元测试
cd frontend
npm run test:unit

# 前端 E2E 测试
npm run test:e2e

# 测试覆盖率
npm run test:coverage
```

## 文档贡献

### 文档类型

1. **API 文档**：使用 OpenAPI/Swagger 规范
2. **用户文档**：Markdown 格式，存放在 `docs/` 目录
3. **代码注释**：内联文档，使用 JSDoc 和 XML 注释
4. **README**：项目概述和快速开始指南

### 文档规范

#### Markdown 规范

```markdown
# 一级标题

## 二级标题

### 三级标题

- 使用无序列表
- 保持一致的格式

1. 使用有序列表
2. 编号自动递增

**粗体文本**
*斜体文本*
`行内代码`

\```typescript
// 代码块
const example = 'code block';
\```

[链接文本](https://example.com)

![图片描述](image-url)
```

#### API 文档规范

```csharp
/// <summary>
/// 获取代码片段列表
/// </summary>
/// <param name="filter">筛选条件</param>
/// <returns>分页的代码片段列表</returns>
/// <response code="200">成功返回代码片段列表</response>
/// <response code="400">请求参数错误</response>
/// <response code="401">未授权访问</response>
[HttpGet]
[ProducesResponseType(typeof(PaginatedResult<CodeSnippetDto>), 200)]
[ProducesResponseType(400)]
[ProducesResponseType(401)]
public async Task<ActionResult<PaginatedResult<CodeSnippetDto>>> GetSnippets(
    [FromQuery] SnippetFilterDto filter)
{
    // 实现代码
}
```

## 问题报告

### 报告 Bug

使用 GitHub Issues 报告 Bug，请包含以下信息：

1. **问题描述**：清晰简洁地描述问题
2. **重现步骤**：详细的重现步骤
3. **预期行为**：描述期望的正确行为
4. **实际行为**：描述实际发生的行为
5. **环境信息**：
   - 操作系统和版本
   - 浏览器和版本
   - 应用程序版本
6. **截图或日志**：如果适用，提供截图或错误日志

### Bug 报告模板

```markdown
## Bug 描述
简洁清晰地描述这个 bug。

## 重现步骤
1. 进入 '...'
2. 点击 '....'
3. 滚动到 '....'
4. 看到错误

## 预期行为
清晰简洁地描述你期望发生什么。

## 实际行为
清晰简洁地描述实际发生了什么。

## 截图
如果适用，添加截图来帮助解释你的问题。

## 环境信息
- 操作系统: [例如 Windows 10, macOS 12.0]
- 浏览器: [例如 Chrome 96, Safari 15]
- 版本: [例如 v1.0.0]

## 附加信息
添加任何其他关于问题的信息。
```

## 功能请求

### 提出新功能

使用 GitHub Issues 提出功能请求，请包含：

1. **功能描述**：详细描述新功能
2. **使用场景**：说明使用场景和价值
3. **解决方案**：描述你希望的解决方案
4. **替代方案**：描述你考虑过的替代方案
5. **附加信息**：任何其他相关信息

### 功能请求模板

```markdown
## 功能描述
清晰简洁地描述你想要的功能。

## 使用场景
描述这个功能解决什么问题，为什么需要这个功能。

## 解决方案
清晰简洁地描述你希望的解决方案。

## 替代方案
清晰简洁地描述你考虑过的任何替代解决方案或功能。

## 附加信息
添加任何其他关于功能请求的信息或截图。
```

## 代码审查流程

### Pull Request 要求

1. **描述清晰**：详细描述更改内容和原因
2. **测试完整**：包含相应的测试用例
3. **文档更新**：如果需要，更新相关文档
4. **代码规范**：遵循项目的代码规范
5. **提交历史**：保持清晰的提交历史

### PR 模板

```markdown
## 更改描述
简要描述这个 PR 的更改内容。

## 更改类型
- [ ] Bug 修复
- [ ] 新功能
- [ ] 重构
- [ ] 文档更新
- [ ] 性能优化
- [ ] 其他

## 测试
- [ ] 添加了新的测试用例
- [ ] 所有测试都通过
- [ ] 手动测试完成

## 检查清单
- [ ] 代码遵循项目规范
- [ ] 自我审查了代码
- [ ] 添加了必要的注释
- [ ] 更新了相关文档
- [ ] 没有引入新的警告

## 相关 Issue
关闭 #(issue 编号)

## 截图（如果适用）
添加截图来展示更改效果。
```

### 审查标准

代码审查将检查以下方面：

1. **功能正确性**：代码是否正确实现了预期功能
2. **代码质量**：代码是否清晰、可维护
3. **性能影响**：是否有性能问题
4. **安全性**：是否存在安全漏洞
5. **测试覆盖**：是否有足够的测试覆盖
6. **文档完整性**：是否更新了相关文档

### 审查流程

1. **自动检查**：CI/CD 流水线自动运行测试和代码检查
2. **人工审查**：至少一名维护者进行代码审查
3. **反馈处理**：根据审查意见修改代码
4. **最终批准**：审查通过后合并到主分支

## 发布流程

### 版本号规范

我们使用 [语义化版本](https://semver.org/) 规范：

- **主版本号**：不兼容的 API 修改
- **次版本号**：向下兼容的功能性新增
- **修订号**：向下兼容的问题修正

### 发布步骤

1. **创建发布分支**
   ```bash
   git checkout -b release/v1.1.0
   ```

2. **更新版本号**
   - 更新 `package.json`
   - 更新 `.csproj` 文件
   - 更新文档中的版本引用

3. **更新 CHANGELOG**
   - 记录新功能
   - 记录 Bug 修复
   - 记录破坏性更改

4. **测试发布版本**
   - 运行完整测试套件
   - 执行手动测试
   - 验证部署流程

5. **创建发布 PR**
   - 合并到 main 分支
   - 创建 Git 标签
   - 发布到生产环境

## 社区参与

### 讨论和交流

- **GitHub Discussions**：项目讨论和问答
- **Issues**：Bug 报告和功能请求
- **Pull Requests**：代码贡献和审查

### 获得帮助

如果您需要帮助：

1. 查看 [文档](docs/)
2. 搜索现有的 [Issues](https://github.com/your-org/codesnippet-manager/issues)
3. 创建新的 Issue
4. 联系维护者：maintainers@your-domain.com

### 成为维护者

如果您想成为项目维护者：

1. 持续贡献高质量的代码
2. 积极参与代码审查
3. 帮助其他贡献者
4. 联系现有维护者表达意愿

## 致谢

感谢所有为这个项目做出贡献的开发者！您的贡献让这个项目变得更好。

### 贡献者列表

- [贡献者姓名](https://github.com/username) - 功能描述
- [贡献者姓名](https://github.com/username) - 功能描述

### 特别感谢

- 感谢所有提供反馈和建议的用户
- 感谢开源社区的支持和启发
- 感谢所有依赖项目的维护者

---

再次感谢您的贡献！让我们一起构建更好的代码片段管理工具。