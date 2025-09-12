import { test, expect } from '@playwright/test';

test.describe('系统设置功能端到端测试', () => {
  test.beforeEach(async ({ page }) => {
    // 模拟管理员登录
    await page.goto('/login');
    await page.fill('[data-test="username"]', 'admin');
    await page.fill('[data-test="password"]', 'admin123');
    await page.click('[data-test="login-btn"]');
    await page.waitForURL('/dashboard');
  });

  test('应该能够访问系统设置页面', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 验证页面标题
    await expect(page.locator('h1')).toContainText('系统设置');
    
    // 验证选项卡存在
    await expect(page.locator('[data-test="site-settings-tab"]')).toBeVisible();
    await expect(page.locator('[data-test="security-settings-tab"]')).toBeVisible();
    await expect(page.locator('[data-test="feature-settings-tab"]')).toBeVisible();
    await expect(page.locator('[data-test="email-settings-tab"]')).toBeVisible();
    await expect(page.locator('[data-test="settings-history-tab"]')).toBeVisible();
    await expect(page.locator('[data-test="import-export-tab"]')).toBeVisible();
  });

  test('应该能够配置站点设置', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击站点设置选项卡
    await page.click('[data-test="site-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="site-name"]');
    
    // 填写站点信息
    await page.fill('[data-test="site-name"]', '测试站点');
    await page.fill('[data-test="site-description"]', '这是一个测试站点');
    await page.fill('[data-test="site-keywords"]', '测试,代码,片段');
    await page.fill('[data-test="footer-text"]', '© 2024 测试站点');
    
    // 选择主题
    await page.selectOption('[data-test="theme"]', 'light');
    
    // 配置功能开关
    await page.check('[data-test="enable-comments"]');
    await page.check('[data-test="enable-ratings"]');
    await page.check('[data-test="enable-sharing"]');
    
    // 提交表单
    await page.click('[data-test="save-settings"]');
    
    // 等待保存成功提示
    await expect(page.locator('[data-test="success-message"]')).toBeVisible();
    await expect(page.locator('[data-test="success-message"]')).toContainText('保存成功');
  });

  test('应该能够配置安全设置', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击安全设置选项卡
    await page.click('[data-test="security-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="min-password-length"]');
    
    // 配置密码策略
    await page.fill('[data-test="min-password-length"]', '8');
    await page.fill('[data-test="max-password-length"]', '32');
    await page.fill('[data-test="max-login-attempts"]', '5');
    await page.fill('[data-test="account-lockout-duration"]', '15');
    
    // 配置会话设置
    await page.fill('[data-test="session-timeout"]', '30');
    await page.check('[data-test="enable-remember-me"]');
    
    // 配置安全功能
    await page.check('[data-test="enable-2fa"]');
    await page.check('[data-test="enable-security-logging"]');
    await page.check('[data-test="enable-password-strength']);
    
    // 提交表单
    await page.click('[data-test="save-security-settings"]');
    
    // 等待保存成功提示
    await expect(page.locator('[data-test="success-message"]')).toBeVisible();
  });

  test('应该能够配置功能设置', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击功能设置选项卡
    await page.click('[data-test="feature-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="enable-code-snippets"]');
    
    // 配置功能开关
    await page.check('[data-test="enable-code-snippets"]');
    await page.check('[data-test="enable-sharing"]');
    await page.check('[data-test="enable-clipboard"]');
    await page.check('[data-test="enable-file-upload"]');
    await page.check('[data-test="enable-search"]');
    await page.check('[data-test="enable-api"]');
    
    // 配置限制
    await page.fill('[data-test="max-snippet-length"]', '50000');
    await page.fill('[data-test="max-upload-size"]', '10');
    await page.fill('[data-test="max-file-count"]', '5');
    
    // 提交表单
    await page.click('[data-test="save-feature-settings"]');
    
    // 等待保存成功提示
    await expect(page.locator('[data-test="success-message"]')).toBeVisible();
  });

  test('应该能够配置邮件设置', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击邮件设置选项卡
    await page.click('[data-test="email-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="smtp-host"]');
    
    // 配置SMTP设置
    await page.fill('[data-test="smtp-host"]', 'smtp.test.com');
    await page.fill('[data-test="smtp-port"]', '587');
    await page.fill('[data-test="smtp-username"]', 'test@test.com');
    await page.fill('[data-test="smtp-password"]', 'password123');
    
    // 配置发件人
    await page.fill('[data-test="from-email"]', 'noreply@test.com');
    await page.fill('[data-test="from-name"]', '测试站点');
    
    // 配置通知设置
    await page.check('[data-test="enable-email-notifications"]');
    await page.check('[data-test="enable-user-registration-email"]');
    await page.check('[data-test="enable-password-reset-email"]');
    
    // 提交表单
    await page.click('[data-test="save-email-settings"]');
    
    // 等待保存成功提示
    await expect(page.locator('[data-test="success-message"]')).toBeVisible();
  });

  test('应该能够测试邮件连接', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击邮件设置选项卡
    await page.click('[data-test="email-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="smtp-host"]');
    
    // 填写SMTP配置
    await page.fill('[data-test="smtp-host"]', 'smtp.test.com');
    await page.fill('[data-test="smtp-port"]', '587');
    await page.fill('[data-test="smtp-username"]', 'test@test.com');
    await page.fill('[data-test="smtp-password"]', 'password123');
    
    // 测试连接
    await page.click('[data-test="test-connection"]');
    
    // 等待测试结果
    await expect(page.locator('[data-test="connection-result"]')).toBeVisible();
  });

  test('应该能够查看设置历史', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击设置历史选项卡
    await page.click('[data-test="settings-history-tab"]');
    
    // 等待历史记录加载
    await page.waitForSelector('[data-test="history-table"]');
    
    // 验证历史记录表格存在
    await expect(page.locator('[data-test="history-table"]')).toBeVisible();
    
    // 验证列标题
    await expect(page.locator('[data-test="col-setting-type"]')).toBeVisible();
    await expect(page.locator('[data-test="col-setting-key"]')).toBeVisible();
    await expect(page.locator('[data-test="col-old-value"]')).toBeVisible();
    await expect(page.locator('[data-test="col-new-value"]')).toBeVisible();
    await expect(page.locator('[data-test="col-changed-by"]')).toBeVisible();
    await expect(page.locator('[data-test="col-changed-at"]')).toBeVisible();
  });

  test('应该能够导出设置', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击导入导出选项卡
    await page.click('[data-test="import-export-tab"]');
    
    // 等待导入导出界面加载
    await page.waitForSelector('[data-test="export-section"]');
    
    // 选择导出格式
    await page.selectOption('[data-test="export-format"]', 'json');
    
    // 选择要导出的设置类型
    await page.check('[data-test="export-site-settings"]');
    await page.check('[data-test="export-security-settings"]');
    await page.check('[data-test="export-feature-settings"]');
    await page.check('[data-test="export-email-settings"]');
    
    // 点击导出按钮
    const downloadPromise = page.waitForEvent('download');
    await page.click('[data-test="export-btn"]');
    const download = await downloadPromise;
    
    // 验证下载的文件
    expect(download.suggestedFilename()).toMatch(/settings-export.*\.json$/);
  });

  test('应该能够导入设置', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击导入导出选项卡
    await page.click('[data-test="import-export-tab"]');
    
    // 等待导入导出界面加载
    await page.waitForSelector('[data-test="import-section"]');
    
    // 准备测试数据文件
    const testData = {
      siteSettings: {
        siteName: '导入测试站点',
        siteDescription: '从文件导入的测试站点',
        theme: 'dark'
      },
      securitySettings: {
        minPasswordLength: 10,
        maxLoginAttempts: 3
      }
    };
    
    // 创建文件上传
    const fileInput = page.locator('[data-test="file-upload"] input');
    await fileInput.setInputFiles({
      name: 'test-settings.json',
      mimeType: 'application/json',
      buffer: Buffer.from(JSON.stringify(testData, null, 2))
    });
    
    // 选择导入模式
    await page.check('[data-test="import-mode-merge"]');
    
    // 点击验证按钮
    await page.click('[data-test="validate-btn"]');
    
    // 等待验证结果
    await expect(page.locator('[data-test="validation-result"]')).toBeVisible();
    
    // 点击导入按钮
    await page.click('[data-test="import-btn"]');
    
    // 等待导入完成
    await expect(page.locator('[data-test="import-success"]')).toBeVisible();
  });

  test('应该能够筛选设置历史', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击设置历史选项卡
    await page.click('[data-test="settings-history-tab"]');
    
    // 等待历史记录加载
    await page.waitForSelector('[data-test="history-table"]');
    
    // 按设置类型筛选
    await page.selectOption('[data-test="filter-setting-type"]', 'Site');
    
    // 等待筛选结果
    await page.waitForTimeout(1000);
    
    // 验证筛选结果
    const rows = page.locator('[data-test="history-row"]');
    const count = await rows.count();
    
    if (count > 0) {
      // 如果有数据，验证筛选结果
      const firstRowType = await rows.first().locator('[data-test="setting-type"]').textContent();
      expect(firstRowType).toContain('Site');
    }
  });

  test('应该能够查看设置统计', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击设置历史选项卡
    await page.click('[data-test="settings-history-tab"]');
    
    // 等待统计信息加载
    await page.waitForSelector('[data-test="statistics-section"]');
    
    // 验证统计信息存在
    await expect(page.locator('[data-test="total-changes"]')).toBeVisible();
    await expect(page.locator('[data-test="today-changes"]')).toBeVisible();
    await expect(page.locator('[data-test="week-changes"]')).toBeVisible();
    await expect(page.locator('[data-test="month-changes"]')).toBeVisible();
    await expect(page.locator('[data-test="most-active-user"]')).toBeVisible();
    await expect(page.locator('[data-test="most-changed-setting"]')).toBeVisible();
  });

  test('非管理员用户应该无法访问系统设置', async ({ page }) => {
    // 登出管理员账号
    await page.click('[data-test="user-menu"]');
    await page.click('[data-test="logout"]');
    
    // 登录普通用户账号
    await page.fill('[data-test="username"]', 'user');
    await page.fill('[data-test="password"]', 'user123');
    await page.click('[data-test="login-btn"]');
    await page.waitForURL('/dashboard');
    
    // 尝试访问系统设置页面
    await page.goto('/admin/settings');
    
    // 验证被重定向或显示权限不足
    await expect(page.url()).toContain('/dashboard');
    // 或者验证显示权限不足消息
    const noPermission = page.locator('[data-test="no-permission"]');
    if (await noPermission.isVisible()) {
      await expect(noPermission).toContainText('没有权限');
    }
  });

  test('应该能够处理表单验证错误', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击站点设置选项卡
    await page.click('[data-test="site-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="site-name"]');
    
    // 清空必填字段
    await page.fill('[data-test="site-name"]', '');
    
    // 尝试提交表单
    await page.click('[data-test="save-settings"]');
    
    // 验证错误提示
    await expect(page.locator('[data-test="validation-error"]')).toBeVisible();
    await expect(page.locator('[data-test="validation-error"]')).toContainText('站点名称不能为空');
  });

  test('应该能够重置表单', async ({ page }) => {
    // 导航到系统设置页面
    await page.click('[data-test="settings-menu"]');
    await page.click('[data-test="system-settings"]');
    await page.waitForURL('/admin/settings');
    
    // 点击站点设置选项卡
    await page.click('[data-test="site-settings-tab"]');
    
    // 等待表单加载
    await page.waitForSelector('[data-test="site-name"]');
    
    // 修改表单字段
    await page.fill('[data-test="site-name"]', '修改后的站点名称');
    await page.fill('[data-test="site-description"]', '修改后的描述');
    
    // 点击重置按钮
    await page.click('[data-test="reset-form"]');
    
    // 验证表单被重置
    const siteName = await page.inputValue('[data-test="site-name"]');
    expect(siteName).not.toBe('修改后的站点名称');
  });
});