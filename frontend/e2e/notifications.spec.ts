import { test, expect } from '@playwright/test'

test.describe('通知功能端到端测试', () => {
  test.beforeEach(async ({ page }) => {
    // 用户登录
    await page.goto('/login')
    await page.fill('[data-test="username"]', 'testuser')
    await page.fill('[data-test="password"]', 'password123')
    await page.click('[data-test="login-btn"]')
    await page.waitForURL('/dashboard')
  })

  test.describe('通知列表页面加载测试', () => {
    test('应该能够加载通知列表页面', async ({ page }) => {
      // 导航到通知页面
      await page.goto('/notifications')
      
      // 等待通知列表加载
      await page.waitForSelector('.notification-list')
      
      // 验证通知列表存在
      await expect(page.locator('.notification-list')).toBeVisible()
      
      // 验证页面标题
      await expect(page.locator('h1')).toContainText('通知中心')
    })

    test('应该能够显示通知统计信息', async ({ page }) => {
      await page.goto('/notifications')
      
      // 等待统计信息加载
      await page.waitForSelector('.notification-stats')
      
      // 验证统计信息显示
      await expect(page.locator('.notification-stats')).toBeVisible()
      await expect(page.locator('.unread-count')).toBeVisible()
      await expect(page.locator('.total-count')).toBeVisible()
    })

    test('通知列表应该能够正确分类显示', async ({ page }) => {
      await page.goto('/notifications')
      
      // 验证不同类型的通知分类
      await expect(page.locator('.notification-category-all')).toBeVisible()
      await expect(page.locator('.notification-category-system')).toBeVisible()
      await expect(page.locator('.notification-category-user')).toBeVisible()
      await expect(page.locator('.notification-category-security')).toBeVisible()
    })

    test('通知列表应该在网络错误时显示错误信息', async ({ page }) => {
      // 模拟网络错误
      await page.route('**/api/notifications/**', route => route.abort('failed'))
      
      await page.goto('/notifications')
      
      // 等待错误信息显示
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('加载通知失败')
      
      // 恢复网络
      await page.unroute('**/api/notifications/**')
    })
  })

  test.describe('通知设置页面测试', () => {
    test('应该能够访问通知设置页面', async ({ page }) => {
      await page.goto('/notifications')
      
      // 点击设置按钮
      await page.click('.notification-settings-btn')
      
      // 等待设置页面加载
      await page.waitForSelector('.notification-settings')
      
      // 验证设置页面存在
      await expect(page.locator('.notification-settings')).toBeVisible()
      await expect(page.locator('.settings-title')).toContainText('通知设置')
    })

    test('应该能够配置通知偏好', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 等待设置表单加载
      await page.waitForSelector('.notification-preferences')
      
      // 配置桌面通知
      await page.check('.desktop-notifications-enabled')
      
      // 配置邮件通知
      await page.check('.email-notifications-enabled')
      
      // 配置推送通知
      await page.check('.push-notifications-enabled')
      
      // 保存设置
      await page.click('.save-settings-btn')
      
      // 验证保存成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('设置保存成功')
    })

    test('应该能够配置通知类型', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 等待通知类型设置加载
      await page.waitForSelector('.notification-types')
      
      // 配置系统通知
      await page.check('.system-notifications-enabled')
      
      // 配置用户交互通知
      await page.check('.user-interaction-notifications-enabled')
      
      // 配置安全通知
      await page.check('.security-notifications-enabled')
      
      // 保存设置
      await page.click('.save-settings-btn')
      
      // 验证保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('设置表单验证应该正常工作', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 清空所有设置
      await page.uncheck('.desktop-notifications-enabled')
      await page.uncheck('.email-notifications-enabled')
      await page.uncheck('.push-notifications-enabled')
      
      // 尝试保存
      await page.click('.save-settings-btn')
      
      // 验证错误提示
      await expect(page.locator('.validation-error')).toBeVisible()
      await expect(page.locator('.validation-error')).toContainText('至少需要启用一种通知方式')
    })
  })

  test.describe('通知创建功能测试', () => {
    test('系统应该能够创建系统通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 获取初始通知数量
      const initialCount = await page.locator('.notification-item').count()
      
      // 模拟系统通知创建
      await page.evaluate(() => {
        const event = new CustomEvent('systemNotification', {
          detail: {
            id: 'system-notification-1',
            type: 'system',
            title: '系统维护通知',
            content: '系统将于今晚10点进行维护，预计持续2小时',
            priority: 'high',
            createdAt: new Date().toISOString(),
            isRead: false
          }
        })
        window.dispatchEvent(event)
      })
      
      // 等待新通知显示
      await page.waitForTimeout(2000)
      
      // 验证新通知创建成功
      await expect(page.locator('.notification-item')).toHaveCount(initialCount + 1)
      await expect(page.locator('.notification-item:has-text("系统维护通知")')).toBeVisible()
    })

    test('应该能够创建用户交互通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 获取初始通知数量
      const initialCount = await page.locator('.notification-item').count()
      
      // 模拟用户交互通知
      await page.evaluate(() => {
        const event = new CustomEvent('userInteractionNotification', {
          detail: {
            id: 'user-notification-1',
            type: 'user',
            title: '新的评论',
            content: 'otheruser 评论了你的代码片段',
            priority: 'normal',
            createdAt: new Date().toISOString(),
            isRead: false,
            actor: 'otheruser',
            action: 'commented',
            target: '代码片段'
          }
        })
        window.dispatchEvent(event)
      })
      
      // 等待新通知显示
      await page.waitForTimeout(2000)
      
      // 验证新通知创建成功
      await expect(page.locator('.notification-item')).toHaveCount(initialCount + 1)
      await expect(page.locator('.notification-item:has-text("新的评论")')).toBeVisible()
    })

    test('应该能够创建安全通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 获取初始通知数量
      const initialCount = await page.locator('.notification-item').count()
      
      // 模拟安全通知
      await page.evaluate(() => {
        const event = new CustomEvent('securityNotification', {
          detail: {
            id: 'security-notification-1',
            type: 'security',
            title: '登录提醒',
            content: '检测到新的登录活动，请确认是否为本人操作',
            priority: 'high',
            createdAt: new Date().toISOString(),
            isRead: false,
            action: 'login',
            location: '北京市',
            device: 'Chrome on Windows'
          }
        })
        window.dispatchEvent(event)
      })
      
      // 等待新通知显示
      await page.waitForTimeout(2000)
      
      // 验证新通知创建成功
      await expect(page.locator('.notification-item')).toHaveCount(initialCount + 1)
      await expect(page.locator('.notification-item:has-text("登录提醒")')).toBeVisible()
    })

    test('高优先级通知应该有特殊标识', async ({ page }) => {
      await page.goto('/notifications')
      
      // 创建高优先级通知
      await page.evaluate(() => {
        const event = new CustomEvent('highPriorityNotification', {
          detail: {
            id: 'high-priority-1',
            type: 'system',
            title: '紧急通知',
            content: '系统检测到异常活动',
            priority: 'high',
            createdAt: new Date().toISOString(),
            isRead: false
          }
        })
        window.dispatchEvent(event)
      })
      
      // 等待通知显示
      await page.waitForTimeout(1000)
      
      // 验证高优先级标识
      await expect(page.locator('.notification-item:has-text("紧急通知")')).toHaveClass(/high-priority/)
      await expect(page.locator('.priority-badge')).toContainText('高')
    })
  })

  test.describe('通知状态管理测试', () => {
    test('应该能够标记通知为已读', async ({ page }) => {
      await page.goto('/notifications')
      
      // 找到未读通知
      const unreadNotification = page.locator('.notification-item.unread').first()
      if (await unreadNotification.isVisible()) {
        // 点击标记已读按钮
        await unreadNotification.click('.mark-read-btn')
        
        // 验证通知状态改变
        await expect(unreadNotification).not.toHaveClass(/unread/)
        
        // 验证未读数量减少
        const unreadCount = await page.locator('.unread-count').textContent()
        expect(parseInt(unreadCount || '0')).toBeLessThan(10)
      }
    })

    test('应该能够批量标记通知为已读', async ({ page }) => {
      await page.goto('/notifications')
      
      // 选择多个通知
      await page.check('.notification-checkbox:first-child')
      await page.check('.notification-checkbox:nth-child(2)')
      
      // 点击批量标记已读
      await page.click('.bulk-mark-read-btn')
      
      // 验证选中通知的状态改变
      await expect(page.locator('.notification-item:first-child')).not.toHaveClass(/unread/)
      await expect(page.locator('.notification-item:nth-child(2)')).not.toHaveClass(/unread/)
    })

    test('应该能够标记通知为重要', async ({ page }) => {
      await page.goto('/notifications')
      
      // 点击标记重要按钮
      await page.click('.notification-item:first-child .mark-important-btn')
      
      // 验证重要标识
      await expect(page.locator('.notification-item:first-child')).toHaveClass(/important/)
      await expect(page.locator('.important-indicator')).toBeVisible()
    })

    test('应该能够删除通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 获取初始通知数量
      const initialCount = await page.locator('.notification-item').count()
      
      // 点击删除按钮
      await page.click('.notification-item:first-child .delete-notification-btn')
      
      // 确认删除
      await page.click('.confirm-delete-btn')
      
      // 验证通知数量减少
      await expect(page.locator('.notification-item')).toHaveCount(initialCount - 1)
    })

    test('应该能够清空所有通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 点击清空按钮
      await page.click('.clear-all-btn')
      
      // 确认清空
      await page.click('.confirm-clear-btn')
      
      // 验证所有通知被清空
      await expect(page.locator('.notification-item')).toHaveCount(0)
      await expect(page.locator('.empty-state')).toBeVisible()
    })
  })

  test.describe('通知批量操作测试', () => {
    test('应该能够批量删除通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 获取初始通知数量
      const initialCount = await page.locator('.notification-item').count()
      
      // 选择多个通知
      await page.check('.notification-checkbox:first-child')
      await page.check('.notification-checkbox:nth-child(2)')
      await page.check('.notification-checkbox:nth-child(3)')
      
      // 点击批量删除
      await page.click('.bulk-delete-btn')
      
      // 确认删除
      await page.click('.confirm-bulk-delete-btn')
      
      // 验证通知数量减少
      await expect(page.locator('.notification-item')).toHaveCount(initialCount - 3)
    })

    test('应该能够批量标记为重要', async ({ page }) => {
      await page.goto('/notifications')
      
      // 选择多个通知
      await page.check('.notification-checkbox:first-child')
      await page.check('.notification-checkbox:nth-child(2)')
      
      // 点击批量标记重要
      await page.click('.bulk-mark-important-btn')
      
      // 验证选中通知都标记为重要
      await expect(page.locator('.notification-item:first-child')).toHaveClass(/important/)
      await expect(page.locator('.notification-item:nth-child(2)')).toHaveClass(/important/)
    })

    test('应该能够批量取消重要标记', async ({ page }) => {
      await page.goto('/notifications')
      
      // 先标记一些通知为重要
      await page.check('.notification-checkbox:first-child')
      await page.check('.notification-checkbox:nth-child(2)')
      await page.click('.bulk-mark-important-btn')
      
      // 选择相同的通知
      await page.check('.notification-checkbox:first-child')
      await page.check('.notification-checkbox:nth-child(2)')
      
      // 点击批量取消重要
      await page.click('.bulk-unmark-important-btn')
      
      // 验证重要标记取消
      await expect(page.locator('.notification-item:first-child')).not.toHaveClass(/important/)
      await expect(page.locator('.notification-item:nth-child(2)')).not.toHaveClass(/important/)
    })

    test('应该能够全选/取消全选通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 点击全选
      await page.check('.select-all-checkbox')
      
      // 验证所有通知都被选中
      const checkboxes = page.locator('.notification-checkbox')
      const count = await checkboxes.count()
      for (let i = 0; i < count; i++) {
        await expect(checkboxes.nth(i)).toBeChecked()
      }
      
      // 取消全选
      await page.uncheck('.select-all-checkbox')
      
      // 验证所有通知都取消选中
      for (let i = 0; i < count; i++) {
        await expect(checkboxes.nth(i)).not.toBeChecked()
      }
    })
  })

  test.describe('通知实时接收测试', () => {
    test('新通知应该实时推送', async ({ page }) => {
      await page.goto('/notifications')
      
      // 获取初始通知数量
      const initialCount = await page.locator('.notification-item').count()
      
      // 模拟WebSocket新通知
      await page.evaluate(() => {
        const wsEvent = new CustomEvent('newNotification', {
          detail: {
            id: 'realtime-notification-1',
            type: 'system',
            title: '实时通知测试',
            content: '这是一条实时推送的通知',
            priority: 'normal',
            timestamp: new Date().toISOString()
          }
        })
        window.dispatchEvent(wsEvent)
      })
      
      // 等待实时更新
      await page.waitForTimeout(2000)
      
      // 验证新通知显示
      await expect(page.locator('.notification-item')).toHaveCount(initialCount + 1)
      await expect(page.locator('.notification-item')).toContainText('实时通知测试')
    })

    test('实时通知应该显示推送提示', async ({ page }) => {
      await page.goto('/notifications')
      
      // 模拟实时通知
      await page.evaluate(() => {
        const event = new CustomEvent('notificationPush', {
          detail: {
            id: 'push-notification-1',
            title: '推送通知测试',
            message: '这是一条推送通知',
            icon: 'info'
          }
        })
        window.dispatchEvent(event)
      })
      
      // 验证推送提示显示
      await expect(page.locator('.notification-toast')).toBeVisible()
      await expect(page.locator('.notification-toast')).toContainText('推送通知测试')
      
      // 点击推送提示
      await page.click('.notification-toast')
      
      // 验证跳转到通知详情
      await expect(page.locator('.notification-detail')).toBeVisible()
    })

    test('浏览器原生通知应该正常工作', async ({ page }) => {
      await page.goto('/notifications')
      
      // 请求通知权限
      await page.click('.request-permission-btn')
      
      // 模拟权限授予
      await page.evaluate(() => {
        // 模拟权限授予
        Notification.requestPermission = () => Promise.resolve('granted')
      })
      
      // 触发原生通知
      await page.evaluate(() => {
        new Notification('原生通知测试', {
          body: '这是一条浏览器原生通知',
          icon: '/favicon.ico'
        })
      })
      
      // 验证权限状态
      await expect(page.locator('.permission-status')).toContainText('已授权')
    })

    test('通知同步状态应该实时更新', async ({ page }) => {
      await page.goto('/notifications')
      
      // 标记通知为已读
      await page.click('.notification-item:first-child .mark-read-btn')
      
      // 模拟状态同步
      await page.evaluate(() => {
        const syncEvent = new CustomEvent('notificationSync', {
          detail: {
            notificationId: '1',
            status: 'read',
            timestamp: new Date().toISOString()
          }
        })
        window.dispatchEvent(syncEvent)
      })
      
      // 等待同步完成
      await page.waitForTimeout(1000)
      
      // 验证状态同步
      await expect(page.locator('.notification-item:first-child')).not.toHaveClass(/unread/)
    })
  })

  test.describe('通知筛选和搜索测试', () => {
    test('应该能够按类型筛选通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 选择筛选类型
      await page.selectOption('.notification-filter', 'system')
      
      // 验证筛选结果
      await expect(page.locator('.notification-item.system')).toHaveCountGreaterThan(0)
      await expect(page.locator('.notification-item:not(.system)')).toHaveCount(0)
    })

    test('应该能够按优先级筛选通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 选择优先级筛选
      await page.selectOption('.priority-filter', 'high')
      
      // 验证筛选结果
      await expect(page.locator('.notification-item.high-priority')).toHaveCountGreaterThan(0)
    })

    test('应该能够按时间筛选通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 选择时间筛选
      await page.selectOption('.time-filter', 'today')
      
      // 验证筛选结果
      await expect(page.locator('.notification-item')).toHaveCountGreaterThan(0)
      
      // 验证通知都是今天的
      const notificationDates = await page.locator('.notification-time').allTextContents()
      const today = new Date().toISOString().split('T')[0]
      notificationDates.forEach(date => {
        expect(date).toContain(today)
      })
    })

    test('应该能够搜索通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 等待搜索框加载
      await page.waitForSelector('.notification-search-input')
      
      // 输入搜索关键词
      await page.fill('.notification-search-input', '系统')
      
      // 执行搜索
      await page.press('.notification-search-input', 'Enter')
      
      // 等待搜索结果
      await page.waitForSelector('.search-results')
      
      // 验证搜索结果
      await expect(page.locator('.search-results')).toBeVisible()
      await expect(page.locator('.notification-item:has-text("系统")')).toHaveCountGreaterThan(0)
    })

    test('应该能够按已读/未读状态筛选', async ({ page }) => {
      await page.goto('/notifications')
      
      // 选择状态筛选
      await page.selectOption('.status-filter', 'unread')
      
      // 验证筛选结果
      await expect(page.locator('.notification-item.unread')).toHaveCountGreaterThan(0)
      await expect(page.locator('.notification-item:not(.unread)')).toHaveCount(0)
    })

    test('应该能够组合筛选条件', async ({ page }) => {
      await page.goto('/notifications')
      
      // 组合筛选条件
      await page.selectOption('.notification-filter', 'system')
      await page.selectOption('.priority-filter', 'high')
      await page.selectOption('.time-filter', 'this_week')
      
      // 验证组合筛选结果
      await expect(page.locator('.notification-item.system')).toHaveCountGreaterThan(0)
      await expect(page.locator('.notification-item.high-priority')).toHaveCountGreaterThan(0)
    })

    test('应该能够清除筛选条件', async ({ page }) => {
      await page.goto('/notifications')
      
      // 应用筛选条件
      await page.selectOption('.notification-filter', 'system')
      
      // 清除筛选
      await page.click('.clear-filters-btn')
      
      // 验证筛选清除
      await expect(page.locator('.notification-filter')).toHaveValue('all')
      await expect(page.locator('.notification-item')).toHaveCountGreaterThan(0)
    })
  })

  test.describe('通知偏好设置测试', () => {
    test('应该能够配置免打扰时段', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 启用免打扰
      await page.check('.do-not-disturb-enabled')
      
      // 设置免打扰时段
      await page.fill('.start-time', '22:00')
      await page.fill('.end-time', '08:00')
      
      // 保存设置
      await page.click('.save-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
      
      // 验证免打扰时段显示
      await expect(page.locator('.do-not-disturb-info')).toContainText('22:00 - 08:00')
    })

    test('应该能够配置通知频率', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 选择通知频率
      await page.selectOption('.notification-frequency', 'digest')
      
      // 设置摘要频率
      await page.selectOption('.digest-frequency', 'daily')
      
      // 保存设置
      await page.click('.save-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够配置特定类型的通知设置', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 配置评论通知
      await page.check('.comment-notifications-email')
      await page.check('.comment-notifications-push')
      await page.uncheck('.comment-notifications-desktop')
      
      // 配置点赞通知
      await page.check('.like-notifications-email')
      await page.uncheck('.like-notifications-push')
      
      // 保存设置
      await page.click('.save-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够配置通知声音', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 启用通知声音
      await page.check('.notification-sound-enabled')
      
      // 选择通知声音
      await page.selectOption('.notification-sound', 'default')
      
      // 测试声音
      await page.click('.test-sound-btn')
      
      // 验证声音测试
      await expect(page.locator('.sound-test-result')).toBeVisible()
      
      // 保存设置
      await page.click('.save-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })
  })

  test.describe('通知渠道配置测试', () => {
    test('应该能够配置邮件通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入邮件通知设置
      await page.click('.email-notification-settings')
      
      // 启用邮件通知
      await page.check('.email-notifications-enabled')
      
      // 配置邮件通知类型
      await page.check('.email-security-alerts')
      await page.check('.email-weekly-digest')
      await page.uncheck('.email-real-time')
      
      // 保存设置
      await page.click('.save-email-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够配置推送通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入推送通知设置
      await page.click('.push-notification-settings')
      
      // 启用推送通知
      await page.check('.push-notifications-enabled')
      
      // 配置推送通知类型
      await page.check('.push-urgent-notifications')
      await page.check('.push-direct-messages')
      await page.uncheck('.push-general-updates')
      
      // 保存设置
      await page.click('.save-push-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够配置桌面通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入桌面通知设置
      await page.click('.desktop-notification-settings')
      
      // 启用桌面通知
      await page.check('.desktop-notifications-enabled')
      
      // 配置桌面通知类型
      await page.check('.desktop-system-notifications')
      await page.check('.desktop-user-notifications')
      
      // 配置显示位置
      await page.selectOption('.desktop-notification-position', 'bottom-right')
      
      // 保存设置
      await page.click('.save-desktop-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够配置Webhook通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入Webhook设置
      await page.click('.webhook-settings')
      
      // 启用Webhook
      await page.check('.webhook-enabled')
      
      // 配置Webhook URL
      await page.fill('.webhook-url', 'https://hooks.slack.com/services/test')
      
      // 配置Webhook事件
      await page.check('.webhook-security-events')
      await page.check('.webhook-system-events')
      
      // 测试Webhook
      await page.click('.test-webhook-btn')
      
      // 验证测试结果
      await expect(page.locator('.webhook-test-result')).toBeVisible()
      
      // 保存设置
      await page.click('.save-webhook-settings-btn')
      
      // 验证设置保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })
  })

  test.describe('通知模板管理测试', () => {
    test('应该能够查看通知模板', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入模板管理
      await page.click('.template-management')
      
      // 等待模板列表加载
      await page.waitForSelector('.template-list')
      
      // 验证模板列表显示
      await expect(page.locator('.template-list')).toBeVisible()
      await expect(page.locator('.template-item')).toHaveCountGreaterThan(0)
    })

    test('应该能够编辑通知模板', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入模板管理
      await page.click('.template-management')
      
      // 编辑模板
      await page.click('.template-item:first-child .edit-template-btn')
      
      // 等待编辑器加载
      await page.waitForSelector('.template-editor')
      
      // 修改模板内容
      await page.fill('.template-subject', '修改后的通知标题')
      await page.fill('.template-content', '修改后的通知内容：{{message}}')
      
      // 保存模板
      await page.click('.save-template-btn')
      
      // 验证保存成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够预览通知模板', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入模板管理
      await page.click('.template-management')
      
      // 预览模板
      await page.click('.template-item:first-child .preview-template-btn')
      
      // 等待预览显示
      await page.waitForSelector('.template-preview')
      
      // 验证预览内容
      await expect(page.locator('.template-preview')).toBeVisible()
      await expect(page.locator('.preview-subject')).toBeVisible()
      await expect(page.locator('.preview-content')).toBeVisible()
    })

    test('应该能够重置模板为默认', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入模板管理
      await page.click('.template-management')
      
      // 重置模板
      await page.click('.template-item:first-child .reset-template-btn')
      
      // 确认重置
      await page.click('.confirm-reset-btn')
      
      // 验证重置成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('模板已重置为默认')
    })
  })

  test.describe('通知订阅管理测试', () => {
    test('应该能够管理通知订阅', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入订阅管理
      await page.click('.subscription-management')
      
      // 等待订阅列表加载
      await page.waitForSelector('.subscription-list')
      
      // 验证订阅列表显示
      await expect(page.locator('.subscription-list')).toBeVisible()
      await expect(page.locator('.subscription-item')).toHaveCountGreaterThan(0)
    })

    test('应该能够创建新的订阅', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入订阅管理
      await page.click('.subscription-management')
      
      // 创建新订阅
      await page.click('.new-subscription-btn')
      
      // 等待订阅表单加载
      await page.waitForSelector('.subscription-form')
      
      // 填写订阅信息
      await page.fill('.subscription-name', '测试订阅')
      await page.selectOption('.subscription-type', 'rss')
      await page.fill('.subscription-url', 'https://example.com/feed.xml')
      
      // 配置通知规则
      await page.check('.notify-on-new-items')
      await page.selectOption('.notification-frequency', 'immediate')
      
      // 保存订阅
      await page.click('.save-subscription-btn')
      
      // 验证订阅创建成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.subscription-item:has-text("测试订阅")')).toBeVisible()
    })

    test('应该能够编辑订阅', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入订阅管理
      await page.click('.subscription-management')
      
      // 编辑订阅
      await page.click('.subscription-item:first-child .edit-subscription-btn')
      
      // 等待编辑表单加载
      await page.waitForSelector('.subscription-form')
      
      // 修改订阅信息
      await page.fill('.subscription-name', '修改后的订阅名称')
      
      // 保存修改
      await page.click('.save-subscription-btn')
      
      // 验证修改成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.subscription-item:has-text("修改后的订阅名称")')).toBeVisible()
    })

    test('应该能够测试订阅', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入订阅管理
      await page.click('.subscription-management')
      
      // 测试订阅
      await page.click('.subscription-item:first-child .test-subscription-btn')
      
      // 等待测试结果
      await page.waitForSelector('.subscription-test-result')
      
      // 验证测试结果
      await expect(page.locator('.subscription-test-result')).toBeVisible()
    })

    test('应该能够删除订阅', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入订阅管理
      await page.click('.subscription-management')
      
      // 获取初始订阅数量
      const initialCount = await page.locator('.subscription-item').count()
      
      // 删除订阅
      await page.click('.subscription-item:first-child .delete-subscription-btn')
      
      // 确认删除
      await page.click('.confirm-delete-btn')
      
      // 验证订阅数量减少
      await expect(page.locator('.subscription-item')).toHaveCount(initialCount - 1)
    })

    test('应该能够启用/禁用订阅', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 进入订阅管理
      await page.click('.subscription-management')
      
      // 禁用订阅
      await page.click('.subscription-item:first-child .toggle-subscription-btn')
      
      // 验证订阅状态
      await expect(page.locator('.subscription-item:first-child')).toHaveClass(/disabled/)
      
      // 重新启用订阅
      await page.click('.subscription-item:first-child .toggle-subscription-btn')
      
      // 验证订阅状态
      await expect(page.locator('.subscription-item:first-child')).not.toHaveClass(/disabled/)
    })
  })

  test.describe('通知移动端适配测试', () => {
    test.beforeEach(async ({ page }) => {
      // 设置移动端视口
      await page.setViewportSize({ width: 375, height: 667 })
    })

    test('通知界面应该适配移动端', async ({ page }) => {
      await page.goto('/notifications')
      
      // 验证界面响应式
      await expect(page.locator('.notification-container')).toHaveCSS('width', '100%')
      
      // 验证通知列表适配
      await expect(page.locator('.notification-item')).toHaveCSS('padding', '12px')
    })

    test('移动端通知设置应该易于操作', async ({ page }) => {
      await page.goto('/notifications')
      
      // 进入设置页面
      await page.click('.notification-settings-btn')
      
      // 验证设置项适合触摸
      const settingItems = page.locator('.setting-item')
      await expect(settingItems.first()).toBeVisible()
      
      const firstItem = await settingItems.first().boundingBox()
      expect(firstItem?.height).toBeGreaterThan(40)
    })

    test('移动端应该支持滑动删除通知', async ({ page }) => {
      await page.goto('/notifications')
      
      // 模拟滑动操作
      const notificationItem = page.locator('.notification-item:first-child')
      const box = await notificationItem.boundingBox()
      
      if (box) {
        // 向左滑动
        await page.mouse.move(box.x + box.width - 10, box.y + box.height / 2)
        await page.mouse.down()
        await page.mouse.move(box.x + 50, box.y + box.height / 2, { steps: 5 })
        await page.mouse.up()
        
        // 验证删除按钮出现
        await expect(page.locator('.swipe-delete-btn')).toBeVisible()
      }
    })

    test('移动端应该有下拉刷新功能', async ({ page }) => {
      await page.goto('/notifications')
      
      // 模拟下拉刷新
      await page.mouse.move(200, 100)
      await page.mouse.down()
      await page.mouse.move(200, 300, { steps: 10 })
      await page.mouse.up()
      
      // 验证刷新指示器显示
      await expect(page.locator('.pull-to-refresh')).toBeVisible()
    })

    test('移动端应该有底部导航', async ({ page }) => {
      await page.goto('/notifications')
      
      // 验证底部导航显示
      await expect(page.locator('.mobile-bottom-nav')).toBeVisible()
      await expect(page.locator('.nav-item')).toHaveCount(4) // 通知、设置、关于、更多
    })
  })

  test.describe('通知性能测试', () => {
    test('大量通知加载应该有良好的性能', async ({ page }) => {
      // 设置性能监控
      const startTime = Date.now()
      
      await page.goto('/notifications')
      
      // 等待通知列表加载完成
      await page.waitForSelector('.notification-list')
      
      const loadTime = Date.now() - startTime
      
      // 验证加载时间在可接受范围内
      expect(loadTime).toBeLessThan(3000)
      console.log(`通知列表加载时间: ${loadTime}ms`)
    })

    test('通知搜索应该快速返回结果', async ({ page }) => {
      await page.goto('/notifications')
      
      // 测试搜索响应时间
      const searchStartTime = Date.now()
      await page.fill('.notification-search-input', '系统')
      await page.press('.notification-search-input', 'Enter')
      await page.waitForSelector('.search-results')
      const searchTime = Date.now() - searchStartTime
      
      expect(searchTime).toBeLessThan(2000)
      console.log(`通知搜索响应时间: ${searchTime}ms`)
    })

    test('实时通知更新应该及时', async ({ page }) => {
      await page.goto('/notifications')
      
      // 测试实时更新延迟
      const updateStartTime = Date.now()
      
      // 模拟新通知
      await page.evaluate(() => {
        const event = new CustomEvent('newNotification', {
          detail: {
            id: 'performance-test-notification',
            type: 'system',
            title: '性能测试通知',
            content: '测试实时更新性能',
            priority: 'normal',
            timestamp: new Date().toISOString()
          }
        })
        window.dispatchEvent(event)
      })
      
      // 等待通知显示
      await expect(page.locator('.notification-item:has-text("性能测试通知")')).toBeVisible()
      
      const updateTime = Date.now() - updateStartTime
      
      // 验证更新延迟在可接受范围内
      expect(updateTime).toBeLessThan(1000)
      console.log(`实时通知更新延迟: ${updateTime}ms`)
    })

    test('通知批量操作应该高效', async ({ page }) => {
      await page.goto('/notifications')
      
      // 测试批量操作性能
      const batchStartTime = Date.now()
      
      // 全选通知
      await page.check('.select-all-checkbox')
      
      // 批量标记已读
      await page.click('.bulk-mark-read-btn')
      
      // 等待操作完成
      await page.waitForSelector('.success-message')
      
      const batchTime = Date.now() - batchStartTime
      
      // 验证批量操作时间在可接受范围内
      expect(batchTime).toBeLessThan(2000)
      console.log(`批量操作时间: ${batchTime}ms`)
    })
  })

  test.describe('通知错误处理测试', () => {
    test('应该优雅处理网络错误', async ({ page }) => {
      // 模拟网络错误
      await page.route('**/api/notifications/**', route => route.abort('failed'))
      
      await page.goto('/notifications')
      
      // 尝试标记通知为已读
      await page.click('.notification-item:first-child .mark-read-btn')
      
      // 验证错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('网络连接失败')
      
      // 验证重试按钮
      await expect(page.locator('.retry-btn')).toBeVisible()
      
      // 恢复网络
      await page.unroute('**/api/notifications/**')
    })

    test('应该处理服务器错误', async ({ page }) => {
      // 模拟服务器错误
      await page.route('**/api/notifications/**', route => route.fulfill({
        status: 500,
        body: JSON.stringify({ error: '服务器内部错误' })
      }))
      
      await page.goto('/notifications')
      
      // 尝试删除通知
      await page.click('.notification-item:first-child .delete-notification-btn')
      await page.click('.confirm-delete-btn')
      
      // 验证错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('服务器内部错误')
      
      // 恢复正常路由
      await page.unroute('**/api/notifications/**')
    })

    test('应该处理WebSocket连接错误', async ({ page }) => {
      await page.goto('/notifications')
      
      // 模拟WebSocket连接错误
      await page.evaluate(() => {
        // 触发连接错误事件
        const errorEvent = new Event('websocket_error')
        window.dispatchEvent(errorEvent)
      })
      
      // 验证错误提示
      await expect(page.locator('.connection-error')).toBeVisible()
      await expect(page.locator('.connection-error')).toContainText('实时连接已断开')
      
      // 验证重连按钮
      await expect(page.locator('.reconnect-btn')).toBeVisible()
    })

    test('应该处理通知权限错误', async ({ page }) => {
      await page.goto('/notifications')
      
      // 模拟权限被拒绝
      await page.evaluate(() => {
        Notification.requestPermission = () => Promise.resolve('denied')
      })
      
      // 请求通知权限
      await page.click('.request-permission-btn')
      
      // 验证权限错误提示
      await expect(page.locator('.permission-error')).toBeVisible()
      await expect(page.locator('.permission-error')).toContainText('通知权限被拒绝')
    })
  })

  test.describe('通知无障碍访问测试', () => {
    test('通知界面应该支持键盘导航', async ({ page }) => {
      await page.goto('/notifications')
      
      // 测试Tab键导航
      await page.keyboard.press('Tab')
      await expect(page.locator('.notification-search-input')).toBeFocused()
      
      await page.keyboard.press('Tab')
      await expect(page.locator('.notification-settings-btn')).toBeFocused()
      
      // 测试Enter键选择通知
      await page.keyboard.press('ArrowDown')
      await page.keyboard.press('Enter')
      
      // 验证通知详情打开
      await expect(page.locator('.notification-detail')).toBeVisible()
    })

    test('通知组件应该有正确的ARIA标签', async ({ page }) => {
      await page.goto('/notifications')
      
      // 验证通知列表ARIA属性
      await expect(page.locator('.notification-list')).toHaveAttribute('role', 'list')
      await expect(page.locator('.notification-item')).toHaveAttribute('role', 'listitem')
      
      // 验证按钮ARIA标签
      await expect(page.locator('.notification-settings-btn')).toHaveAttribute('aria-label')
      await expect(page.locator('.mark-read-btn')).toHaveAttribute('aria-label')
      await expect(page.locator('.delete-notification-btn')).toHaveAttribute('aria-label')
    })

    test('通知状态变化应该有屏幕阅读器支持', async ({ page }) => {
      await page.goto('/notifications')
      
      // 标记通知为已读
      await page.click('.notification-item:first-child .mark-read-btn')
      
      // 验证状态变化通知
      await expect(page.locator('.sr-only')).toContainText('通知已标记为已读')
    })

    test('高优先级通知应该有适当的ARIA实时区域', async ({ page }) => {
      await page.goto('/notifications')
      
      // 创建高优先级通知
      await page.evaluate(() => {
        const event = new CustomEvent('highPriorityNotification', {
          detail: {
            id: 'aria-test-notification',
            type: 'security',
            title: '安全警告',
            content: '检测到异常登录活动',
            priority: 'high',
            createdAt: new Date().toISOString(),
            isRead: false
          }
        })
        window.dispatchEvent(event)
      })
      
      // 验证实时区域更新
      await expect(page.locator('[aria-live="assertive"]')).toBeVisible()
      await expect(page.locator('[aria-live="assertive"]')).toContainText('安全警告')
    })

    test('应该支持高对比度模式', async ({ page }) => {
      await page.goto('/notifications')
      
      // 验证高对比度样式
      await expect(page.locator('.notification-item')).toHaveCSS('background-color', 'rgb(255, 255, 255)')
      await expect(page.locator('.notification-item.unread')).toHaveCSS('background-color', 'rgb(248, 249, 250)')
    })
  })
})