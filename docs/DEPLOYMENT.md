# Deployment Guide

## Overview

This guide covers deploying both frontend (Angular) and backend (.NET Core) applications independently.

---

## Prerequisites

### Development Tools
- Git 2.40+
- Node.js 18+ and npm 9+
- .NET 8 SDK
- SQL Server 2019+ or PostgreSQL 14+
- Visual Studio Code or Visual Studio 2022

### Cloud Services (Optional)
- Azure Account (for Azure deployment)
- AWS Account (for AWS deployment)
- Docker Hub Account (for container deployment)

---

## Frontend Deployment (Angular)

### Local Development

```powershell
cd frontend
npm install
npm start
```

Access at: `http://localhost:4200`

### Production Build

```powershell
cd frontend
npm run build -- --configuration production
```

Output directory: `frontend/dist/attendance-app`

### Environment Configuration

**development** (`src/environments/environment.ts`):
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5001/api',
  appName: 'Attendance Management',
  tokenKey: 'auth_token'
};
```

**production** (`src/environments/environment.prod.ts`):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.attendance.company.com/api',
  appName: 'Attendance Management',
  tokenKey: 'auth_token'
};
```

### Deployment Options

#### 1. Azure Static Web Apps

```powershell
# Install Azure CLI
# Login to Azure
az login

# Create resource group
az group create --name attendance-rg --location eastus

# Create static web app
az staticwebapp create `
  --name attendance-frontend `
  --resource-group attendance-rg `
  --source . `
  --location eastus `
  --branch main `
  --app-location "frontend" `
  --output-location "dist/attendance-app"
```

#### 2. Azure Blob Storage + CDN

```powershell
# Create storage account
az storage account create `
  --name attendancestorage `
  --resource-group attendance-rg `
  --location eastus `
  --sku Standard_LRS

# Enable static website
az storage blob service-properties update `
  --account-name attendancestorage `
  --static-website `
  --index-document index.html `
  --404-document index.html

# Upload build files
az storage blob upload-batch `
  --account-name attendancestorage `
  --source frontend/dist/attendance-app `
  --destination '$web'
```

#### 3. Netlify

```powershell
# Install Netlify CLI
npm install -g netlify-cli

# Login to Netlify
netlify login

# Deploy
cd frontend
netlify deploy --prod --dir=dist/attendance-app
```

**netlify.toml**:
```toml
[build]
  command = "npm run build -- --configuration production"
  publish = "dist/attendance-app"

[[redirects]]
  from = "/*"
  to = "/index.html"
  status = 200
```

#### 4. Docker Container

**Dockerfile** (frontend/Dockerfile):
```dockerfile
# Build stage
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

# Production stage
FROM nginx:alpine
COPY --from=build /app/dist/attendance-app /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

**nginx.conf**:
```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }
}
```

Build and run:
```powershell
docker build -t attendance-frontend:latest ./frontend
docker run -p 80:80 attendance-frontend:latest
```

---

## Backend Deployment (.NET Core)

### Local Development

```powershell
cd backend
dotnet restore
dotnet run --project AttendanceAPI
```

Access at: `https://localhost:7001` and `http://localhost:5001`

### Production Build

```powershell
cd backend
dotnet publish -c Release -o ./publish
```

### Configuration

**appsettings.json** (Development):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AttendanceDB;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters-long",
    "Issuer": "AttendanceAPI",
    "Audience": "AttendanceApp",
    "ExpirationMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["http://localhost:4200"]
  }
}
```

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=AttendanceDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
  },
  "Jwt": {
    "Secret": "production-secret-key-should-be-stored-in-key-vault",
    "Issuer": "AttendanceAPI",
    "Audience": "AttendanceApp",
    "ExpirationMinutes": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "Cors": {
    "AllowedOrigins": ["https://attendance.company.com"]
  }
}
```

### Deployment Options

#### 1. Azure App Service

```powershell
# Create App Service Plan
az appservice plan create `
  --name attendance-plan `
  --resource-group attendance-rg `
  --sku B1 `
  --is-linux

# Create Web App
az webapp create `
  --name attendance-api `
  --resource-group attendance-rg `
  --plan attendance-plan `
  --runtime "DOTNET|8.0"

# Deploy
cd backend
az webapp deploy `
  --resource-group attendance-rg `
  --name attendance-api `
  --src-path ./publish.zip `
  --type zip
```

#### 2. Docker Container

**Dockerfile** (backend/Dockerfile):
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AttendanceAPI/AttendanceAPI.csproj", "AttendanceAPI/"]
RUN dotnet restore "AttendanceAPI/AttendanceAPI.csproj"
COPY . .
WORKDIR "/src/AttendanceAPI"
RUN dotnet build "AttendanceAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AttendanceAPI.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "AttendanceAPI.dll"]
```

Build and run:
```powershell
docker build -t attendance-api:latest ./backend
docker run -p 5001:80 -p 7001:443 attendance-api:latest
```

#### 3. IIS Deployment (Windows Server)

1. Install prerequisites:
   - .NET 8 Hosting Bundle
   - IIS with ASP.NET Core Module

2. Create IIS Site:
   ```powershell
   # Create application pool
   New-WebAppPool -Name "AttendanceAPI" -Force
   Set-ItemProperty IIS:\AppPools\AttendanceAPI -name "managedRuntimeVersion" -value ""
   
   # Create website
   New-Website -Name "AttendanceAPI" `
     -ApplicationPool "AttendanceAPI" `
     -PhysicalPath "C:\inetpub\wwwroot\attendance-api" `
     -Port 5001
   ```

