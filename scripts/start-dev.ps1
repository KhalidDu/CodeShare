# CodeSnippet 开发环境一键启动脚本 (PowerShell版本)
# 使用方法: .\scripts\start-dev.ps1

# 设置控制台编码为UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# 颜色输出函数
function Write-ColoredOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    $originalColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $originalColor
}

# 输出标题
Write-ColoredOutput "========================================" "Cyan"
Write-ColoredOutput "    CodeSnippet 开发环境一键启动脚本" "Cyan"
Write-ColoredOutput "========================================" "Cyan"
Write-Output ""

# 检查是否在正确的目录
$projectRoot = Get-Location
if (-not (Test-Path "$projectRoot\frontend") -or -not (Test-Path "$projectRoot\backend")) {
    # 检查是否在scripts文件夹内
    if ((Split-Path (Get-Location) -Leaf) -eq "scripts") {
        Set-Location ".."
        $projectRoot = Get-Location
        if (-not (Test-Path "$projectRoot\frontend") -or -not (Test-Path "$projectRoot\backend")) {
            Write-ColoredOutput "错误: 请在项目根目录运行此脚本" "Red"
            Write-ColoredOutput "    当前目录: $(Get-Location)" "Red"
            Read-Host "按任意键退出"
            exit 1
        }
        Write-ColoredOutput "自动切换到项目根目录: $projectRoot" "Yellow"
    } else {
        Write-ColoredOutput "错误: 请在项目根目录运行此脚本" "Red"
        Write-ColoredOutput "    当前目录: $(Get-Location)" "Red"
        Read-Host "按任意键退出"
        exit 1
    }
}

# 检查必要的命令是否存在
function Test-Command {
    param([string]$CommandName)
    
    try {
        Get-Command $CommandName -ErrorAction Stop | Out-Null
        return $true
    } catch {
        return $false
    }
}

Write-ColoredOutput "检查系统环境..." "Yellow"

if (-not (Test-Command "dotnet")) {
    Write-ColoredOutput "错误: 未找到 .NET CLI，请先安装 .NET SDK" "Red"
    Read-Host "按任意键退出"
    exit 1
}

if (-not (Test-Command "npm")) {
    Write-ColoredOutput "错误: 未找到 npm，请先安装 Node.js" "Red"
    Read-Host "按任意键退出"
    exit 1
}

if (-not (Test-Command "node")) {
    Write-ColoredOutput "错误: 未找到 node，请先安装 Node.js" "Red"
    Read-Host "按任意键退出"
    exit 1
}

# 检查端口是否被占用
function Test-Port {
    param([int]$Port)
    
    $connection = New-Object System.Net.Sockets.TcpClient
    try {
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    } catch {
        return $false
    }
}

