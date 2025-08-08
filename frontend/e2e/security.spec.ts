import { test, expect } from '@playwright/test'

test.describe('Security Features E2E Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('/')
  })

  test.describe('Authentication Security', () => {
    test('prevents XSS attacks in login form', async ({ page }) => {
      await page.click('text=登录')

      // Try to inject XSS in username field
      await page.fill('input[placeholder="用户名"]', '<script>alert("xss")</script>')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')

      // Should show validation error, not execute script
      await expect(page.locator('text=输入包含不安全的内容')).toBeVisible()

      // Verify no alert was triggered
      page.on('dialog', () => {
        throw new Error('XSS alert should not be triggered')
      })
    })

    test('validates password strength', async ({ page }) => {
      await page.click('text=注册')

      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="邮箱"]', 'test@example.com')
      await page.fill('input[placeholder="密码"]', 'weak')
      await page.fill('input[placeholder="确认密码"]', 'weak')

      await page.click('button[type="submit"]')

      await expect(page.locator('text=密码必须至少8个字符')).toBeVisible()
    })

    test('sanitizes user input', async ({ page }) => {
      await page.click('text=登录')

      // Input with extra whitespace
      await page.fill('input[placeholder="用户名"]', '  testuser  ')
      await page.blur('input[placeholder="用户名"]')

      // Should be trimmed
      const usernameValue = await page.inputValue('input[placeholder="用户名"]')
      expect(usernameValue).toBe('testuser')
    })

    test('implements rate limiting', async ({ page }) => {
      await page.click('text=登录')

      // Fill form with invalid credentials
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'wrongpassword')

      // Submit multiple times rapidly
      for (let i = 0; i < 15; i++) {
        await page.click('button[type="submit"]')
        await page.waitForTimeout(100)
      }

      // Should show rate limit error
      await expect(page.locator('text=请求过于频繁')).toBeVisible()
    })
  })

  test.describe('Code Snippet Security', () => {
    test.beforeEach(async ({ page }) => {
      // Login first
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')
    })

    test('prevents XSS in snippet creation', async ({ page }) => {
      await page.click('text=创建代码片段')

      // Try to inject XSS in title
      await page.fill('input[placeholder="代码片段标题"]', '<img src="x" onerror="alert(\'xss\')">')
      await page.fill('textarea[placeholder="描述（可选）"]', 'Test description')
      await page.selectOption('select', 'javascript')

      // Fill code editor
      const codeEditor = page.locator('.code-editor')
      await codeEditor.fill('console.log("Hello World");')

      await page.click('button[type="submit"]')

      // Should show validation error
      await expect(page.locator('text=输入包含不安全的内容')).toBeVisible()
    })

    test('validates input lengths', async ({ page }) => {
      await page.click('text=创建代码片段')

      // Try to input overly long title
      const longTitle = 'a'.repeat(201)
      await page.fill('input[placeholder="代码片段标题"]', longTitle)
      await page.blur('input[placeholder="代码片段标题"]')

      await expect(page.locator('text=标题长度不能超过200个字符')).toBeVisible()
    })

    test('validates programming language', async ({ page }) => {
      await page.click('text=创建代码片段')

      await page.fill('input[placeholder="代码片段标题"]', 'Test Snippet')
      await page.selectOption('select', '') // Empty selection

      const codeEditor = page.locator('.code-editor')
      await codeEditor.fill('console.log("Hello World");')

      await page.click('button[type="submit"]')

      await expect(page.locator('text=请选择编程语言')).toBeVisible()
    })

    test('validates tag names', async ({ page }) => {
      await page.click('text=创建代码片段')

      await page.fill('input[placeholder="代码片段标题"]', 'Test Snippet')
      await page.selectOption('select', 'javascript')

      const codeEditor = page.locator('.code-editor')
      await codeEditor.fill('console.log("Hello World");')

      // Try to add invalid tag
      const tagInput = page.locator('input[placeholder="添加标签"]')
      await tagInput.fill('invalid@tag')
      await tagInput.press('Enter')

      await expect(page.locator('text=标签名称只能包含字母、数字、中文、连字符和下划线')).toBeVisible()
    })
  })

  test.describe('CSRF Protection', () => {
    test('includes CSRF token in requests', async ({ page }) => {
      // Monitor network requests
      const requests: any[] = []
      page.on('request', request => {
        if (request.method() === 'POST') {
          requests.push(request)
        }
      })

      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')

      // Check that POST requests include CSRF token
      const loginRequest = requests.find(req => req.url().includes('/auth/login'))
      if (loginRequest) {
        const headers = loginRequest.headers()
        expect(headers['x-csrf-token']).toBeDefined()
      }
    })
  })

  test.describe('Content Security Policy', () => {
    test('blocks inline scripts', async ({ page }) => {
      // Monitor console errors
      const consoleErrors: string[] = []
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleErrors.push(msg.text())
        }
      })

      // Try to execute inline script (should be blocked by CSP)
      await page.evaluate(() => {
        const script = document.createElement('script')
        script.innerHTML = 'console.log("This should be blocked")'
        document.head.appendChild(script)
      })

      // Should have CSP violation error
      expect(consoleErrors.some(error =>
        error.includes('Content Security Policy') ||
        error.includes('script-src')
      )).toBe(true)
    })
  })

  test.describe('Input Sanitization', () => {
    test('removes dangerous HTML from user input', async ({ page }) => {
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')

      await page.click('text=创建代码片段')

      // Input with HTML tags
      await page.fill('input[placeholder="代码片段标题"]', '<b>Bold Title</b>')
      await page.blur('input[placeholder="代码片段标题"]')

      // HTML tags should be removed
      const titleValue = await page.inputValue('input[placeholder="代码片段标题"]')
      expect(titleValue).toBe('Bold Title')
    })

    test('preserves code content integrity', async ({ page }) => {
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')

      await page.click('text=创建代码片段')

      await page.fill('input[placeholder="代码片段标题"]', 'HTML Example')
      await page.selectOption('select', 'html')

      // Code with HTML should be preserved
      const htmlCode = '<div class="container">\n  <h1>Hello World</h1>\n</div>'
      const codeEditor = page.locator('.code-editor')
      await codeEditor.fill(htmlCode)

      // Code content should remain unchanged
      const codeValue = await codeEditor.inputValue()
      expect(codeValue).toBe(htmlCode)
    })
  })

  test.describe('Error Handling', () => {
    test('handles network errors gracefully', async ({ page }) => {
      // Intercept and fail network requests
      await page.route('**/api/**', route => {
        route.abort('failed')
      })

      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')

      // Should show user-friendly error message
      await expect(page.locator('text=网络连接失败，请检查网络设置')).toBeVisible()
    })

    test('handles server errors gracefully', async ({ page }) => {
      // Intercept and return server error
      await page.route('**/api/auth/login', route => {
        route.fulfill({
          status: 500,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'Internal Server Error' })
        })
      })

      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')

      // Should show user-friendly error message
      await expect(page.locator('text=服务器暂时不可用，请稍后重试')).toBeVisible()
    })
  })

  test.describe('Session Management', () => {
    test('handles session expiration', async ({ page }) => {
      // Login first
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
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

      // Try to create a snippet
      await page.click('text=创建代码片段')

      // Should redirect to login
      await expect(page).toHaveURL('/login')
      await expect(page.locator('text=会话已过期，请重新登录')).toBeVisible()
    })

    test('clears sensitive data on logout', async ({ page }) => {
      // Login first
      await page.click('text=登录')
      await page.fill('input[placeholder="用户名"]', 'testuser')
      await page.fill('input[placeholder="密码"]', 'password123')
      await page.click('button[type="submit"]')
      await page.waitForURL('/snippets')

      // Logout
      await page.click('text=退出登录')

      // Check that localStorage is cleared
      const token = await page.evaluate(() => localStorage.getItem('auth_token'))
      expect(token).toBeNull()

      // Should redirect to home page
      await expect(page).toHaveURL('/')
    })
  })
})