3. Copy published files to `C:\inetpub\wwwroot\attendance-api`

4. Configure web.config (auto-generated during publish)

#### 4. AWS Elastic Beanstalk

```powershell
# Install EB CLI
# pip install awsebcli

# Initialize EB application
cd backend
eb init -p "64bit Amazon Linux 2023 v3.0.0 running .NET 8" attendance-api

# Create environment
eb create attendance-api-env

# Deploy
eb deploy
```

---

## Database Deployment

### SQL Server (Azure SQL Database)

```powershell
# Create SQL Server
az sql server create `
  --name attendance-sql-server `
  --resource-group attendance-rg `
  --location eastus `
  --admin-user sqladmin `
  --admin-password YourStrongPassword123!

# Create database
az sql db create `
  --resource-group attendance-rg `
  --server attendance-sql-server `
  --name AttendanceDB `
  --service-objective S0

# Configure firewall
az sql server firewall-rule create `
  --resource-group attendance-rg `
  --server attendance-sql-server `
  --name AllowAzureServices `
  --start-ip-address 0.0.0.0 `
  --end-ip-address 0.0.0.0
```

### Run Migrations

```powershell
cd backend
dotnet ef database update --project AttendanceAPI
```

Or using SQL scripts:
```powershell
sqlcmd -S attendance-sql-server.database.windows.net -d AttendanceDB -U sqladmin -P YourPassword -i schema.sql
```

---

## Docker Compose (Full Stack)

**docker-compose.yml**:
```yaml
version: '3.8'

services:
  frontend:
    build: ./frontend
    ports:
      - "80:80"
    depends_on:
      - backend
    environment:
      - API_URL=http://backend:80/api

  backend:
    build: ./backend
    ports:
      - "5001:80"
    depends_on:
      - database
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=database;Database=AttendanceDB;User Id=sa;Password=YourStrong@Password;TrustServerCertificate=True

  database:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
      - MSSQL_PID=Developer
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

Run:
```powershell
docker-compose up -d
```

---

## CI/CD Pipeline

### GitHub Actions

**.github/workflows/deploy.yml**:
```yaml
name: Deploy Attendance System

on:
  push:
    branches: [ main ]

jobs:
  deploy-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install dependencies
        run: |
          cd frontend
          npm ci
      
      - name: Build
        run: |
          cd frontend
          npm run build -- --configuration production
      
      - name: Deploy to Azure Static Web Apps
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
          repo_token: ${{ secrets.GITHUB_TOKEN }}
          action: "upload"
          app_location: "frontend"
          output_location: "dist/attendance-app"

  deploy-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: |
          cd backend
          dotnet restore
      
      - name: Build
        run: |
          cd backend
          dotnet build --configuration Release --no-restore
      
      - name: Publish
        run: |
          cd backend
          dotnet publish -c Release -o ./publish
      
      - name: Deploy to Azure App Service
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'attendance-api'
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: './backend/publish'
```

---

## SSL/TLS Configuration

### Let's Encrypt (Free SSL)

```powershell
# Install Certbot
# For Windows, download from https://certbot.eff.org/

# Generate certificate
certbot certonly --standalone -d api.attendance.company.com

# Certificate location: C:\Certbot\live\api.attendance.company.com\
```

### Azure App Service
SSL is automatically configured for `*.azurewebsites.net` domains.

For custom domains:
```powershell
az webapp config ssl bind `
  --resource-group attendance-rg `
  --name attendance-api `
  --certificate-thumbprint <thumbprint> `
  --ssl-type SNI
```

---

## Monitoring & Logging

### Application Insights (Azure)

```powershell
# Create Application Insights
az monitor app-insights component create `
  --app attendance-insights `
  --location eastus `
  --resource-group attendance-rg `
  --application-type web

# Get instrumentation key
az monitor app-insights component show `
  --app attendance-insights `
  --resource-group attendance-rg `
  --query instrumentationKey
```

Add to backend appsettings.json:
```json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}
```

---

## Health Checks

Backend includes health check endpoint:
- `/health` - Basic health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

Configure in Kubernetes:
```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 80
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 80
  initialDelaySeconds: 5
  periodSeconds: 5
```

---

## Backup Strategy

### Database Backups

```powershell
# Azure SQL automated backups (7-35 days retention)
# Manual backup
az sql db export `
  --resource-group attendance-rg `
  --server attendance-sql-server `
  --name AttendanceDB `
  --admin-user sqladmin `
  --admin-password YourPassword `
  --storage-key-type StorageAccessKey `
  --storage-key <key> `
  --storage-uri https://attendancestorage.blob.core.windows.net/backups/db.bacpac
```

---

## Troubleshooting

### Frontend Issues
- Clear browser cache
- Check console for errors
- Verify API URL in environment files
- Check CORS settings

### Backend Issues
- Check application logs
- Verify database connection string
- Test database connectivity
- Check JWT secret configuration

### Database Issues
- Verify connection string
- Check firewall rules
- Ensure migrations are applied
- Verify user permissions

---

**Last Updated**: October 15, 2025
