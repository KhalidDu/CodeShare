import axios from 'axios'
import type { User, PaginatedResult } from '@/types'

export interface CreateUserRequest {
  username: string
  email: string
  password: string
  role: number
}

export interface UpdateUserRequest {
  username?: string
  email?: string
  role?: number
  isActive?: boolean
}

export interface ResetPasswordRequest {
  newPassword: string
}

/**
 * 用户管理服务 - 提供用户CRUD操作的API接口
 */
class UserService {
  private readonly baseURL = '/api/users'

  /**
   * 获取所有用户列表 (仅管理员)
   */
  async getAllUsers(): Promise<User[]> {
    const response = await axios.get<User[]>(this.baseURL)
    return response.data
  }

  /**
   * 根据ID获取用户详情
   */
  async getUserById(id: string): Promise<User> {
    const response = await axios.get<User>(`${this.baseURL}/${id}`)
    return response.data
  }

  /**
   * 创建新用户 (仅管理员)
   */
  async createUser(userData: CreateUserRequest): Promise<User> {
    const response = await axios.post<User>(this.baseURL, userData)
    return response.data
  }

  /**
   * 更新用户信息
   */
  async updateUser(id: string, userData: UpdateUserRequest): Promise<User> {
    const response = await axios.put<User>(`${this.baseURL}/${id}`, userData)
    return response.data
  }

  /**
   * 删除用户 (仅管理员)
   */
  async deleteUser(id: string): Promise<void> {
    await axios.delete(`${this.baseURL}/${id}`)
  }

  /**
   * 切换用户状态 (启用/禁用) (仅管理员)
   */
  async toggleUserStatus(id: string, isActive: boolean): Promise<User> {
    const response = await axios.patch<User>(`${this.baseURL}/${id}/status`, isActive)
    return response.data
  }

  /**
   * 重置用户密码 (仅管理员)
   */
  async resetPassword(id: string, newPassword: string): Promise<void> {
    await axios.post(`${this.baseURL}/${id}/reset-password`, { newPassword })
  }
}

export const userService = new UserService()