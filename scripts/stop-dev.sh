#!/bin/bash

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 输出标题
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}    CodeSnippet 开发环境停止脚本${NC}"
echo -e "${BLUE}========================================${NC}"
echo

# 检查是否在正确的目录
if [ ! -d "frontend" ] || [ ! -d "backend" ]; then
    # 检查是否在scripts文件夹内
    if [ "$(basename "$(pwd)")" = "scripts" ]; then
        cd ..
        if [ ! -d "frontend" ] || [ ! -d "backend" ]; then
            echo -e "${RED}❌ 错误: 请在项目根目录运行此脚本${NC}"
            echo -e "${RED}    当前目录: $(pwd)${NC}"
            exit 1
        fi
        echo -e "${YELLOW}📂 自动切换到项目根目录: $(pwd)${NC}"
    else
        echo -e "${RED}❌ 错误: 请在项目根目录运行此脚本${NC}"
        echo -e "${RED}    当前目录: $(pwd)${NC}"
        exit 1
    fi
fi

# 停止函数
stop_process() {
    local pid_file=$1
    local service_name=$2
    
    if [ -f "$pid_file" ]; then
        local pid=$(cat "$pid_file")
        if ps -p $pid > /dev/null 2>&1; then
            echo -e "${YELLOW}🛑 停止 $service_name (PID: $pid)...${NC}"
            kill $pid
            sleep 2
            
            # 检查进程是否还在运行
            if ps -p $pid > /dev/null 2>&1; then
                echo -e "${RED}❌ 强制停止 $service_name (PID: $pid)...${NC}"
                kill -9 $pid
            fi
            
            echo -e "${GREEN}✅ $service_name 已停止${NC}"
        else
            echo -e "${YELLOW}⚠️  $service_name 进程不存在${NC}"
        fi
        rm -f "$pid_file"
    else
        echo -e "${YELLOW}⚠️  未找到 $service_name 的PID文件${NC}"
    fi
}

# 检查并停止端口上的进程
stop_by_port() {
    local port=$1
    local service_name=$2
    
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${YELLOW}🛑 通过端口停止 $service_name (端口: $port)...${NC}"
        local pids=$(lsof -Pi :$port -sTCP:LISTEN -t)
        for pid in $pids; do
            kill $pid 2>/dev/null
            echo -e "${GREEN}✅ 已停止进程 (PID: $pid)${NC}"
        done
    fi
}

# 停止所有服务
echo -e "${BLUE}🔍 查找并停止所有服务...${NC}"
echo

# 停止后端服务
stop_process ".backend.pid" "后端服务"
stop_by_port 6676 "后端服务"

# 停止前端服务
stop_process ".frontend.pid" "前端服务"
stop_by_port 6677 "前端服务"

# 停止所有相关的Node.js进程
echo -e "${YELLOW}🛑 检查并停止相关Node.js进程...${NC}"
pkill -f "npm run dev" 2>/dev/null
pkill -f "vite" 2>/dev/null
echo -e "${GREEN}✅ Node.js进程已清理${NC}"

# 停止所有相关的.NET进程
echo -e "${YELLOW}🛑 检查并停止相关.NET进程...${NC}"
pkill -f "dotnet run" 2>/dev/null
echo -e "${GREEN}✅ .NET进程已清理${NC}"

echo
echo -e "${GREEN}✅ 所有服务已停止${NC}"
echo
echo -e "${BLUE}💡 提示:${NC}"
echo -e "    - 如需重新启动，请运行: ./scripts/start-dev.sh"
echo -e "    - 手动启动: 在backend目录运行 'dotnet run'"
echo -e "    -           在frontend目录运行 'npm run dev'"
echo