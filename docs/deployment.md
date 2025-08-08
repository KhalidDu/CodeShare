# 部署文档

## 概述

本文档详细说明如何在生产环境中部署代码片段管理工具。系统支持多种部署方式，包括 Docker、Kubernetes 和传统服务器部署。

## 系统要求

### 最低配置

- **CPU**: 2 核心
- **内存**: 4GB RAM
- **存储**: 20GB 可用空间
- **网络**: 稳定的互联网连接

### 推荐配置

- **CPU**: 4 核心或更多
- **内存**: 8GB RAM 或更多
- **存储**: 50GB SSD
- **网络**: 高速互联网连接

### 软件要求

- **操作系统**: Ubuntu 20.04+, CentOS 8+, Windows Server 2019+
- **Docker**: 20.10+ 和 Docker Compose 2.0+
- **数据库**: MySQL 8.0+
- **反向代理**: Nginx 1.18+ 或 Apache 2.4+

## Docker 部署（推荐）

### 1. 准备环境

```bash
# 安装 Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# 安装 Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# 验证安装
docker --version
docker-compose --version
```

### 2. 获取项目代码

```bash
# 克隆项目
git clone https://github.com/your-org/codesnippet-manager.git
cd codesnippet-manager

# 切换到稳定版本
git checkout v1.0.0
```

### 3. 配置环境变量

```bash
# 复制环境变量模板
cp .env.example .env.production

# 编辑环境变量
nano .env.production
```

**重要环境变量**：

```env
# 数据库配置
MYSQL_ROOT_PASSWORD=your_secure_root_password
MYSQL_USER=codesnippet_user
MYSQL_PASSWORD=your_secure_user_password

# JWT 配置
JWT_SECRET_KEY=your-super-secure-jwt-secret-key-at-least-64-characters-long
JWT_ISSUER=https://your-domain.com
JWT_AUDIENCE=https://your-domain.com

# 应用配置
FRONTEND_URL=https://your-domain.com
DB_CONNECTION_STRING=Server=mysql;Database=CodeSnippetManager;Uid=codesnippet_user;Pwd=your_secure_user_password;CharSet=utf8mb4;
```

### 4. 部署应用

```bash
# 使用部署脚本（推荐）
./scripts/deploy.sh production

# 或手动部署
docker-compose -f docker-compose.prod.yml --env-file .env.production up -d
```

### 5. 验证部署

```bash
# 检查容器状态
docker-compose -f docker-compose.prod.yml ps

# 查看日志
docker-compose -f docker-compose.prod.yml logs -f

# 测试健康检查
curl http://localhost/health
```

### 6. 配置反向代理

创建 Nginx 配置文件 `/etc/nginx/sites-available/codesnippet`:

```nginx
server {
    listen 80;
    server_name your-domain.com;
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name your-domain.com;

    ssl_certificate /etc/ssl/certs/your-domain.com.crt;
    ssl_certificate_key /etc/ssl/private/your-domain.com.key;

    location / {
        proxy_pass http://localhost:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

启用站点：

```bash
sudo ln -s /etc/nginx/sites-available/codesnippet /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

## Kubernetes 部署

### 1. 准备 Kubernetes 集群

确保您有一个运行中的 Kubernetes 集群，并且 `kubectl` 已正确配置。

### 2. 创建命名空间

```bash
kubectl apply -f k8s/namespace.yaml
```

### 3. 配置密钥

```bash
# 创建数据库密钥
kubectl create secret generic codesnippet-secrets \
  --from-literal=DB_CONNECTION_STRING="Server=mysql;Database=CodeSnippetManager;Uid=app_user;Pwd=secure_password;CharSet=utf8mb4;" \
  --from-literal=JWT_SECRET_KEY="your-super-secure-jwt-secret-key" \
  --from-literal=MYSQL_ROOT_PASSWORD="secure_root_password" \
  --from-literal=MYSQL_USER="app_user" \
  --from-literal=MYSQL_PASSWORD="secure_password" \
  -n codesnippet-manager
```

### 4. 部署数据库

```bash
kubectl apply -f k8s/mysql-deployment.yaml
```

### 5. 部署应用

```bash
# 部署后端
kubectl apply -f k8s/backend-deployment.yaml

# 部署前端
kubectl apply -f k8s/frontend-deployment.yaml

# 配置 Ingress
kubectl apply -f k8s/ingress.yaml
```

### 6. 验证部署

```bash
# 检查 Pod 状态
kubectl get pods -n codesnippet-manager

# 查看服务
kubectl get services -n codesnippet-manager

# 查看 Ingress
kubectl get ingress -n codesnippet-manager
```

## 传统服务器部署

### 1. 安装依赖

#### Ubuntu/Debian

```bash
# 更新包列表
sudo apt update

# 安装 .NET 8
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y dotnet-sdk-8.0

# 安装 Node.js
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
sudo apt install -y nodejs

# 安装 MySQL
sudo apt install -y mysql-server

# 安装 Nginx
sudo apt install -y nginx
```

#### CentOS/RHEL

