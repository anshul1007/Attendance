# 🎉 Infrastructure Deployed Successfully! - Next Steps

Congratulations! Your Azure infrastructure is now live. Follow these steps to complete the deployment.

---

## 📋 Step 1: Get Your Deployment URLs

From the GitHub Actions workflow output, you should have seen:

```
✅ INFRASTRUCTURE CREATED SUCCESSFULLY!

📱 Frontend URL: https://vermillion-web-XXXXXXXXXX.azurestaticapps.net
🔧 Backend URL: https://vermillion-api-XXXXXXXXXX.azurewebsites.net
💾 SQL Server: vermillion-sql-XXXXXXXXXX.database.windows.net
🌍 SQL Location: [region where it was deployed]
```

**Action:** Copy these URLs - you'll need them!

If you missed them, find them in:
- GitHub → Actions → "Setup Azure Infrastructure" → Latest run → Scroll to bottom

---

## 📋 Step 2: Get Deployment Credentials

You need to get two more secrets for automatic deployments.

### A. Open Azure Cloud Shell

1. Go to: https://portal.azure.com
2. Click the **Cloud Shell** icon (>_) at the top right
3. Select **Bash** or **PowerShell**

### B. Get Backend Publish Profile

Run this command (replace `XXXXXXXXXX` with your actual backend app name):

```bash
az webapp deployment list-publishing-profiles \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg \
  --xml
```

