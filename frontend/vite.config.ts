import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'
import monacoEditorPlugin from './src/utils/vite-monaco-plugin'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    vue(),
    vueJsx(),
    vueDevTools(),
    monacoEditorPlugin({
      enableWorker: true,
      publicPath: '/monaco-editor',
      enableCodeSplitting: true,
      enableLanguagePackCompression: true,
      languages: [
        'javascript',
        'typescript',
        'python',
        'java',
        'csharp',
        'html',
        'css',
        'json',
        'sql',
        'markdown',
        'shell',
        'xml',
        'yaml'
      ]
    }),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    port: 6677,
    strictPort: true,
    host: true
  },
  build: {
    // 代码分割优化
    rollupOptions: {
      output: {
        // 手动分割代码块
        manualChunks: {
          // Vue 核心库
          'vue-vendor': ['vue', 'vue-router', 'pinia'],
          // UI 组件库
          'ui-vendor': ['@monaco-editor/loader', 'monaco-editor'],
          // 工具库
          'utils-vendor': ['axios'],
          // 编辑器相关
          'editor': [
            './src/components/editor/CodeEditor.vue',
            './src/components/editor/CodeViewer.vue',
            './src/components/editor/EditorSettings.vue'
          ]
        },
        // 优化文件名
        chunkFileNames: 'js/[name]-[hash].js',
        entryFileNames: 'js/[name]-[hash].js',
        assetFileNames: (assetInfo) => {
          const info = assetInfo.name?.split('.') || []
          const ext = info[info.length - 1]
          if (/\.(png|jpe?g|gif|svg|webp|ico)$/i.test(assetInfo.name || '')) {
            return `images/[name]-[hash].${ext}`
          }
          if (/\.(css)$/i.test(assetInfo.name || '')) {
            return `css/[name]-[hash].${ext}`
          }
          return `assets/[name]-[hash].${ext}`
        }
      }
    },
    // 压缩配置
    minify: 'esbuild',
    // 启用 gzip 压缩
    reportCompressedSize: true,
    // 设置 chunk 大小警告限制
    chunkSizeWarningLimit: 1000
  },
  // 开发服务器优化
  optimizeDeps: {
    include: [
      'vue',
      'vue-router',
      'pinia',
      'axios'
    ]
  }
})
