import { vi } from 'vitest'

// Mock Monaco Editor
vi.mock('monaco-editor', () => ({
  editor: {
    create: vi.fn(() => ({
      getValue: vi.fn(() => ''),
      setValue: vi.fn(),
      dispose: vi.fn(),
      onDidChangeModelContent: vi.fn(),
      updateOptions: vi.fn(),
      layout: vi.fn(),
      focus: vi.fn(),
      getModel: vi.fn(() => ({
        getLanguageId: vi.fn(() => 'javascript')
      }))
    })),
    setTheme: vi.fn(),
    defineTheme: vi.fn(),
    getModels: vi.fn(() => []),
    createModel: vi.fn()
  },
  languages: {
    register: vi.fn(),
    setMonarchTokensProvider: vi.fn(),
    setLanguageConfiguration: vi.fn(),
    registerCompletionItemProvider: vi.fn()
  },
  KeyCode: {
    F1: 59
  },
  KeyMod: {
    CtrlCmd: 2048
  }
}))

// Mock Monaco Editor Loader
vi.mock('@monaco-editor/loader', () => ({
  default: {
    init: vi.fn(() => Promise.resolve()),
    config: vi.fn()
  }
}))

// Global test utilities
global.ResizeObserver = vi.fn(() => ({
  observe: vi.fn(),
  unobserve: vi.fn(),
  disconnect: vi.fn(),
}))

// Mock window.matchMedia
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(), // deprecated
    removeListener: vi.fn(), // deprecated
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
})