**Action:** Copy the ENTIRE XML output (it's long!)

### C. Get Frontend Deployment Token

Run this command (replace `XXXXXXXXXX` with your actual frontend app name):

```bash
az staticwebapp secrets list \
  --name vermillion-web-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg \
  --query "properties.apiKey" -o tsv
```

**Action:** Copy the token (long alphanumeric string)

---

## 📋 Step 3: Add Deployment Secrets to GitHub

Now add these credentials as GitHub secrets:

### A. Go to Repository Settings

1. Go to: https://github.com/anshul1007/Attendance
2. Click **Settings** tab
3. Click **Secrets and variables** → **Actions**
4. Click **New repository secret**

### B. Add Backend Publish Profile

- **Name**: `AZURE_WEBAPP_PUBLISH_PROFILE`
- **Value**: Paste the entire XML from Step 2B
- Click **Add secret**

### C. Add Frontend Deployment Token

- **Name**: `AZURE_STATIC_WEB_APPS_API_TOKEN`
- **Value**: Paste the token from Step 2C
- Click **Add secret**

**✅ You should now have 4 secrets total:**
1. AZURE_CREDENTIALS ✅
2. SQL_ADMIN_PASSWORD ✅
3. AZURE_WEBAPP_PUBLISH_PROFILE ✅ (new)
4. AZURE_STATIC_WEB_APPS_API_TOKEN ✅ (new)

---

## 📋 Step 4: Update Workflow Configuration

Update the backend workflow with your actual app name.

### A. Edit Backend Workflow File

Edit file: `.github/workflows/deploy-backend.yml`

Find this line:
```yaml
env:
  AZURE_WEBAPP_NAME: vermillion-api    # Change this to your app name
```

Change to your actual app name:
```yaml
env:
  AZURE_WEBAPP_NAME: vermillion-api-XXXXXXXXXX    # ⬅ Your actual name from Step 1
```

### B. Commit the Change

```powershell
cd E:\Attendance
git add .github/workflows/deploy-backend.yml
git commit -m "Update backend app name in deployment workflow"
git push
```

---

## 📋 Step 5: Update Frontend Configuration

Update the frontend to connect to your Azure backend API.

### A. Edit Production Environment File

Edit file: `frontend/src/environments/environment.prod.ts`

Change the apiUrl to your actual backend URL:

```typescript
export const environment = {
  production: true,
  apiUrl: 'https://vermillion-api-XXXXXXXXXX.azurewebsites.net/api'  // ⬅ Update this
};
```

### B. Commit the Change

```powershell
cd E:\Attendance
git add frontend/src/environments/environment.prod.ts
git commit -m "Configure frontend to use Azure backend API URL"
git push
```

---

## 📋 Step 6: Deploy Backend API

Now deploy your .NET backend to Azure!

### A. Trigger Backend Deployment

1. Go to: https://github.com/anshul1007/Attendance
2. Click **Actions** tab
3. Click **"Deploy Backend to Azure"** (left sidebar)
4. Click **"Run workflow"** button (right side)
5. Click **"Run workflow"**

### B. Watch the Deployment

The workflow will:
- ✅ Checkout code
- ✅ Setup .NET 8
- ✅ Restore dependencies
- ✅ Build backend
- ✅ Run tests (if any)
- ✅ Publish
- ✅ Deploy to Azure App Service

**Time: ~3-5 minutes**

### C. Verify Backend is Live

After deployment completes, test the API:

**Option 1: In Browser**
```
https://vermillion-api-XXXXXXXXXX.azurewebsites.net
```
You should see a welcome page or 404 (normal - no root route).

**Option 2: Test Health Endpoint**
```
https://vermillion-api-XXXXXXXXXX.azurewebsites.net/api/auth/health
```
Should return 200 OK or similar.

**Option 3: PowerShell**
```powershell
Invoke-WebRequest -Uri "https://vermillion-api-XXXXXXXXXX.azurewebsites.net" -Method Get
```

---

## 📋 Step 7: Run Database Migrations

Your database needs to be initialized with tables and seed data.

### A. Open Azure Cloud Shell

Go to: https://portal.azure.com → Click Cloud Shell (>_)

### B. Get Database Connection String

```bash
az sql db show-connection-string \
  --server vermillion-sql-XXXXXXXXXX \
  --name AttendanceDB \
  --client ado.net \
  -o tsv
```

Copy the connection string and replace `<username>` with `sqladmin` and `<password>` with your SQL password.

### C. Connect to Database

**Option 1: Use Azure Data Studio** (Recommended)

1. Download: https://aka.ms/azuredatastudio
2. Install and open
3. Click "New Connection"
4. Fill in:
   - **Server**: `vermillion-sql-XXXXXXXXXX.database.windows.net`
   - **Authentication**: SQL Login
   - **Username**: `sqladmin`
   - **Password**: Your SQL password from secrets
   - **Database**: `AttendanceDB`
   - **Encrypt**: Yes
5. Click Connect

**Option 2: Run Migrations from Local**

```powershell
cd E:\Attendance\backend\AttendanceAPI

# Set connection string temporarily
$env:ConnectionStrings__DefaultConnection = "Server=tcp:vermillion-sql-XXXXXXXXXX.database.windows.net,1433;Database=AttendanceDB;User ID=sqladmin;Password=YOUR_PASSWORD;Encrypt=true;TrustServerCertificate=false;"

# Run migrations
dotnet ef database update

# Verify
dotnet ef migrations list
```

**Option 3: Automatic on First API Call**

The backend may auto-run migrations on startup if configured. Check backend logs:

```bash
az webapp log tail \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg
```

---

## 📋 Step 8: Deploy Frontend Application

Deploy your Angular frontend to Azure Static Web Apps!

### A. Trigger Frontend Deployment

1. Go to: https://github.com/anshul1007/Attendance
2. Click **Actions** tab
3. Click **"Deploy Frontend to Azure"** (left sidebar)
4. Click **"Run workflow"** button (right side)
5. Click **"Run workflow"**

### B. Watch the Deployment

The workflow will:
- ✅ Checkout code
- ✅ Setup Node.js
- ✅ Install dependencies
- ✅ Build Angular app
- ✅ Deploy to Azure Static Web Apps

**Time: ~5-7 minutes** (includes build time)

### C. Verify Frontend is Live

After deployment completes, open your frontend URL in a browser:

```
https://vermillion-web-XXXXXXXXXX.azurestaticapps.net
```

**You should see:**
- ✅ VermillionIndia login page
- ✅ Vermillion branding with gradient
- ✅ "PRECISION DEFINED • SOLUTIONS DELIVERED" tagline
- ✅ Username and Password fields

---

## 📋 Step 9: Test the Application

Now test that everything works end-to-end!

### A. Test Login

1. Go to your frontend URL
2. Login with default admin credentials:
   - **Username**: `admin`
   - **Password**: `Admin@123`

**Expected:**
- ✅ Successful login
- ✅ Redirect to Admin Dashboard
- ✅ See vermillion-themed interface

### B. Test Admin Functions

In the Admin Dashboard:
- ✅ Create a new employee
- ✅ View employee list
- ✅ Check attendance reports
- ✅ View public holidays

### C. Check Browser Console

Press F12 and check:
- ❌ Should see no CORS errors
- ❌ Should see no API connection errors
- ✅ All API calls should succeed

---

## 📋 Step 10: Configure CORS (If Needed)

If you see CORS errors in browser console:

### A. Add CORS Settings

```bash
az webapp cors add \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg \
  --allowed-origins "https://vermillion-web-XXXXXXXXXX.azurestaticapps.net"
```

### B. Verify CORS Settings

```bash
az webapp cors show \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg
```

---

## 🎉 Step 11: Celebrate! You're Live!

Congratulations! Your VermillionIndia Attendance System is now fully deployed! 🎉

### ✅ What You've Accomplished:

- ✅ Azure infrastructure deployed (~$18/month)
- ✅ Backend API running on Azure App Service
- ✅ Frontend running on Azure Static Web Apps
- ✅ Database initialized with migrations
- ✅ Admin user seeded and ready
- ✅ Automatic deployments configured (every git push)
- ✅ Professional vermillion branding live

### 🌐 Your Live URLs:

- **Frontend**: `https://vermillion-web-XXXXXXXXXX.azurestaticapps.net`
- **Backend**: `https://vermillion-api-XXXXXXXXXX.azurewebsites.net`
- **Database**: `vermillion-sql-XXXXXXXXXX.database.windows.net`

### 💰 Monthly Cost: ~$18 USD

- Frontend: $0 (Free tier)
- Backend: $13 (App Service B1)
- Database: $5 (Azure SQL Basic)

---

## 🔄 Future Deployments

Now that everything is set up, future deployments are **automatic**!

### Automatic Deployment Triggers:

**Backend Auto-Deploys When:**
- You push changes to `backend/**` folder
- You manually trigger "Deploy Backend to Azure" workflow

**Frontend Auto-Deploys When:**
- You push changes to `frontend/**` folder
- You manually trigger "Deploy Frontend to Azure" workflow

### How to Deploy Updates:

```powershell
# Make your changes to code
cd E:\Attendance

# Commit and push
git add .
git commit -m "Added new feature"
git push

# GitHub Actions automatically:
# 1. Builds your code
# 2. Runs tests
# 3. Deploys to Azure
# 4. Your site updates in ~5 minutes!
```

---

## 📊 Monitoring Your Application

### View Application Logs

**Backend Logs:**
```bash
az webapp log tail \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg
```

**Download Logs:**
```bash
az webapp log download \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg \
  --log-file backend-logs.zip
```

### View in Azure Portal

1. Go to: https://portal.azure.com
2. Navigate to **Resource Groups** → `vermillion-attendance-rg`
3. Click on each resource to view:
   - Metrics
   - Logs
   - Performance
   - Costs

### Set Up Alerts

1. Azure Portal → Resource Groups → `vermillion-attendance-rg`
2. Click "Alerts" → "Create alert rule"
3. Set up alerts for:
   - High CPU usage
   - High memory usage
   - Failed requests
   - Response time

---

## 🔒 Security Checklist

After deployment, secure your application:

### 1. Change Default Admin Password

- [ ] Login to your app
- [ ] Go to user settings
- [ ] Change admin password from `Admin@123` to something secure

### 2. Configure SQL Firewall

```bash
# Add your office IP
az sql server firewall-rule create \
  --resource-group vermillion-attendance-rg \
  --server vermillion-sql-XXXXXXXXXX \
  --name "OfficeIP" \
  --start-ip-address YOUR.OFFICE.IP \
  --end-ip-address YOUR.OFFICE.IP
```

### 3. Enable HTTPS Only

```bash
az webapp update \
  --name vermillion-api-XXXXXXXXXX \
  --resource-group vermillion-attendance-rg \
  --https-only true
```

### 4. Review GitHub Secrets

- [ ] Ensure all 4 secrets are set and correct
- [ ] Never commit secrets to code
- [ ] Rotate SQL password periodically

---

## 📚 Documentation Reference

- **GITHUB_ACTIONS_SETUP.md** - Complete setup guide
- **COST_OPTIMIZATION_SUMMARY.md** - Cost breakdown and optimization
- **DEPLOYMENT_TROUBLESHOOTING.md** - Common issues and solutions
- **AZURE_REGIONS_REFERENCE.md** - Valid Azure regions
- **API_DOCUMENTATION.md** - API endpoints reference
- **PROJECT_SUMMARY.md** - Project overview

---

## 🆘 Need Help?

### If Something Doesn't Work:

1. **Check GitHub Actions logs** - Most detailed information
2. **Check DEPLOYMENT_TROUBLESHOOTING.md** - Common issues
3. **Check Azure Portal** - Resource health and logs
4. **Check browser console** - Frontend errors

### Common Issues:

**Login Fails:**
- Check backend is running: Visit backend URL
- Check database migrations ran: Connect to DB
- Check CORS settings: Browser console for errors

**API Not Responding:**
- Check backend deployment succeeded
- Check application logs in Azure Portal
- Verify connection string is set correctly

**Frontend Shows Errors:**
- Check environment.prod.ts has correct API URL
- Check CORS configuration
- Check browser console for specific errors

---

## 🎯 Next Steps (Optional Enhancements)

### 1. Custom Domain (Optional)

Add your company domain instead of Azure URLs:
- Cost: ~$12-15/year for domain
- Setup: ~30 minutes
- Documentation: Azure Portal → Static Web Apps → Custom domains

### 2. Enable CI/CD for Pull Requests

- Automatic preview deployments
- Test changes before merging
- Already configured in workflows!

### 3. Add Monitoring & Alerts

- Application Insights (Free tier)
- Email alerts for issues
- Performance monitoring

### 4. Scale Up (When Needed)

When you need more capacity:
```bash
# Add second backend instance
az appservice plan update \
  --name vermillion-attendance-plan \
  --resource-group vermillion-attendance-rg \
  --number-of-workers 2
# Cost: +$13/month = $31 total
```

---

## 🎉 Congratulations!

You've successfully deployed a complete enterprise application to Azure with:
- ✅ Professional CI/CD pipeline
- ✅ Automatic deployments
- ✅ Scalable infrastructure
- ✅ Cost-optimized setup (~$18/month)
- ✅ Production-ready configuration

**Your application is LIVE and ready for users!** 🚀

---

**Questions?** Check the troubleshooting guide or open an issue in your repository!
