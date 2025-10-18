# 🚀 VermillionIndia - GitHub Actions Deployment Guide

This guide will help you deploy to Azure using GitHub Actions - no Azure CLI installation needed on your local machine!

---

## ✅ Advantages of GitHub Actions Deployment

- ✅ **No local Azure CLI needed** - Everything runs in the cloud
- ✅ **Automatic deployments** - Push code and it deploys automatically
- ✅ **Version control** - Track all deployments in Git history
- ✅ **Rollback capability** - Easy to revert to previous versions
- ✅ **Professional CI/CD** - Industry-standard deployment pipeline
- ✅ **Free for public repos** - 2000 minutes/month for private repos

---

## 📋 Prerequisites

1. ✅ **GitHub account** - Free account is fine
2. ✅ **Azure account** - Your Visual Studio Enterprise Subscription
3. ✅ **Your code** - Already have this in `E:\Attendance`

---

## Step 1: Create GitHub Repository

### A. Initialize Git (if not already done)

```powershell
cd E:\Attendance

# Initialize git repository
git init

# Add all files
git add .

# Make first commit
git commit -m "Initial commit - VermillionIndia Attendance System"
```

### B. Create Repository on GitHub

1. Go to: https://github.com/new
2. **Repository name**: `vermillionIndia-attendance` (or your preferred name)
3. **Visibility**: Private (recommended) or Public
4. ✅ **Don't** initialize with README (you already have files)
5. Click "Create repository"

### C. Push Your Code to GitHub

```powershell
# Add GitHub as remote (replace YOUR-USERNAME with your GitHub username)
git remote add origin https://github.com/YOUR-USERNAME/vermillionIndia-attendance.git

# Push code to GitHub
git branch -M main
git push -u origin main
```

---

## Step 2: Create Azure Service Principal

This gives GitHub permission to create resources in your Azure subscription.

### A. Login to Azure Portal

1. Go to: https://portal.azure.com
2. Login with your Visual Studio Enterprise account

### B. Open Cloud Shell

1. Click the **Cloud Shell** icon (>_) in the top menu
2. Select **PowerShell** or **Bash**
3. Wait for it to initialize

### C. Create Service Principal

Run this command in Cloud Shell:

```bash
# Create service principal with Contributor role
az ad sp create-for-rbac \
  --name "GitHub-VermillionIndia-Actions" \
  --role contributor \
  --scopes /subscriptions/YOUR-SUBSCRIPTION-ID \
  --sdk-auth
```

**To get your subscription ID:**
```bash
az account show --query id -o tsv
```

**Output will look like:**
```json
{
  "clientId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "clientSecret": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "subscriptionId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "tenantId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "activeDirectoryEndpointUrl": "https://login.microsoftonline.com",
  "resourceManagerEndpointUrl": "https://management.azure.com/",
  ...
}
```

**⚠️ IMPORTANT: Copy this entire JSON output! You'll need it in the next step.**

---

## Step 3: Add Secrets to GitHub

### A. Go to Repository Settings

1. Go to your GitHub repository
2. Click **Settings** tab
3. Click **Secrets and variables** → **Actions**
4. Click **New repository secret**

### B. Add Required Secrets

#### Secret 1: AZURE_CREDENTIALS
- **Name**: `AZURE_CREDENTIALS`
- **Value**: Paste the entire JSON from Step 2
- Click **Add secret**

#### Secret 2: SQL_ADMIN_PASSWORD
- **Name**: `SQL_ADMIN_PASSWORD`
- **Value**: Create a strong password like: `VermInd@2025!Strong#Pass`
- Requirements: 8+ chars, uppercase, lowercase, number, special char
- Click **Add secret**

**You now have 2 secrets configured!** ✅

---

## Step 4: Deploy Azure Infrastructure

Now let's create all Azure resources using GitHub Actions!

### A. Go to Actions Tab

1. In your GitHub repository, click **Actions** tab
2. You'll see workflow: **Setup Azure Infrastructure**
3. Click on it

### B. Run the Workflow

1. Click **Run workflow** button (right side)
2. Fill in the inputs:
   - **Resource Group Name**: `vermillion-attendance-rg`
   - **Azure Region**: Select `westeurope` (West Europe - Netherlands/Amsterdam)
3. Click **Run workflow**

### C. Monitor Progress

The workflow will run for about **5-10 minutes**:

