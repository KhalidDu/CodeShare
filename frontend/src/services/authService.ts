import api from './api'
import type { LoginRequest, RegisterRequest, AuthResponse, User } from '@/types'

export class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', credentials)

    // 保存认证信息到本地存储
    localStorage.setItem('token', response.data.token)
    localStorage.setItem('user', JSON.stringify(response.data.user))

    return response.data
  }

  async register(userData: RegisterRequest): Promise<User> {
    const response = await api.post<User>('/auth/register', userData)
    return response.data
  }

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout')
    } finally {
      // 清除本地存储
      localStorage.removeItem('token')
      localStorage.removeItem('user')
    }
  }

  getCurrentUser(): User | null {
    const userStr = localStorage.getItem('user')
    return userStr ? JSON.parse(userStr) : null
  }

  getToken(): string | null {
    return localStorage.getItem('token')
  }

  isAuthenticated(): boolean {
    return !!this.getToken()
  }

  async changePassword(data: { currentPassword: string; newPassword: string }): Promise<void> {
    await api.post('/auth/change-password', data)
  }
}

export const authService = new AuthService()
