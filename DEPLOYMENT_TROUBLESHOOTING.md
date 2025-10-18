# Azure Deployment - Complete Troubleshooting Guide

## 🎯 Quick Reference

If your deployment fails, check this guide for solutions!

---

## ✅ Issues Already Fixed (No Action Needed)

The workflow now automatically handles these common issues:

| Issue | Status | How It's Fixed |
|-------|--------|----------------|
| Invalid region name (`centraleurope`) | ✅ Auto-Fixed | Changed to `westeurope` |
| Missing resource providers | ✅ Auto-Fixed | Auto-registers Microsoft.Sql, Web, Storage, Insights |
| Region capacity full | ✅ Auto-Fixed | Tries 7+ regions automatically |
| SQL Server name conflicts | ✅ Auto-Fixed | Generates unique name with timestamp |

---

## 🔧 Current Deployment Issues

### Issue 1: "SubscriptionDoesNotHaveServer" Error

**Error Message:**
```
ERROR: Subscription does not have the server 'vermillion-sql-XXXXXXXXXX'
```

**What Happened:**
SQL Server creation failed silently, but the workflow continued to the next step.

**Solution (Already Applied):**
✅ Updated workflow to:
- Generate new server name for each region attempt
- Check if SQL Server was actually created
- Exit with clear error if all regions fail
- Show which region succeeded

**Action:** Re-run the workflow - it's now fixed!

---

## 🚀 How to Run the Workflow Successfully

### Step 1: Verify Secrets Are Set

Go to: **Repository → Settings → Secrets and variables → Actions**

You should have these 4 secrets:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `AZURE_CREDENTIALS` | Azure service principal JSON | `{"clientId": "xxx", ...}` |
| `SQL_ADMIN_PASSWORD` | SQL Server admin password | `VermInd@2025!Strong#` |
| `AZURE_WEBAPP_PUBLISH_PROFILE` | Backend publish profile (XML) | `<publishData>...</publishData>` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Frontend deployment token | Long alphanumeric string |

**Note:** Secrets 3 & 4 are only needed AFTER infrastructure is created (Step 5).

### Step 2: Run "Setup Azure Infrastructure" Workflow

1. Go to **Actions** tab
2. Click **"Setup Azure Infrastructure"** (left sidebar)
3. Click **"Run workflow"** button (right side)
4. Fill in:
   - Resource Group: `vermillion-attendance-rg`
   - Location: `westeurope` (or any from dropdown)
5. Click **"Run workflow"**

### Step 3: Watch the Logs

The workflow will show progress:

```
✅ Azure Login
✅ Register Azure Resource Providers (2-5 min)
✅ Create Resource Group
✅ Create Azure SQL Server (tries multiple regions if needed)
✅ Configure SQL Firewall
✅ Create SQL Database
✅ Create App Service Plan
✅ Create Web App
✅ Configure Connection String
✅ Configure App Settings
✅ Create Static Web App
✅ Get Deployment URLs
```

**Total time: 10-15 minutes**

### Step 4: Check Deployment Summary

At the end, you'll see:

```
✅ INFRASTRUCTURE CREATED SUCCESSFULLY!

📱 Frontend URL: https://vermillion-web-XXXX.azurestaticapps.net
🔧 Backend URL: https://vermillion-api-XXXX.azurewebsites.net
💾 SQL Server: vermillion-sql-XXXX.database.windows.net
🌍 SQL Location: northeurope (or whichever region worked)
```

**Copy these URLs!** You'll need them later.

### Step 5: Add Remaining Secrets

After infrastructure is created, get the deployment credentials:

**In Azure Cloud Shell (portal.azure.com → click >_):**

```bash
# Replace XXXX with your actual web app name from Step 4
az webapp deployment list-publishing-profiles \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg \
  --xml

# Replace XXXX with your actual static app name from Step 4
az staticwebapp secrets list \
  --name vermillion-web-XXXX \
  --resource-group vermillion-attendance-rg
```

