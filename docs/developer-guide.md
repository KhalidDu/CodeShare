# 开发者文档

## 目录

1. [开发环境设置](#开发环境设置)
2. [项目架构](#项目架构)
3. [编码规范](#编码规范)
4. [API 开发指南](#api-开发指南)
5. [前端开发指南](#前端开发指南)
6. [测试指南](#测试指南)
7. [部署指南](#部署指南)
8. [贡献指南](#贡献指南)

## 开发环境设置

### 系统要求

- **操作系统**：Windows 10+, macOS 10.15+, Ubuntu 18.04+
- **.NET SDK**：8.0 或更高版本
- **Node.js**：20.x 或更高版本
- **MySQL**：8.0 或更高版本
- **Git**：2.30 或更高版本

### 开发工具推荐

- **IDE**：Visual Studio 2022, VS Code, JetBrains Rider
- **数据库工具**：MySQL Workbench, DBeaver
- **API 测试**：Postman, Insomnia
- **版本控制**：Git, GitHub Desktop

### 环境配置

#### 1. 克隆项目

```bash
git clone https://github.com/your-org/codesnippet-manager.git
cd codesnippet-manager
```

#### 2. 数据库设置

```sql
CREATE DATABASE CodeSnippetManager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
mysql -u root -p CodeSnippetManager < database/init.sql
```

#### 3. 后端设置

```bash
cd backend
dotnet restore
dotnet build
```

#### 4. 前端设置

```bash
cd frontend
npm install
npm run dev
```##
 项目架构

### 后端架构（三层架构）

```
┌─────────────────┐
│  Controllers    │  ← 表现层（API 端点）
├─────────────────┤
│  Services       │  ← 业务逻辑层
├─────────────────┤
│  Repositories   │  ← 数据访问层
├─────────────────┤
│  Database       │  ← 数据存储层
└─────────────────┘
```

### 设计原则

项目严格遵循 SOLID 原则：

1. **单一职责原则 (SRP)**：每个类只负责一个功能
2. **开闭原则 (OCP)**：对扩展开放，对修改关闭
3. **里氏替换原则 (LSP)**：子类可以替换父类
4. **接口隔离原则 (ISP)**：接口应该小而专一
5. **依赖倒置原则 (DIP)**：依赖抽象而非具体实现

### 依赖注入

使用 .NET 内置 DI 容器：

```csharp
// 注册服务
builder.Services.AddScoped<ICodeSnippetService, CodeSnippetService>();
builder.Services.AddScoped<ICodeSnippetRepository, CodeSnippetRepository>();

// 构造函数注入
public class CodeSnippetService : ICodeSnippetService
{
    private readonly ICodeSnippetRepository _repository;
    
    public CodeSnippetService(ICodeSnippetRepository repository)
    {
        _repository = repository;
    }
}
```

## 编码规范

### C# 编码规范

#### 命名约定

- **类名**：PascalCase (`CodeSnippetService`)
- **方法名**：PascalCase (`GetSnippetAsync`)
- **属性名**：PascalCase (`CreatedAt`)
- **字段名**：camelCase with underscore (`_repository`)
- **参数名**：camelCase (`snippetId`)
- **常量**：PascalCase (`MaxRetryCount`)

#### 代码示例

```csharp
/// <summary>
/// 代码片段服务接口
/// </summary>
public interface ICodeSnippetService
{
    /// <summary>
    /// 异步获取代码片段
    /// </summary>
    /// <param name="id">片段ID</param>
    /// <returns>代码片段DTO</returns>
    Task<CodeSnippetDto> GetSnippetAsync(Guid id);
}

/// <summary>
/// 代码片段服务实现
/// </summary>
public class CodeSnippetService : ICodeSnippetService
{
    private readonly ICodeSnippetRepository _repository;
    private readonly ILogger<CodeSnippetService> _logger;

    public CodeSnippetService(
        ICodeSnippetRepository repository,
        ILogger<CodeSnippetService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CodeSnippetDto> GetSnippetAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("ID cannot be empty", nameof(id));

        try
        {
            var snippet = await _repository.GetByIdAsync(id);
            if (snippet == null)
                throw new NotFoundException($"Snippet with ID {id} not found");

            return MapToDto(snippet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving snippet {SnippetId}", id);
            throw;
        }
    }
}
```

### TypeScript 编码规范

#### 命名约定

- **接口名**：PascalCase (`CodeSnippet`)
- **类型名**：PascalCase (`UserRole`)
- **变量名**：camelCase (`snippetList`)
- **函数名**：camelCase (`fetchSnippets`)
- **常量**：UPPER_SNAKE_CASE (`API_BASE_URL`)

#### 代码示例

```typescript
// 接口定义
interface CodeSnippet {
  id: string;
  title: string;
  description: string;
  code: string;
  language: string;
  createdAt: Date;
  updatedAt: Date;
}

// 服务类
class CodeSnippetService {
  private readonly apiClient: ApiClient;

  constructor(apiClient: ApiClient) {
    this.apiClient = apiClient;
  }

  /**
   * 获取代码片段列表
   * @param filter 筛选条件
   * @returns 代码片段列表
   */
  async getSnippets(filter: SnippetFilter): Promise<PaginatedResult<CodeSnippet>> {
    try {
      const response = await this.apiClient.get<PaginatedResult<CodeSnippet>>(
        '/api/codesnippets',
        { params: filter }
      );
      return response.data;
    } catch (error) {
      console.error('Failed to fetch snippets:', error);
      throw new Error('Failed to fetch snippets');
    }
  }
}
```

## API 开发指南

### RESTful API 设计

#### 资源命名

- 使用复数名词：`/api/codesnippets`
- 使用连字符分隔：`/api/code-snippets`（如果需要）
- 避免动词：使用 HTTP 方法表示操作

#### HTTP 方法

- `GET`：获取资源
- `POST`：创建资源
- `PUT`：完整更新资源
- `PATCH`：部分更新资源
- `DELETE`：删除资源

#### 状态码

- `200 OK`：成功
- `201 Created`：创建成功
- `204 No Content`：删除成功
- `400 Bad Request`：请求错误
- `401 Unauthorized`：未认证
- `403 Forbidden`：权限不足
- `404 Not Found`：资源不存在
- `500 Internal Server Error`：服务器错误

### 控制器开发

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CodeSnippetsController : ControllerBase
{
    private readonly ICodeSnippetService _service;

    public CodeSnippetsController(ICodeSnippetService service)
    {
        _service = service;
    }

    /// <summary>
    /// 获取代码片段列表
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<CodeSnippetDto>), 200)]
    public async Task<ActionResult<PaginatedResult<CodeSnippetDto>>> GetSnippets(
        [FromQuery] SnippetFilterDto filter)
    {
        var result = await _service.GetSnippetsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// 创建代码片段
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CodeSnippetDto), 201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<CodeSnippetDto>> CreateSnippet(
        [FromBody] CreateSnippetDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var snippet = await _service.CreateSnippetAsync(dto);
        return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, snippet);
    }
}
```

### 数据传输对象 (DTO)

```csharp
/// <summary>
/// 创建代码片段请求 DTO
/// </summary>
public class CreateSnippetDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [StringLength(1000)]
    public string Description { get; set; }

    [Required]
    public string Code { get; set; }

    [Required]
    [StringLength(50)]
    public string Language { get; set; }

    public List<string> Tags { get; set; } = new();

    public bool IsPublic { get; set; }
}
```

## 前端开发指南

### Vue 3 组件开发

#### 组合式 API

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useCodeSnippetStore } from '@/stores/codeSnippet';

// Props 定义
interface Props {
  snippetId: string;
}

const props = defineProps<Props>();

// 响应式数据
const snippet = ref<CodeSnippet | null>(null);
const loading = ref(false);
const error = ref<string | null>(null);

// Store
const snippetStore = useCodeSnippetStore();

// 计算属性
const formattedDate = computed(() => {
  return snippet.value?.createdAt 
    ? new Date(snippet.value.createdAt).toLocaleDateString()
    : '';
});

// 方法
const fetchSnippet = async () => {
  loading.value = true;
  error.value = null;
  
  try {
    snippet.value = await snippetStore.fetchSnippet(props.snippetId);
  } catch (err) {
    error.value = 'Failed to load snippet';
    console.error(err);
  } finally {
    loading.value = false;
  }
};

// 生命周期
onMounted(() => {
  fetchSnippet();
});
</script>

<template>
  <div class="snippet-detail">
    <div v-if="loading" class="loading">
      Loading...
    </div>
    
    <div v-else-if="error" class="error">
      {{ error }}
    </div>
    
    <div v-else-if="snippet" class="snippet-content">
      <h1>{{ snippet.title }}</h1>
      <p>{{ snippet.description }}</p>
      <pre><code>{{ snippet.code }}</code></pre>
      <small>Created: {{ formattedDate }}</small>
    </div>
  </div>
</template>

<style scoped>
.snippet-detail {
  padding: 1rem;
}

.loading, .error {
  text-align: center;
  padding: 2rem;
}

.error {
  color: #e74c3c;
}

.snippet-content h1 {
  margin-bottom: 0.5rem;
}

pre {
  background: #f8f9fa;
  padding: 1rem;
  border-radius: 4px;
  overflow-x: auto;
}
</style>
```

### 状态管理 (Pinia)

```typescript
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { codeSnippetService } from '@/services/codeSnippetService';

export const useCodeSnippetStore = defineStore('codeSnippet', () => {
  // State
  const snippets = ref<CodeSnippet[]>([]);
  const currentSnippet = ref<CodeSnippet | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  // Getters
  const snippetCount = computed(() => snippets.value.length);
  const publicSnippets = computed(() => 
    snippets.value.filter(s => s.isPublic)
  );

  // Actions
  const fetchSnippets = async (filter?: SnippetFilter) => {
    loading.value = true;
    error.value = null;

    try {
      const result = await codeSnippetService.getSnippets(filter);
      snippets.value = result.items;
    } catch (err) {
      error.value = 'Failed to fetch snippets';
      throw err;
    } finally {
      loading.value = false;
    }
  };

  const createSnippet = async (snippet: CreateSnippetRequest) => {
    try {
      const newSnippet = await codeSnippetService.createSnippet(snippet);
      snippets.value.unshift(newSnippet);
      return newSnippet;
    } catch (err) {
      error.value = 'Failed to create snippet';
      throw err;
    }
  };

  return {
    // State
    snippets,
    currentSnippet,
    loading,
    error,
    // Getters
    snippetCount,
    publicSnippets,
    // Actions
    fetchSnippets,
    createSnippet,
  };
});
```

## 测试指南

### 后端单元测试

```csharp
[TestClass]
public class CodeSnippetServiceTests
{
    private Mock<ICodeSnippetRepository> _mockRepository;
    private Mock<ILogger<CodeSnippetService>> _mockLogger;
    private CodeSnippetService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ICodeSnippetRepository>();
        _mockLogger = new Mock<ILogger<CodeSnippetService>>();
        _service = new CodeSnippetService(_mockRepository.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetSnippetAsync_ValidId_ReturnsSnippet()
    {
        // Arrange
        var snippetId = Guid.NewGuid();
        var expectedSnippet = new CodeSnippet
        {
            Id = snippetId,
            Title = "Test Snippet",
            Code = "console.log('test');"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(snippetId))
            .ReturnsAsync(expectedSnippet);

        // Act
        var result = await _service.GetSnippetAsync(snippetId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedSnippet.Title, result.Title);
        _mockRepository.Verify(r => r.GetByIdAsync(snippetId), Times.Once);
    }

    [TestMethod]
    public async Task GetSnippetAsync_InvalidId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(
            () => _service.GetSnippetAsync(Guid.Empty)
        );
    }
}
```

### 前端单元测试

```typescript
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import SnippetCard from '@/components/SnippetCard.vue';

describe('SnippetCard', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it('renders snippet information correctly', () => {
    const snippet = {
      id: '1',
      title: 'Test Snippet',
      description: 'A test snippet',
      language: 'javascript',
      createdAt: new Date('2024-01-01'),
      isPublic: true,
    };

    const wrapper = mount(SnippetCard, {
      props: { snippet },
    });

    expect(wrapper.find('.snippet-title').text()).toBe('Test Snippet');
    expect(wrapper.find('.snippet-description').text()).toBe('A test snippet');
    expect(wrapper.find('.snippet-language').text()).toBe('javascript');
  });

  it('emits copy event when copy button is clicked', async () => {
    const snippet = {
      id: '1',
      title: 'Test Snippet',
      code: 'console.log("test");',
    };

    const wrapper = mount(SnippetCard, {
      props: { snippet },
    });

    await wrapper.find('.copy-button').trigger('click');

    expect(wrapper.emitted('copy')).toBeTruthy();
    expect(wrapper.emitted('copy')[0]).toEqual([snippet]);
  });
});
```

### 集成测试

```csharp
[TestClass]
public class CodeSnippetsControllerIntegrationTests
{
    private TestServer _server;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        var builder = new WebHostBuilder()
            .UseStartup<TestStartup>()
            .UseEnvironment("Testing");

        _server = new TestServer(builder);
        _client = _server.CreateClient();
    }

    [TestMethod]
    public async Task GetSnippets_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/codesnippets");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PaginatedResult<CodeSnippetDto>>(content);
        
        Assert.IsNotNull(result);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _server?.Dispose();
    }
}
```

## 部署指南

### 开发环境部署

```bash
# 启动后端
cd backend
dotnet run

# 启动前端
cd frontend
npm run dev
```

### 生产环境部署

```bash
# 使用部署脚本
./scripts/deploy.sh production

# 或手动部署
docker-compose -f docker-compose.prod.yml up -d
```

### 环境变量配置

创建 `.env.production` 文件：

```env
DB_CONNECTION_STRING=Server=mysql-server;Database=CodeSnippetManager;Uid=app_user;Pwd=secure_password;CharSet=utf8mb4;
JWT_SECRET_KEY=your-super-secure-jwt-secret-key
JWT_ISSUER=https://your-domain.com
JWT_AUDIENCE=https://your-domain.com
FRONTEND_URL=https://your-domain.com
```

## 贡献指南

### 开发流程

1. **Fork 项目**到您的 GitHub 账户
2. **创建功能分支**：`git checkout -b feature/new-feature`
3. **编写代码**并遵循编码规范
4. **编写测试**确保代码质量
5. **提交更改**：`git commit -m "Add new feature"`
6. **推送分支**：`git push origin feature/new-feature`
7. **创建 Pull Request**

### 提交消息规范

使用 [Conventional Commits](https://www.conventionalcommits.org/) 规范：

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

类型：
- `feat`: 新功能
- `fix`: 错误修复
- `docs`: 文档更新
- `style`: 代码格式化
- `refactor`: 代码重构
- `test`: 测试相关
- `chore`: 构建过程或辅助工具的变动

示例：
```
feat(auth): add JWT token refresh mechanism

Implement automatic token refresh to improve user experience
and reduce login frequency.

Closes #123
```

### 代码审查

所有 Pull Request 都需要经过代码审查：

1. **自动检查**：CI/CD 流水线会自动运行测试
2. **人工审查**：至少需要一名维护者审查
3. **反馈处理**：根据审查意见修改代码
4. **合并**：审查通过后合并到主分支

### 问题报告

报告 Bug 时请包含：

1. **问题描述**：清晰描述问题
2. **重现步骤**：详细的重现步骤
3. **预期行为**：期望的正确行为
4. **实际行为**：实际发生的行为
5. **环境信息**：操作系统、浏览器版本等
6. **截图或日志**：如果适用

### 功能请求

提出新功能时请包含：

1. **功能描述**：详细描述新功能
2. **使用场景**：说明使用场景和价值
3. **实现建议**：如果有实现想法
4. **替代方案**：考虑过的其他方案

感谢您对项目的贡献！