- ✅ Create Resource Group
- ✅ Create SQL Server
- ✅ Create SQL Database
- ✅ Create App Service Plan
- ✅ Create Web App (Backend)
- ✅ Create Static Web App (Frontend)
- ✅ Configure everything

**Watch the logs in real-time!**

---

## Step 5: Get Deployment Credentials

After infrastructure is created, you need to get credentials for automatic deployments.

### A. Get Backend Publish Profile

In Azure Cloud Shell, run:

```bash
# Replace with your actual web app name from Step 4 output
az webapp deployment list-publishing-profiles \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg \
  --xml
```

**Copy the entire XML output.**

### B. Get Frontend API Token

In Azure Cloud Shell, run:

```bash
# Replace with your actual static web app name from Step 4 output
az staticwebapp secrets list \
  --name vermillion-web-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg
```

**Copy the API token value (starts with `deployment_token`).**

### C. Add These Secrets to GitHub

Go back to: **Settings** → **Secrets and variables** → **Actions**

#### Secret 3: AZURE_WEBAPP_PUBLISH_PROFILE
- **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
- **Value**: Paste the XML from backend publish profile
- Click **Add secret**

#### Secret 4: AZURE_STATIC_WEB_APPS_API_TOKEN
- **Name**: `AZURE_STATIC_WEB_APPS_API_TOKEN`
- **Value**: Paste the token from static web app
- Click **Add secret**

**You now have 4 secrets configured!** ✅

---

## Step 6: Update Workflow Files

Update the workflow files with your actual resource names:

### A. Update Backend Workflow

Edit: `.github/workflows/deploy-backend.yml`

```yaml
env:
  AZURE_WEBAPP_NAME: vermillion-api-XXXXXXXXXX  # ⬅ Change to your actual name
```

### B. Update Frontend Environment

Edit: `frontend/src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://vermillion-api-XXXXXXXXXX.azurewebsites.net/api'  // ⬅ Update
};
```

### C. Commit and Push Changes

```powershell
git add .
git commit -m "Update deployment configuration with actual resource names"
git push
```

---

## Step 7: Automatic Deployment

Now every time you push code, it automatically deploys!

### Manual Deployment (First Time)

1. Go to **Actions** tab
2. Click **Deploy Backend to Azure**
3. Click **Run workflow** → **Run workflow**
4. Wait for completion (~3-5 minutes)

5. Click **Deploy Frontend to Azure**
6. Click **Run workflow** → **Run workflow**
7. Wait for completion (~2-3 minutes)

### Automatic Deployment (From Now On)

Just push your code:

```powershell
# Make changes to your code
git add .
git commit -m "Added new feature"
git push
```

**GitHub Actions will automatically:**
- ✅ Build your code
- ✅ Run tests
- ✅ Deploy to Azure
- ✅ Notify you of success/failure

---

## 📊 Monitoring Deployments

### View Deployment Status

1. Go to **Actions** tab in GitHub
2. See all deployments with status (✅ success, ❌ failed)
3. Click any deployment to see detailed logs

### View Live Application

1. **Frontend**: `https://vermillion-web-XXXX.azurestaticapps.net`
2. **Backend API**: `https://vermillion-api-XXXX.azurewebsites.net`

### View in Azure Portal

1. Go to: https://portal.azure.com
2. Navigate to **Resource Groups**
3. Click `vermillion-attendance-rg`
4. See all your resources

---

## 🔧 Workflow Triggers

### Backend Deploys When:
- You push changes to `backend/**` folder
- You manually trigger from Actions tab
- Someone creates a pull request

### Frontend Deploys When:
- You push changes to `frontend/**` folder
- You manually trigger from Actions tab
- Someone creates a pull request

---

## 🔒 Security Best Practices

### ✅ Current Security (Already Configured):

- ✅ Secrets stored in GitHub (encrypted)
- ✅ Service principal with minimal permissions
- ✅ SQL firewall enabled
- ✅ HTTPS enforced
- ✅ Connection strings in Azure (not in code)

### 🔐 Additional Security (Recommended):

#### 1. Enable Branch Protection

Settings → Branches → Add rule:
- Branch name pattern: `main`
- ✅ Require pull request reviews
- ✅ Require status checks to pass

#### 2. Rotate SQL Password Regularly

```bash
az sql server update \
  --name vermillion-sql-XXXX \
  --resource-group vermillion-attendance-rg \
  --admin-password "NewPassword123!"
```

Then update `SQL_ADMIN_PASSWORD` secret in GitHub.

#### 3. Enable Azure AD Authentication (Optional)

