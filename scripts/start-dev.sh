#!/bin/bash

# 设置颜色输出
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 输出标题
echo -e "${BLUE}========================================${NC}"
echo -e "${BLUE}    CodeSnippet 开发环境一键启动脚本${NC}"
echo -e "${BLUE}========================================${NC}"
echo

# 检查是否在正确的目录
if [ ! -d "frontend" ] || [ ! -d "backend" ]; then
    # 检查是否在scripts文件夹内
    if [ "$(basename "$(pwd)")" = "scripts" ]; then
        cd ..
        if [ ! -d "frontend" ] || [ ! -d "backend" ]; then
            echo -e "${RED}❌ 错误: 请在项目根目录运行此脚本${NC}"
            echo -e "    当前目录: $(pwd)"
            exit 1
        fi
        echo -e "${YELLOW}📂 自动切换到项目根目录: $(pwd)${NC}"
    else
        echo -e "${RED}❌ 错误: 请在项目根目录运行此脚本${NC}"
        echo -e "    当前目录: $(pwd)"
        exit 1
    fi
fi

# 检查必要的命令是否存在
check_command() {
    if ! command -v $1 &> /dev/null; then
        echo -e "${RED}❌ 错误: 未找到命令 '$1'，请先安装${NC}"
        exit 1
    fi
}

echo -e "${YELLOW}🔍 检查系统环境...${NC}"
check_command "dotnet"
check_command "npm"
check_command "node"

# 检查端口是否被占用
check_port() {
    local port=$1
    local service_name=$2
    
    if lsof -Pi :$port -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${YELLOW}⚠️  警告: 端口$port已被占用，$service_name可能无法启动${NC}"
        echo -e "    请关闭占用该端口的程序后重试"
        read -p "是否继续启动? (y/n): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
}

check_port 6676 "后端服务"
check_port 6677 "前端服务"

echo
echo -e "${GREEN}🚀 开始启动开发环境...${NC}"
echo

# 创建日志目录
mkdir -p logs

# 启动后端服务
echo -e "${BLUE}📂 启动后端服务 (端口6676)...${NC}"
cd backend
nohup dotnet run > ../logs/backend.log 2>&1 &
BACKEND_PID=$!
echo -e "${GREEN}✅ 后端服务已启动 (PID: $BACKEND_PID)${NC}"
cd ..

# 等待后端服务启动
echo -e "${YELLOW}⏳ 等待后端服务启动...${NC}"
sleep 5

# 检查后端服务是否正常启动
if curl -s http://localhost:6676/api/codesnippets > /dev/null 2>&1; then
    echo -e "${GREEN}✅ 后端服务启动成功${NC}"
else
    echo -e "${YELLOW}⚠️  后端服务可能还在启动中，请查看日志: logs/backend.log${NC}"
fi

# 启动前端服务
echo -e "${BLUE}📂 启动前端服务 (端口6677)...${NC}"
cd frontend
nohup npm run dev > ../logs/frontend.log 2>&1 &
FRONTEND_PID=$!
echo -e "${GREEN}✅ 前端服务已启动 (PID: $FRONTEND_PID)${NC}"
cd ..

# 等待前端服务启动
echo -e "${YELLOW}⏳ 等待前端服务启动...${NC}"
sleep 10

# 检查前端服务是否正常启动
if curl -s http://localhost:6677 > /dev/null 2>&1; then
    echo -e "${GREEN}✅ 前端服务启动成功${NC}"
else
    echo -e "${YELLOW}⚠️  前端服务可能还在启动中，请查看日志: logs/frontend.log${NC}"
fi

echo
echo -e "${GREEN}✅ 开发环境启动完成！${NC}"
echo
echo -e "${BLUE}📍 服务地址:${NC}"
echo -e "    后端API: ${GREEN}http://localhost:6676${NC}"
echo -e "    前端界面: ${GREEN}http://localhost:6677${NC}"
echo
echo -e "${BLUE}📋 快捷链接:${NC}"
echo -e "    API文档: ${GREEN}http://localhost:6676/swagger${NC}"
echo -e "    代码片段列表: ${GREEN}http://localhost:6677${NC}"
echo
echo -e "${BLUE}📋 进程信息:${NC}"
echo -e "    后端PID: ${GREEN}$BACKEND_PID${NC}"
echo -e "    前端PID: ${GREEN}$FRONTEND_PID${NC}"
echo
echo -e "${BLUE}📋 日志文件:${NC}"
echo -e "    后端日志: ${GREEN}logs/backend.log${NC}"
echo -e "    前端日志: ${GREEN}logs/frontend.log${NC}"
echo
echo -e "${YELLOW}💡 提示:${NC}"
echo -e "    - 服务已在后台运行"
echo -e "    - 查看实时日志: tail -f logs/backend.log 或 tail -f logs/frontend.log"
echo -e "    - 停止服务: 运行 ./scripts/stop-dev.sh"
echo -e "    - 或手动停止: kill $BACKEND_PID $FRONTEND_PID"
echo
echo -e "${GREEN}🎉 开发环境已就绪，开始编码吧！${NC}"
echo

# 保存进程ID到文件
echo $BACKEND_PID > .backend.pid
echo $FRONTEND_PID > .frontend.pid

echo -e "${BLUE}📝 进程ID已保存到文件:${NC}"
echo -e "    后端PID文件: ${GREEN}.backend.pid${NC}"
echo -e "    前端PID文件: ${GREEN}.frontend.pid${NC}"
echo