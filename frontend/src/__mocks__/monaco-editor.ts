import { vi } from 'vitest'

// Monaco Editor Mock for testing
export const editor = {
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
}

export const languages = {
  register: vi.fn(),
  setMonarchTokensProvider: vi.fn(),
  setLanguageConfiguration: vi.fn(),
  registerCompletionItemProvider: vi.fn()
}

export const KeyCode = {
  F1: 59
}

export const KeyMod = {
  CtrlCmd: 2048
}

export default {
  editor,
  languages,
  KeyCode,
  KeyMod
}