if (Test-Port 6676) {
    Write-ColoredOutput "警告: 端口6676已被占用，后端服务可能无法启动" "Yellow"
    Write-ColoredOutput "    请关闭占用该端口的程序后重试" "Yellow"
    $continue = Read-Host "是否继续启动? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

if (Test-Port 6677) {
    Write-ColoredOutput "警告: 端口6677已被占用，前端服务可能无法启动" "Yellow"
    Write-ColoredOutput "    请关闭占用该端口的程序后重试" "Yellow"
    $continue = Read-Host "是否继续启动? (y/n)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

Write-Output ""
Write-ColoredOutput "开始启动开发环境..." "Green"
Write-Output ""

# 创建日志目录
New-Item -ItemType Directory -Force -Path "logs" | Out-Null

# 启动后端服务
Write-ColoredOutput "启动后端服务 (端口6676)..." "Blue"
Set-Location "backend"
$backendJob = Start-Job -ScriptBlock {
    param($WorkingDir)
    Set-Location $WorkingDir
    dotnet run
} -ArgumentList (Get-Location)

Set-Location ".."
Write-ColoredOutput "后端服务已启动 (Job ID: $($backendJob.Id))" "Green"

# 等待后端服务启动
Write-ColoredOutput "等待后端服务启动..." "Yellow"
Start-Sleep -Seconds 5

# 检查后端服务是否正常启动
try {
    $response = Invoke-RestMethod -Uri "http://localhost:6676/api/codesnippets" -Method Get -TimeoutSec 5 -ErrorAction Stop
    Write-ColoredOutput "后端服务启动成功" "Green"
} catch {
    Write-ColoredOutput "后端服务可能还在启动中，请查看日志" "Yellow"
}

# 启动前端服务
Write-ColoredOutput "启动前端服务 (端口6677)..." "Blue"
Set-Location "frontend"
$frontendJob = Start-Job -ScriptBlock {
    param($WorkingDir)
    Set-Location $WorkingDir
    npm run dev
} -ArgumentList (Get-Location)

Set-Location ".."
Write-ColoredOutput "前端服务已启动 (Job ID: $($frontendJob.Id))" "Green"

# 等待前端服务启动
Write-ColoredOutput "等待前端服务启动..." "Yellow"
Start-Sleep -Seconds 10

# 检查前端服务是否正常启动
try {
    $response = Invoke-WebRequest -Uri "http://localhost:6677" -TimeoutSec 5 -ErrorAction Stop
    Write-ColoredOutput "前端服务启动成功" "Green"
} catch {
    Write-ColoredOutput "前端服务可能还在启动中，请查看日志" "Yellow"
}

Write-Output ""
Write-ColoredOutput "开发环境启动完成！" "Green"
Write-Output ""

Write-ColoredOutput "服务地址:" "Blue"
Write-ColoredOutput "    后端API: http://localhost:6676" "Green"
Write-ColoredOutput "    前端界面: http://localhost:6677" "Green"
Write-Output ""

Write-ColoredOutput "快捷链接:" "Blue"
Write-ColoredOutput "    API文档: http://localhost:6676/swagger" "Green"
Write-ColoredOutput "    代码片段列表: http://localhost:6677" "Green"
Write-Output ""

Write-ColoredOutput "任务信息:" "Blue"
Write-ColoredOutput "    后端任务ID: $($backendJob.Id)" "Green"
Write-ColoredOutput "    前端任务ID: $($frontendJob.Id)" "Green"
Write-Output ""

Write-ColoredOutput "日志查看:" "Blue"
Write-ColoredOutput "    查看任务状态: Get-Job" "Green"
Write-ColoredOutput "    查看任务输出: Receive-Job -Id <JobId>" "Green"
Write-ColoredOutput "    停止任务: Stop-Job -Id <JobId>" "Green"
Write-Output ""

Write-ColoredOutput "提示:" "Yellow"
Write-ColoredOutput "    - 服务已在后台运行" "Yellow"
Write-ColoredOutput "    - 查看 PowerShell 任务: Get-Job" "Yellow"
Write-ColoredOutput "    - 查看任务输出: Receive-Job -Id $($backendJob.Id) 或 Receive-Job -Id $($frontendJob.Id)" "Yellow"
Write-ColoredOutput "    - 停止服务: 运行 .\scripts\stop-dev.ps1" "Yellow"
Write-ColoredOutput "    - 或手动停止: Stop-Job -Id $($backendJob.Id), $($frontendJob.Id)" "Yellow"
Write-Output ""

Write-ColoredOutput "开发环境已就绪，开始编码吧！" "Green"
Write-Output ""

# 保存任务ID到文件
$backendJob.Id | Out-File -FilePath ".backend.jobid" -Encoding UTF8
$frontendJob.Id | Out-File -FilePath ".frontend.jobid" -Encoding UTF8

Write-ColoredOutput "任务ID已保存到文件:" "Blue"
Write-ColoredOutput "    后端任务ID文件: .backend.jobid" "Green"
Write-ColoredOutput "    前端任务ID文件: .frontend.jobid" "Green"
Write-Output ""

Write-ColoredOutput "有用的PowerShell命令:" "Blue"
Write-ColoredOutput "    # 查看所有任务" "White"
Write-ColoredOutput "    Get-Job" "White"
Write-Output ""
Write-ColoredOutput "    # 查看特定任务输出" "White"
Write-ColoredOutput "    Receive-Job -Id $($backendJob.Id)" "White"
Write-ColoredOutput "    Receive-Job -Id $($frontendJob.Id)" "White"
Write-Output ""
Write-ColoredOutput "    # 停止特定任务" "White"
Write-ColoredOutput "    Stop-Job -Id $($backendJob.Id)" "White"
Write-ColoredOutput "    Stop-Job -Id $($frontendJob.Id)" "White"
Write-Output ""
Write-ColoredOutput "    # 移除已完成任务" "White"
Write-ColoredOutput "    Remove-Job -Id $($backendJob.Id)" "White"
Write-ColoredOutput "    Remove-Job -Id $($frontendJob.Id)" "White"
Write-Output ""