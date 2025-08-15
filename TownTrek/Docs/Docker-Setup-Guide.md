# Docker Setup and Troubleshooting Guide

## Overview

This guide documents the Docker containerization setup for TownTrek, including common issues encountered and their solutions.

## Architecture

The application uses a multi-container setup with:
- **Web Application**: ASP.NET Core 8.0 application
- **Database**: Microsoft SQL Server 2022

## Initial Issues and Solutions

### Problem 1: Database Connection Failures

**Issue**: Application crashed with `SqlException: Cannot open database "TownTrekDb_Prod"`

**Root Cause**: 
- Database didn't exist
- No automatic database creation or migration application

**Solution**:
```csharp
// Added to Program.cs
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync(); // Automatically apply migrations
    
    await DbSeeder.SeedAsync(scope.ServiceProvider);
    
    var roleInitService = scope.ServiceProvider.GetRequiredService<IRoleInitializationService>();
    await roleInitService.InitializeRolesAsync();
}
```

### Problem 2: Project Reference Issues

**Issue**: Build failures due to missing `TownTrek.ServiceDefaults` project

**Solution**: Removed non-existent project references from:
- `TownTrek.csproj`
- `Program.cs` (removed `AddServiceDefaults()` and `MapDefaultEndpoints()`)

### Problem 3: Dockerfile Path Issues

**Issue**: `COPY` commands failing due to incorrect path structure

**Original**:
```dockerfile
COPY ["TownTrek/TownTrek.csproj", "TownTrek/"]
```

**Fixed**:
```dockerfile
COPY ["TownTrek.csproj", "./"]
```

### Problem 4: SQL Server Health Check

**Issue**: Health check failing due to incorrect sqlcmd path

**Solution**: Updated docker-compose.yml:
```yaml
healthcheck:
  test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Delsup123@sch -C -Q 'SELECT 1' || exit 1"]
```

## Essential Commands

### Development Commands

```bash
# Build and start all services
docker-compose up --build

# Start in detached mode
docker-compose up -d

# View logs
docker-compose logs web
docker-compose logs sqldb

# Stop all services
docker-compose down

# Rebuild specific service
docker-compose build web

# Execute commands in running container
docker exec towntrek-web-1 dotnet --version
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "password" -C -Q "SELECT 1"
```

### Database Management

```bash
# Create database manually (if needed)
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "CREATE DATABASE TownTrekDb_Prod"

# Check database exists
docker exec towntrek-sqldb-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Delsup123@sch" -C -Q "SELECT name FROM sys.databases"
```

### Troubleshooting Commands

```bash
# Check container status
docker-compose ps

# View container resource usage
docker stats

# Inspect container configuration
docker inspect towntrek-web-1

# Access container shell
docker exec -it towntrek-web-1 /bin/bash

# Check application health
curl -I http://localhost:8080
```

## Configuration Files

### docker-compose.yml
```yaml
services:
  web:
    build:
      context: .
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=sqldb;Database=TownTrekDb_Prod;User Id=sa;Password=Delsup123@sch;TrustServerCertificate=true;MultipleActiveResultSets=true
    ports:
      - "8080:8080"
      - "8081:8081"
    depends_on:
      sqldb:
        condition: service_healthy
    restart: on-failure

  sqldb:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=Delsup123@sch
    ports:
      - "1433:1433"
    volumes:
      - mssql-data:/var/opt/mssql
    restart: unless-stopped
    healthcheck:
      test: ["CMD-SHELL", "/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Delsup123@sch -C -Q 'SELECT 1' || exit 1"]
      interval: 10s
      retries: 10
      start_period: 10s
      timeout: 3s

volumes:
  mssql-data:
```

### Key Features
- **Health Checks**: Ensures SQL Server is ready before starting web app
- **Automatic Restarts**: Containers restart on failure
- **Volume Persistence**: Database data persists between container restarts
- **Environment Variables**: Connection string configured via environment

## Security Considerations

### Development vs Production

**Development** (Current Setup):
- Uses default SA password
- TrustServerCertificate=true
- No SSL/TLS for web traffic

**Production Requirements**:
- Strong, unique passwords
- Proper SSL certificates
- Network security groups
- Secrets management
- Regular security updates

## Next Steps

1. **Environment-Specific Configurations**: Create separate docker-compose files for different environments
2. **Secrets Management**: Move sensitive data to secure secret stores
3. **Monitoring**: Add health checks and logging
4. **CI/CD Integration**: Automate builds and deployments