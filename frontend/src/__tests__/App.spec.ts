import { describe, it, expect, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import App from '../App.vue'

// 创建一个简单的路由用于测试
const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: { template: '<div>Home</div>' } }
  ]
})

describe('App', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('应该能够挂载App组件', async () => {
    const wrapper = mount(App, {
      global: {
        plugins: [router]
      }
    })

    expect(wrapper.exists()).toBe(true)
  })
})
