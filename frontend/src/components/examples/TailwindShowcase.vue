<template>
  <div class="max-w-4xl mx-auto p-8 space-y-8">
    <div class="text-center">
      <h1 class="text-3xl font-bold text-slate-900 mb-2">Tailwind CSS 组件展示</h1>
      <p class="text-slate-600">展示项目中可用的 Tailwind 工具类和组件样式</p>
    </div>

    <!-- 按钮展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">按钮样式</h2>
      <div class="flex flex-wrap gap-4">
        <button class="btn-primary">主要按钮</button>
        <button class="btn-secondary">次要按钮</button>
        <button class="btn-danger">危险按钮</button>
        <button class="btn-ghost">幽灵按钮</button>
        <button class="btn-primary" disabled>禁用按钮</button>
      </div>
    </section>

    <!-- 输入框展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">输入框样式</h2>
      <div class="space-y-4 max-w-md">
        <input type="text" class="input" placeholder="普通输入框" />
        <input type="text" class="input-error" placeholder="错误状态输入框" />
        <select class="input">
          <option>选择选项</option>
          <option>选项 1</option>
          <option>选项 2</option>
        </select>
      </div>
    </section>

    <!-- 卡片展示 -->
    <section class="space-y-4">
      <h2 class="text-xl font-semibold text-slate-900">卡片样式</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="card p-6">
          <h3 class="font-semibold text-slate-900 mb-2">普通卡片</h3>
          <p class="text-slate-600">这是一个普通的卡片组件</p>
        </div>
        <div class="card-hover p-6">
          <h3 class="font-semibold text-slate-900 mb-2">悬停卡片</h3>
          <p class="text-slate-600">这个卡片有悬停效果</p>
        </div>
        <div class="glass p-6 rounded-xl">
          <h3 class="font-semibold text-slate-900 mb-2">玻璃效果卡片</h3>
          <p class="text-slate-600">这个卡片有玻璃毛玻璃效果</p>
        </div>
      </div>
    </section>

    <!-- 标签展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">标签样式</h2>
      <div class="flex flex-wrap gap-2">
        <span class="tag-default">默认标签</span>
        <span class="tag-primary">主要标签</span>
        <span class="tag-success">成功标签</span>
        <span class="tag-warning">警告标签</span>
        <span class="tag-danger">危险标签</span>
      </div>
    </section>

    <!-- 代码块展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">代码块样式</h2>
      <div class="code-block">
        <pre><code>function greet(name: string): string {
  return `Hello, ${name}!`;
}

const message = greet('World');
console.log(message);</code></pre>
      </div>
    </section>

    <!-- 文本截断展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">文本截断</h2>
      <div class="space-y-4">
        <div>
          <h3 class="font-medium text-slate-900 mb-2">单行截断</h3>
          <p class="line-clamp-1 text-slate-600">
            这是一段很长的文本，用来演示单行截断效果。这段文本会被截断，只显示一行内容，超出的部分会用省略号表示。
          </p>
        </div>
        <div>
          <h3 class="font-medium text-slate-900 mb-2">两行截断</h3>
          <p class="line-clamp-2 text-slate-600">
            这是一段很长的文本，用来演示两行截断效果。这段文本会被截断，只显示两行内容，超出的部分会用省略号表示。这样可以保持界面的整洁和一致性。
          </p>
        </div>
        <div>
          <h3 class="font-medium text-slate-900 mb-2">三行截断</h3>
          <p class="line-clamp-3 text-slate-600">
            这是一段很长的文本，用来演示三行截断效果。这段文本会被截断，只显示三行内容，超出的部分会用省略号表示。这样可以保持界面的整洁和一致性，同时给用户提供足够的信息预览。
          </p>
        </div>
      </div>
    </section>

    <!-- 图标大小展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">图标大小</h2>
      <div class="flex items-center gap-6">
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-xs text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-xs (12px)</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-sm text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-sm (16px)</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-md text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-md (20px)</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-lg text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-lg (24px)</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-xl text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-xl (32px)</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-2xl text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-2xl (48px)</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <svg class="icon-3xl text-slate-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/>
          </svg>
          <span class="text-xs text-slate-500">icon-3xl (64px)</span>
        </div>
      </div>
    </section>

    <!-- 动画展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">动画效果</h2>
      <div class="space-y-4">
        <div class="animate-fade-in p-4 bg-blue-50 rounded-lg">
          <p class="text-blue-800">淡入动画效果</p>
        </div>
        <div class="animate-slide-up p-4 bg-green-50 rounded-lg">
          <p class="text-green-800">滑入动画效果</p>
        </div>
        <div class="animate-pulse-slow p-4 bg-purple-50 rounded-lg">
          <p class="text-purple-800">慢速脉冲动画</p>
        </div>
      </div>
    </section>

    <!-- 滚动条展示 -->
    <section class="card p-6">
      <h2 class="text-xl font-semibold text-slate-900 mb-4">滚动条样式</h2>
      <div class="h-32 overflow-y-auto scrollbar-thin bg-slate-50 rounded-lg p-4">
        <p class="text-slate-600 mb-2">这是一个带有自定义滚动条的容器。</p>
        <p class="text-slate-600 mb-2">滚动条使用了 scrollbar-thin 类。</p>
        <p class="text-slate-600 mb-2">内容足够长时会显示滚动条。</p>
        <p class="text-slate-600 mb-2">滚动条样式更加美观。</p>
        <p class="text-slate-600 mb-2">这是更多的内容...</p>
        <p class="text-slate-600 mb-2">继续添加内容...</p>
        <p class="text-slate-600 mb-2">更多内容...</p>
        <p class="text-slate-600">最后一行内容</p>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
// 这个组件纯粹用于展示样式，不需要逻辑
</script>

<style scoped>
/* 组件特定样式 */
</style>
