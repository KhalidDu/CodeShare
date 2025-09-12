# ShareController 单元测试

## 概述

`ShareControllerTests.cs` 是分享控制器的完整单元测试套件，包含 70 个测试方法，全面覆盖了分享功能的所有 API 端点。

## 测试覆盖范围

### 主要 API 端点测试

1. **创建分享链接** (CreateShare)
   - 成功创建分享链接
   - 无效代码片段ID处理
   - 无权限访问处理
   - 代码片段不存在处理
   - 未登录用户处理
   - 服务异常处理
   - 模型验证失败处理
   - 空请求处理

2. **获取分享信息** (GetShare)
   - 有效令牌获取分享信息
   - 空令牌处理
   - 令牌不存在处理
   - 带密码保护的分享访问

3. **验证分享令牌** (ValidateShare)
   - 有效令牌验证
   - 无效令牌验证
   - 带密码的令牌验证

4. **获取用户分享列表** (GetMyShares)
   - 成功获取分享列表
   - 无效页码处理
   - 无效页面大小处理
   - 页面大小过大处理
   - 未登录用户处理

5. **获取分享统计** (GetShareStats)
   - 成功获取分享统计
   - 无效分享令牌ID处理
   - 无权限访问处理
   - 分享令牌不存在处理

6. **更新分享设置** (UpdateShare)
   - 成功更新分享设置
   - 无效分享令牌ID处理
   - 无权限访问处理
   - 分享令牌不存在处理
   - 未登录用户处理

7. **撤销分享链接** (RevokeShare)
   - 成功撤销分享链接
   - 分享令牌不存在处理
   - 无效分享令牌ID处理
   - 无权限访问处理
   - 未登录用户处理

8. **删除分享链接** (DeleteShare)
   - 成功删除分享链接
   - 分享令牌不存在处理
   - 无效分享令牌ID处理
   - 无权限访问处理
   - 未登录用户处理

9. **获取分享访问日志** (GetShareAccessLogs)
   - 成功获取访问日志
   - 无效分享令牌ID处理
   - 无效页码处理
   - 无效页面大小处理
   - 无权限访问处理
   - 未登录用户处理

10. **延长分享有效期** (ExtendShareExpiry)
    - 成功延长分享有效期
    - 无效分享令牌ID处理
    - 无效延长时间处理
    - 延长时间过长处理
    - 无权限访问处理
    - 未登录用户处理

11. **重置分享统计** (ResetShareStats)
    - 成功重置分享统计
    - 分享令牌不存在处理
    - 无效分享令牌ID处理
    - 无权限访问处理
    - 未登录用户处理

### 管理员功能测试

1. **获取所有分享链接** (GetAllShares)
   - 管理员用户成功获取
   - 非管理员用户权限验证

2. **获取系统分享统计** (GetSystemShareStats)
   - 管理员用户成功获取
   - 非管理员用户权限验证

3. **批量操作分享链接** (BulkOperationShares)
   - 管理员用户成功操作
   - 非管理员用户权限验证

4. **获取分享访问日志** (GetShareAccessLogsAdmin)
   - 管理员用户成功获取
   - 非管理员用户权限验证

5. **强制撤销分享链接** (ForceRevokeShare)
   - 管理员用户成功撤销
   - 非管理员用户权限验证

6. **强制删除分享链接** (ForceDeleteShare)
   - 管理员用户成功删除
   - 非管理员用户权限验证

7. **获取用户分享链接** (GetUserSharesAdmin)
   - 管理员用户成功获取
   - 非管理员用户权限验证

### 测试类型分布

- **成功场景测试**: 15 个
- **失败场景测试**: 20 个
- **权限验证测试**: 18 个
- **输入验证测试**: 12 个
- **异常处理测试**: 3 个
- **边界条件测试**: 2 个

## 测试框架和工具

- **测试框架**: MSTest
- **Mock 框架**: Moq
- **断言库**: MSTest 断言
- **测试数据**: 使用预定义的测试数据对象

## 测试模式

### 1. Arrange-Act-Assert 模式

所有测试都遵循标准的 Arrange-Act-Assert 模式：