Add these as GitHub secrets:
- `AZURE_WEBAPP_PUBLISH_PROFILE` - Full XML output from first command
- `AZURE_STATIC_WEB_APPS_API_TOKEN` - Token value from second command

### Step 6: Deploy Backend & Frontend

1. Go to **Actions** tab
2. Run **"Deploy Backend to Azure"** workflow
3. Run **"Deploy Frontend to Azure"** workflow

✅ Done! Your application is live!

---

## 🐛 Common Errors & Solutions

### Error: "Azure login failed"

**Cause:** Invalid `AZURE_CREDENTIALS` secret

**Solution:**
1. Go to Azure Cloud Shell
2. Run:
   ```bash
   az account show --query id -o tsv  # Get subscription ID
   
   az ad sp create-for-rbac \
     --name "GitHub-VermillionIndia-Actions" \
     --role contributor \
     --scopes /subscriptions/YOUR-SUBSCRIPTION-ID \
     --sdk-auth
   ```
3. Copy the ENTIRE JSON output
4. Update `AZURE_CREDENTIALS` secret in GitHub

---

### Error: "Resource providers not registered"

**Cause:** New Azure subscription

**Solution:** ✅ Already fixed! Workflow auto-registers providers.

**Manual fix (if needed):**
```bash
az provider register --namespace Microsoft.Sql --wait
az provider register --namespace Microsoft.Web --wait
```

---

### Error: "Location not accepting SQL Server creation"

**Cause:** Region at capacity

**Solution:** ✅ Already fixed! Workflow tries 7+ regions automatically.

**Manual fix (if needed):**
- Select different region in workflow dropdown
- Try `northeurope`, `francecentral`, or `eastus`

---

### Error: "SQL Server name already exists"

**Cause:** Previous failed deployment left partial resources

**Solution:** ✅ Already fixed! Each attempt generates new unique name.

**Manual cleanup (if needed):**
```bash
# Delete the resource group and start fresh
az group delete --name vermillion-attendance-rg --yes
```

---

### Error: "Subscription quota exceeded"

**Cause:** Free/trial subscription limits

**Solution:**
1. Check your subscription type in Azure Portal
2. Visual Studio Enterprise should have sufficient quota
3. Contact Azure support to increase limits
4. Or delete unused resources:
   ```bash
   az resource list --resource-group vermillion-attendance-rg -o table
   az group delete --name old-resource-group --yes
   ```

---

### Error: "Static Web App creation failed"

**Cause:** Region doesn't support Static Web Apps

**Solution:**
Static Web Apps have limited regions. Workflow will try:
- westeurope
- northeurope
- eastus
- westus2

**Manual fix:**
Edit workflow file `.github/workflows/azure-infrastructure.yml`:
```yaml
az staticwebapp create \
  --location eastus2  # Try different region
```

---

### Error: "Database migration failed"

**Cause:** Connection string not configured or database not accessible

**Solution:**
1. Check SQL firewall allows Azure services:
   ```bash
   az sql server firewall-rule list \
     --server vermillion-sql-XXXX \
     --resource-group vermillion-attendance-rg
   ```

2. Add your IP if needed:
   ```bash
   az sql server firewall-rule create \
     --server vermillion-sql-XXXX \
     --resource-group vermillion-attendance-rg \
     --name "MyIP" \
     --start-ip-address YOUR.IP.HERE \
     --end-ip-address YOUR.IP.HERE
   ```

3. Test connection:
   ```bash
   az sql db show-connection-string \
     --server vermillion-sql-XXXX \
     --name AttendanceDB \
     --client ado.net
   ```

---

## 🔍 Debugging Tips

### Check Workflow Logs

1. Go to **Actions** tab
2. Click on the failed workflow run
3. Click on the failed step (red X)
4. Read the error message carefully
5. Search for the error in this guide

