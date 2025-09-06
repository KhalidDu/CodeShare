#!/bin/bash

echo "🧪 测试 CodeSnippet 开发环境启动脚本"
echo "=========================================="

# 测试目录检查
echo "📂 测试目录检查..."
if [ -d "frontend" ] && [ -d "backend" ]; then
    echo "✅ 目录检查通过"
else
    echo "❌ 目录检查失败"
    exit 1
fi

# 测试脚本文件
echo "📄 测试脚本文件..."
scripts=("start-dev.sh" "stop-dev.sh" "start-dev.bat" "start-dev.ps1" "stop-dev.ps1")
for script in "${scripts[@]}"; do
    if [ -f "scripts/$script" ]; then
        echo "✅ $script 存在"
    else
        echo "❌ $script 不存在"
    fi
done

# 测试执行权限
echo "🔐 测试执行权限..."
if [ -x "scripts/start-dev.sh" ]; then
    echo "✅ start-dev.sh 有执行权限"
else
    echo "❌ start-dev.sh 没有执行权限"
fi

if [ -x "scripts/stop-dev.sh" ]; then
    echo "✅ stop-dev.sh 有执行权限"
else
    echo "❌ stop-dev.sh 没有执行权限"
fi

# 测试必要命令
echo "🔍 测试必要命令..."
commands=("dotnet" "npm" "node")
for cmd in "${commands[@]}"; do
    if command -v "$cmd" &> /dev/null; then
        echo "✅ $cmd 可用"
    else
        echo "❌ $cmd 不可用"
    fi
done

# 测试端口占用
echo "🌐 测试端口占用..."
if lsof -Pi :6676 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "⚠️  端口6676被占用"
else
    echo "✅ 端口6676可用"
fi

if lsof -Pi :6677 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo "⚠️  端口6677被占用"
else
    echo "✅ 端口6677可用"
fi

echo ""
echo "🎉 测试完成！"
echo ""
echo "💡 使用方法："
echo "   Linux/macOS: ./scripts/start-dev.sh"
echo "   Windows:     .\\scripts\\start-dev.bat"
echo "   PowerShell:  .\\scripts\\start-dev.ps1"