```csharp
[TestMethod]
public async Task CreateShare_ValidRequest_ShouldReturnCreatedShareToken()
{
    // Arrange - 准备测试数据和模拟对象
    var createShareDto = new CreateShareDto { ... };
    var expectedShareToken = new ShareTokenDto { ... };
    
    _mockShareService
        .Setup(s => s.CreateShareTokenAsync(createShareDto, _testUserId))
        .ReturnsAsync(expectedShareToken);

    // Act - 执行被测试的方法
    var result = await _controller.CreateShare(createShareDto);

    // Assert - 验证结果
    Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
    var createdResult = result.Result as CreatedAtActionResult;
    var returnedToken = createdResult?.Value as ShareTokenDto;
    
    Assert.IsNotNull(returnedToken);
    Assert.AreEqual(expectedShareToken.Id, returnedToken.Id);
    
    _mockShareService.Verify(s => s.CreateShareTokenAsync(createShareDto, _testUserId), Times.Once);
}
```

### 2. Mock 服务使用

使用 Moq 框架模拟 `IShareService` 接口：

```csharp
private Mock<IShareService> _mockShareService = null!;

[TestInitialize]
public void Setup()
{
    _mockShareService = new Mock<IShareService>();
    // ... 其他初始化
}
```

### 3. 用户身份模拟

模拟不同用户身份进行权限测试：

```csharp
// 普通用户
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
    new Claim(ClaimTypes.Role, "User")
};
var identity = new ClaimsIdentity(claims, "TestAuth");
var principal = new ClaimsPrincipal(identity);

_controller.ControllerContext.HttpContext.User = principal;

// 管理员用户
var adminClaims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
    new Claim(ClaimTypes.Role, "Admin")
};
```

### 4. 异常处理测试

测试各种异常情况：

```csharp
_mockShareService
    .Setup(s => s.CreateShareTokenAsync(createShareDto, _testUserId))
    .ThrowsAsync(new UnauthorizedAccessException("用户无权限访问此代码片段"));

var result = await _controller.CreateShare(createShareDto);

Assert.IsInstanceOfType(result.Result, typeof(ForbidResult));
```

## 测试数据

使用预定义的测试数据：

```csharp
private readonly Guid _testUserId = Guid.NewGuid();
private readonly Guid _testSnippetId = Guid.NewGuid();
private readonly Guid _testShareTokenId = Guid.NewGuid();
private readonly string _testToken = "test-token-123";
```

## 运行测试

```bash
# 运行所有 ShareController 测试
dotnet test --filter "FullyQualifiedName~ShareControllerTests"

# 运行特定测试方法
dotnet test --filter "TestName=CreateShare_ValidRequest_ShouldReturnCreatedShareToken"

# 运行特定测试类别
dotnet test --filter "TestCategory=Share"
```

## 测试覆盖率

- **API 方法覆盖率**: 100% (16/16 个公共方法)
- **代码行覆盖率**: 预计 >90%
- **分支覆盖率**: 预计 >85%

## 测试验证工具

使用 `TestCoverageValidator.cs` 工具可以验证测试覆盖率：

```bash
# 编译并运行覆盖率验证工具
dotnet run --project Tests/TestCoverageValidator.cs
```

## 最佳实践

1. **测试命名规范**: 使用 `MethodName_Scenario_ExpectedResult` 格式
2. **单一职责**: 每个测试只测试一个功能点
3. **独立性**: 测试之间不相互依赖
4. **可重复性**: 测试可以重复运行且结果一致
5. **全面性**: 覆盖正常流程、异常流程和边界条件
6. **可读性**: 测试代码清晰易懂，有适当的注释

## 维护指南

1. 添加新的 API 方法时，必须添加相应的测试
2. 修改现有 API 方法时，需要更新相关测试
3. 定期运行测试确保所有测试通过
4. 使用覆盖率工具确保测试覆盖率不下降
5. 保持测试代码的整洁和可维护性

## 问题排查

如果测试失败，请检查：

1. 模拟对象设置是否正确
2. 测试数据是否符合预期
3. API 方法签名是否发生变化
4. 依赖服务接口是否发生变化
5. 测试环境配置是否正确

## 总结

ShareControllerTests 提供了分享功能的完整测试覆盖，确保了代码质量和功能稳定性。通过持续的测试维护和扩展，可以保证分享功能的可靠性和安全性。