For production, consider Azure AD instead of SQL authentication.

---

## 💰 Cost Summary

| Service | Tier | Cost/Month |
|---------|------|------------|
| Frontend (Static Web App) | Free | $0.00 |
| Backend (App Service B1) | 1 instance | $13.14 |
| Database (Azure SQL Basic) | 2GB | $4.90 |
| GitHub Actions | Free/Included | $0.00 |
| **TOTAL** | | **$18.04/month** |

**GitHub Actions**: 
- 2000 minutes/month free (private repos)
- Unlimited for public repos
- Each deployment uses ~5 minutes

---

## 🐛 Troubleshooting

### Issue 1: "Failed to authenticate"

**Solution:**
- Check `AZURE_CREDENTIALS` secret is correct
- Verify service principal has "Contributor" role
- Re-create service principal if needed

### Issue 2: "Resource name already exists"

**Solution:**
Edit `.github/workflows/azure-infrastructure.yml` and change:
```yaml
SQL_SERVER_NAME="vermillion-sql-$(date +%s)"  # Adds timestamp for uniqueness
```

### Issue 3: "Workflow not triggering"

**Solution:**
- Check workflow file is in `.github/workflows/` folder
- Check YAML syntax is correct (indentation matters!)
- Manually trigger from Actions tab first

### Issue 4: "Database connection failed"

**Solution:**
```bash
# Allow your IP in SQL firewall
az sql server firewall-rule create \
  --resource-group vermillion-attendance-rg \
  --server vermillion-sql-XXXX \
  --name "MyIP" \
  --start-ip-address YOUR.IP.ADDRESS \
  --end-ip-address YOUR.IP.ADDRESS
```

### Issue 5: "Build failed"

**Solution:**
- Check error logs in Actions tab
- Common issues:
  - Missing packages: Run `dotnet restore` locally first
  - TypeScript errors: Run `npm run build` locally first
  - Tests failing: Fix tests or set `continue-on-error: true`

---

## 📝 Useful Commands

### View Deployment History
```bash
# Backend deployments
az webapp deployment list \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg

# Frontend deployments
az staticwebapp show \
  --name vermillion-web-XXXX \
  --resource-group vermillion-attendance-rg
```

### View Application Logs
```bash
# Stream backend logs
az webapp log tail \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg
```

### Rollback Deployment
```bash
# List previous backend versions
az webapp deployment list \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg

# Rollback to specific version
az webapp deployment source config-zip \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg \
  --src path/to/previous-version.zip
```

---

## 🎯 Deployment Workflow Summary

```
Local Development → Git Commit → Git Push → GitHub Actions
                                                ↓
                                    ┌─────────────────────┐
                                    │  Automatic Build    │
                                    │  - Restore packages │
                                    │  - Compile code     │
                                    │  - Run tests        │
                                    └─────────────────────┘
                                                ↓
                                    ┌─────────────────────┐
                                    │  Deploy to Azure    │
                                    │  - Backend API      │
                                    │  - Frontend SPA     │
                                    └─────────────────────┘
                                                ↓
                                    ┌─────────────────────┐
                                    │  Live Application   │
                                    │  ✅ Deployed!        │
                                    └─────────────────────┘
```

---

## 🚀 Quick Start Checklist

- [ ] Create GitHub repository
- [ ] Push code to GitHub
- [ ] Create Azure Service Principal
- [ ] Add 4 secrets to GitHub
- [ ] Run "Setup Azure Infrastructure" workflow
- [ ] Get publish profiles and add to secrets
- [ ] Update workflow files with resource names
- [ ] Run "Deploy Backend" workflow
- [ ] Run "Deploy Frontend" workflow
- [ ] Test application!

**Total time: ~30 minutes** ⏱️

---

## 🆘 Need Help?

### GitHub Actions Documentation
- https://docs.github.com/en/actions

### Azure Documentation
- https://docs.microsoft.com/en-us/azure/

### Check Workflow Logs
- GitHub → Actions → Click any workflow run → View logs

### Community Support
- GitHub Discussions
- Stack Overflow
- Azure Forums

---

## 🎉 Congratulations!

You now have a professional CI/CD pipeline! 🚀

**Every time you push code:**
- ✅ Automatic build
- ✅ Automatic tests
- ✅ Automatic deployment
- ✅ Instant rollback capability
- ✅ Full deployment history

**No Azure CLI installation needed on your local machine!**

---

**Ready to start?** Follow Step 1 above! 🚀
