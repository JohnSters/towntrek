# Azure Container Deployment Guide

## Overview

This guide covers deploying the TownTrek application to Azure using various container services, with production-ready configurations and best practices.

## Azure Deployment Options

### 1. Azure Container Instances (ACI) - Simple Deployment

**Best for**: Development, testing, simple workloads

#### Prerequisites
```bash
# Install Azure CLI
az login
az account set --subscription "your-subscription-id"
```

#### Resource Group Setup
```bash
# Create resource group
az group create --name rg-towntrek-prod --location eastus

# Create Azure SQL Database
az sql server create \
  --name towntrek-sql-server \
  --resource-group rg-towntrek-prod \
  --location eastus \
  --admin-user sqladmin \
  --admin-password "YourStrongPassword123!"

az sql db create \
  --resource-group rg-towntrek-prod \
  --server towntrek-sql-server \
  --name TownTrekDb_Prod \
  --service-objective Basic
```

#### Container Deployment
```bash
# Deploy web application
az container create \
  --resource-group rg-towntrek-prod \
  --name towntrek-web \
  --image your-registry.azurecr.io/towntrek:latest \
  --cpu 1 \
  --memory 2 \
  --ports 80 443 \
  --environment-variables \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection="Server=towntrek-sql-server.database.windows.net;Database=TownTrekDb_Prod;User Id=sqladmin;Password=YourStrongPassword123!;TrustServerCertificate=false;MultipleActiveResultSets=true" \
  --dns-name-label towntrek-app
```

### 2. Azure Container Apps - Recommended for Production

**Best for**: Production workloads, microservices, auto-scaling

#### Setup Container Apps Environment
```bash
# Install Container Apps extension
az extension add --name containerapp

# Create Container Apps environment
az containerapp env create \
  --name towntrek-env \
  --resource-group rg-towntrek-prod \
  --location eastus
```

#### Deploy Application
```bash
az containerapp create \
  --name towntrek-web \
  --resource-group rg-towntrek-prod \
  --environment towntrek-env \
  --image your-registry.azurecr.io/towntrek:latest \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 10 \
  --cpu 1.0 \
  --memory 2.0Gi \
  --env-vars \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__DefaultConnection=secretref:connection-string
```

### 3. Azure Kubernetes Service (AKS) - Enterprise Scale

**Best for**: Large-scale applications, complex orchestration needs

#### Create AKS Cluster
```bash
az aks create \
  --resource-group rg-towntrek-prod \
  --name towntrek-aks \
  --node-count 3 \
  --node-vm-size Standard_D2s_v3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group rg-towntrek-prod --name towntrek-aks
```

#### Kubernetes Manifests

**deployment.yaml**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: towntrek-web
spec:
  replicas: 3
  selector:
    matchLabels:
      app: towntrek-web
  template:
    metadata:
      labels:
        app: towntrek-web
    spec:
      containers:
      - name: web
        image: your-registry.azurecr.io/towntrek:latest
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: towntrek-secrets
              key: connection-string
        resources:
          requests:
            memory: "1Gi"
            cpu: "500m"
          limits:
            memory: "2Gi"
            cpu: "1000m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: towntrek-service
spec:
  selector:
    app: towntrek-web
  ports:
  - port: 80
    targetPort: 8080
  type: LoadBalancer
```

## Production Configuration Changes

### 1. Update appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=towntrek-sql-server.database.windows.net;Database=TownTrekDb_Prod;User Id=sqladmin;Password=#{SQL_PASSWORD}#;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning"
    },
    "WriteTo": [
      {
        "Name": "ApplicationInsights",
        "Args": {
          "connectionString": "#{APPINSIGHTS_CONNECTION_STRING}#"
        }
      }
    ]
  },
  "Email": {
    "Host": "smtp.sendgrid.net",
    "Port": 587,
    "Username": "apikey",
    "Password": "#{SENDGRID_API_KEY}#",
    "FromName": "TownTrek",
    "FromAddress": "noreply@towntrek.co.za",
    "UseStartTls": true
  },
  "PayFast": {
    "MerchantId": "#{PAYFAST_MERCHANT_ID}#",
    "MerchantKey": "#{PAYFAST_MERCHANT_KEY}#",
    "PassPhrase": "#{PAYFAST_PASSPHRASE}#",
    "PaymentUrl": "https://www.payfast.co.za/eng/process",
    "IsLive": true,
    "Environment": "production"
  }
}
```

### 2. Production Dockerfile Optimizations
```dockerfile
# Multi-stage build for production
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TownTrek.csproj", "./"]
RUN dotnet restore "./TownTrek.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./TownTrek.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./TownTrek.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "TownTrek.dll"]
```

## Security Best Practices

