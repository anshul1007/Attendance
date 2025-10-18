# 🚀 VermillionIndia Attendance System - Deployment

## Quick Overview

This application is configured for **automated deployment to Azure** using **GitHub Actions CI/CD**.

---

## 📋 What's Included

### Application Files
- **Backend**: .NET 8 Web API (`/backend`)
- **Frontend**: Angular 20 (`/frontend`)
- **Database**: Azure SQL Database configuration

### Deployment Configuration
- **`.github/workflows/`** - GitHub Actions workflows
  - `azure-infrastructure.yml` - Creates Azure resources
  - `deploy-backend.yml` - Auto-deploys backend
  - `deploy-frontend.yml` - Auto-deploys frontend

### Documentation
- **`GITHUB_ACTIONS_SETUP.md`** - Complete deployment guide
- **`COST_OPTIMIZATION_SUMMARY.md`** - Cost breakdown (~$18/month)

---

## 🎯 How to Deploy

### Prerequisites
- ✅ GitHub account
- ✅ Azure account (Visual Studio Enterprise Subscription)
- ✅ Code pushed to GitHub repository

### Quick Start

**Follow the step-by-step guide:**
```
📖 GITHUB_ACTIONS_SETUP.md
```

**Summary:**
1. Push code to GitHub
2. Create Azure Service Principal
3. Add 4 secrets to GitHub
4. Run "Setup Azure Infrastructure" workflow
5. Deploy! ✅

**Total time: ~30 minutes**

---

## 💰 Cost Estimate

| Service | Monthly Cost |
|---------|-------------|
| Frontend (Static Web App) | $0.00 |
| Backend (App Service B1) | $13.14 |
| Database (Azure SQL Basic) | $4.90 |
| **Total** | **~$18/month** |

**Location**: Central Europe (Netherlands)

---

## 🔄 Automatic Deployments

Once set up, deployments are **automatic**:

```
Git Push → GitHub Actions → Azure (Live in 5 min!)
```

- Backend deploys when you change `/backend` files
- Frontend deploys when you change `/frontend` files
- No manual deployment needed!

---

## 📚 Full Documentation

- **Deployment Guide**: `GITHUB_ACTIONS_SETUP.md`
- **Cost Details**: `COST_OPTIMIZATION_SUMMARY.md`
- **API Documentation**: `API_DOCUMENTATION.md`
- **Project Summary**: `PROJECT_SUMMARY.md`
- **Quick Start**: `QUICK_START.md`

---

## 🆘 Support

**Questions?** Check `GITHUB_ACTIONS_SETUP.md` for:
- Detailed setup instructions
- Troubleshooting guide
- Security best practices
- Monitoring tips

---

**Ready to deploy?** → Open `GITHUB_ACTIONS_SETUP.md` and follow the steps! 🚀