```bash
# 安装 .NET 8
sudo dnf install -y dotnet-sdk-8.0

# 安装 Node.js
sudo dnf install -y nodejs npm

# 安装 MySQL
sudo dnf install -y mysql-server

# 安装 Nginx
sudo dnf install -y nginx
```

### 2. 配置数据库

```bash
# 启动 MySQL
sudo systemctl start mysql
sudo systemctl enable mysql

# 安全配置
sudo mysql_secure_installation

# 创建数据库和用户
mysql -u root -p << EOF
CREATE DATABASE CodeSnippetManager CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'codesnippet_user'@'localhost' IDENTIFIED BY 'secure_password';
GRANT ALL PRIVILEGES ON CodeSnippetManager.* TO 'codesnippet_user'@'localhost';
FLUSH PRIVILEGES;
EOF

# 导入数据库结构
mysql -u codesnippet_user -p CodeSnippetManager < database/init.sql
```

### 3. 部署后端

```bash
# 创建应用目录
sudo mkdir -p /opt/codesnippet/backend
sudo chown $USER:$USER /opt/codesnippet/backend

# 构建应用
cd backend
dotnet publish -c Release -o /opt/codesnippet/backend

# 创建配置文件
sudo tee /opt/codesnippet/backend/appsettings.Production.json > /dev/null << EOF
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CodeSnippetManager;Uid=codesnippet_user;Pwd=secure_password;CharSet=utf8mb4;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secure-jwt-secret-key",
    "ExpiryHours": 8
  }
}
EOF

# 创建 systemd 服务
sudo tee /etc/systemd/system/codesnippet-backend.service > /dev/null << EOF
[Unit]
Description=CodeSnippet Backend API
After=network.target

[Service]
Type=notify
ExecStart=/usr/bin/dotnet /opt/codesnippet/backend/CodeSnippetManager.Api.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=codesnippet-backend
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://localhost:5000

[Install]
WantedBy=multi-user.target
EOF

# 启动服务
sudo systemctl daemon-reload
sudo systemctl enable codesnippet-backend
sudo systemctl start codesnippet-backend
```

### 4. 部署前端

```bash
# 创建前端目录
sudo mkdir -p /var/www/codesnippet
sudo chown $USER:$USER /var/www/codesnippet

# 构建前端
cd frontend
npm install
npm run build

# 复制构建文件
cp -r dist/* /var/www/codesnippet/
```

### 5. 配置 Nginx

```bash
# 创建 Nginx 配置
sudo tee /etc/nginx/sites-available/codesnippet > /dev/null << 'EOF'
server {
    listen 80;
    server_name your-domain.com;
    root /var/www/codesnippet;
    index index.html;

    # 前端路由支持
    location / {
        try_files $uri $uri/ /index.html;
    }

    # API 代理
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # 静态资源缓存
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
EOF

# 启用站点
sudo ln -s /etc/nginx/sites-available/codesnippet /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

## SSL 证书配置

### 使用 Let's Encrypt

```bash
# 安装 Certbot
sudo apt install -y certbot python3-certbot-nginx

# 获取证书
sudo certbot --nginx -d your-domain.com

# 设置自动续期
sudo crontab -e
# 添加以下行：
# 0 12 * * * /usr/bin/certbot renew --quiet
```

### 使用自签名证书（仅用于测试）

```bash
# 生成私钥
sudo openssl genrsa -out /etc/ssl/private/codesnippet.key 2048

# 生成证书
sudo openssl req -new -x509 -key /etc/ssl/private/codesnippet.key -out /etc/ssl/certs/codesnippet.crt -days 365

# 更新 Nginx 配置以使用 SSL
```

## 监控和日志

### 1. 应用监控

```bash
# 检查后端服务状态
sudo systemctl status codesnippet-backend

# 查看后端日志
sudo journalctl -u codesnippet-backend -f

# 检查 Nginx 状态
sudo systemctl status nginx

# 查看 Nginx 日志
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### 2. 数据库监控

```bash
# 检查 MySQL 状态
sudo systemctl status mysql

# 连接数据库
mysql -u codesnippet_user -p CodeSnippetManager

# 查看数据库大小
SELECT 
    table_schema AS 'Database',
    ROUND(SUM(data_length + index_length) / 1024 / 1024, 2) AS 'Size (MB)'
FROM information_schema.tables 
WHERE table_schema = 'CodeSnippetManager';
```

### 3. 性能监控

```bash
# 系统资源使用
htop
iostat -x 1

# 网络连接
netstat -tulpn | grep :80
netstat -tulpn | grep :443
netstat -tulpn | grep :5000
```

## 备份策略

### 1. 数据库备份

创建备份脚本 `/opt/scripts/backup-db.sh`：

```bash
#!/bin/bash
BACKUP_DIR="/opt/backups/mysql"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="CodeSnippetManager"
DB_USER="codesnippet_user"
DB_PASS="secure_password"

mkdir -p $BACKUP_DIR

mysqldump -u $DB_USER -p$DB_PASS \
    --single-transaction \
    --routines \
    --triggers \
    $DB_NAME > $BACKUP_DIR/backup_$DATE.sql

gzip $BACKUP_DIR/backup_$DATE.sql

# 删除 7 天前的备份
find $BACKUP_DIR -name "backup_*.sql.gz" -mtime +7 -delete

echo "Backup completed: backup_$DATE.sql.gz"
```

