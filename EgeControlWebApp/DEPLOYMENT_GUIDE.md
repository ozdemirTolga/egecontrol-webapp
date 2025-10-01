# 🚀 EGE Control Web App - Windows Hosting Deployment Guide# 🚀 Production Deployment Guide



## 📋 Ön Gereksinimler## 1. Database Setup (hosting.com.tr)



### Hosting Sunucusunda Olması Gerekenler:### Hosting Panelinde:

1. **IIS (Internet Information Services)** - Windows Server 2016+ veya Windows 10/111. **SQL Server Database** oluşturun:

2. **.NET 9.0 Hosting Bundle** - [İndir](https://dotnet.microsoft.com/download/dotnet/9.0)   - Database Name: `egecontr1_`

   - ASP.NET Core Runtime 9.0.x   - Username: `egekontrol`

   - .NET Runtime 9.0.x   - Password: `Baba1420**`

3. **ASP.NET Core Module v2** (Hosting Bundle ile birlikte gelir)

2. **Migration Script** çalıştırın:

### Geliştirme Bilgisayarında:   - `migration.sql` dosyasını SQL Server Management'da açın

- .NET 9.0 SDK   - Komple script'i çalıştırın

- Visual Studio 2022 veya VS Code

## 2. File Upload

---

### FTP ile:

## 📦 Adım 1: Projeyi Publish Etme```

Host: hosting.com.tr

### PowerShell ile:Username: egekontrol

```powershellPassword: Baba1420**

cd f:\egecontrol-webapp-main\egecontrol-webapp-main\EgeControlWebAppPort: 21 (FTP) veya 990 (FTPS)

dotnet publish -c Release -o ..\publish --runtime win-x64 --self-contained false```

```

### Upload edilecek dosyalar:

### Visual Studio ile:- `bin/` klasörü

1. Solution'ı aç- `wwwroot/` klasörü  

2. `EgeControlWebApp` projesine sağ tıkla → **Publish**- `Pages/` klasörü

3. **FolderProfile** seç → **Publish**- `web.config`

4. Dosyalar `publish/` klasörüne gelecek- `appsettings.Production.json`



---## 3. Configuration Check



## 📂 Adım 2: Dosyaları Hostinge Yükleme### web.config:

```xml

### Yüklenecek Dosyalar (publish klasöründen):<configuration>

```  <system.webServer>

✅ EgeControlWebApp.dll (Ana uygulama)    <handlers>

✅ web.config (IIS yapılandırması)      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />

✅ appsettings.json    </handlers>

✅ appsettings.Production.json    <aspNetCore processPath="dotnet" arguments=".\EgeControlWebApp.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="inprocess" />

✅ app.db (SQLite veritabanı)  </system.webServer>

✅ wwwroot/ (tüm statik dosyalar, CSS, JS, resimler)</configuration>

✅ Tüm .dll dosyaları (bağımlılıklar)```

✅ runtimes/ klasörü (varsa, SQLite native kütüphaneleri)

```## 4. Verification Steps



### 🚨 Önemli Notlar:1. Browser'da sitenizi açın

- **app.db** dosyasını ilk yüklemeden sonra bir daha değiştirmeyin (müşteri verileri kaybolur)2. `/Identity/Account/Register` sayfasına gidin

- İlk yüklemede `app.db` boşsa, uygulama otomatik tablo oluşturur3. Test kullanıcısı oluşturun

- `wwwroot/teklifler/` klasörü otomatik oluşturulur (PDF'ler için)4. Database'de tabloları kontrol edin

- `logs/` klasörü otomatik oluşturulur

## 5. Troubleshooting

---

### 502.5 - ANCM Out-Of-Process Startup Failure

## 🔧 Adım 3: IIS Yapılandırması

**Çözüm Adımları:**

### 3.1 Application Pool Oluşturma:

1. **IIS Manager** aç (inetmgr)1. **web.config'i güncelleyin:**

2. **Application Pools** → Sağ tıkla → **Add Application Pool**```xml

   - Name: `EgeControlAppPool`<aspNetCore processPath=".\EgeControlWebApp.exe" 

   - .NET CLR Version: **No Managed Code** ⚠️ (ÇOK ÖNEMLİ!)            arguments="" 

   - Managed Pipeline Mode: `Integrated`            hostingModel="outofprocess">

   - Start Immediately: ✅```



### 3.2 Application Pool Ayarları:2. **.NET Runtime Kontrolü:**

1. `EgeControlAppPool` → **Advanced Settings**   - Hosting panelinde .NET 9.0 runtime yüklü olmalı

   - **Process Model** → Identity: `ApplicationPoolIdentity`   - Eğer yoksa .NET 8.0 için tekrar publish edin

   - **Recycling** → Regular Time Interval: `1740` (29 saat - gece 3'te restart)

   - **CPU** → Limit: `0` (sınırsız)3. **Logs klasörünü oluşturun:**

   - `logs/` klasörünü manuel oluşturun

### 3.3 Site/Application Oluşturma:   - Write permission verin

1. **Sites** → **Default Web Site** → Sağ tıkla → **Add Application**

   - Alias: `egecontrol` (veya domain için root ise boş bırak)4. **Database Connection:**

   - Application Pool: `EgeControlAppPool` seç   - `appsettings.Production.json` connection string'i kontrol edin

   - Physical Path: Dosyaları yüklediğiniz klasör     - SQL Server erişimi test edin

     Örnek: `C:\inetpub\wwwroot\egecontrol`

5. **File Permissions:**

### 3.4 Dosya İzinleri (ÇOK ÖNEMLİ!):   - `EgeControlWebApp.exe` execute permission

```powershell   - `wwwroot/` read permission

# PowerShell (Admin olarak çalıştır)   - `logs/` write permission

cd C:\inetpub\wwwroot\egecontrol

icacls . /grant "IIS_IUSRS:(OI)(CI)M" /T### .NET 8.0 için Publish (Alternatif):

icacls app.db /grant "IIS_IUSRS:M"```bash

icacls wwwroot /grant "IIS_IUSRS:(OI)(CI)M" /Tdotnet publish -c Release -f net8.0 -o ./publish-net8

``````



Veya manuel:### Common Issues:

1. Klasöre sağ tıkla → **Properties** → **Security**- **500 Error**: `web.config` kontrol edin

2. **Edit** → **Add** → `IIS_IUSRS` ekle- **Database Error**: Connection string kontrol edin

3. **Modify** (Değiştir) yetkisini ✅ işaretle- **Missing Files**: `bin/` klasörü eksik olabilir

4. **Apply** → **OK**- **502.5 Error**: .NET runtime veya web.config sorunu



---### Logs:

- IIS logs: Hosting panelinde Error Logs bölümünü kontrol edin

## 🌐 Adım 4: Domain ve SSL Ayarları- Application logs: `logs/` klasöründe



### 4.1 Domain Bağlama (www.egecontrol.com):## 6. Manual Deployment (GitHub Actions Alternatifi)

1. **Sites** → Site seç → **Bindings** → **Add**

2. **HTTP Binding:**1. `dotnet publish -c Release` çalıştırın

   - Type: `http`2. `bin/Release/net9.0/publish/` klasörünü FTP ile upload edin

   - IP Address: `All Unassigned`3. `migration.sql`'i hosting panelinde çalıştırın

   - Port: `80`4. Domain'i test edin

   - Host name: `www.egecontrol.com`

---

### 4.2 SSL Sertifikası Kurulumu (Opsiyonel):*Bu rehber hosting.com.tr Plesk panel için hazırlanmıştır.*

#### Let's Encrypt (Ücretsiz):
1. [Win-ACME](https://www.win-acme.com/) indir
2. Çalıştır ve domain seç
3. Otomatik sertifika alır ve IIS'e kurar

#### Hosting Firması SSL:
1. Hosting panel → SSL Certificates
2. Sertifikayı indir (.pfx veya .cer + .key)
3. IIS Manager → Server Certificates → Import
4. Site Bindings → HTTPS:443 ekle

#### SSL Aktif Olduktan Sonra:
`Program.cs` dosyasındaki yorumları kaldır:
```csharp
// Satır 110 civarı - HSTS ekle
app.UseHsts();

// Satır 121 civarı - HTTPS yönlendirmesi
app.UseHttpsRedirection();
```

---

## 🔍 Adım 5: Test ve Doğrulama

### 5.1 Sağlık Kontrolü:
```
http://www.egecontrol.com/health
```
**Beklenen Yanıt:** `OK` (HTTP 200)

### 5.2 Ana Sayfa:
```
http://www.egecontrol.com/
```
Ege Control ana sayfası görünmeli

### 5.3 Admin Girişi:
```
http://www.egecontrol.com/Identity/Account/Login
```

**Varsayılan Admin Hesabı:**
- Email: `admin@egecontrol.com`
- Şifre: `Admin123!`

**🔒 ÇOK ÖNEMLİ:** İlk girişte admin şifresini mutlaka değiştirin!

---

## 📝 Adım 6: Log Kontrolü ve Hata Ayıklama

### Log Dosyaları:
1. **Application Logs:** `logs/stdout_*.log` (sitenin kök dizini)
2. **IIS Logs:** `C:\inetpub\logs\LogFiles\`
3. **Windows Event Viewer:**
   - Windows Logs → Application
   - Kaynak: `IIS AspNetCore Module V2`

### Yaygın Hatalar ve Çözümleri:

#### ❌ 500.19 - Configuration Error
**Sebep:** `web.config` dosyası hatalı veya okunamıyor  
**Çözüm:**
- XML syntax'ı kontrol et
- Dosya izinlerini kontrol et (IIS_IUSRS okuma yetkisi olmalı)

#### ❌ 500.30 - ASP.NET Core app failed to start
**Sebep:** .NET Runtime yüklü değil  
**Çözüm:**
1. [.NET 9.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0) indir
2. Kur ve sunucuyu yeniden başlat
3. `iisreset` komutunu çalıştır

#### ❌ 500.31 - Failed to load ASP.NET Core runtime
**Sebep:** Application Pool ayarı yanlış  
**Çözüm:**
- Application Pool → .NET CLR Version = **No Managed Code** olmalı

#### ❌ 500.37 - Failed to start within startup time limit
**Sebep:** Uygulama 120 saniyede başlamadı (ilk yükleme)  
**Çözüm:**
`web.config` içine ekle:
```xml
<aspNetCore ... startupTimeLimit="600">
```

#### ❌ 403.14 - Forbidden (Directory listing denied)
**Sebep:** Statik dosyalar eksik veya yanlış yerde  
**Çözüm:**
- `wwwroot/` klasörünün doğru yüklendiğinden emin ol
- IIS'de Static Content module'ünün yüklü olduğunu kontrol et

#### ❌ SQLite Error: Unable to open database file
**Sebep:** `app.db` yazma izni yok  
**Çözüm:**
```powershell
icacls app.db /grant "IIS_IUSRS:M"
```

---

## 🔄 Güncelleme (Update) Adımları

### Yeni Versiyon Yüklerken:

1. **Bakım Modunu Aç:**
   ```powershell
   # app_offline.htm dosyasını site köküne kopyala
   Copy-Item app_offline.htm C:\inetpub\wwwroot\egecontrol\
   ```

2. **Dosyaları Yedekle:**
   ```powershell
   $date = Get-Date -Format "yyyyMMdd_HHmmss"
   Copy-Item C:\inetpub\wwwroot\egecontrol\app.db C:\Backups\app_$date.db
   Copy-Item C:\inetpub\wwwroot\egecontrol\wwwroot\teklifler C:\Backups\teklifler_$date -Recurse
   ```

3. **Yeni Dosyaları Yükle:**
   - Tüm dosyaları publish klasöründen kopyala
   - **DİKKAT:** `app.db` dosyasını değiştirme!
   - **DİKKAT:** `wwwroot/teklifler/` klasörünü silme!

4. **Application Pool'u Yeniden Başlat:**
   ```powershell
   Restart-WebAppPool -Name "EgeControlAppPool"
   ```

5. **Bakım Modunu Kapat:**
   ```powershell
   Remove-Item C:\inetpub\wwwroot\egecontrol\app_offline.htm
   ```

6. **Test Et:**
   - `/health` endpoint'ini kontrol et
   - Ana sayfayı aç
   - Login testi yap

---

## 🗄️ Yedekleme (Backup) Stratejisi

### Düzenli Yedeklenecekler:
- ✅ `app.db` (SQLite veritabanı - TÜM DATA!)
- ✅ `wwwroot/teklifler/` (PDF dosyaları)
- ✅ `appsettings.Production.json` (SMTP şifreleri)

### Otomatik Yedekleme Script (PowerShell):
```powershell
# backup_egecontrol.ps1
$date = Get-Date -Format "yyyyMMdd_HHmmss"
$sourceDir = "C:\inetpub\wwwroot\egecontrol"
$backupDir = "C:\Backups\EgeControl\$date"

# Klasör oluştur
New-Item -ItemType Directory -Path $backupDir -Force

# Veritabanı
Copy-Item "$sourceDir\app.db" -Destination "$backupDir\app.db"

# PDF dosyaları
Copy-Item "$sourceDir\wwwroot\teklifler" -Destination "$backupDir\teklifler" -Recurse -ErrorAction SilentlyContinue

# Ayarlar
Copy-Item "$sourceDir\appsettings.Production.json" -Destination "$backupDir\" -ErrorAction SilentlyContinue

# Eski yedekleri temizle (30 günden eski)
Get-ChildItem "C:\Backups\EgeControl" | Where-Object { $_.CreationTime -lt (Get-Date).AddDays(-30) } | Remove-Item -Recurse -Force

Write-Host "Yedekleme tamamlandı: $backupDir"
```

### Windows Task Scheduler ile Otomatik:
1. Task Scheduler → Create Basic Task
2. Name: `EgeControl Daily Backup`
3. Trigger: Daily - 03:00 AM
4. Action: Start a Program
   - Program: `powershell.exe`
   - Arguments: `-File C:\Scripts\backup_egecontrol.ps1`

---

## 🔐 Güvenlik Önerileri

1. **Admin Şifresini Değiştir:**
   - İlk girişte mutlaka değiştir
   - En az 12 karakter, büyük/küçük harf, rakam, özel karakter

2. **appsettings.Production.json Şifrele:**
   - SMTP şifresi açık yazılı
   - Dosya izinlerini kısıtla (sadece IIS_IUSRS okuyabilsin)

3. **HTTPS Kullan:**
   - Let's Encrypt ile ücretsiz SSL
   - HTTP'den HTTPS'e yönlendir

4. **Firewall Kuralları:**
   - Sadece 80 (HTTP) ve 443 (HTTPS) portları açık
   - Gereksiz portları kapat

5. **Düzenli Güncellemeler:**
   - Windows Update
   - .NET Runtime güncellemeleri
   - Güvenlik yamaları

---

## 📞 Destek ve Sorun Giderme

### Sorun Yaşarsanız:
1. ✅ `logs/stdout_*.log` dosyalarını inceleyin
2. ✅ IIS Event Viewer → Application logs
3. ✅ Application Pool'un çalıştığından emin olun
4. ✅ Dosya izinlerini kontrol edin
5. ✅ .NET 9.0 Hosting Bundle yüklü mü?

### Hosting Destek:
- Hosting firmanızın destek ekibine logs'ları gönderin
- `web.config` ve hata kodunu belirtin

### Geliştirici:
- Tolga Özdemir
- Email: tolga.ozdemir@egecontrol.com

---

## ✅ Başarı Kontrol Listesi

- [ ] .NET 9.0 Hosting Bundle yüklendi
- [ ] IIS Application Pool oluşturuldu (No Managed Code!)
- [ ] Dosyalar publish edildi ve yüklendi
- [ ] app.db dosyasına yazma izni verildi
- [ ] wwwroot/ klasörüne yazma izni verildi
- [ ] Domain binding yapıldı
- [ ] /health endpoint çalışıyor
- [ ] Ana sayfa açılıyor
- [ ] Admin login çalışıyor
- [ ] Admin şifresi değiştirildi
- [ ] Yedekleme script'i kuruldu
- [ ] SSL sertifikası kuruldu (opsiyonel)

**Başarılar! 🎉 Site artık production ortamında çalışıyor!**