### Check Azure Portal

1. Go to https://portal.azure.com
2. Navigate to **Resource Groups**
3. Click `vermillion-attendance-rg`
4. Check which resources were created
5. Check deployment history

### Check Resource Status

```bash
# List all resources in resource group
az resource list --resource-group vermillion-attendance-rg -o table

# Check SQL Server status
az sql server show \
  --name vermillion-sql-XXXX \
  --resource-group vermillion-attendance-rg

# Check App Service status
az webapp show \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg

# Check Static Web App status
az staticwebapp show \
  --name vermillion-web-XXXX \
  --resource-group vermillion-attendance-rg
```

### Check Application Logs

```bash
# Stream backend logs
az webapp log tail \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg

# Download backend logs
az webapp log download \
  --name vermillion-api-XXXX \
  --resource-group vermillion-attendance-rg \
  --log-file backend-logs.zip
```

---

## 🔄 Clean Start (Nuclear Option)

If everything is broken and you want to start fresh:

### Option 1: Delete Resource Group

```bash
# Delete everything
az group delete --name vermillion-attendance-rg --yes

# Wait 2-3 minutes for deletion to complete

# Run workflow again from scratch
```

### Option 2: Delete and Recreate Resources Individually

```bash
# Delete SQL Server
az sql server delete --name vermillion-sql-XXXX --resource-group vermillion-attendance-rg --yes

# Delete App Service
az webapp delete --name vermillion-api-XXXX --resource-group vermillion-attendance-rg

# Delete Static Web App
az staticwebapp delete --name vermillion-web-XXXX --resource-group vermillion-attendance-rg --yes

# Run workflow again
```

---

## 📞 Getting Help

### GitHub Actions Logs
- Most detailed information
- Shows exact error messages
- Available in your repository Actions tab

### Azure Portal
- Visual interface
- Resource health status
- Activity log for deployments

### Azure Cloud Shell
- Run diagnostic commands
- Check resource status
- Fix issues manually

### Azure Support
Visual Studio Enterprise includes support:
1. Go to https://portal.azure.com
2. Click "?" (Help) → "Support"
3. Create support ticket
4. Response within 24 hours

### Community Help
- Stack Overflow: Tag `azure`, `github-actions`
- Azure Forums: https://docs.microsoft.com/answers
- GitHub Discussions: In your repository

---

## ✅ Verification Checklist

After successful deployment:

- [ ] Infrastructure workflow completed successfully
- [ ] All 4 GitHub secrets are configured
- [ ] Backend deployment workflow succeeded
- [ ] Frontend deployment workflow succeeded
- [ ] Can access frontend URL in browser
- [ ] Can see login page with VermillionIndia branding
- [ ] Backend API responds (test: `https://YOUR-API.azurewebsites.net/api/auth/health`)
- [ ] Can login with admin credentials
- [ ] Database is accessible and seeded with admin user

---

## 🎉 Success Indicators

**You're successfully deployed when:**

✅ All 3 workflows show green checkmarks  
✅ Frontend URL loads the login page  
✅ Backend API health check returns 200 OK  
✅ Can login with admin/Admin@123  
✅ Dashboard loads with vermillion branding  
✅ Can create employees and mark attendance  

**Monthly cost: ~$18 USD**  
**Deployment time: ~15 minutes**  
**Automatic deployments: On every git push**

---

## 📚 Related Documentation

- **GITHUB_ACTIONS_SETUP.md** - Complete setup guide
- **COST_OPTIMIZATION_SUMMARY.md** - Cost breakdown
- **AZURE_REGIONS_REFERENCE.md** - Valid Azure regions
- **AZURE_PROVIDER_REGISTRATION_FIX.md** - Resource provider issues
- **AZURE_REGION_CAPACITY_FIX.md** - Region capacity issues

---

**Need help?** Check the logs first, then refer to this guide! 🚀