设置定时备份：

```bash
sudo chmod +x /opt/scripts/backup-db.sh
sudo crontab -e
# 添加：0 2 * * * /opt/scripts/backup-db.sh
```

### 2. 应用备份

```bash
# 备份配置文件
tar -czf /opt/backups/config_$(date +%Y%m%d).tar.gz \
    /opt/codesnippet/backend/appsettings.Production.json \
    /etc/nginx/sites-available/codesnippet \
    /etc/systemd/system/codesnippet-backend.service

# 备份前端文件
tar -czf /opt/backups/frontend_$(date +%Y%m%d).tar.gz \
    /var/www/codesnippet
```

## 更新和维护

### 1. 应用更新

```bash
# 停止服务
sudo systemctl stop codesnippet-backend

# 备份当前版本
cp -r /opt/codesnippet/backend /opt/codesnippet/backend.backup

# 获取新版本
git pull origin main

# 构建新版本
cd backend
dotnet publish -c Release -o /opt/codesnippet/backend

# 更新前端
cd frontend
npm install
npm run build
cp -r dist/* /var/www/codesnippet/

# 重启服务
sudo systemctl start codesnippet-backend
sudo systemctl reload nginx
```

### 2. 数据库维护

```bash
# 优化数据库表
mysql -u codesnippet_user -p CodeSnippetManager << EOF
OPTIMIZE TABLE Users, CodeSnippets, SnippetVersions, Tags, SnippetTags, ClipboardHistory;
EOF

# 分析表统计信息
mysql -u codesnippet_user -p CodeSnippetManager << EOF
ANALYZE TABLE Users, CodeSnippets, SnippetVersions, Tags, SnippetTags, ClipboardHistory;
EOF
```

## 故障排除

### 常见问题

#### 1. 后端服务无法启动

```bash
# 检查日志
sudo journalctl -u codesnippet-backend -n 50

# 检查端口占用
sudo netstat -tulpn | grep :5000

# 检查配置文件
cat /opt/codesnippet/backend/appsettings.Production.json
```

#### 2. 数据库连接失败

```bash
# 测试数据库连接
mysql -u codesnippet_user -p -h localhost CodeSnippetManager

# 检查 MySQL 状态
sudo systemctl status mysql

# 查看 MySQL 错误日志
sudo tail -f /var/log/mysql/error.log
```

#### 3. 前端页面无法加载

```bash
# 检查 Nginx 配置
sudo nginx -t

# 查看 Nginx 错误日志
sudo tail -f /var/log/nginx/error.log

# 检查文件权限
ls -la /var/www/codesnippet/
```

### 性能优化

#### 1. 数据库优化

```sql
-- 添加索引
CREATE INDEX idx_snippets_language ON CodeSnippets(Language);
CREATE INDEX idx_snippets_created_at ON CodeSnippets(CreatedAt);
CREATE INDEX idx_clipboard_user_copied ON ClipboardHistory(UserId, CopiedAt);

-- 配置查询缓存
SET GLOBAL query_cache_type = ON;
SET GLOBAL query_cache_size = 67108864; -- 64MB
```

#### 2. Nginx 优化

```nginx
# 启用 Gzip 压缩
gzip on;
gzip_vary on;
gzip_min_length 1024;
gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;

# 启用缓存
location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2)$ {
    expires 1y;
    add_header Cache-Control "public, immutable";
}
```

## 安全加固

### 1. 系统安全

```bash
# 更新系统
sudo apt update && sudo apt upgrade -y

# 配置防火墙
sudo ufw enable
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp

# 禁用不必要的服务
sudo systemctl disable apache2 # 如果安装了
```

### 2. 应用安全

```bash
# 设置正确的文件权限
sudo chown -R www-data:www-data /var/www/codesnippet
sudo chmod -R 755 /var/www/codesnippet

sudo chown -R www-data:www-data /opt/codesnippet
sudo chmod -R 755 /opt/codesnippet
```

### 3. 数据库安全

```sql
-- 删除测试数据库
DROP DATABASE IF EXISTS test;

-- 删除匿名用户
DELETE FROM mysql.user WHERE User='';

-- 禁止 root 远程登录
DELETE FROM mysql.user WHERE User='root' AND Host NOT IN ('localhost', '127.0.0.1', '::1');

FLUSH PRIVILEGES;
```

## 支持和联系

如果在部署过程中遇到问题：

1. 查看本文档和 [故障排除指南](troubleshooting.md)
2. 检查 [GitHub Issues](https://github.com/your-org/codesnippet-manager/issues)
3. 联系技术支持：support@your-domain.com

## 更新日志

- **v1.0.0**: 初始部署文档
- **v1.0.1**: 添加 Kubernetes 部署支持
- **v1.0.2**: 增强安全配置和监控指南