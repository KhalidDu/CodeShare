#!/bin/bash

# 代码片段管理工具部署脚本
# 使用方法: ./scripts/deploy.sh [environment]
# 环境: development, staging, production

set -e

ENVIRONMENT=${1:-development}
PROJECT_ROOT=$(dirname $(dirname $(realpath $0)))

echo "🚀 开始部署代码片段管理工具到 $ENVIRONMENT 环境..."

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# 日志函数
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# 检查必要的工具
check_dependencies() {
    log_info "检查依赖工具..."
    
    if ! command -v docker &> /dev/null; then
        log_error "Docker 未安装"
        exit 1
    fi
    
    if ! command -v docker-compose &> /dev/null; then
        log_error "Docker Compose 未安装"
        exit 1
    fi
    
    log_info "依赖检查完成"
}

# 检查环境变量文件
check_env_file() {
    local env_file=".env.${ENVIRONMENT}"
    
    if [ ! -f "$env_file" ]; then
        log_warn "环境变量文件 $env_file 不存在，使用 .env.example 创建"
        cp .env.example "$env_file"
        log_error "请编辑 $env_file 文件并设置正确的环境变量"
        exit 1
    fi
    
    log_info "环境变量文件检查完成"
}

# 构建镜像
build_images() {
    log_info "构建 Docker 镜像..."
    
    # 构建后端镜像
    log_info "构建后端镜像..."
    docker build -t codesnippet-backend:${ENVIRONMENT} -f Dockerfile .
    
    # 构建前端镜像
    log_info "构建前端镜像..."
    docker build -t codesnippet-frontend:${ENVIRONMENT} -f frontend.Dockerfile .
    
    log_info "镜像构建完成"
}

# 部署到开发环境
deploy_development() {
    log_info "部署到开发环境..."
    
    docker-compose --env-file .env.development up -d
    
    log_info "等待服务启动..."
    sleep 30
    
    # 健康检查
    if curl -f http://localhost:5000/health > /dev/null 2>&1; then
        log_info "后端服务启动成功"
    else
        log_error "后端服务启动失败"
        exit 1
    fi
    
    if curl -f http://localhost:3000/health > /dev/null 2>&1; then
        log_info "前端服务启动成功"
    else
        log_error "前端服务启动失败"
        exit 1
    fi
}

# 部署到生产环境
deploy_production() {
    log_info "部署到生产环境..."
    
    # 备份当前版本
    if docker-compose -f docker-compose.prod.yml ps | grep -q "Up"; then
        log_info "备份当前运行的容器..."
        docker-compose -f docker-compose.prod.yml stop
        docker tag codesnippet-backend:production codesnippet-backend:backup-$(date +%Y%m%d-%H%M%S)
        docker tag codesnippet-frontend:production codesnippet-frontend:backup-$(date +%Y%m%d-%H%M%S)
    fi
    
    # 部署新版本
    docker-compose -f docker-compose.prod.yml --env-file .env.production up -d
    
    log_info "等待服务启动..."
    sleep 60
    
    # 健康检查
    local max_attempts=10
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if curl -f http://localhost/health > /dev/null 2>&1; then
            log_info "生产环境部署成功"
            break
        else
            log_warn "健康检查失败，尝试 $attempt/$max_attempts"
            sleep 10
            ((attempt++))
        fi
    done
    
    if [ $attempt -gt $max_attempts ]; then
        log_error "生产环境部署失败，正在回滚..."
        rollback_production
        exit 1
    fi
}

# 生产环境回滚
rollback_production() {
    log_warn "开始回滚生产环境..."
    
    # 停止当前服务
    docker-compose -f docker-compose.prod.yml down
    
    # 恢复备份镜像
    local backup_tag=$(docker images --format "table {{.Repository}}:{{.Tag}}" | grep codesnippet-backend:backup | head -1 | cut -d: -f2)
    if [ -n "$backup_tag" ]; then
        docker tag codesnippet-backend:$backup_tag codesnippet-backend:production
        docker tag codesnippet-frontend:$backup_tag codesnippet-frontend:production
        
        # 重新启动服务
        docker-compose -f docker-compose.prod.yml up -d
        
        log_info "回滚完成"
    else
        log_error "未找到备份镜像，无法回滚"
    fi
}

# 清理旧镜像
cleanup_old_images() {
    log_info "清理旧的 Docker 镜像..."
    
    # 保留最近的 3 个备份镜像
    docker images --format "table {{.Repository}}:{{.Tag}}" | grep backup | tail -n +4 | while read image; do
        docker rmi "$image" || true
    done
    
    # 清理未使用的镜像
    docker image prune -f
    
    log_info "镜像清理完成"
}

# 显示部署状态
show_status() {
    log_info "部署状态:"
    
    if [ "$ENVIRONMENT" = "production" ]; then
        docker-compose -f docker-compose.prod.yml ps
    else
        docker-compose ps
    fi
    
    echo ""
    log_info "服务地址:"
    if [ "$ENVIRONMENT" = "production" ]; then
        echo "  前端: https://your-domain.com"
        echo "  API:  https://your-domain.com/api"
    else
        echo "  前端: http://localhost:3000"
        echo "  API:  http://localhost:5000"
        echo "  Swagger: http://localhost:5000/swagger"
    fi
}

# 主函数
main() {
    cd "$PROJECT_ROOT"
    
    check_dependencies
    check_env_file
    build_images
    
    case $ENVIRONMENT in
        development)
            deploy_development
            ;;
        staging)
            deploy_development  # 暂时使用开发环境配置
            ;;
        production)
            deploy_production
            cleanup_old_images
            ;;
        *)
            log_error "不支持的环境: $ENVIRONMENT"
            echo "支持的环境: development, staging, production"
            exit 1
            ;;
    esac
    
    show_status
    log_info "部署完成! 🎉"
}

# 如果脚本被直接执行
if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    main "$@"
fi