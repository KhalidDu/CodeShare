# CodeSnippet 开发环境停止脚本 (PowerShell版本)
# 使用方法: .\scripts\stop-dev.ps1

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
Write-ColoredOutput "    CodeSnippet 开发环境停止脚本" "Cyan"
Write-ColoredOutput "========================================" "Cyan"
Write-Output ""

# 停止函数
function Stop-ServiceByJobId {
    param([string]$JobIdFile, [string]$ServiceName)
    
    if (Test-Path $JobIdFile) {
        try {
            $jobId = Get-Content $JobIdFile
            $job = Get-Job -Id $jobId -ErrorAction SilentlyContinue
            
            if ($job) {
                Write-ColoredOutput "🛑 停止 $ServiceName (Job ID: $jobId)..." "Yellow"
                Stop-Job -Id $jobId
                Remove-Job -Id $jobId
                Write-ColoredOutput "✅ $ServiceName 已停止" "Green"
            } else {
                Write-ColoredOutput "⚠️  未找到 $ServiceName 的任务" "Yellow"
            }
        } catch {
            Write-ColoredOutput "❌ 停止 $ServiceName 时出错: $($_.Exception.Message)" "Red"
        }
        Remove-Item $JobIdFile -Force -ErrorAction SilentlyContinue
    } else {
        Write-ColoredOutput "⚠️  未找到 $ServiceName 的任务ID文件" "Yellow"
    }
}

# 停止通过端口运行的进程
function Stop-ProcessByPort {
    param([int]$Port, [string]$ServiceName)
    
    try {
        $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue | 
                        Where-Object { $_.State -eq 'Listen' }
        
        if ($connections) {
            Write-ColoredOutput "🛑 通过端口停止 $ServiceName (端口: $Port)..." "Yellow"
            foreach ($conn in $connections) {
                $process = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
                if ($process) {
                    Stop-Process -Id $conn.OwningProcess -Force
                    Write-ColoredOutput "✅ 已停止进程 $($process.Name) (PID: $($conn.OwningProcess))" "Green"
                }
            }
        }
    } catch {
        # 在Windows 7或没有Get-NetTCPConnection命令的系统上使用netstat
        try {
            $processes = netstat -ano | findstr ":$Port"
            if ($processes) {
                Write-ColoredOutput "🛑 通过端口停止 $ServiceName (端口: $Port)..." "Yellow"
                foreach ($line in $processes) {
                    if ($line -match '\s+(\d+)$') {
                        $pid = $matches[1]
                        $process = Get-Process -Id $pid -ErrorAction SilentlyContinue
                        if ($process) {
                            Stop-Process -Id $pid -Force
                            Write-ColoredOutput "✅ 已停止进程 $($process.Name) (PID: $pid)" "Green"
                        }
                    }
                }
            }
        } catch {
            Write-ColoredOutput "⚠️  无法通过端口停止 $ServiceName: $($_.Exception.Message)" "Yellow"
        }
    }
}

# 停止所有相关的进程
function Stop-RelatedProcesses {
    param([string]$ProcessName, [string]$ServiceName)
    
    $processes = Get-Process $ProcessName -ErrorAction SilentlyContinue
    if ($processes) {
        Write-ColoredOutput "🛑 停止所有 $ServiceName 进程..." "Yellow"
        foreach ($process in $processes) {
            Stop-Process -Id $process.Id -Force
            Write-ColoredOutput "✅ 已停止进程 $($process.Name) (PID: $($process.Id))" "Green"
        }
    }
}

# 检查是否在正确的目录
$projectRoot = Get-Location
if (-not (Test-Path "$projectRoot\frontend") -or -not (Test-Path "$projectRoot\backend")) {
    # 检查是否在scripts文件夹内
    if ((Split-Path (Get-Location) -Leaf) -eq "scripts") {
        Set-Location ".."
        $projectRoot = Get-Location
        if (-not (Test-Path "$projectRoot\frontend") -or -not (Test-Path "$projectRoot\backend")) {
            Write-ColoredOutput "❌ 错误: 请在项目根目录运行此脚本" "Red"
            Write-ColoredOutput "    当前目录: $(Get-Location)" "Red"
            Read-Host "按任意键退出"
            exit 1
        }
        Write-ColoredOutput "📂 自动切换到项目根目录: $projectRoot" "Yellow"
    } else {
        Write-ColoredOutput "❌ 错误: 请在项目根目录运行此脚本" "Red"
        Write-ColoredOutput "    当前目录: $(Get-Location)" "Red"
        Read-Host "按任意键退出"
        exit 1
    }
}