### 1. Azure Key Vault Integration
```bash
# Create Key Vault
az keyvault create \
  --name towntrek-keyvault \
  --resource-group rg-towntrek-prod \
  --location eastus

# Store secrets
az keyvault secret set \
  --vault-name towntrek-keyvault \
  --name "ConnectionString" \
  --value "Server=towntrek-sql-server.database.windows.net;Database=TownTrekDb_Prod;User Id=sqladmin;Password=YourStrongPassword123!;Encrypt=true;TrustServerCertificate=false;MultipleActiveResultSets=true"
```

### 2. Managed Identity Setup
```bash
# Enable managed identity for Container App
az containerapp identity assign \
  --name towntrek-web \
  --resource-group rg-towntrek-prod \
  --system-assigned

# Grant Key Vault access
az keyvault set-policy \
  --name towntrek-keyvault \
  --object-id $(az containerapp identity show --name towntrek-web --resource-group rg-towntrek-prod --query principalId -o tsv) \
  --secret-permissions get list
```

### 3. Network Security
```bash
# Create Virtual Network
az network vnet create \
  --resource-group rg-towntrek-prod \
  --name towntrek-vnet \
  --address-prefix 10.0.0.0/16 \
  --subnet-name container-subnet \
  --subnet-prefix 10.0.1.0/24

# Create Network Security Group
az network nsg create \
  --resource-group rg-towntrek-prod \
  --name towntrek-nsg

# Allow HTTP/HTTPS traffic
az network nsg rule create \
  --resource-group rg-towntrek-prod \
  --nsg-name towntrek-nsg \
  --name AllowHTTP \
  --protocol tcp \
  --priority 1000 \
  --destination-port-range 80 \
  --access allow
```

## Monitoring and Logging

### 1. Application Insights
```bash
# Create Application Insights
az monitor app-insights component create \
  --app towntrek-insights \
  --location eastus \
  --resource-group rg-towntrek-prod \
  --application-type web
```

### 2. Log Analytics Workspace
```bash
# Create Log Analytics workspace
az monitor log-analytics workspace create \
  --resource-group rg-towntrek-prod \
  --workspace-name towntrek-logs \
  --location eastus
```

## CI/CD Pipeline Example (Azure DevOps)

### azure-pipelines.yml
```yaml
trigger:
- main

variables:
  dockerRegistryServiceConnection: 'your-acr-connection'
  imageRepository: 'towntrek'
  containerRegistry: 'your-registry.azurecr.io'
  dockerfilePath: '$(Build.SourcesDirectory)/Dockerfile'
  tag: '$(Build.BuildId)'

stages:
- stage: Build
  displayName: Build and push stage
  jobs:
  - job: Build
    displayName: Build
    pool:
      vmImage: ubuntu-latest
    steps:
    - task: Docker@2
      displayName: Build and push an image to container registry
      inputs:
        command: buildAndPush
        repository: $(imageRepository)
        dockerfile: $(dockerfilePath)
        containerRegistry: $(dockerRegistryServiceConnection)
        tags: |
          $(tag)
          latest

- stage: Deploy
  displayName: Deploy stage
  dependsOn: Build
  jobs:
  - deployment: Deploy
    displayName: Deploy
    pool:
      vmImage: ubuntu-latest
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureContainerApps@1
            displayName: Deploy to Container Apps
            inputs:
              azureSubscription: 'your-subscription'
              containerAppName: 'towntrek-web'
              resourceGroup: 'rg-towntrek-prod'
              imageToDeploy: '$(containerRegistry)/$(imageRepository):$(tag)'
```

## Cost Optimization

### 1. Resource Sizing
- **Development**: 1 vCPU, 2GB RAM
- **Production**: 2 vCPU, 4GB RAM (with auto-scaling)

### 2. Database Tiers
- **Development**: Basic (5 DTU)
- **Production**: Standard S2 (50 DTU) or Premium for high performance

### 3. Monitoring Costs
```bash
# Set up budget alerts
az consumption budget create \
  --budget-name towntrek-budget \
  --amount 100 \
  --resource-group rg-towntrek-prod \
  --time-grain Monthly \
  --time-period start-date=2024-01-01 end-date=2024-12-31
```

## Disaster Recovery

### 1. Database Backup
```bash
# Enable automated backups (enabled by default)
az sql db show \
  --resource-group rg-towntrek-prod \
  --server towntrek-sql-server \
  --name TownTrekDb_Prod \
  --query '{backupRetentionPeriod:earliestRestoreDate}'
```

### 2. Multi-Region Deployment
```bash
# Create secondary region deployment
az group create --name rg-towntrek-dr --location westus

# Set up geo-replication for database
az sql db replica create \
  --name TownTrekDb_Prod \
  --partner-server towntrek-sql-server-dr \
  --partner-resource-group rg-towntrek-dr \
  --resource-group rg-towntrek-prod \
  --server towntrek-sql-server
```

This comprehensive guide provides everything needed to deploy TownTrek to Azure in a production-ready configuration with proper security, monitoring, and disaster recovery measures.