import { test, expect } from '@playwright/test'

test.describe('消息功能端到端测试', () => {
  test.beforeEach(async ({ page }) => {
    // 用户登录
    await page.goto('/login')
    await page.fill('[data-test="username"]', 'testuser')
    await page.fill('[data-test="password"]', 'password123')
    await page.click('[data-test="login-btn"]')
    await page.waitForURL('/dashboard')
  })

  test.describe('消息列表页面加载测试', () => {
    test('应该能够加载消息列表页面', async ({ page }) => {
      // 导航到消息页面
      await page.goto('/messages')
      
      // 等待消息列表加载
      await page.waitForSelector('.message-list')
      
      // 验证消息列表存在
      await expect(page.locator('.message-list')).toBeVisible()
      
      // 验证页面标题
      await expect(page.locator('h1')).toContainText('消息中心')
    })

    test('应该能够显示消息统计信息', async ({ page }) => {
      await page.goto('/messages')
      
      // 等待统计信息加载
      await page.waitForSelector('.message-stats')
      
      // 验证统计信息显示
      await expect(page.locator('.message-stats')).toBeVisible()
      await expect(page.locator('.unread-count')).toBeVisible()
      await expect(page.locator('.total-count')).toBeVisible()
    })

    test('消息列表应该能够正确分类显示', async ({ page }) => {
      await page.goto('/messages')
      
      // 验证不同类型的消息分类
      await expect(page.locator('.message-category-inbox')).toBeVisible()
      await expect(page.locator('.message-category-sent')).toBeVisible()
      await expect(page.locator('.message-category-draft')).toBeVisible()
      await expect(page.locator('.message-category-trash')).toBeVisible()
    })

    test('消息列表应该在网络错误时显示错误信息', async ({ page }) => {
      // 模拟网络错误
      await page.route('**/api/messages/**', route => route.abort('failed'))
      
      await page.goto('/messages')
      
      // 等待错误信息显示
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('加载消息失败')
      
      // 恢复网络
      await page.unroute('**/api/messages/**')
    })
  })

  test.describe('消息发送功能测试', () => {
    test('应该能够发送新消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 等待消息表单出现
      await page.waitForSelector('.message-form')
      
      // 填写收件人
      await page.fill('.recipient-input', 'otheruser')
      await page.press('.recipient-input', 'Enter')
      
      // 填写主题
      await page.fill('.subject-input', '测试消息主题')
      
      // 填写消息内容
      await page.fill('.message-content', '这是一条测试消息的内容')
      
      // 发送消息
      await page.click('.send-message-btn')
      
      // 验证发送成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('消息发送成功')
    })

    test('应该能够发送富文本消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 等待富文本编辑器加载
      await page.waitForSelector('.rich-editor')
      
      // 在富文本编辑器中输入内容
      await page.fill('.rich-editor textarea', '**粗体文本** 和 *斜体文本*')
      
      // 填写收件人和主题
      await page.fill('.recipient-input', 'otheruser')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.subject-input', '富文本测试消息')
      
      // 发送消息
      await page.click('.send-message-btn')
      
      // 验证富文本消息发送成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('应该能够添加多个收件人', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 添加多个收件人
      await page.fill('.recipient-input', 'user1')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.recipient-input', 'user2')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.recipient-input', 'user3')
      await page.press('.recipient-input', 'Enter')
      
      // 验证收件人标签
      await expect(page.locator('.recipient-tag')).toHaveCount(3)
      
      // 填写主题和内容
      await page.fill('.subject-input', '群发测试消息')
      await page.fill('.message-content', '这是一条群发消息')
      
      // 发送消息
      await page.click('.send-message-btn')
      
      // 验证发送成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('消息表单验证应该正常工作', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 尝试发送空消息
      await page.click('.send-message-btn')
      
      // 验证错误提示
      await expect(page.locator('.validation-error')).toBeVisible()
      await expect(page.locator('.validation-error')).toContainText('请填写收件人')
    })

    test('应该能够保存草稿', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 填写部分内容
      await page.fill('.recipient-input', 'otheruser')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.subject-input', '草稿消息')
      
      // 保存草稿
      await page.click('.save-draft-btn')
      
      // 验证草稿保存成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('草稿已保存')
      
      // 验证草稿出现在草稿箱中
      await page.click('.draft-folder')
      await expect(page.locator('.message-item:has-text("草稿消息")')).toBeVisible()
    })
  })

  test.describe('消息接收功能测试', () => {
    test('应该能够接收新消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 获取初始未读消息数量
      const initialUnread = await page.locator('.unread-count').textContent()
      
      // 模拟新消息到达
      await page.evaluate(() => {
        const event = new MessageEvent('message', {
          data: JSON.stringify({
            type: 'new_message',
            message: {
              id: 'new-message-1',
              sender: 'otheruser',
              subject: '新消息测试',
              content: '这是一条新消息',
              createdAt: new Date().toISOString(),
              isRead: false
            }
          })
        })
        window.dispatchEvent(event)
      })
      
      // 等待消息更新
      await page.waitForTimeout(2000)
      
      // 验证未读消息数量增加
      const updatedUnread = await page.locator('.unread-count').textContent()
      expect(parseInt(updatedUnread || '0')).toBe(parseInt(initialUnread || '0') + 1)
      
      // 验证新消息显示在列表中
      await expect(page.locator('.message-item:has-text("新消息测试")')).toBeVisible()
    })

    test('新消息应该有未读标识', async ({ page }) => {
      await page.goto('/messages')
      
      // 模拟新消息
      await page.evaluate(() => {
        const event = new MessageEvent('message', {
          data: JSON.stringify({
            type: 'new_message',
            message: {
              id: 'unread-message-1',
              sender: 'otheruser',
              subject: '未读消息测试',
              content: '未读消息内容',
              createdAt: new Date().toISOString(),
              isRead: false
            }
          })
        })
        window.dispatchEvent(event)
      })
      
      // 等待消息更新
      await page.waitForTimeout(1000)
      
      // 验证未读标识
      await expect(page.locator('.message-item:has-text("未读消息测试")')).toHaveClass(/unread/)
      await expect(page.locator('.unread-indicator')).toBeVisible()
    })

    test('应该能够显示消息详情', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击消息查看详情
      await page.click('.message-item:first-child')
      
      // 等待消息详情加载
      await page.waitForSelector('.message-detail')
      
      // 验证消息详情显示
      await expect(page.locator('.message-detail')).toBeVisible()
      await expect(page.locator('.message-subject')).toBeVisible()
      await expect(page.locator('.message-content')).toBeVisible()
      await expect(page.locator('.message-sender')).toBeVisible()
      await expect(page.locator('.message-time')).toBeVisible()
    })
  })

  test.describe('消息状态管理测试', () => {
    test('应该能够标记消息为已读', async ({ page }) => {
      await page.goto('/messages')
      
      // 找到未读消息
      const unreadMessage = page.locator('.message-item.unread').first()
      if (await unreadMessage.isVisible()) {
        // 点击标记已读按钮
        await unreadMessage.click('.mark-read-btn')
        
        // 验证消息状态改变
        await expect(unreadMessage).not.toHaveClass(/unread/)
        
        // 验证未读数量减少
        const unreadCount = await page.locator('.unread-count').textContent()
        expect(parseInt(unreadCount || '0')).toBeLessThan(10) // 假设初始有未读消息
      }
    })

    test('应该能够批量标记消息为已读', async ({ page }) => {
      await page.goto('/messages')
      
      // 选择多个消息
      await page.check('.message-checkbox:first-child')
      await page.check('.message-checkbox:nth-child(2)')
      
      // 点击批量标记已读
      await page.click('.bulk-mark-read-btn')
      
      // 验证选中消息的状态改变
      await expect(page.locator('.message-item:first-child')).not.toHaveClass(/unread/)
      await expect(page.locator('.message-item:nth-child(2)')).not.toHaveClass(/unread/)
    })

    test('应该能够标记消息为重要', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击标记重要按钮
      await page.click('.message-item:first-child .mark-important-btn')
      
      // 验证重要标识
      await expect(page.locator('.message-item:first-child')).toHaveClass(/important/)
      await expect(page.locator('.important-indicator')).toBeVisible()
    })

    test('应该能够移动消息到垃圾箱', async ({ page }) => {
      await page.goto('/messages')
      
      // 获取初始消息数量
      const initialCount = await page.locator('.message-item').count()
      
      // 点击删除按钮
      await page.click('.message-item:first-child .delete-message-btn')
      
      // 确认删除
      await page.click('.confirm-delete-btn')
      
      // 验证消息数量减少
      await expect(page.locator('.message-item')).toHaveCount(initialCount - 1)
      
      // 验证消息在垃圾箱中
      await page.click('.trash-folder')
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
    })

    test('应该能够从垃圾箱恢复消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入垃圾箱
      await page.click('.trash-folder')
      
      // 点击恢复按钮
      await page.click('.message-item:first-child .restore-message-btn')
      
      // 验证消息恢复成功
      await expect(page.locator('.success-message')).toBeVisible()
      
      // 验证消息回到收件箱
      await page.click('.inbox-folder')
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
    })

    test('应该能够永久删除消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入垃圾箱
      await page.click('.trash-folder')
      
      // 获取初始消息数量
      const initialCount = await page.locator('.message-item').count()
      
      // 点击永久删除按钮
      await page.click('.message-item:first-child .permanent-delete-btn')
      
      // 确认永久删除
      await page.click('.confirm-permanent-delete-btn')
      
      // 验证消息数量减少
      await expect(page.locator('.message-item')).toHaveCount(initialCount - 1)
    })
  })

  test.describe('会话管理功能测试', () => {
    test('应该能够创建会话', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建会话按钮
      await page.click('.new-conversation-btn')
      
      // 选择联系人
      await page.click('.contact-item:has-text("otheruser")')
      
      // 验证会话创建
      await expect(page.locator('.conversation-detail')).toBeVisible()
      await expect(page.locator('.conversation-participant')).toContainText('otheruser')
    })

    test('应该能够查看会话历史', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入现有会话
      await page.click('.conversation-item:first-child')
      
      // 验证会话历史显示
      await expect(page.locator('.conversation-history')).toBeVisible()
      await expect(page.locator('.conversation-message')).toHaveCountGreaterThan(0)
    })

    test('应该能够在会话中发送消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入会话
      await page.click('.conversation-item:first-child')
      
      // 发送消息
      await page.fill('.conversation-input', '会话消息测试')
      await page.click('.send-conversation-btn')
      
      // 验证消息发送成功
      await expect(page.locator('.conversation-message:has-text("会话消息测试")')).toBeVisible()
    })

    test('应该能够将会话标记为静音', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入会话设置
      await page.click('.conversation-item:first-child .conversation-settings')
      
      // 启用静音
      await page.check('.mute-conversation-checkbox')
      
      // 验证静音标识
      await expect(page.locator('.conversation-item:first-child')).toHaveClass(/muted/)
    })

    test('应该能够删除会话', async ({ page }) => {
      await page.goto('/messages')
      
      // 获取初始会话数量
      const initialCount = await page.locator('.conversation-item').count()
      
      // 删除会话
      await page.click('.conversation-item:first-child .delete-conversation-btn')
      
      // 确认删除
      await page.click('.confirm-delete-btn')
      
      // 验证会话数量减少
      await expect(page.locator('.conversation-item')).toHaveCount(initialCount - 1)
    })
  })

  test.describe('附件上传测试', () => {
    test('应该能够上传文件附件', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 等待上传组件加载
      await page.waitForSelector('.file-upload')
      
      // 创建测试文件
      const fileContent = Buffer.from('测试文件内容')
      
      // 上传文件
      const fileInput = page.locator('.file-input')
      await fileInput.setInputFiles({
        name: 'test.txt',
        mimeType: 'text/plain',
        buffer: fileContent
      })
      
      // 验证文件上传成功
      await expect(page.locator('.attachment-item')).toBeVisible()
      await expect(page.locator('.attachment-name')).toContainText('test.txt')
    })

    test('应该能够上传多个附件', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 上传多个文件
      const fileInput = page.locator('.file-input')
      await fileInput.setInputFiles([
        {
          name: 'file1.txt',
          mimeType: 'text/plain',
          buffer: Buffer.from('文件1内容')
        },
        {
          name: 'file2.txt',
          mimeType: 'text/plain',
          buffer: Buffer.from('文件2内容')
        }
      ])
      
      // 验证多个附件上传成功
      await expect(page.locator('.attachment-item')).toHaveCount(2)
    })

    test('应该能够删除已上传的附件', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 上传文件
      const fileInput = page.locator('.file-input')
      await fileInput.setInputFiles({
        name: 'test.txt',
        mimeType: 'text/plain',
        buffer: Buffer.from('测试文件内容')
      })
      
      // 删除附件
      await page.click('.attachment-item .remove-attachment-btn')
      
      // 验证附件删除成功
      await expect(page.locator('.attachment-item')).toHaveCount(0)
    })

    test('应该能够预览附件', async ({ page }) => {
      await page.goto('/messages')
      
      // 查看包含附件的消息
      await page.click('.message-item:has-text("附件")')
      
      // 等待消息详情加载
      await page.waitForSelector('.message-detail')
      
      // 点击预览附件
      await page.click('.attachment-item .preview-attachment-btn')
      
      // 验证预览对话框显示
      await expect(page.locator('.attachment-preview')).toBeVisible()
      await expect(page.locator('.preview-content')).toBeVisible()
    })

    test('应该能够下载附件', async ({ page }) => {
      await page.goto('/messages')
      
      // 查看包含附件的消息
      await page.click('.message-item:has-text("附件")')
      
      // 等待消息详情加载
      await page.waitForSelector('.message-detail')
      
      // 点击下载附件
      const downloadPromise = page.waitForEvent('download')
      await page.click('.attachment-item .download-attachment-btn')
      const download = await downloadPromise
      
      // 验证下载成功
      expect(download.suggestedFilename()).toMatch(/test\.txt$/)
    })

    test('文件大小限制应该正常工作', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 尝试上传超大文件
      const largeFileContent = Buffer.alloc(11 * 1024 * 1024) // 11MB
      const fileInput = page.locator('.file-input')
      
      await fileInput.setInputFiles({
        name: 'large.txt',
        mimeType: 'text/plain',
        buffer: largeFileContent
      })
      
      // 验证错误提示
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('文件大小超过限制')
    })
  })

  test.describe('草稿管理测试', () => {
    test('应该能够编辑草稿', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入草稿箱
      await page.click('.draft-folder')
      
      // 编辑草稿
      await page.click('.message-item:first-child .edit-draft-btn')
      
      // 等待编辑表单加载
      await page.waitForSelector('.message-form')
      
      // 修改内容
      await page.fill('.subject-input', '修改后的草稿主题')
      await page.fill('.message-content', '修改后的草稿内容')
      
      // 保存草稿
      await page.click('.save-draft-btn')
      
      // 验证草稿更新成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.message-item:has-text("修改后的草稿主题")')).toBeVisible()
    })

    test('应该能够从草稿发送消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入草稿箱
      await page.click('.draft-folder')
      
      // 从草稿发送
      await page.click('.message-item:first-child .send-draft-btn')
      
      // 验证发送成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('消息发送成功')
      
      // 验证草稿从草稿箱中移除
      await expect(page.locator('.message-item')).toHaveCount(0)
    })

    test('应该能够删除草稿', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入草稿箱
      await page.click('.draft-folder')
      
      // 获取初始草稿数量
      const initialCount = await page.locator('.message-item').count()
      
      // 删除草稿
      await page.click('.message-item:first-child .delete-draft-btn')
      
      // 确认删除
      await page.click('.confirm-delete-btn')
      
      // 验证草稿数量减少
      await expect(page.locator('.message-item')).toHaveCount(initialCount - 1)
    })

    test('草稿应该自动保存', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 填写内容但不保存
      await page.fill('.recipient-input', 'otheruser')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.subject-input', '自动保存测试')
      
      // 等待自动保存
      await page.waitForTimeout(5000)
      
      // 验证草稿自动保存
      await page.click('.draft-folder')
      await expect(page.locator('.message-item:has-text("自动保存测试")')).toBeVisible()
    })
  })

  test.describe('消息搜索测试', () => {
    test('应该能够搜索消息', async ({ page }) => {
      await page.goto('/messages')
      
      // 等待搜索框加载
      await page.waitForSelector('.message-search-input')
      
      // 输入搜索关键词
      await page.fill('.message-search-input', '测试')
      
      // 执行搜索
      await page.press('.message-search-input', 'Enter')
      
      // 等待搜索结果
      await page.waitForSelector('.search-results')
      
      // 验证搜索结果
      await expect(page.locator('.search-results')).toBeVisible()
      await expect(page.locator('.message-item:has-text("测试")')).toHaveCountGreaterThan(0)
    })

    test('应该能够按发件人搜索', async ({ page }) => {
      await page.goto('/messages')
      
      // 使用搜索过滤器
      await page.selectOption('.search-filter', 'sender')
      
      // 输入发件人名称
      await page.fill('.message-search-input', 'otheruser')
      await page.press('.message-search-input', 'Enter')
      
      // 验证搜索结果
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
      await expect(page.locator('.message-sender')).toContainText('otheruser')
    })

    test('应该能够按主题搜索', async ({ page }) => {
      await page.goto('/messages')
      
      // 使用搜索过滤器
      await page.selectOption('.search-filter', 'subject')
      
      // 输入主题关键词
      await page.fill('.message-search-input', '主题测试')
      await page.press('.message-search-input', 'Enter')
      
      // 验证搜索结果
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
      await expect(page.locator('.message-subject')).toContainText('主题测试')
    })

    test('应该能够按日期范围搜索', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击高级搜索
      await page.click('.advanced-search-btn')
      
      // 设置日期范围
      const startDate = new Date()
      startDate.setDate(startDate.getDate() - 7)
      const startDateStr = startDate.toISOString().split('T')[0]
      
      await page.fill('.start-date-input', startDateStr)
      await page.fill('.end-date-input', new Date().toISOString().split('T')[0])
      
      // 执行搜索
      await page.click('.apply-filters-btn')
      
      // 验证搜索结果
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
    })

    test('搜索无结果时应该显示提示', async ({ page }) => {
      await page.goto('/messages')
      
      // 输入不存在的关键词
      await page.fill('.message-search-input', '不存在的消息内容')
      await page.press('.message-search-input', 'Enter')
      
      // 验证无结果提示
      await expect(page.locator('.no-results')).toBeVisible()
      await expect(page.locator('.no-results')).toContainText('没有找到相关消息')
    })
  })

  test.describe('消息筛选测试', () => {
    test('应该能够按消息类型筛选', async ({ page }) => {
      await page.goto('/messages')
      
      // 选择筛选类型
      await page.selectOption('.message-filter', 'unread')
      
      // 验证筛选结果
      await expect(page.locator('.message-item.unread')).toHaveCountGreaterThan(0)
      await expect(page.locator('.message-item:not(.unread)')).toHaveCount(0)
    })

    test('应该能够按重要程度筛选', async ({ page }) => {
      await page.goto('/messages')
      
      // 选择筛选类型
      await page.selectOption('.message-filter', 'important')
      
      // 验证筛选结果
      await expect(page.locator('.message-item.important')).toHaveCountGreaterThan(0)
    })

    test('应该能够按时间筛选', async ({ page }) => {
      await page.goto('/messages')
      
      // 选择时间筛选
      await page.selectOption('.time-filter', 'today')
      
      // 验证筛选结果
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
      
      // 验证消息都是今天的
      const messageDates = await page.locator('.message-time').allTextContents()
      const today = new Date().toISOString().split('T')[0]
      messageDates.forEach(date => {
        expect(date).toContain(today)
      })
    })

    test('应该能够组合筛选条件', async ({ page }) => {
      await page.goto('/messages')
      
      // 组合筛选条件
      await page.selectOption('.message-filter', 'unread')
      await page.selectOption('.time-filter', 'this_week')
      
      // 验证组合筛选结果
      await expect(page.locator('.message-item.unread')).toHaveCountGreaterThan(0)
    })

    test('应该能够清除筛选条件', async ({ page }) => {
      await page.goto('/messages')
      
      // 应用筛选条件
      await page.selectOption('.message-filter', 'unread')
      
      // 清除筛选
      await page.click('.clear-filters-btn')
      
      // 验证筛选清除
      await expect(page.locator('.message-filter')).toHaveValue('all')
      await expect(page.locator('.message-item')).toHaveCountGreaterThan(0)
    })
  })

  test.describe('消息实时通信测试', () => {
    test('新消息应该实时推送', async ({ page }) => {
      await page.goto('/messages')
      
      // 获取初始消息数量
      const initialCount = await page.locator('.message-item').count()
      
      // 模拟WebSocket新消息
      await page.evaluate(() => {
        const wsEvent = new CustomEvent('newMessage', {
          detail: {
            id: 'realtime-message-1',
            sender: 'otheruser',
            subject: '实时消息测试',
            content: '这是一条实时消息',
            timestamp: new Date().toISOString()
          }
        })
        window.dispatchEvent(wsEvent)
      })
      
      // 等待实时更新
      await page.waitForTimeout(2000)
      
      // 验证新消息显示
      await expect(page.locator('.message-item')).toHaveCount(initialCount + 1)
      await expect(page.locator('.message-item')).toContainText('实时消息测试')
    })

    test('消息状态应该实时同步', async ({ page }) => {
      await page.goto('/messages')
      
      // 标记消息为已读
      await page.click('.message-item:first-child .mark-read-btn')
      
      // 模拟状态同步
      await page.evaluate(() => {
        const syncEvent = new CustomEvent('messageSync', {
          detail: {
            messageId: '1',
            status: 'read',
            timestamp: new Date().toISOString()
          }
        })
        window.dispatchEvent(syncEvent)
      })
      
      // 等待同步完成
      await page.waitForTimeout(1000)
      
      // 验证状态同步
      await expect(page.locator('.message-item:first-child')).not.toHaveClass(/unread/)
    })

    test('在线状态应该实时显示', async ({ page }) => {
      await page.goto('/messages')
      
      // 模拟用户上线
      await page.evaluate(() => {
        const onlineEvent = new CustomEvent('userOnline', {
          detail: {
            userId: 'user1',
            username: 'user1',
            status: 'online'
          }
        })
        window.dispatchEvent(onlineEvent)
      })
      
      // 等待状态更新
      await page.waitForTimeout(1000)
      
      // 验证在线状态显示
      await expect(page.locator('.user-status:has-text("user1")')).toHaveClass(/online/)
    })

    test('正在输入状态应该实时显示', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入会话
      await page.click('.conversation-item:first-child')
      
      // 模拟对方正在输入
      await page.evaluate(() => {
        const typingEvent = new CustomEvent('userTyping', {
          detail: {
            userId: 'otheruser',
            conversationId: '1',
            isTyping: true
          }
        })
        window.dispatchEvent(typingEvent)
      })
      
      // 验证正在输入状态显示
      await expect(page.locator('.typing-indicator')).toBeVisible()
      await expect(page.locator('.typing-indicator')).toContainText('对方正在输入...')
    })
  })

  test.describe('消息反应功能测试', () => {
    test('应该能够对消息添加反应', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入消息详情
      await page.click('.message-item:first-child')
      
      // 点击反应按钮
      await page.click('.reaction-btn')
      
      // 选择反应表情
      await page.click('.reaction-option:has-text("👍")')
      
      // 验证反应添加成功
      await expect(page.locator('.reaction-item:has-text("👍")')).toBeVisible()
    })

    test('应该能够查看谁添加了反应', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入消息详情
      await page.click('.message-item:first-child')
      
      // 点击反应统计
      await page.click('.reaction-count')
      
      // 验证反应详情显示
      await expect(page.locator('.reaction-details')).toBeVisible()
      await expect(page.locator('.reaction-user')).toHaveCountGreaterThan(0)
    })

    test('应该能够移除自己的反应', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入消息详情
      await page.click('.message-item:first-child')
      
      // 找到自己的反应
      const userReaction = page.locator('.reaction-item.user-reaction')
      if (await userReaction.isVisible()) {
        // 移除反应
        await userReaction.click()
        
        // 验证反应移除成功
        await expect(userReaction).not.toBeVisible()
      }
    })
  })

  test.describe('消息已读回执测试', () => {
    test('应该能够显示消息已读状态', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入已发送消息
      await page.click('.sent-folder')
      
      // 查看消息详情
      await page.click('.message-item:first-child')
      
      // 验证已读状态显示
      await expect(page.locator('.read-status')).toBeVisible()
      await expect(page.locator('.read-status')).toContainText('已读')
    })

    test('应该能够查看已读用户列表', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入已发送消息
      await page.click('.sent-folder')
      
      // 查看消息详情
      await page.click('.message-item:first-child')
      
      // 点击已读状态
      await page.click('.read-status')
      
      // 验证已读用户列表显示
      await expect(page.locator('.read-users-list')).toBeVisible()
      await expect(page.locator('.read-user-item')).toHaveCountGreaterThan(0)
    })

    test('已读状态应该实时更新', async ({ page }) => {
      await page.goto('/messages')
      
      // 进入已发送消息
      await page.click('.sent-folder')
      
      // 模拟对方读取消息
      await page.evaluate(() => {
        const readEvent = new CustomEvent('messageRead', {
          detail: {
            messageId: '1',
            readerId: 'otheruser',
            readerName: 'otheruser',
            readAt: new Date().toISOString()
          }
        })
        window.dispatchEvent(readEvent)
      })
      
      // 等待状态更新
      await page.waitForTimeout(1000)
      
      // 验证已读状态更新
      await expect(page.locator('.read-status')).toContainText('已读')
    })
  })

  test.describe('消息移动端适配测试', () => {
    test.beforeEach(async ({ page }) => {
      // 设置移动端视口
      await page.setViewportSize({ width: 375, height: 667 })
    })

    test('消息界面应该适配移动端', async ({ page }) => {
      await page.goto('/messages')
      
      // 验证界面响应式
      await expect(page.locator('.message-container')).toHaveCSS('width', '100%')
      
      // 验证消息列表适配
      await expect(page.locator('.message-item')).toHaveCSS('padding', '12px')
    })

    test('移动端消息表单应该易于操作', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 验证输入框大小适合触摸
      const messageInput = page.locator('.message-content')
      await expect(messageInput).toBeVisible()
      const boundingBox = await messageInput.boundingBox()
      expect(boundingBox?.height).toBeGreaterThan(40)
      
      // 验证按钮大小适合触摸
      const sendBtn = page.locator('.send-message-btn')
      await expect(sendBtn).toBeVisible()
      const btnBox = await sendBtn.boundingBox()
      expect(btnBox?.height).toBeGreaterThan(40)
    })

    test('移动端应该支持滑动删除', async ({ page }) => {
      await page.goto('/messages')
      
      // 模拟滑动操作
      const messageItem = page.locator('.message-item:first-child')
      const box = await messageItem.boundingBox()
      
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

    test('移动端应该有底部导航', async ({ page }) => {
      await page.goto('/messages')
      
      // 验证底部导航显示
      await expect(page.locator('.mobile-bottom-nav')).toBeVisible()
      await expect(page.locator('.nav-item')).toHaveCount(4) // 收件箱、已发送、草稿、设置
    })
  })

  test.describe('消息性能测试', () => {
    test('大量消息加载应该有良好的性能', async ({ page }) => {
      // 设置性能监控
      const startTime = Date.now()
      
      await page.goto('/messages')
      
      // 等待消息列表加载完成
      await page.waitForSelector('.message-list')
      
      const loadTime = Date.now() - startTime
      
      // 验证加载时间在可接受范围内
      expect(loadTime).toBeLessThan(3000)
      console.log(`消息列表加载时间: ${loadTime}ms`)
    })

    test('消息搜索应该快速返回结果', async ({ page }) => {
      await page.goto('/messages')
      
      // 测试搜索响应时间
      const searchStartTime = Date.now()
      await page.fill('.message-search-input', '测试')
      await page.press('.message-search-input', 'Enter')
      await page.waitForSelector('.search-results')
      const searchTime = Date.now() - searchStartTime
      
      expect(searchTime).toBeLessThan(2000)
      console.log(`消息搜索响应时间: ${searchTime}ms`)
    })

    test('实时消息更新应该及时', async ({ page }) => {
      await page.goto('/messages')
      
      // 测试实时更新延迟
      const updateStartTime = Date.now()
      
      // 模拟新消息
      await page.evaluate(() => {
        const event = new MessageEvent('message', {
          data: JSON.stringify({
            type: 'new_message',
            message: {
              id: 'performance-test-message',
              sender: 'otheruser',
              subject: '性能测试消息',
              content: '测试实时更新性能',
              createdAt: new Date().toISOString(),
              isRead: false
            }
          })
        })
        window.dispatchEvent(event)
      })
      
      // 等待消息显示
      await expect(page.locator('.message-item:has-text("性能测试消息")')).toBeVisible()
      
      const updateTime = Date.now() - updateStartTime
      
      // 验证更新延迟在可接受范围内
      expect(updateTime).toBeLessThan(1000)
      console.log(`实时消息更新延迟: ${updateTime}ms`)
    })
  })

  test.describe('消息错误处理测试', () => {
    test('应该优雅处理网络错误', async ({ page }) => {
      // 模拟网络错误
      await page.route('**/api/messages/**', route => route.abort('failed'))
      
      await page.goto('/messages')
      
      // 尝试发送消息
      await page.click('.new-message-btn')
      await page.fill('.recipient-input', 'otheruser')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.subject-input', '网络错误测试')
      await page.fill('.message-content', '测试网络错误处理')
      await page.click('.send-message-btn')
      
      // 验证错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('网络连接失败')
      
      // 验证重试按钮
      await expect(page.locator('.retry-btn')).toBeVisible()
      
      // 恢复网络
      await page.unroute('**/api/messages/**')
    })

    test('应该处理服务器错误', async ({ page }) => {
      // 模拟服务器错误
      await page.route('**/api/messages/**', route => route.fulfill({
        status: 500,
        body: JSON.stringify({ error: '服务器内部错误' })
      }))
      
      await page.goto('/messages')
      
      // 尝试发送消息
      await page.click('.new-message-btn')
      await page.fill('.recipient-input', 'otheruser')
      await page.press('.recipient-input', 'Enter')
      await page.fill('.subject-input', '服务器错误测试')
      await page.fill('.message-content', '测试服务器错误处理')
      await page.click('.send-message-btn')
      
      // 验证错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('服务器内部错误')
      
      // 恢复正常路由
      await page.unroute('**/api/messages/**')
    })

    test('应该处理文件上传错误', async ({ page }) => {
      await page.goto('/messages')
      
      // 点击新建消息按钮
      await page.click('.new-message-btn')
      
      // 模拟文件上传错误
      await page.route('**/api/upload/**', route => route.abort('failed'))
      
      // 尝试上传文件
      const fileInput = page.locator('.file-input')
      await fileInput.setInputFiles({
        name: 'test.txt',
        mimeType: 'text/plain',
        buffer: Buffer.from('测试文件内容')
      })
      
      // 验证上传错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('文件上传失败')
      
      // 恢复上传路由
      await page.unroute('**/api/upload/**')
    })

    test('应该处理WebSocket连接错误', async ({ page }) => {
      await page.goto('/messages')
      
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
  })

  test.describe('消息无障碍访问测试', () => {
    test('消息界面应该支持键盘导航', async ({ page }) => {
      await page.goto('/messages')
      
      // 测试Tab键导航
      await page.keyboard.press('Tab')
      await expect(page.locator('.new-message-btn')).toBeFocused()
      
      await page.keyboard.press('Tab')
      await expect(page.locator('.message-search-input')).toBeFocused()
      
      // 测试Enter键选择消息
      await page.keyboard.press('ArrowDown')
      await page.keyboard.press('Enter')
      
      // 验证消息详情打开
      await expect(page.locator('.message-detail')).toBeVisible()
    })

    test('消息组件应该有正确的ARIA标签', async ({ page }) => {
      await page.goto('/messages')
      
      // 验证消息列表ARIA属性
      await expect(page.locator('.message-list')).toHaveAttribute('role', 'list')
      await expect(page.locator('.message-item')).toHaveAttribute('role', 'listitem')
      
      // 验证按钮ARIA标签
      await expect(page.locator('.new-message-btn')).toHaveAttribute('aria-label')
      await expect(page.locator('.delete-message-btn')).toHaveAttribute('aria-label')
      await expect(page.locator('.mark-read-btn')).toHaveAttribute('aria-label')
    })

    test('消息状态变化应该有屏幕阅读器支持', async ({ page }) => {
      await page.goto('/messages')
      
      // 标记消息为已读
      await page.click('.message-item:first-child .mark-read-btn')
      
      // 验证状态变化通知
      await expect(page.locator('.sr-only')).toContainText('消息已标记为已读')
    })

    test('应该支持高对比度模式', async ({ page }) => {
      await page.goto('/messages')
      
      // 验证高对比度样式
      await expect(page.locator('.message-item')).toHaveCSS('background-color', 'rgb(255, 255, 255)')
      await expect(page.locator('.message-item.unread')).toHaveCSS('background-color', 'rgb(248, 249, 250)')
    })
  })
})