import { test, expect } from '@playwright/test'

test.describe('评论功能端到端测试', () => {
  test.beforeEach(async ({ page }) => {
    // 用户登录
    await page.goto('/login')
    await page.fill('[data-test="username"]', 'testuser')
    await page.fill('[data-test="password"]', 'password123')
    await page.click('[data-test="login-btn"]')
    await page.waitForURL('/dashboard')
  })

  test.describe('评论列表页面加载测试', () => {
    test('应该能够加载评论列表页面', async ({ page }) => {
      // 导航到代码片段详情页面
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-list')
      
      // 验证评论列表存在
      await expect(page.locator('.comment-list')).toBeVisible()
      
      // 验证评论统计信息
      await expect(page.locator('.comment-title')).toBeVisible()
      await expect(page.locator('.comment-count')).toBeVisible()
    })

    test('应该能够显示评论统计信息', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-stats')
      
      // 验证统计信息显示
      await expect(page.locator('.comment-meta')).toBeVisible()
      await expect(page.locator('.meta-item')).toHaveCount(3)
    })

    test('评论列表应该在网络错误时显示错误信息', async ({ page }) => {
      // 模拟网络错误
      await page.route('**/api/comments/**', route => route.abort('failed'))
      
      await page.goto('/snippets/1')
      
      // 等待错误信息显示
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('加载评论失败')
      
      // 恢复网络
      await page.unroute('**/api/comments/**')
    })
  })

  test.describe('评论创建功能测试', () => {
    test('应该能够创建新评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论表单加载
      await page.waitForSelector('.comment-form')
      
      // 填写评论内容
      await page.fill('.comment-input', '这是一个测试评论')
      
      // 提交评论
      await page.click('.submit-comment-btn')
      
      // 等待评论创建成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('评论发布成功')
      
      // 验证新评论出现在列表中
      await expect(page.locator('.comment-item')).toContainText('这是一个测试评论')
    })

    test('应该能够创建富文本评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待富文本编辑器加载
      await page.waitForSelector('.rich-editor')
      
      // 在富文本编辑器中输入内容
      await page.fill('.rich-editor textarea', '**粗体文本** 和 *斜体文本*')
      
      // 提交评论
      await page.click('.submit-comment-btn')
      
      // 验证富文本评论创建成功
      await expect(page.locator('.comment-item .comment-content')).toContainText('粗体文本')
      await expect(page.locator('.comment-item .comment-content')).toContainText('斜体文本')
    })

    test('评论表单验证应该正常工作', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 尝试提交空评论
      await page.click('.submit-comment-btn')
      
      // 验证错误提示
      await expect(page.locator('.validation-error')).toBeVisible()
      await expect(page.locator('.validation-error')).toContainText('评论内容不能为空')
    })

    test('应该能够添加评论标签', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 填写评论内容
      await page.fill('.comment-input', '带标签的评论')
      
      // 添加标签
      await page.click('.add-tag-btn')
      await page.fill('.tag-input', 'bug')
      await page.press('.tag-input', 'Enter')
      
      // 提交评论
      await page.click('.submit-comment-btn')
      
      // 验证标签显示
      await expect(page.locator('.comment-tags')).toContainText('bug')
    })
  })

  test.describe('评论编辑功能测试', () => {
    test('应该能够编辑自己的评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-list')
      
      // 找到自己的评论并点击编辑
      await page.click('.comment-item:has-text("测试评论") .edit-comment-btn')
      
      // 等待编辑表单出现
      await page.waitForSelector('.edit-comment-form')
      
      // 修改评论内容
      await page.fill('.edit-comment-input', '修改后的评论内容')
      
      // 保存修改
      await page.click('.save-edit-btn')
      
      // 验证修改成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.comment-item')).toContainText('修改后的评论内容')
    })

    test('应该不能编辑他人的评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-list')
      
      // 尝试找到他人的评论
      const otherComment = page.locator('.comment-item:has-text("其他用户的评论") .edit-comment-btn')
      
      // 验证编辑按钮不存在或不可用
      await expect(otherComment).toHaveCount(0)
    })

    test('编辑表单验证应该正常工作', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 找到自己的评论并点击编辑
      await page.click('.comment-item:has-text("测试评论") .edit-comment-btn')
      
      // 清空评论内容
      await page.fill('.edit-comment-input', '')
      
      // 尝试保存
      await page.click('.save-edit-btn')
      
      // 验证错误提示
      await expect(page.locator('.validation-error')).toBeVisible()
      await expect(page.locator('.validation-error')).toContainText('评论内容不能为空')
    })

    test('应该能够取消编辑', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 找到自己的评论并点击编辑
      await page.click('.comment-item:has-text("测试评论") .edit-comment-btn')
      
      // 修改内容但不保存
      await page.fill('.edit-comment-input', '临时修改的内容')
      
      // 取消编辑
      await page.click('.cancel-edit-btn')
      
      // 验证内容恢复原状
      await expect(page.locator('.comment-item')).toContainText('测试评论')
      await expect(page.locator('.comment-item')).not.toContainText('临时修改的内容')
    })
  })

  test.describe('评论删除功能测试', () => {
    test('应该能够删除自己的评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-list')
      
      // 获取初始评论数量
      const initialCount = await page.locator('.comment-item').count()
      
      // 找到自己的评论并点击删除
      await page.click('.comment-item:has-text("测试评论") .delete-comment-btn')
      
      // 确认删除
      await page.click('.confirm-delete-btn')
      
      // 等待删除完成
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('评论已删除')
      
      // 验证评论数量减少
      await expect(page.locator('.comment-item')).toHaveCount(initialCount - 1)
    })

    test('删除评论时应该显示确认对话框', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 点击删除按钮
      await page.click('.comment-item:has-text("测试评论") .delete-comment-btn')
      
      // 验证确认对话框显示
      await expect(page.locator('.confirm-dialog')).toBeVisible()
      await expect(page.locator('.confirm-dialog')).toContainText('确定要删除这条评论吗？')
      
      // 取消删除
      await page.click('.cancel-delete-btn')
      
      // 验证评论仍然存在
      await expect(page.locator('.comment-item:has-text("测试评论")')).toBeVisible()
    })

    test('应该不能删除他人的评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 尝试找到他人的评论
      const otherComment = page.locator('.comment-item:has-text("其他用户的评论") .delete-comment-btn')
      
      // 验证删除按钮不存在或不可用
      await expect(otherComment).toHaveCount(0)
    })
  })

  test.describe('评论点赞功能测试', () => {
    test('应该能够点赞评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-list')
      
      // 找到评论并点赞
      await page.click('.comment-item:first-child .like-comment-btn')
      
      // 验证点赞状态
      await expect(page.locator('.comment-item:first-child .like-comment-btn')).toHaveClass(/active/)
      
      // 验证点赞数量增加
      const likeCount = await page.locator('.comment-item:first-child .like-count').textContent()
      expect(parseInt(likeCount || '0')).toBeGreaterThan(0)
    })

    test('应该能够取消点赞', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 先点赞
      await page.click('.comment-item:first-child .like-comment-btn')
      
      // 再次点击取消点赞
      await page.click('.comment-item:first-child .like-comment-btn')
      
      // 验证点赞状态取消
      await expect(page.locator('.comment-item:first-child .like-comment-btn')).not.toHaveClass(/active/)
    })

    test('点赞按钮应该显示实时数量', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 获取初始点赞数
      const initialCount = await page.locator('.comment-item:first-child .like-count').textContent()
      
      // 点赞
      await page.click('.comment-item:first-child .like-comment-btn')
      
      // 等待数量更新
      await page.waitForTimeout(1000)
      
      // 获取更新后的点赞数
      const updatedCount = await page.locator('.comment-item:first-child .like-count').textContent()
      
      // 验证数量变化
      expect(parseInt(updatedCount || '0')).toBe(parseInt(initialCount || '0') + 1)
    })
  })

  test.describe('评论回复功能测试', () => {
    test('应该能够回复评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论列表加载
      await page.waitForSelector('.comment-list')
      
      // 点击回复按钮
      await page.click('.comment-item:first-child .reply-comment-btn')
      
      // 等待回复表单出现
      await page.waitForSelector('.reply-form')
      
      // 填写回复内容
      await page.fill('.reply-input', '这是一个回复')
      
      // 提交回复
      await page.click('.submit-reply-btn')
      
      // 验证回复成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.reply-item')).toContainText('这是一个回复')
    })

    test('回复应该显示在原评论下方', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 点击回复按钮
      await page.click('.comment-item:first-child .reply-comment-btn')
      
      // 填写并提交回复
      await page.fill('.reply-input', '测试回复位置')
      await page.click('.submit-reply-btn')
      
      // 验证回复在正确的位置
      const parentComment = page.locator('.comment-item:first-child')
      const reply = parentComment.locator('.reply-item:has-text("测试回复位置")')
      await expect(reply).toBeVisible()
    })

    test('应该能够嵌套回复', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 回复主评论
      await page.click('.comment-item:first-child .reply-comment-btn')
      await page.fill('.reply-input', '第一层回复')
      await page.click('.submit-reply-btn')
      
      // 回复回复
      await page.click('.reply-item:has-text("第一层回复") .reply-comment-btn')
      await page.fill('.reply-input', '第二层回复')
      await page.click('.submit-reply-btn')
      
      // 验证嵌套回复
      await expect(page.locator('.reply-item:has-text("第二层回复")')).toBeVisible()
    })
  })

  test.describe('评论举报功能测试', () => {
    test('应该能够举报评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 点击举报按钮
      await page.click('.comment-item:first-child .report-comment-btn')
      
      // 等待举报对话框
      await page.waitForSelector('.report-dialog')
      
      // 选择举报原因
      await page.selectOption('.report-reason', 'spam')
      
      // 填写举报说明
      await page.fill('.report-description', '这条评论包含垃圾信息')
      
      // 提交举报
      await page.click('.submit-report-btn')
      
      // 验证举报成功
      await expect(page.locator('.success-message')).toBeVisible()
      await expect(page.locator('.success-message')).toContainText('举报已提交')
    })

    test('举报表单验证应该正常工作', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 点击举报按钮
      await page.click('.comment-item:first-child .report-comment-btn')
      
      // 不选择原因直接提交
      await page.click('.submit-report-btn')
      
      // 验证错误提示
      await expect(page.locator('.validation-error')).toBeVisible()
      await expect(page.locator('.validation-error')).toContainText('请选择举报原因')
    })

    test('应该能够取消举报', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 点击举报按钮
      await page.click('.comment-item:first-child .report-comment-btn')
      
      // 取消举报
      await page.click('.cancel-report-btn')
      
      // 验证对话框关闭
      await expect(page.locator('.report-dialog')).not.toBeVisible()
    })
  })

  test.describe('评论搜索功能测试', () => {
    test('应该能够搜索评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待搜索框加载
      await page.waitForSelector('.comment-search-input')
      
      // 输入搜索关键词
      await page.fill('.comment-search-input', '测试')
      
      // 执行搜索
      await page.press('.comment-search-input', 'Enter')
      
      // 等待搜索结果
      await page.waitForSelector('.search-results')
      
      // 验证搜索结果
      await expect(page.locator('.search-results')).toBeVisible()
      await expect(page.locator('.comment-item:has-text("测试")')).toHaveCountGreaterThan(0)
    })

    test('搜索无结果时应该显示提示', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 输入不存在的关键词
      await page.fill('.comment-search-input', '不存在的评论内容')
      await page.press('.comment-search-input', 'Enter')
      
      // 验证无结果提示
      await expect(page.locator('.no-results')).toBeVisible()
      await expect(page.locator('.no-results')).toContainText('没有找到相关评论')
    })

    test('应该能够清除搜索', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 执行搜索
      await page.fill('.comment-search-input', '测试')
      await page.press('.comment-search-input', 'Enter')
      
      // 清除搜索
      await page.click('.clear-search-btn')
      
      // 验证搜索框清空
      await expect(page.locator('.comment-search-input')).toHaveValue('')
      
      // 验证显示所有评论
      await expect(page.locator('.comment-item')).toHaveCountGreaterThan(0)
    })
  })

  test.describe('评论筛选功能测试', () => {
    test('应该能够按时间排序评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待排序选项加载
      await page.waitForSelector('.comment-sort-select')
      
      // 选择最新发布
      await page.selectOption('.comment-sort-select', 'created_at_desc')
      
      // 等待排序结果
      await page.waitForTimeout(1000)
      
      // 验证排序应用
      await expect(page.locator('.comment-sort-select')).toHaveValue('created_at_desc')
    })

    test('应该能够按点赞数排序评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 选择最多点赞
      await page.selectOption('.comment-sort-select', 'like_count_desc')
      
      // 等待排序结果
      await page.waitForTimeout(1000)
      
      // 验证排序应用
      await expect(page.locator('.comment-sort-select')).toHaveValue('like_count_desc')
    })

    test('应该能够按回复数排序评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 选择最多回复
      await page.selectOption('.comment-sort-select', 'reply_count_desc')
      
      // 等待排序结果
      await page.waitForTimeout(1000)
      
      // 验证排序应用
      await expect(page.locator('.comment-sort-select')).toHaveValue('reply_count_desc')
    })
  })

  test.describe('评论分页功能测试', () => {
    test('应该能够分页加载评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待分页组件加载
      await page.waitForSelector('.comment-pagination')
      
      // 验证分页控件存在
      await expect(page.locator('.pagination-btn')).toHaveCountGreaterThan(0)
      
      // 点击下一页
      await page.click('.next-page-btn:enabled')
      
      // 等待页面加载
      await page.waitForTimeout(1000)
      
      // 验证页面变化
      await expect(page.locator('.current-page')).toContainText('2')
    })

    test('应该能够直接跳转到指定页面', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 输入页码
      await page.fill('.page-input', '2')
      
      // 跳转
      await page.click('.go-to-page-btn')
      
      // 验证跳转成功
      await expect(page.locator('.current-page')).toContainText('2')
    })

    test('分页按钮状态应该正确', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 验证第一页时上一页按钮禁用
      await expect(page.locator('.prev-page-btn')).toBeDisabled()
      
      // 验证最后一页时下一页按钮禁用（如果有最后一页）
      const nextPageBtn = page.locator('.next-page-btn')
      if (await nextPageBtn.isVisible()) {
        await expect(nextPageBtn).toBeEnabled()
      }
    })
  })

  test.describe('评论实时更新测试', () => {
    test('新评论应该实时显示', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 获取初始评论数量
      const initialCount = await page.locator('.comment-item').count()
      
      // 模拟新评论到达（通过WebSocket或轮询）
      await page.evaluate(() => {
        // 模拟WebSocket消息
        const event = new MessageEvent('message', {
          data: JSON.stringify({
            type: 'new_comment',
            comment: {
              id: 'new-comment-1',
              content: '实时评论测试',
              author: 'otheruser',
              createdAt: new Date().toISOString()
            }
          })
        })
        window.dispatchEvent(event)
      })
      
      // 等待实时更新
      await page.waitForTimeout(2000)
      
      // 验证新评论显示
      await expect(page.locator('.comment-item')).toHaveCount(initialCount + 1)
      await expect(page.locator('.comment-item')).toContainText('实时评论测试')
    })

    test('评论点赞状态应该实时更新', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 获取初始点赞数
      const initialCount = await page.locator('.comment-item:first-child .like-count').textContent()
      
      // 模拟点赞更新
      await page.evaluate(() => {
        const event = new MessageEvent('message', {
          data: JSON.stringify({
            type: 'comment_liked',
            commentId: '1',
            likeCount: parseInt(initialCount || '0') + 1,
            liked: true
          })
        })
        window.dispatchEvent(event)
      })
      
      // 等待更新
      await page.waitForTimeout(1000)
      
      // 验证点赞数更新
      const updatedCount = await page.locator('.comment-item:first-child .like-count').textContent()
      expect(parseInt(updatedCount || '0')).toBe(parseInt(initialCount || '0') + 1)
    })
  })

  test.describe('评论权限验证测试', () => {
    test('未登录用户应该只能查看评论', async ({ page }) => {
      // 登出
      await page.click('[data-test="user-menu"]')
      await page.click('[data-test="logout"]')
      
      await page.goto('/snippets/1')
      
      // 验证能查看评论
      await expect(page.locator('.comment-list')).toBeVisible()
      
      // 验证不能创建评论
      await expect(page.locator('.comment-form')).not.toBeVisible()
      
      // 验证显示登录提示
      await expect(page.locator('.login-prompt')).toBeVisible()
    })

    test('被禁言用户应该不能发表评论', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 等待评论表单
      await page.waitForSelector('.comment-form')
      
      // 验证评论表单禁用
      await expect(page.locator('.comment-input')).toBeDisabled()
      await expect(page.locator('.submit-comment-btn')).toBeDisabled()
      
      // 验证禁言提示
      await expect(page.locator('.banned-message')).toBeVisible()
    })

    test('管理员应该能够删除任何评论', async ({ page }) => {
      // 切换到管理员账号
      await page.click('[data-test="user-menu"]')
      await page.click('[data-test="logout"]')
      
      await page.goto('/login')
      await page.fill('[data-test="username"]', 'admin')
      await page.fill('[data-test="password"]', 'admin123')
      await page.click('[data-test="login-btn"]')
      
      await page.goto('/snippets/1')
      
      // 验证管理员能看到所有评论的删除按钮
      const deleteButtons = page.locator('.delete-comment-btn')
      await expect(deleteButtons).toHaveCountGreaterThan(0)
    })
  })

  test.describe('评论移动端适配测试', () => {
    test.beforeEach(async ({ page }) => {
      // 设置移动端视口
      await page.setViewportSize({ width: 375, height: 667 })
    })

    test('评论界面应该适配移动端', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 验证评论列表容器响应式
      await expect(page.locator('.comment-list')).toHaveCSS('width', '100%')
      
      // 验证评论项目响应式
      await expect(page.locator('.comment-item')).toHaveCSS('padding', '16px')
    })

    test('移动端评论表单应该易于操作', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 验证输入框大小适合触摸
      const commentInput = page.locator('.comment-input')
      await expect(commentInput).toBeVisible()
      const boundingBox = await commentInput.boundingBox()
      expect(boundingBox?.height).toBeGreaterThan(40)
      
      // 验证按钮大小适合触摸
      const submitBtn = page.locator('.submit-comment-btn')
      await expect(submitBtn).toBeVisible()
      const btnBox = await submitBtn.boundingBox()
      expect(btnBox?.height).toBeGreaterThan(40)
    })

    test('移动端应该有滑动删除功能', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 模拟滑动操作
      const commentItem = page.locator('.comment-item:first-child')
      const box = await commentItem.boundingBox()
      
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
  })

  test.describe('评论性能测试', () => {
    test('大量评论加载应该有良好的性能', async ({ page }) => {
      // 设置性能监控
      const startTime = Date.now()
      
      await page.goto('/snippets/1')
      
      // 等待评论列表加载完成
      await page.waitForSelector('.comment-list')
      
      const loadTime = Date.now() - startTime
      
      // 验证加载时间在可接受范围内
      expect(loadTime).toBeLessThan(3000)
      console.log(`评论列表加载时间: ${loadTime}ms`)
    })

    test('评论操作应该响应迅速', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 测试点赞响应时间
      const likeStartTime = Date.now()
      await page.click('.comment-item:first-child .like-comment-btn')
      await expect(page.locator('.comment-item:first-child .like-comment-btn')).toHaveClass(/active/)
      const likeTime = Date.now() - likeStartTime
      
      expect(likeTime).toBeLessThan(1000)
      console.log(`点赞响应时间: ${likeTime}ms`)
    })

    test('评论搜索应该快速返回结果', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 测试搜索响应时间
      const searchStartTime = Date.now()
      await page.fill('.comment-search-input', '测试')
      await page.press('.comment-search-input', 'Enter')
      await page.waitForSelector('.search-results')
      const searchTime = Date.now() - searchStartTime
      
      expect(searchTime).toBeLessThan(2000)
      console.log(`搜索响应时间: ${searchTime}ms`)
    })
  })

  test.describe('评论无障碍访问测试', () => {
    test('评论界面应该支持键盘导航', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 测试Tab键导航
      await page.keyboard.press('Tab')
      await expect(page.locator('.comment-input')).toBeFocused()
      
      await page.keyboard.press('Tab')
      await expect(page.locator('.submit-comment-btn')).toBeFocused()
      
      // 测试Enter键提交
      await page.fill('.comment-input', '键盘导航测试')
      await page.keyboard.press('Enter')
      
      // 验证提交成功
      await expect(page.locator('.success-message')).toBeVisible()
    })

    test('评论组件应该有正确的ARIA标签', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 验证评论列表ARIA属性
      await expect(page.locator('.comment-list')).toHaveAttribute('role', 'list')
      await expect(page.locator('.comment-item')).toHaveAttribute('role', 'listitem')
      
      // 验证按钮ARIA标签
      await expect(page.locator('.like-comment-btn')).toHaveAttribute('aria-label')
      await expect(page.locator('.reply-comment-btn')).toHaveAttribute('aria-label')
      await expect(page.locator('.delete-comment-btn')).toHaveAttribute('aria-label')
    })

    test('评论应该支持屏幕阅读器', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 验证实时更新通知
      await expect(page.locator('.sr-only')).toHaveCountGreaterThan(0)
      
      // 验证状态变化通知
      await page.click('.comment-item:first-child .like-comment-btn')
      await expect(page.locator('.sr-only')).toContainText('已点赞')
    })
  })

  test.describe('评论错误处理测试', () => {
    test('应该优雅处理网络错误', async ({ page }) => {
      // 模拟网络错误
      await page.route('**/api/comments/**', route => route.abort('failed'))
      
      await page.goto('/snippets/1')
      
      // 尝试发表评论
      await page.fill('.comment-input', '网络错误测试')
      await page.click('.submit-comment-btn')
      
      // 验证错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('网络连接失败')
      
      // 验证重试按钮
      await expect(page.locator('.retry-btn')).toBeVisible()
      
      // 恢复网络
      await page.unroute('**/api/comments/**')
    })

    test('应该处理服务器错误', async ({ page }) => {
      // 模拟服务器错误
      await page.route('**/api/comments/**', route => route.fulfill({
        status: 500,
        body: JSON.stringify({ error: '服务器内部错误' })
      }))
      
      await page.goto('/snippets/1')
      
      // 尝试发表评论
      await page.fill('.comment-input', '服务器错误测试')
      await page.click('.submit-comment-btn')
      
      // 验证错误处理
      await expect(page.locator('.error-message')).toBeVisible()
      await expect(page.locator('.error-message')).toContainText('服务器内部错误')
      
      // 恢复正常路由
      await page.unroute('**/api/comments/**')
    })

    test('应该处理并发操作冲突', async ({ page }) => {
      await page.goto('/snippets/1')
      
      // 模拟并发删除操作
      const deletePromises = [
        page.click('.comment-item:first-child .delete-comment-btn'),
        page.click('.confirm-delete-btn')
      ]
      
      // 等待操作完成
      await Promise.all(deletePromises)
      
      // 验证系统能够正确处理
      await expect(page.locator('.error-message')).not.toBeVisible()
    })
  })
})