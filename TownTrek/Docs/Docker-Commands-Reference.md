# Docker Commands Reference

## Quick Reference

This document provides a comprehensive reference of Docker commands used for the TownTrek application development and troubleshooting.

## Container Management

### Basic Operations
```bash
# Start all services
docker-compose up

# Start in background (detached mode)
docker-compose up -d

# Build and start (rebuild images)
docker-compose up --build

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v

# Restart specific service
docker-compose restart web
docker-compose restart sqldb

# View running containers
docker-compose ps

# View all containers (including stopped)
docker ps -a
```

### Building and Images
```bash
# Build specific service
docker-compose build web

# Build without cache
docker-compose build --no-cache web

# Pull latest images
docker-compose pull

# List images
docker images

# Remove unused images
docker image prune

# Remove specific image
docker rmi towntrek-web:latest
```

## Logging and Debugging

### View Logs
```bash
# View logs for all services
docker-compose logs

# View logs for specific service
docker-compose logs web
docker-compose logs sqldb

# Follow logs (real-time)
docker-compose logs -f web

# View last N lines
docker-compose logs --tail 50 web

# View logs with timestamps
docker-compose logs -t web

# View logs for specific container
docker logs towntrek-web-1
docker logs towntrek-sqldb-1
```

### Container Inspection
```bash
# Execute command in running container
docker exec towntrek-web-1 ls -la
docker exec towntrek-web-1 dotnet --version

# Interactive shell access
docker exec -it towntrek-web-1 /bin/bash
docker exec -it towntrek-sqldb-1 /bin/bash

# Inspect container configuration
docker inspect towntrek-web-1

# View container resource usage
docker stats

# View container processes
docker exec towntrek-web-1 ps aux
```

## Database Operations

### SQL Server Management
```bash
# Connect to SQL Server
docker exec -it towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C

# Execute single query
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "SELECT 1"

# List databases
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "SELECT name FROM sys.databases"

# Create database
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "CREATE DATABASE TownTrekDb_Prod"

# Check database size
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "SELECT DB_NAME(database_id) AS DatabaseName, (size * 8.0) / 1024 AS SizeMB FROM sys.master_files WHERE type = 0"
```

### Database Backup and Restore
```bash
# Backup database
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "BACKUP DATABASE TownTrekDb_Prod TO DISK = '/var/opt/mssql/data/TownTrekDb_Prod.bak'"

# Restore database
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "RESTORE DATABASE TownTrekDb_Prod FROM DISK = '/var/opt/mssql/data/TownTrekDb_Prod.bak' WITH REPLACE"

# Copy backup file from container
docker cp towntrek-sqldb-1:/var/opt/mssql/data/TownTrekDb_Prod.bak ./backup/

# Copy backup file to container
docker cp ./backup/TownTrekDb_Prod.bak towntrek-sqldb-1:/var/opt/mssql/data/
```

## Application Testing

### Health Checks
```bash
# Test HTTP endpoint
curl -I http://localhost:8080

# Test with verbose output
curl -v http://localhost:8080

# Test HTTPS endpoint (with self-signed cert)
curl -I -k https://localhost:8081

# Test specific endpoint
curl http://localhost:8080/health
curl http://localhost:8080/api/health
```

### Performance Testing
```bash
# Simple load test with curl
for i in {1..10}; do curl -w "%{time_total}\n" -o /dev/null -s http://localhost:8080; done

# Using Apache Bench (if installed)
ab -n 100 -c 10 http://localhost:8080/

# Monitor resource usage during testing
docker stats towntrek-web-1
```

## Volume and Data Management

### Volume Operations
```bash
# List volumes
docker volume ls

# Inspect volume
docker volume inspect towntrek_mssql-data

# Remove unused volumes
docker volume prune

# Remove specific volume (WARNING: Data loss)
docker volume rm towntrek_mssql-data

# Backup volume data
docker run --rm -v towntrek_mssql-data:/data -v $(pwd):/backup alpine tar czf /backup/mssql-backup.tar.gz -C /data .

# Restore volume data
docker run --rm -v towntrek_mssql-data:/data -v $(pwd):/backup alpine tar xzf /backup/mssql-backup.tar.gz -C /data
```

