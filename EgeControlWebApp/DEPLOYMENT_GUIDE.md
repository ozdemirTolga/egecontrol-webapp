# 🚀 Production Deployment Guide

## 1. Database Setup (hosting.com.tr)

### Hosting Panelinde:
1. **SQL Server Database** oluşturun:
   - Database Name: `egecontr1_`
   - Username: `egekontrol`
   - Password: `Baba1420**`

2. **Migration Script** çalıştırın:
   - `migration.sql` dosyasını SQL Server Management'da açın
   - Komple script'i çalıştırın

## 2. File Upload

### FTP ile:
```
Host: hosting.com.tr
Username: egekontrol
Password: Baba1420**
Port: 21 (FTP) veya 990 (FTPS)
```

### Upload edilecek dosyalar:
- `bin/` klasörü
- `wwwroot/` klasörü  
- `Pages/` klasörü
- `web.config`
- `appsettings.Production.json`

## 3. Configuration Check

### web.config:
```xml
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments=".\EgeControlWebApp.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />
  </system.webServer>
</configuration>
```

## 4. Verification Steps

1. Browser'da sitenizi açın
2. `/Identity/Account/Register` sayfasına gidin
3. Test kullanıcısı oluşturun
4. Database'de tabloları kontrol edin

## 5. Troubleshooting

### 502.5 - ANCM Out-Of-Process Startup Failure

**Çözüm Adımları:**

1. **web.config'i güncelleyin:**
```xml
<aspNetCore processPath=".\EgeControlWebApp.exe" 
            arguments="" 
            hostingModel="outofprocess">
```

2. **.NET Runtime Kontrolü:**
   - Hosting panelinde .NET 9.0 runtime yüklü olmalı
   - Eğer yoksa .NET 8.0 için tekrar publish edin

3. **Logs klasörünü oluşturun:**
   - `logs/` klasörünü manuel oluşturun
   - Write permission verin

4. **Database Connection:**
   - `appsettings.Production.json` connection string'i kontrol edin
   - SQL Server erişimi test edin

5. **File Permissions:**
   - `EgeControlWebApp.exe` execute permission
   - `wwwroot/` read permission
   - `logs/` write permission

### .NET 8.0 için Publish (Alternatif):
```bash
dotnet publish -c Release -f net8.0 -o ./publish-net8
```

### Common Issues:
- **500 Error**: `web.config` kontrol edin
- **Database Error**: Connection string kontrol edin
- **Missing Files**: `bin/` klasörü eksik olabilir
- **502.5 Error**: .NET runtime veya web.config sorunu

### Logs:
- IIS logs: Hosting panelinde Error Logs bölümünü kontrol edin
- Application logs: `logs/` klasöründe

## 6. Manual Deployment (GitHub Actions Alternatifi)

1. `dotnet publish -c Release` çalıştırın
2. `bin/Release/net9.0/publish/` klasörünü FTP ile upload edin
3. `migration.sql`'i hosting panelinde çalıştırın
4. Domain'i test edin

---
*Bu rehber hosting.com.tr Plesk panel için hazırlanmıştır.*
