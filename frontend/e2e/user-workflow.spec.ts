import { test, expect } from '@playwright/test'

test.describe('Complete User Workflow E2E Tests', () => {
  test.describe('User Registration and Authentication Flow', () => {
    test('complete user registration and login workflow', async ({ page }) => {
      await page.goto('/')

      // Register new user
      await page.click('text=注册')
      await page.fill('input[placeholder="用户名"]', 'e2euser')
      await page.fill('input[placeholder="邮箱"]', 'e2euser@example.com')
      await page.fill('input[placeholder="密码"]', 'Password123!')
      await page.fill('input[placeholder="确认密码"]', 'Password123!')
      await page.click('button[type="submit"]')

      // Should redirect to snippets page after successful registration
      await expect(page).toHaveURL('/snippets')
      await expect(page.locator('text=欢迎, e2euser')).toBeVisible()

      // Logout
      await page.click('text=退出登录')
      await expect(page).toHaveURL('/')

      // Login with registered credentials
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'e2euser')
      await page.fill('input[placeholder="密码"]', 'Password123!')
      await page.click('button[type="submit"]')

      // Should redirect to snippets page
      await expect(page).toHaveURL('/snippets')
      await expect(page.locator('text=欢迎, e2euser')).toBeVisible()
    })
  })

  test.describe('Code Snippet Management Workflow', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('complete snippet creation workflow', async ({ page }) => {
      // Create new snippet
      await page.click('text=创建代码片段')

      // Fill form
      await page.fill('input[placeholder="代码片段标题"]', 'React Hook Example')
      await page.fill('textarea[placeholder="描述（可选）"]', 'A custom React hook for API calls')
      await page.selectOption('select', 'javascript')

      // Fill code editor
      const codeEditor = page.locator('.code-editor')
      await codeEditor.fill(`import { useState, useEffect } from 'react';

export function useApi(url) {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetch(url)
      .then(response => response.json())
      .then(data => {
        setData(data);
        setLoading(false);
      })
      .catch(error => {
        setError(error);
        setLoading(false);
      });
  }, [url]);

  return { data, loading, error };
}`)

      // Add tags
      const tagInput = page.locator('input[placeholder="添加标签"]')
      await tagInput.fill('react')
      await tagInput.press('Enter')
      await tagInput.fill('hooks')
      await tagInput.press('Enter')
      await tagInput.fill('api')
      await tagInput.press('Enter')

      // Submit form
      await page.click('button[type="submit"]')

      // Should redirect to snippet detail page
      await expect(page).toHaveURL(/\/snippets\/[a-f0-9-]+/)
      await expect(page.locator('h1:has-text("React Hook Example")')).toBeVisible()
      await expect(page.locator('text=A custom React hook for API calls')).toBeVisible()
      await expect(page.locator('.tag:has-text("react")')).toBeVisible()
      await expect(page.locator('.tag:has-text("hooks")')).toBeVisible()
      await expect(page.locator('.tag:has-text("api")')).toBeVisible()
    })

    test('snippet search and filtering workflow', async ({ page }) => {
      // Search for snippets
      await page.fill('input[placeholder="搜索代码片段..."]', 'React')
      await page.press('input[placeholder="搜索代码片段..."]', 'Enter')

      // Should show filtered results
      await expect(page.locator('.snippet-card')).toHaveCount(1)
      await expect(page.locator('text=React Hook Example')).toBeVisible()

      // Filter by language
      await page.selectOption('select[aria-label="编程语言"]', 'javascript')
      await expect(page.locator('.snippet-card')).toHaveCount(1)

      // Filter by tag
      await page.click('.tag-filter:has-text("react")')
      await expect(page.locator('.snippet-card')).toHaveCount(1)

      // Clear filters
      await page.click('button:has-text("清除筛选")')
      expect(page.locator('.snippet-card').count()).toBeGreaterThan(1)
    })

    test('snippet editing workflow', async ({ page }) => {
      // Find and click on a snippet
      await page.click('.snippet-card:has-text("React Hook Example")')

      // Edit snippet
      await page.click('button:has-text("编辑")')

      // Update title
      await page.fill('input[placeholder="代码片段标题"]', 'Updated React Hook Example')

      // Update description
      await page.fill('textarea[placeholder="描述（可选）"]', 'An updated custom React hook for API calls with error handling')

      // Add new tag
      const tagInput = page.locator('input[placeholder="添加标签"]')
      await tagInput.fill('typescript')
      await tagInput.press('Enter')

      // Submit changes
      await page.click('button[type="submit"]')

      // Should show updated content
      await expect(page.locator('h1:has-text("Updated React Hook Example")')).toBeVisible()
      await expect(page.locator('text=An updated custom React hook for API calls with error handling')).toBeVisible()
      await expect(page.locator('.tag:has-text("typescript")')).toBeVisible()
    })

    test('snippet copying workflow', async ({ page }) => {
      // Find and click on a snippet
      await page.click('.snippet-card:has-text("Updated React Hook Example")')

      // Copy snippet code
      await page.click('button:has-text("复制代码")')

      // Should show success message
      await expect(page.locator('text=代码已复制到剪贴板')).toBeVisible()

      // Verify copy count increased
      const copyCount = await page.locator('.copy-count').textContent()
      expect(parseInt(copyCount || '0')).toBeGreaterThan(0)
    })

    test('snippet deletion workflow', async ({ page }) => {
      // Find and click on a snippet
      await page.click('.snippet-card:has-text("Updated React Hook Example")')

      // Delete snippet
      await page.click('button:has-text("删除")')

      // Confirm deletion
      await page.click('button:has-text("确认删除")')

      // Should redirect to snippets list
      await expect(page).toHaveURL('/snippets')

      // Snippet should no longer exist
      await expect(page.locator('text=Updated React Hook Example')).toBeHidden()
    })
  })

  test.describe('Tag Management Workflow', () => {
    test.beforeEach(async ({ page }) => {
      // Login as admin user
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'admin')
      await page.fill('input[placeholder="密码"]', 'admin123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('tag creation and management workflow', async ({ page }) => {
      // Navigate to tag management
      await page.click('text=标签管理')

      // Create new tag
      await page.click('button:has-text("创建标签")')
      await page.fill('input[placeholder="标签名称"]', 'frontend')
      await page.fill('input[type="color"]', '#ff6b6b')
      await page.click('button[type="submit"]')

      // Should show new tag in list
      await expect(page.locator('.tag-item:has-text("frontend")')).toBeVisible()

      // Edit tag
      await page.click('.tag-item:has-text("frontend") button:has-text("编辑")')
      await page.fill('input[placeholder="标签名称"]', 'frontend-dev')
      await page.click('button[type="submit"]')

      // Should show updated tag
      await expect(page.locator('.tag-item:has-text("frontend-dev")')).toBeVisible()

      // Delete tag
      await page.click('.tag-item:has-text("frontend-dev") button:has-text("删除")')
      await page.click('button:has-text("确认删除")')

      // Tag should be removed
      await expect(page.locator('.tag-item:has-text("frontend-dev")')).toBeHidden()
    })
  })

  test.describe('User Profile and Settings Workflow', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('user profile update workflow', async ({ page }) => {
      // Navigate to profile
      await page.click('text=个人资料')

      // Update email
      await page.fill('input[placeholder="邮箱"]', 'newemail@example.com')

      // Save changes
      await page.click('button:has-text("保存更改")')

      // Should show success message
      await expect(page.locator('text=个人资料已更新')).toBeVisible()
    })

    test('password change workflow', async ({ page }) => {
      // Navigate to profile
      await page.click('text=个人资料')

      // Change password
      await page.click('text=修改密码')
      await page.fill('input[placeholder="当前密码"]', 'password123')
      await page.fill('input[placeholder="新密码"]', 'NewPassword123!')
      await page.fill('input[placeholder="确认新密码"]', 'NewPassword123!')
      await page.click('button:has-text("修改密码")')

      // Should show success message
      await expect(page.locator('text=密码修改成功')).toBeVisible()

      // Logout and login with new password
      await page.click('text=退出登录')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'NewPassword123!')
      await page.click('button[type="submit"]')

      // Should successfully login
      await expect(page).toHaveURL('/snippets')
    })
  })

  test.describe('Clipboard History Workflow', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'NewPassword123!')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('clipboard history tracking workflow', async ({ page }) => {
      // Create and copy multiple snippets
      const snippets = [
        { title: 'Snippet 1', code: 'console.log("Hello 1");' },
        { title: 'Snippet 2', code: 'console.log("Hello 2");' },
        { title: 'Snippet 3', code: 'console.log("Hello 3");' }
      ]

      for (const snippet of snippets) {
        // Create snippet
        await page.click('text=创建代码片段')
        await page.fill('input[placeholder="代码片段标题"]', snippet.title)
        await page.selectOption('select', 'javascript')
        await page.locator('.code-editor').fill(snippet.code)
        await page.click('button[type="submit"]')

        // Copy the snippet
        await page.click('button:has-text("复制代码")')
        await page.waitForSelector('text=代码已复制到剪贴板')

        // Go back to snippets list
        await page.click('text=代码片段')
      }

      // View clipboard history
      await page.click('text=剪贴板历史')

      // Should show all copied snippets
      await expect(page.locator('.clipboard-item')).toHaveCount(3)
      await expect(page.locator('text=Snippet 1')).toBeVisible()
      await expect(page.locator('text=Snippet 2')).toBeVisible()
      await expect(page.locator('text=Snippet 3')).toBeVisible()

      // Clear clipboard history
      await page.click('button:has-text("清空历史")')
      await page.click('button:has-text("确认清空")')

      // History should be empty
      await expect(page.locator('.clipboard-item')).toHaveCount(0)
      await expect(page.locator('text=暂无剪贴板历史')).toBeVisible()
    })
  })

  test.describe('Error Handling and Recovery Workflow', () => {
    test('handles network interruption gracefully', async ({ page }) => {
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'NewPassword123!')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')

      // Start creating a snippet
      await page.click('text=创建代码片段')
      await page.fill('input[placeholder="代码片段标题"]', 'Network Test Snippet')
      await page.selectOption('select', 'javascript')
      await page.locator('.code-editor').fill('console.log("test");')

      // Simulate network failure
      await page.route('**/api/**', route => route.abort('failed'))

      // Try to submit
      await page.click('button[type="submit"]')

      // Should show error message
      await expect(page.locator('text=网络连接失败')).toBeVisible()

      // Restore network
      await page.unroute('**/api/**')

      // Retry submission
      await page.click('button:has-text("重试")')

      // Should succeed
      await expect(page).toHaveURL(/\/snippets\/[a-f0-9-]+/)
    })

    test('recovers from session expiration', async ({ page }) => {
      await page.goto('/')
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'NewPassword123!')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')

      // Simulate session expiration
      await page.route('**/api/**', route => {
        route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Token expired' })
        })
      })

      // Try to access protected resource
      await page.click('text=创建代码片段')

      // Should redirect to login with message
      await expect(page).toHaveURL('/login')
      await expect(page.locator('text=会话已过期，请重新登录')).toBeVisible()

      // Restore API and login again
      await page.unroute('**/api/**')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'NewPassword123!')
      await page.click('button[type="submit"]')

      // Should be able to access protected resources again
      await expect(page).toHaveURL('/snippets')
    })
  })
})