### File Operations
```bash
# Copy files from container to host
docker cp towntrek-web-1:/app/appsettings.json ./

# Copy files from host to container
docker cp ./appsettings.Production.json towntrek-web-1:/app/

# View file contents in container
docker exec towntrek-web-1 cat /app/appsettings.json

# Edit file in container (if nano/vi available)
docker exec -it towntrek-web-1 nano /app/appsettings.json
```

## Network Troubleshooting

### Network Inspection
```bash
# List networks
docker network ls

# Inspect network
docker network inspect towntrek_default

# Test connectivity between containers
docker exec towntrek-web-1 ping sqldb
docker exec towntrek-web-1 telnet sqldb 1433

# Check port binding
netstat -tulpn | grep :8080
netstat -tulpn | grep :1433
```

### DNS Resolution
```bash
# Check DNS resolution in container
docker exec towntrek-web-1 nslookup sqldb
docker exec towntrek-web-1 cat /etc/hosts
docker exec towntrek-web-1 cat /etc/resolv.conf
```

## Environment and Configuration

### Environment Variables
```bash
# View environment variables in container
docker exec towntrek-web-1 env

# View specific environment variable
docker exec towntrek-web-1 echo $ASPNETCORE_ENVIRONMENT

# Set environment variable temporarily
docker exec towntrek-web-1 env ASPNETCORE_ENVIRONMENT=Production dotnet TownTrek.dll
```

### Configuration Files
```bash
# View configuration files
docker exec towntrek-web-1 cat /app/appsettings.json
docker exec towntrek-web-1 cat /app/appsettings.Development.json

# Check application files
docker exec towntrek-web-1 ls -la /app/
docker exec towntrek-web-1 find /app -name "*.dll"
```

## Cleanup Operations

### Remove Everything
```bash
# Stop and remove containers, networks, volumes
docker-compose down -v --remove-orphans

# Remove all stopped containers
docker container prune

# Remove all unused images
docker image prune -a

# Remove all unused volumes
docker volume prune

# Remove all unused networks
docker network prune

# Nuclear option - remove everything
docker system prune -a --volumes
```

### Selective Cleanup
```bash
# Remove only TownTrek containers
docker rm $(docker ps -a -q --filter "name=towntrek")

# Remove only TownTrek images
docker rmi $(docker images --filter "reference=towntrek*" -q)

# Remove containers older than 24 hours
docker container prune --filter "until=24h"
```

## Troubleshooting Common Issues

### Container Won't Start
```bash
# Check container status
docker-compose ps

# View detailed logs
docker-compose logs web

# Check if ports are in use
netstat -tulpn | grep :8080

# Restart with fresh build
docker-compose down
docker-compose up --build
```

### Database Connection Issues
```bash
# Test SQL Server connectivity
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "SELECT 1"

# Check if database exists
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "SELECT name FROM sys.databases WHERE name = 'TownTrekDb_Prod'"

# Verify connection string
docker exec towntrek-web-1 env | grep ConnectionStrings
```

### Performance Issues
```bash
# Monitor resource usage
docker stats

# Check container limits
docker inspect towntrek-web-1 | grep -A 10 "Memory"

# View system resources
docker system df
```

### Application Errors
```bash
# View application logs
docker-compose logs -f web

# Check application health
curl -f http://localhost:8080/health || echo "Health check failed"

# Restart application container
docker-compose restart web
```

## Development Workflow

### Typical Development Session
```bash
# 1. Start development environment
docker-compose up -d

# 2. View logs to ensure everything started
docker-compose logs

# 3. Make code changes...

# 4. Rebuild and restart
docker-compose build web
docker-compose restart web

# 5. Test changes
curl http://localhost:8080

# 6. View logs for debugging
docker-compose logs -f web

# 7. Clean up when done
docker-compose down
```

### Quick Debugging Session
```bash
# Start with logs visible
docker-compose up

# In another terminal, execute commands
docker exec -it towntrek-web-1 /bin/bash

# Check application status
curl -I http://localhost:8080

# Stop everything
docker-compose down
```

This reference guide covers the most commonly used Docker commands for developing and troubleshooting the TownTrek application. Keep this handy for quick reference during development and deployment activities.