# TownTrek Documentation

Welcome to the TownTrek documentation. This directory contains comprehensive guides for Docker containerization, Azure deployment, and troubleshooting.

## Documentation Overview

### 📋 [Docker Setup and Troubleshooting Guide](Docker-Setup-Guide.md)
Complete guide covering:
- Initial Docker setup issues and solutions
- Architecture overview
- Configuration files explanation
- Security considerations
- Development workflow

### ☁️ [Azure Deployment Guide](Azure-Deployment-Guide.md)
Production deployment strategies including:
- Azure Container Instances (ACI)
- Azure Container Apps (Recommended)
- Azure Kubernetes Service (AKS)
- Security best practices
- Monitoring and logging
- CI/CD pipeline setup
- Cost optimization
- Disaster recovery

### 🐳 [Docker Commands Reference](Docker-Commands-Reference.md)
Quick reference for:
- Container management commands
- Logging and debugging
- Database operations
- Application testing
- Volume and data management
- Network troubleshooting
- Cleanup operations
- Development workflow

## Quick Start

### Local Development
```bash
# Clone and navigate to project
git clone <repository-url>
cd TownTrek

# Start the application
docker-compose up --build

# Access the application
open http://localhost:8080
```

### Production Deployment
```bash
# Build production image
docker build -t towntrek:prod .

# Deploy to Azure Container Apps
az containerapp create \
  --name towntrek-web \
  --resource-group rg-towntrek-prod \
  --environment towntrek-env \
  --image your-registry.azurecr.io/towntrek:latest
```

## Architecture

```
┌─────────────────┐    ┌─────────────────┐
│   Web App       │    │   SQL Server    │
│   (ASP.NET 8.0) │◄──►│   (2022)        │
│   Port: 8080    │    │   Port: 1433    │
└─────────────────┘    └─────────────────┘
```

## Key Features

- **Automatic Database Migrations**: EF Core migrations applied on startup
- **Health Checks**: Built-in health monitoring for both containers
- **Volume Persistence**: Database data persists between container restarts
- **Environment Configuration**: Separate configs for development/production
- **Security**: Production-ready security configurations for Azure

## Common Issues and Solutions

| Issue | Solution | Reference |
|-------|----------|-----------|
| Database connection fails | Check if database exists and migrations applied | [Docker Setup Guide](Docker-Setup-Guide.md#problem-1-database-connection-failures) |
| Container build fails | Verify Dockerfile paths and project references | [Docker Setup Guide](Docker-Setup-Guide.md#problem-3-dockerfile-path-issues) |
| Health check fails | Update sqlcmd path in docker-compose.yml | [Docker Setup Guide](Docker-Setup-Guide.md#problem-4-sql-server-health-check) |
| Production deployment issues | Follow Azure security best practices | [Azure Deployment Guide](Azure-Deployment-Guide.md#security-best-practices) |

## Environment Variables

### Development
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=sqldb;Database=TownTrekDb_Prod;User Id=sa;Password=Delsup123@sch;TrustServerCertificate=true;MultipleActiveResultSets=true
```

### Production
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=your-server.database.windows.net;Database=TownTrekDb_Prod;User Id=sqladmin;Password=YourStrongPassword123!;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true
```

## Support and Troubleshooting

1. **Check the logs**: `docker-compose logs web`
2. **Verify container status**: `docker-compose ps`
3. **Test connectivity**: `curl -I http://localhost:8080`
4. **Database issues**: See [Docker Commands Reference](Docker-Commands-Reference.md#database-operations)
5. **Performance issues**: Monitor with `docker stats`

## Contributing

When making changes to the Docker configuration:

1. Test locally with `docker-compose up --build`
2. Update relevant documentation
3. Test production build with `docker build -t towntrek:test .`
4. Verify all health checks pass

## Security Notes

- **Development**: Uses default passwords and self-signed certificates
- **Production**: Requires proper secrets management and SSL certificates
- **Database**: Always use strong passwords and encrypted connections in production
- **Container**: Run as non-root user in production environments

For detailed security configurations, see the [Azure Deployment Guide](Azure-Deployment-Guide.md#security-best-practices).

---

*Last updated: August 2025*