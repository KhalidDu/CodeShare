import { test, expect } from '@playwright/test'

test.describe('Code Sharing End-to-End Tests', () => {
  test.describe('Share Link Generation Flow', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('complete share link generation workflow', async ({ page }) => {
      // Navigate to an existing snippet or create one
      await page.click('.snippet-card').first()
      
      // Wait for snippet detail page to load
      await expect(page.locator('h1')).toBeVisible()
      
      // Click share button
      await page.click('button:has-text("分享")')
      
      // Share dialog should appear
      await expect(page.locator('.share-dialog')).toBeVisible()
      await expect(page.locator('text=创建分享链接')).toBeVisible()
      
      // Configure share settings
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.selectOption('select[aria-label="过期时间"]', '7d')
      
      // Enable password protection
      await page.check('input[type="checkbox"][aria-label="密码保护"]')
      await page.fill('input[placeholder="设置访问密码"]', 'share123')
      
      // Create share link
      await page.click('button:has-text("创建分享链接")')
      
      // Wait for share result
      await expect(page.locator('.share-result')).toBeVisible()
      await expect(page.locator('text=分享链接已创建成功！')).toBeVisible()
      
      // Verify share link is displayed
      const shareLinkInput = page.locator('.share-link-input input')
      await expect(shareLinkInput).toBeVisible()
      const shareLink = await shareLinkInput.inputValue()
      expect(shareLink).toContain('/share/')
      
      // Verify QR code is generated
      await expect(page.locator('.qr-code')).toBeVisible()
      
      // Copy share link
      await page.click('.share-link-input button:has-text("复制链接")')
      await expect(page.locator('text=链接已复制到剪贴板')).toBeVisible()
      
      // Test social sharing
      await page.click('button:has-text("分享到Twitter")')
      await expect(page).toHaveURL(/twitter\.com\/intent\/tweet/)
      
      // Go back to test other sharing options
      await page.goBack()
      await expect(page.locator('.share-result')).toBeVisible()
      
      // Test Facebook sharing
      await page.click('button:has-text("分享到Facebook")')
      await expect(page).toHaveURL(/facebook\.com\/sharer/)
      
      // Go back to test email sharing
      await page.goBack()
      await page.click('button:has-text("分享到邮件")')
      await expect(page).toHaveURL(/mailto:.+subject=/)
    })

    test('share link with custom expiration', async ({ page }) => {
      // Navigate to an existing snippet
      await page.click('.snippet-card').first()
      
      // Click share button
      await page.click('button:has-text("分享")')
      
      // Select custom expiration
      await page.selectOption('select[aria-label="过期时间"]', 'custom')
      
      // Set custom expiration date (tomorrow)
      const tomorrow = new Date()
      tomorrow.setDate(tomorrow.getDate() + 1)
      const customDate = tomorrow.toISOString().slice(0, 16)
      await page.fill('input[type="datetime-local"]', customDate)
      
      // Create share link
      await page.click('button:has-text("创建分享链接")')
      
      // Verify share result
      await expect(page.locator('.share-result')).toBeVisible()
      await expect(page.locator('text=分享链接已创建成功！')).toBeVisible()
    })

    test('share link with access limits', async ({ page }) => {
      // Navigate to an existing snippet
      await page.click('.snippet-card').first()
      
      // Click share button
      await page.click('button:has-text("分享")')
      
      // Enable access limit
      await page.check('input[type="checkbox"][aria-label="访问限制"]')
      await page.fill('input[placeholder="最大访问次数"]', '10')
      
      // Create share link
      await page.click('button:has-text("创建分享链接")')
      
      // Verify share result
      await expect(page.locator('.share-result')).toBeVisible()
      await expect(page.locator('text=分享链接已创建成功！')).toBeVisible()
    })

    test('share link validation errors', async ({ page }) => {
      // Navigate to an existing snippet
      await page.click('.snippet-card').first()
      
      // Click share button
      await page.click('button:has-text("分享")')
      
      // Try to create share link without filling required fields
      await page.click('button:has-text("创建分享链接")')
      
      // Should show validation error
      await expect(page.locator('text=请设置分享权限')).toBeVisible()
      
      // Set permission but leave password field empty when password protection is enabled
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.check('input[type="checkbox"][aria-label="密码保护"]')
      await page.click('button:has-text("创建分享链接")')
      
      // Should show password validation error
      await expect(page.locator('text=请设置访问密码')).toBeVisible()
    })
  })

  test.describe('Share Link Access Flow', () => {
    test('access shared snippet without authentication', async ({ page }) => {
      // First, create a share link as authenticated user
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      // Configure basic share settings
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.selectOption('select[aria-label="过期时间"]', '7d')
      await page.click('button:has-text("创建分享链接")')
      
      // Get the share link
      const shareLinkInput = page.locator('.share-link-input input')
      await expect(shareLinkInput).toBeVisible()
      const shareLink = await shareLinkInput.inputValue()
      
      // Logout
      await page.click('text=退出登录')
      
      // Access share link without authentication
      await page.goto(shareLink)
      
      // Should display shared snippet
      await expect(page.locator('.shared-snippet')).toBeVisible()
      await expect(page.locator('h1')).toBeVisible()
      await expect(page.locator('.code-content')).toBeVisible()
      
      // Should show view count
      await expect(page.locator('.view-count')).toBeVisible()
      
      // Copy code should work
      await page.click('button:has-text("复制代码")')
      await expect(page.locator('text=代码已复制到剪贴板')).toBeVisible()
      
      // Verify copy count increased
      await expect(page.locator('.copy-count')).toBeVisible()
    })

    test('access shared snippet with password protection', async ({ page }) => {
      // First, create a password-protected share link
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      // Configure password-protected share
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.check('input[type="checkbox"][aria-label="密码保护"]')
      await page.fill('input[placeholder="设置访问密码"]', 'testpass123')
      await page.click('button:has-text("创建分享链接")')
      
      // Get the share link
      const shareLinkInput = page.locator('.share-link-input input')
      const shareLink = await shareLinkInput.inputValue()
      
      // Logout
      await page.click('text=退出登录')
      
      // Access share link
      await page.goto(shareLink)
      
      // Should show password input form
      await expect(page.locator('.password-form')).toBeVisible()
      await expect(page.locator('text=请输入访问密码')).toBeVisible()
      
      // Try wrong password
      await page.fill('input[placeholder="输入访问密码"]', 'wrongpass')
      await page.click('button:has-text("访问")')
      
      // Should show error
      await expect(page.locator('text=密码错误')).toBeVisible()
      
      // Enter correct password
      await page.fill('input[placeholder="输入访问密码"]', 'testpass123')
      await page.click('button:has-text("访问")')
      
      // Should display shared snippet
      await expect(page.locator('.shared-snippet')).toBeVisible()
    })

    test('access read-only shared snippet', async ({ page }) => {
      // First, create a read-only share link
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      // Configure read-only share
      await page.selectOption('select[aria-label="分享权限"]', 'readonly')
      await page.click('button:has-text("创建分享链接")')
      
      // Get the share link
      const shareLinkInput = page.locator('.share-link-input input')
      const shareLink = await shareLinkInput.inputValue()
      
      // Logout
      await page.click('text=退出登录')
      
      // Access share link
      await page.goto(shareLink)
      
      // Should display shared snippet
      await expect(page.locator('.shared-snippet')).toBeVisible()
      
      // Copy button should be disabled or hidden
      const copyButton = page.locator('button:has-text("复制代码")')
      if (await copyButton.isVisible()) {
        await expect(copyButton).toBeDisabled()
      }
    })

    test('handle invalid share links', async ({ page }) => {
      // Try to access invalid share link
      await page.goto('/share/invalid-token')
      
      // Should show error page
      await expect(page.locator('.error-page')).toBeVisible()
      await expect(page.locator('text=链接已失效')).toBeVisible()
      await expect(page.locator('text=返回首页')).toBeVisible()
      
      // Test return to home
      await page.click('text=返回首页')
      await expect(page).toHaveURL('/')
    })

    test('handle expired share links', async ({ page }) => {
      // Create a share link that expires immediately
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      // Set expiration to past
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.selectOption('select[aria-label="过期时间"]', 'custom')
      
      const pastDate = new Date()
      pastDate.setHours(pastDate.getHours() - 1)
      const customDate = pastDate.toISOString().slice(0, 16)
      await page.fill('input[type="datetime-local"]', customDate)
      
      await page.click('button:has-text("创建分享链接")')
      
      // Get the share link
      const shareLinkInput = page.locator('.share-link-input input')
      const shareLink = await shareLinkInput.inputValue()
      
      // Logout
      await page.click('text=退出登录')
      
      // Try to access expired link
      await page.goto(shareLink)
      
      // Should show expired error
      await expect(page.locator('.error-page')).toBeVisible()
      await expect(page.locator('text=链接已过期')).toBeVisible()
    })
  })

  test.describe('Share Management Flow', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('view and manage share links', async ({ page }) => {
      // Navigate to share management
      await page.click('text=分享管理')
      
      // Should show share management page
      await expect(page).toHaveURL('/shares')
      await expect(page.locator('text=我的分享链接')).toBeVisible()
      
      // Should show list of shared links
      await expect(page.locator('.share-link-item')).toHaveCountGreaterThan(0)
      
      // View share details
      await page.click('.share-link-item:has-text("查看详情")').first()
      
      // Should show share statistics
      await expect(page.locator('.share-stats')).toBeVisible()
      await expect(page.locator('text=访问统计')).toBeVisible()
      await expect(page.locator('text=访问次数')).toBeVisible()
      await expect(page.locator('text=复制次数')).toBeVisible()
      
      // Test share revocation
      await page.click('button:has-text("撤销分享")')
      
      // Should show confirmation dialog
      await expect(page.locator('.confirm-dialog')).toBeVisible()
      await expect(page.locator('text=确定要撤销这个分享链接吗？')).toBeVisible()
      
      // Confirm revocation
      await page.click('button:has-text("确认撤销")')
      
      // Should show success message
      await expect(page.locator('text=分享链接已撤销')).toBeVisible()
      
      // Share link should be marked as inactive
      await expect(page.locator('.share-link-item').first()).toContainText('已撤销')
    })

    test('filter and search share links', async ({ page }) => {
      // Navigate to share management
      await page.click('text=分享管理')
      
      // Search for specific share
      await page.fill('input[placeholder="搜索分享链接..."]', 'React')
      await page.press('input[placeholder="搜索分享链接..."]', 'Enter')
      
      // Should show filtered results
      await expect(page.locator('.share-link-item')).toHaveCount(1)
      
      // Clear search
      await page.click('button:has-text("清除")')
      await expect(page.locator('.share-link-item')).toHaveCountGreaterThan(1)
      
      // Filter by status
      await page.selectOption('select[aria-label="状态筛选"]', 'active')
      await expect(page.locator('.share-link-item')).toHaveCountGreaterThan(0)
      
      // Filter by permission
      await page.selectOption('select[aria-label="权限筛选"]', 'allow_copy')
      await expect(page.locator('.share-link-item')).toHaveCountGreaterThan(0)
    })

    test('export share statistics', async ({ page }) => {
      // Navigate to share management
      await page.click('text=分享管理')
      
      // View share details
      await page.click('.share-link-item:has-text("查看详情")').first()
      
      // Export statistics
      await page.click('button:has-text("导出统计")')
      
      // Should show export options
      await expect(page.locator('.export-dialog')).toBeVisible()
      
      // Select CSV format
      await page.click('input[type="radio"][value="csv"]')
      await page.click('button:has-text("导出")')
      
      // Should trigger download
      const downloadPromise = page.waitForEvent('download')
      await downloadPromise
      
      // Should show success message
      await expect(page.locator('text=统计已导出')).toBeVisible()
    })
  })

  test.describe('Share Analytics Flow', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('view share analytics dashboard', async ({ page }) => {
      // Navigate to analytics
      await page.click('text=分享分析')
      
      // Should show analytics dashboard
      await expect(page).toHaveURL('/analytics/shares')
      await expect(page.locator('text=分享分析')).toBeVisible()
      
      // Should show overview statistics
      await expect(page.locator('.overview-stats')).toBeVisible()
      await expect(page.locator('text=总分享数')).toBeVisible()
      await expect(page.locator('text=总访问数')).toBeVisible()
      await expect(page.locator('text=总复制数')).toBeVisible()
      
      // Should show access trends chart
      await expect(page.locator('.access-chart')).toBeVisible()
      
      // Should show popular shared snippets
      await expect(page.locator('.popular-snippets')).toBeVisible()
      
      // Test date range filter
      await page.click('button:has-text("最近7天")')
      await page.click('text=最近30天')
      
      // Chart should update with new date range
      await expect(page.locator('.access-chart')).toBeVisible()
    })

    test('view detailed share statistics', async ({ page }) => {
      // Navigate to share management
      await page.click('text=分享管理')
      
      // View share details
      await page.click('.share-link-item:has-text("查看详情")').first()
      
      // Should show detailed statistics
      await expect(page.locator('.detailed-stats')).toBeVisible()
      await expect(page.locator('text=访问趋势')).toBeVisible()
      await expect(page.locator('text=地理分布')).toBeVisible()
      await expect(page.locator('text=设备统计')).toBeVisible()
      
      // Test interactive chart features
      await page.click('.chart-point')
      await expect(page.locator('.tooltip')).toBeVisible()
    })
  })

  test.describe('Error Handling and Edge Cases', () => {
    test('handle network interruption during share creation', async ({ page }) => {
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      // Simulate network failure
      await page.route('**/api/**', route => route.abort('failed'))
      
      // Try to create share link
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.click('button:has-text("创建分享链接")')
      
      // Should show error message
      await expect(page.locator('text=网络连接失败')).toBeVisible()
      
      // Restore network
      await page.unroute('**/api/**')
      
      // Retry should work
      await page.click('button:has-text("重试")')
      await expect(page.locator('.share-result')).toBeVisible()
    })

    test('handle concurrent share link access', async ({ page }) => {
      // Create multiple contexts to simulate concurrent access
      const context1 = await page.context()
      const page1 = await context1.newPage()
      const page2 = await context1.newPage()
      
      // Setup share link
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.click('button:has-text("创建分享链接")')
      
      const shareLinkInput = page.locator('.share-link-input input')
      const shareLink = await shareLinkInput.inputValue()
      
      // Access share link concurrently from multiple pages
      await Promise.all([
        page1.goto(shareLink),
        page2.goto(shareLink)
      ])
      
      // Both pages should show the shared snippet
      await expect(page1.locator('.shared-snippet')).toBeVisible()
      await expect(page2.locator('.shared-snippet')).toBeVisible()
      
      // Close additional pages
      await page1.close()
      await page2.close()
    })

    test('handle rate limiting on share access', async ({ page }) => {
      // Create share link
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      await page.click('.snippet-card').first()
      await page.click('button:has-text("分享")')
      
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.click('button:has-text("创建分享链接")')
      
      const shareLinkInput = page.locator('.share-link-input input')
      const shareLink = await shareLinkInput.inputValue()
      
      // Logout
      await page.click('text=退出登录')
      
      // Access share link rapidly to trigger rate limiting
      for (let i = 0; i < 10; i++) {
        await page.goto(shareLink)
      }
      
      // Should show rate limiting error
      await expect(page.locator('text=访问过于频繁')).toBeVisible()
    })
  })

  test.describe('Mobile Responsiveness', () => {
    test.beforeEach(async ({ page }) => {
      // Set mobile viewport
      await page.setViewportSize({ width: 375, height: 667 })
    })

    test('share functionality works on mobile', async ({ page }) => {
      // Login
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      // Navigate to snippet
      await page.click('.snippet-card').first()
      
      // Share button should be visible on mobile
      await expect(page.locator('button:has-text("分享")')).toBeVisible()
      
      // Click share button
      await page.click('button:has-text("分享")')
      
      // Share dialog should be mobile-friendly
      await expect(page.locator('.share-dialog')).toBeVisible()
      await expect(page.locator('.share-dialog')).toHaveCSS('width', '90%')
      
      // Create share link
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.click('button:has-text("创建分享链接")')
      
      // Share result should be mobile-friendly
      await expect(page.locator('.share-result')).toBeVisible()
      await expect(page.locator('.share-link-input')).toBeVisible()
      
      // QR code should be prominently displayed on mobile
      await expect(page.locator('.qr-code')).toBeVisible()
    })

    test('access shared snippet on mobile', async ({ page }) => {
      // Access share link on mobile
      await page.goto('/share/test-token')
      
      // Shared snippet should be mobile-responsive
      await expect(page.locator('.shared-snippet')).toBeVisible()
      await expect(page.locator('.code-content')).toHaveCSS('font-size', '14px')
      
      // Copy button should be easily tappable
      const copyButton = page.locator('button:has-text("复制代码")')
      await expect(copyButton).toBeVisible()
      const boundingBox = await copyButton.boundingBox()
      expect(boundingBox?.height).toBeGreaterThan(40)
    })
  })

  test.describe('Accessibility', () => {
    test('share functionality meets accessibility standards', async ({ page }) => {
      // Login
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      // Navigate to snippet
      await page.click('.snippet-card').first()
      
      // Test keyboard navigation
      await page.keyboard.press('Tab')
      await expect(page.locator('button:has-text("分享")')).toBeFocused()
      
      // Activate share button with keyboard
      await page.keyboard.press('Enter')
      
      // Share dialog should be accessible
      await expect(page.locator('.share-dialog')).toBeVisible()
      
      // Test form navigation with keyboard
      await page.keyboard.press('Tab')
      await expect(page.locator('select[aria-label="分享权限"]')).toBeFocused()
      
      await page.keyboard.press('Tab')
      await expect(page.locator('select[aria-label="过期时间"]')).toBeFocused()
      
      // Complete form with keyboard
      await page.keyboard.press('ArrowDown')
      await page.keyboard.press('Enter')
      
      await page.keyboard.press('Tab')
      await page.keyboard.press('ArrowDown')
      await page.keyboard.press('Enter')
      
      await page.keyboard.press('Tab')
      await page.keyboard.press('Enter')
      
      // Share result should be accessible
      await expect(page.locator('.share-result')).toBeVisible()
      
      // Test screen reader compatibility
      const shareLink = page.locator('.share-link-input input')
      await expect(shareLink).toHaveAttribute('aria-label', '分享链接')
      
      const copyButton = page.locator('.share-link-input button')
      await expect(copyButton).toHaveAttribute('aria-label', '复制分享链接')
    })
  })

  test.describe('Performance Testing', () => {
    test('share link generation performance', async ({ page }) => {
      // Login
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
      
      // Navigate to snippet
      await page.click('.snippet-card').first()
      
      // Measure share link generation time
      const startTime = Date.now()
      
      await page.click('button:has-text("分享")')
      await page.selectOption('select[aria-label="分享权限"]', 'allow_copy')
      await page.click('button:has-text("创建分享链接")')
      
      await expect(page.locator('.share-result')).toBeVisible()
      
      const endTime = Date.now()
      const duration = endTime - startTime
      
      // Share link generation should take less than 3 seconds
      expect(duration).toBeLessThan(3000)
      console.log(`Share link generation took ${duration}ms`)
    })

    test('shared snippet access performance', async ({ page }) => {
      // Access shared snippet
      await page.goto('/share/test-token')
      
      // Measure page load time
      const startTime = Date.now()
      
      await expect(page.locator('.shared-snippet')).toBeVisible()
      
      const endTime = Date.now()
      const duration = endTime - startTime
      
      // Shared snippet access should take less than 2 seconds
      expect(duration).toBeLessThan(2000)
      console.log(`Shared snippet access took ${duration}ms`)
    })
  })
})