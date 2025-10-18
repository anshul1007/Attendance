# 🎯 Fresh Start Deployment Guide - Environment-Based Naming

This guide will help you set up VermillionIndia Attendance System from scratch with clean, environment-based naming (prod, dev, staging).

---

## 🧹 Step 1: Clean Up Existing Resources

### A. Delete Azure Resources

1. Go to: https://portal.azure.com
2. Search for: `vermillion-attendance-rg`
3. Click on the resource group
4. Click **"Delete resource group"** at the top
5. Type the resource group name to confirm: `vermillion-attendance-rg`
6. Click **"Delete"**

⏱️ Wait 5-10 minutes for complete deletion.

### B. Delete GitHub Secrets

1. Go to: https://github.com/anshul1007/Attendance
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Delete ALL existing secrets:
   - `AZURE_CREDENTIALS` (keep this one actually - reusable!)
   - `SQL_ADMIN_PASSWORD` → Delete
   - `AZURE_WEBAPP_PUBLISH_PROFILE` → Delete
   - `AZURE_STATIC_WEB_APPS_API_TOKEN` → Delete

**Note:** Keep `AZURE_CREDENTIALS` if you have it - it's reusable across environments!

---

## 🆕 Step 2: Update Code with New Workflows

The workflows have been updated with environment-based naming!

### Commit and Push the Changes

```powershell
cd E:\Attendance
git add .
git commit -m "Refactor: Use environment-based naming (prod/dev/staging) instead of timestamps"
git push
```

---

## 🔐 Step 3: Create GitHub Secrets

Go to: **Settings → Secrets and variables → Actions**

### Secret 1: AZURE_CREDENTIALS (if not already set)

**In Azure Cloud Shell:**
```bash
# Get subscription ID
az account show --query id -o tsv

# Create service principal (copy the entire JSON output)
az ad sp create-for-rbac \
  --name "GitHub-VermillionIndia-Actions" \
  --role contributor \
  --scopes /subscriptions/YOUR-SUBSCRIPTION-ID \
  --sdk-auth
```

**Add in GitHub:**
- Name: `AZURE_CREDENTIALS`
- Value: Paste the entire JSON

### Secret 2: SQL_ADMIN_PASSWORD_PROD

Create a strong password for production SQL Server.

**Add in GitHub:**
- Name: `SQL_ADMIN_PASSWORD_PROD`
- Value: `YourStrongPassword123!` (create your own strong password)

**Requirements:**
- At least 8 characters
- Contains uppercase letter
- Contains lowercase letter
- Contains number
- Contains special character

---

## 🚀 Step 4: Run Infrastructure Setup

### A. Trigger the Workflow

1. Go to: **Actions** tab
2. Click **"Setup Azure Infrastructure"**
3. Click **"Run workflow"**
4. Select inputs:
   - **Environment**: `prod`
   - **Resource Group Name**: `vermillion-attendance-rg`
   - **Azure Region**: `westeurope` (or your choice)
5. Click **"Run workflow"**

### B. Wait for Completion

⏱️ Takes 10-15 minutes

The workflow will create:
- ✅ Resource Group: `vermillion-attendance-rg`
- ✅ SQL Server: `vermillion-sql-prod`
- ✅ SQL Database: `AttendanceDB`
- ✅ App Service Plan: `vermillion-attendance-plan-prod`
- ✅ Web App (Backend): `vermillion-api-prod`
- ✅ Static Web App (Frontend): `vermillion-web-prod`

### C. Note Your URLs

At the end of the workflow, you'll see:

```
✅ INFRASTRUCTURE CREATED SUCCESSFULLY!

🌍 Environment: prod
📱 Frontend URL: https://vermillion-web-prod.azurestaticapps.net
🔧 Backend URL: https://vermillion-api-prod.azurewebsites.net
💾 SQL Server: vermillion-sql-prod.database.windows.net
```

**Copy these URLs!**

---

## 🔑 Step 5: Get Deployment Credentials

### Open Azure Cloud Shell

Go to: https://portal.azure.com → Click (>_) icon

### A. Get Backend Publish Profile

```bash
az webapp deployment list-publishing-profiles \
  --name vermillion-api-prod \
  --resource-group vermillion-attendance-rg \
  --xml
```

**Copy the entire XML output.**

### B. Get Frontend Deployment Token

```bash
az staticwebapp secrets list \
  --name vermillion-web-prod \
  --resource-group vermillion-attendance-rg \
  --query "properties.apiKey" \
  -o tsv
```

**Copy the token.**

---

## 📝 Step 6: Add Deployment Secrets

Go to: **Settings → Secrets and variables → Actions → New secret**

### Secret 3: AZURE_WEBAPP_PUBLISH_PROFILE_PROD

- Name: `AZURE_WEBAPP_PUBLISH_PROFILE_PROD`
- Value: Paste the XML from Step 5A

### Secret 4: AZURE_STATIC_WEB_APPS_API_TOKEN_PROD

- Name: `AZURE_STATIC_WEB_APPS_API_TOKEN_PROD`
- Value: Paste the token from Step 5B

---

## ⚙️ Step 7: Update Frontend Configuration

Edit: `frontend/src/environments/environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://vermillion-api-prod.azurewebsites.net/api'  // ⬅ Update this
};
```

