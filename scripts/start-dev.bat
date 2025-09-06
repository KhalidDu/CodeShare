@echo off
chcp 65001 >nul
cls
echo.
echo ========================================
echo    CodeSnippet 开发环境一键启动脚本
echo ========================================
echo.

:: 检查是否在正确的目录
if not exist "frontend" (
    if not exist "backend" (
        :: 检查当前文件夹名称是否为scripts
        for %%F in ("%CD%") do set "current_folder=%%~nxF"
        if "%current_folder%"=="scripts" (
            cd ..
            if not exist "frontend" (
                echo ❌ 错误: 请在项目根目录运行此脚本
                echo    当前目录: %CD%
                pause
                exit /b 1
            )
            if not exist "backend" (
                echo ❌ 错误: 请在项目根目录运行此脚本
                echo    当前目录: %CD%
                pause
                exit /b 1
            )
            echo 📂 自动切换到项目根目录: %CD%
        ) else (
            echo ❌ 错误: 请在项目根目录运行此脚本
            echo    当前目录: %CD%
            pause
            exit /b 1
        )
    )
)

:: 检查端口是否被占用
echo 🔍 检查端口占用情况...
netstat -ano | findstr ":6676" >nul
if %errorlevel% equ 0 (
    echo ⚠️  警告: 端口6676已被占用，后端服务可能无法启动
    echo    请关闭占用该端口的程序后重试
    choice /c yn /m "是否继续启动? (y/n): "
    if errorlevel 2 exit /b 1
)

netstat -ano | findstr ":6677" >nul
if %errorlevel% equ 0 (
    echo ⚠️  警告: 端口6677已被占用，前端服务可能无法启动
    echo    请关闭占用该端口的程序后重试
    choice /c yn /m "是否继续启动? (y/n): "
    if errorlevel 2 exit /b 1
)

echo.
echo 🚀 开始启动开发环境...
echo.

:: 启动后端服务
echo 📂 启动后端服务 (端口6676)...
cd backend
start "CodeSnippet Backend" cmd /k "echo 🎯 后端服务启动中... && dotnet run"
cd ..

:: 等待后端服务启动
echo ⏳ 等待后端服务启动...
timeout /t 5 /nobreak >nul

:: 启动前端服务
echo 📂 启动前端服务 (端口6677)...
cd frontend
start "CodeSnippet Frontend" cmd /k "echo 🎯 前端服务启动中... && npm run dev"
cd ..

echo.
echo ✅ 开发环境启动完成！
echo.
echo 📍 服务地址:
echo    后端API: http://localhost:6676
echo    前端界面: http://localhost:6677
echo.
echo 📋 快捷链接:
echo    API文档: http://localhost:6676/swagger
echo    代码片段列表: http://localhost:6677
echo.
echo 💡 提示:
echo    - 两个新窗口已打开，分别运行前端和后端服务
echo    - 关闭窗口即可停止对应服务
echo    - 如需停止所有服务，请关闭所有打开的命令行窗口
echo.
echo 🎉 开发环境已就绪，开始编码吧！
echo.
pause