Write-ColoredOutput "🔍 查找并停止所有服务..." "Blue"
Write-Output ""

# 停止后端服务
Stop-ServiceByJobId ".backend.jobid" "后端服务"
Stop-ProcessByPort 6676 "后端服务"

# 停止前端服务
Stop-ServiceByJobId ".frontend.jobid" "前端服务"
Stop-ProcessByPort 6677 "前端服务"

# 停止所有相关的Node.js进程
Write-ColoredOutput "🛑 检查并停止相关Node.js进程..." "Yellow"
Stop-RelatedProcesses "node" "Node.js"
Stop-RelatedProcesses "npm" "NPM"

# 停止所有相关的.NET进程
Write-ColoredOutput "🛑 检查并停止相关.NET进程..." "Yellow"
Stop-RelatedProcesses "dotnet" ".NET"

# 停止所有相关的命令行窗口
Write-ColoredOutput "🛑 检查并停止相关的命令行窗口..." "Yellow"
try {
    $cmdProcesses = Get-Process cmd -ErrorAction SilentlyContinue | 
                   Where-Object { $_.MainWindowTitle -like "*CodeSnippet*" -or $_.MainWindowTitle -like "*Backend*" -or $_.MainWindowTitle -like "*Frontend*" }
    foreach ($process in $cmdProcesses) {
        Stop-Process -Id $process.Id -Force
        Write-ColoredOutput "✅ 已停止命令行窗口 (PID: $($process.Id))" "Green"
    }
} catch {
    Write-ColoredOutput "⚠️  停止命令行窗口时出错" "Yellow"
}

# 停止所有相关的PowerShell窗口
Write-ColoredOutput "🛑 检查并停止相关的PowerShell窗口..." "Yellow"
try {
    $psProcesses = Get-Process powershell -ErrorAction SilentlyContinue | 
                  Where-Object { $_.MainWindowTitle -like "*CodeSnippet*" -or $_.MainWindowTitle -like "*Backend*" -or $_.MainWindowTitle -like "*Frontend*" }
    foreach ($process in $psProcesses) {
        Stop-Process -Id $process.Id -Force
        Write-ColoredOutput "✅ 已停止PowerShell窗口 (PID: $($process.Id))" "Green"
    }
} catch {
    Write-ColoredOutput "⚠️  停止PowerShell窗口时出错" "Yellow"
}

Write-Output ""
Write-ColoredOutput "✅ 所有服务已停止" "Green"
Write-Output ""

Write-ColoredOutput "💡 提示:" "Yellow"
Write-ColoredOutput "    - 如需重新启动，请运行: .\scripts\start-dev.ps1" "Yellow"
Write-ColoredOutput "    - 手动启动: 在backend目录运行 'dotnet run'" "Yellow"
Write-ColoredOutput "    -           在frontend目录运行 'npm run dev'" "Yellow"
Write-Output ""

Write-ColoredOutput "🔧 手动清理命令:" "Blue"
Write-ColoredOutput "    # 查看所有PowerShell任务" "White"
Write-ColoredOutput "    Get-Job" "White"
Write-Output ""
Write-ColoredOutput "    # 停止所有PowerShell任务" "White"
Write-ColoredOutput "    Get-Job | Stop-Job" "White"
Write-Output ""
Write-ColoredOutput "    # 移除所有PowerShell任务" "White"
Write-ColoredOutput "    Get-Job | Remove-Job" "White"
Write-Output ""
Write-ColoredOutput "    # 查看端口占用情况" "White"
Write-ColoredOutput "    netstat -ano | findstr ':6676'" "White"
Write-ColoredOutput "    netstat -ano | findstr ':6677'" "White"
Write-Output ""