**Commit and push:**
```powershell
git add frontend/src/environments/environment.prod.ts
git commit -m "Configure frontend to use prod backend API"
git push
```

---

## 🎯 Step 8: Deploy Backend

1. Go to: **Actions** tab
2. Click **"Deploy Backend to Azure"**
3. Click **"Run workflow"**
4. Select **Environment**: `prod`
5. Click **"Run workflow"**

⏱️ Wait 3-5 minutes

**Verify:**
- Open: `https://vermillion-api-prod.azurewebsites.net`
- Should see a response (even if 404 is fine - means it's running)

---

## 🎨 Step 9: Deploy Frontend

1. Go to: **Actions** tab
2. Click **"Deploy Frontend to Azure"**
3. Click **"Run workflow"**
4. Select **Environment**: `prod`
5. Click **"Run workflow"**

⏱️ Wait 5-7 minutes

**Verify:**
- Open: `https://vermillion-web-prod.azurestaticapps.net`
- Should see VermillionIndia login page

---

## 🧪 Step 10: Test the Application

### A. Open Frontend URL

```
https://vermillion-web-prod.azurestaticapps.net
```

### B. Login

- Username: `admin`
- Password: `Admin@123`

### C. Test Features

- ✅ Login works
- ✅ Dashboard loads
- ✅ Create employee
- ✅ Mark attendance
- ✅ Apply for leave

---

## 📊 Your New Resource Names

| Resource Type | Production Name | Future Dev Name | Future Staging Name |
|---------------|----------------|-----------------|---------------------|
| SQL Server | `vermillion-sql-prod` | `vermillion-sql-dev` | `vermillion-sql-staging` |
| Backend API | `vermillion-api-prod` | `vermillion-api-dev` | `vermillion-api-staging` |
| Frontend | `vermillion-web-prod` | `vermillion-web-dev` | `vermillion-web-staging` |
| App Service Plan | `vermillion-attendance-plan-prod` | `vermillion-attendance-plan-dev` | `vermillion-attendance-plan-staging` |

**Benefits:**
- ✅ Clean, predictable names
- ✅ Easy to identify environment
- ✅ Can run multiple environments in parallel
- ✅ No timestamp confusion
- ✅ Simple to reference in scripts

---

## 🔐 GitHub Secrets Summary

After setup, you should have **4 secrets**:

| Secret Name | Environment | Value |
|-------------|-------------|-------|
| `AZURE_CREDENTIALS` | All | Service principal JSON |
| `SQL_ADMIN_PASSWORD_PROD` | Production | SQL admin password |
| `AZURE_WEBAPP_PUBLISH_PROFILE_PROD` | Production | Backend publish profile XML |
| `AZURE_STATIC_WEB_APPS_API_TOKEN_PROD` | Production | Frontend deployment token |

**For future dev environment, add:**
- `SQL_ADMIN_PASSWORD_DEV`
- `AZURE_WEBAPP_PUBLISH_PROFILE_DEV`
- `AZURE_STATIC_WEB_APPS_API_TOKEN_DEV`

---

## 🎯 Future: Adding Dev Environment

When you want a dev environment:

### 1. Create Secrets

Add these GitHub secrets:
- `SQL_ADMIN_PASSWORD_DEV`
- `AZURE_WEBAPP_PUBLISH_PROFILE_DEV`
- `AZURE_STATIC_WEB_APPS_API_TOKEN_DEV`

### 2. Run Infrastructure Workflow

- Select **Environment**: `dev`
- Resources will be created as: `vermillion-xxx-dev`

### 3. Deploy

- Run backend deployment with **Environment**: `dev`
- Run frontend deployment with **Environment**: `dev`

**Total cost for 2 environments: ~$36/month** ($18 × 2)

---

## 💰 Cost Breakdown

### Production Environment:
- Frontend: $0 (Free)
- Backend: $13 (App Service B1)
- Database: $5 (Azure SQL Basic)
- **Total: ~$18/month**

### Optional Dev Environment:
- Same resources with `-dev` suffix
- **Additional: ~$18/month**

---

## ✅ Verification Checklist

After completing all steps:

- [ ] Old resources deleted from Azure Portal
- [ ] Old GitHub secrets deleted
- [ ] New code pushed to GitHub
- [ ] 4 GitHub secrets created (with _PROD suffix)
- [ ] Infrastructure workflow completed successfully
- [ ] Backend deployed successfully
- [ ] Frontend deployed successfully
- [ ] Can access frontend URL
- [ ] Can login with admin credentials
- [ ] All features working

---

## 🎉 Congratulations!

You now have a clean, professional deployment with:
- ✅ Environment-based naming (prod/dev/staging)
- ✅ No timestamp confusion
- ✅ Scalable to multiple environments
- ✅ Clean resource names
- ✅ Organized GitHub secrets

**Your production app is live at:**
- Frontend: `https://vermillion-web-prod.azurestaticapps.net`
- Backend: `https://vermillion-api-prod.azurewebsites.net`

**Total time: ~30-40 minutes**
**Monthly cost: ~$18 USD**

---

**Ready to start?** Begin with Step 1 (cleanup) and follow through! 🚀
