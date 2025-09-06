# CodeSnippet 开发环境启动脚本

本文件夹包含了 CodeSnippet 项目的开发环境一键启动脚本，支持 Windows、Linux、macOS 和 PowerShell。

## 📁 文件说明

### 启动脚本
- `start-dev.bat` - Windows 批处理启动脚本
- `start-dev.sh` - Linux/macOS Shell 启动脚本  
- `start-dev.ps1` - PowerShell 启动脚本

### 停止脚本
- `stop-dev.sh` - Linux/macOS Shell 停止脚本
- `stop-dev.ps1` - PowerShell 停止脚本

## 🚀 使用方法

### Windows 系统

#### 方法1: 批处理脚本 (推荐)
```bash
# 在项目根目录运行
.\scripts\start-dev.bat
```

#### 方法2: PowerShell 脚本
```powershell
# 在项目根目录运行
.\scripts\start-dev.ps1
```

### Linux/macOS 系统

```bash
# 在项目根目录运行
chmod +x ./scripts/start-dev.sh
./scripts/start-dev.sh
```

### 停止服务

#### Linux/macOS
```bash
./scripts/stop-dev.sh
```

#### PowerShell
```powershell
.\scripts\stop-dev.ps1
```

## 🔧 功能特性

### 启动脚本功能
- ✅ 自动检查项目目录结构
- ✅ 验证系统环境 (.NET, Node.js, npm)
- ✅ 检查端口占用情况 (6676, 6677)
- ✅ 自动启动后端服务 (端口6676)
- ✅ 自动启动前端服务 (端口6677)
- ✅ 服务健康检查
- ✅ 彩色输出提示信息
- ✅ 显示服务地址和快捷链接

### 停止脚本功能
- ✅ 停止所有相关进程
- ✅ 清理端口占用
- ✅ 删除临时文件
- ✅ 显示清理状态

## 🌐 服务地址

启动成功后，可以通过以下地址访问：

| 服务 | 地址 | 说明 |
|------|------|------|
| 后端API | http://localhost:6676 | REST API服务 |
| 前端界面 | http://localhost:6677 | Vue.js前端应用 |
| API文档 | http://localhost:6676/swagger | Swagger API文档 |

## 📋 系统要求

### 必需软件
- .NET 8.0 SDK 或更高版本
- Node.js 16+ 
- npm 8+

### 端口要求
- 6676 - 后端API服务端口
- 6677 - 前端应用端口

## 🔍 故障排除

### 端口被占用
如果遇到端口被占用的错误：
1. 查看占用端口的进程：
   ```bash
   # Windows
   netstat -ano | findstr :6676
   netstat -ano | findstr :6677
   
   # Linux/macOS
   lsof -i :6676
   lsof -i :6677
   ```
2. 关闭占用端口的程序，或修改配置文件中的端口设置

### 服务启动失败
1. 检查日志文件：
   ```bash
   # Linux/macOS
   tail -f logs/backend.log
   tail -f logs/frontend.log
   ```
2. 手动启动测试：
   ```bash
   # 后端
   cd backend
   dotnet run
   
   # 前端
   cd frontend
   npm run dev
   ```

### 权限问题 (Linux/macOS)
如果遇到权限错误：
```bash
chmod +x ./scripts/*.sh
```

## 📝 日志文件

Linux/macOS 版本的脚本会在项目根目录创建：
- `logs/backend.log` - 后端服务日志
- `logs/frontend.log` - 前端服务日志

## 💡 开发技巧

### 查看实时日志
```bash
# Linux/macOS
tail -f logs/backend.log
tail -f logs/frontend.log

# PowerShell
Get-Content logs/backend.log -Wait
Get-Content logs/frontend.log -Wait
```

### 重启服务
```bash
# 先停止所有服务
./scripts/stop-dev.sh

# 再重新启动
./scripts/start-dev.sh
```

### 手动管理进程
```bash
# Linux/macOS
# 查看进程
ps aux | grep dotnet
ps aux | grep node

# 停止进程
kill <PID>

# PowerShell
# 查看任务
Get-Job

# 查看任务输出
Receive-Job -Id <JobId>

# 停止任务
Stop-Job -Id <JobId>
```

## 🎯 最佳实践

1. **开发前**：运行启动脚本前确保已安装所有必需软件
2. **开发中**：使用日志文件监控服务状态
3. **完成后**：运行停止脚本释放系统资源
4. **问题排查**：查看日志文件定位具体错误

## 🔗 相关链接

- [.NET 下载](https://dotnet.microsoft.com/download)
- [Node.js 下载](https://nodejs.org/download/)
- [Vue.js 文档](https://vuejs.org/)
- [.NET API 文档](https://learn.microsoft.com/en-us/dotnet/)

---

🎉 **祝您开发愉快！**