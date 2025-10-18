# Azure Regions Reference

## ℹ️ Important: Valid Azure Region Names

When deploying to Azure, use these **exact** region names:

## 🇪🇺 Europe Regions (Recommended for your setup)

| Region Name | Location | Description |
|-------------|----------|-------------|
| **westeurope** ⭐ | Netherlands (Amsterdam) | **RECOMMENDED** - Low latency, reliable |
| **northeurope** | Ireland (Dublin) | Good alternative |
| **germanywestcentral** | Germany (Frankfurt) | GDPR compliant |
| **francecentral** | France (Paris) | GDPR compliant |
| **uksouth** | UK (London) | UK data residency |
| **ukwest** | UK (Cardiff) | UK backup |
| **swedencentral** | Sweden (Gävle) | Sustainable energy |
| **switzerlandnorth** | Switzerland (Zürich) | High security |
| **norwayeast** | Norway (Oslo) | Sustainable energy |
| **polandcentral** | Poland (Warsaw) | EU data residency |
| **italynorth** | Italy (Milan) | EU data residency |
| **spaincentral** | Spain (Madrid) | EU data residency |

## 🌍 Other Regions

### North America
- `eastus` - Virginia, USA
- `eastus2` - Virginia, USA
- `westus` - California, USA
- `westus2` - Washington, USA
- `westus3` - Arizona, USA
- `centralus` - Iowa, USA
- `northcentralus` - Illinois, USA
- `southcentralus` - Texas, USA
- `westcentralus` - Wyoming, USA
- `canadacentral` - Toronto, Canada
- `canadaeast` - Quebec, Canada
- `mexicocentral` - Mexico

### Asia Pacific
- `southeastasia` - Singapore
- `eastasia` - Hong Kong
- `australiaeast` - New South Wales, Australia
- `australiasoutheast` - Victoria, Australia
- `japaneast` - Tokyo, Japan
- `japanwest` - Osaka, Japan
- `koreacentral` - Seoul, Korea
- `koreasouth` - Busan, Korea
- `centralindia` - Pune, India
- `southindia` - Chennai, India
- `westindia` - Mumbai, India
- `malaysiawest` - Malaysia
- `indonesiacentral` - Indonesia
- `newzealandnorth` - New Zealand

### Middle East & Africa
- `uaenorth` - Dubai, UAE
- `qatarcentral` - Qatar
- `israelcentral` - Israel
- `southafricanorth` - South Africa

### South America
- `brazilsouth` - São Paulo, Brazil
- `chilecentral` - Chile

## ⚠️ Common Mistakes

| ❌ Wrong | ✅ Correct |
|---------|----------|
| `centraleurope` | `westeurope` or `germanywestcentral` |
| `central-europe` | `westeurope` |
| `europe` | `westeurope` or `northeurope` |
| `eu-west` | `westeurope` |
| `netherlands` | `westeurope` |
| `amsterdam` | `westeurope` |

## 🎯 Recommendation for Your Deployment

**Use: `westeurope`**

**Reasons:**
- ✅ Located in Netherlands (Amsterdam)
- ✅ Excellent connectivity across Europe
- ✅ Lower latency for European users
- ✅ GDPR compliant
- ✅ Mature data center with high reliability
- ✅ Good availability of Azure services

## 📊 Pricing Note

Most regions have the same pricing. Premium regions (with slight price increase):
- Germany West Central
- Switzerland North
- UAE North
- Brazil South

**westeurope** has standard pricing - no premium charges. ✅

## 🔧 How to Use in Deployment

### In GitHub Actions workflow:
```yaml
location:
  default: 'westeurope'
```

### In PowerShell script:
```powershell
-Location "westeurope"
```

### In Azure CLI:
```bash
--location westeurope
```

### In Azure Portal:
Select: **"West Europe"** from dropdown

## ℹ️ Note

The error you encountered:
```
ERROR: The provided location 'centraleurope' is not available
```

This happens because **'centraleurope' is not a valid Azure region name**. 

Use **'westeurope'** instead for deployment in Europe (Netherlands/Amsterdam region).

---

**Updated Configuration:**
All deployment files now use `westeurope` as the default region. ✅
