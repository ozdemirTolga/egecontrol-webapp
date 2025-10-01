# 📝 Deployment Yapılandırma Notları

## ✅ Tamamlanan Konfigürasyonlar (2 Ekim 2025)

### 🎯 Temel Yapılandırma

#### 1. **web.config** - IIS InProcess Hosting
```xml
<aspNetCore processPath="dotnet" 
            arguments=".\EgeControlWebApp.dll"
            hostingModel="InProcess"
            stdoutLogEnabled="true"
            stdoutLogFile=".\logs\stdout">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

**Özellikler:**
- ✅ InProcess hosting (daha hızlı)
- ✅ Framework-dependent deployment
- ✅ Production environment otomatik
- ✅ Stdout logging aktif (troubleshooting için)

---

#### 2. **EgeControlWebApp.csproj** - Build Yapılandırması
```xml
<PropertyGroup>
  <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>
  <PlatformTarget>AnyCPU</PlatformTarget>
</PropertyGroup>

<!-- StaticWebAssets hatasını önle -->
<ItemGroup>
  <Content Remove="wwwroot\**" />
</ItemGroup>
```

**Özellikler:**
- ✅ AnyCPU → 32-bit ve 64-bit IIS ile uyumlu
- ✅ `Content Remove="wwwroot\**"` → MSBuild StaticWebAssets hatası önlendi
- ✅ wwwroot manuel kopyalanarak publish'e dahil ediliyor

**Önemli Not:** Bu proje için `<Content Remove="wwwroot\**" />` satırı **KALDIRILMAMALI**! MSBuild cache sorunu yaratıyor.

---

#### 3. **Program.cs** - HTTPS/HSTS İsteğe Bağlı
```csharp
// Line ~110: HSTS devre dışı (SSL opsiyonel)
// app.UseHsts();

// Line ~121: HTTPS Redirection devre dışı
// app.UseHttpsRedirection();
```

**Sebep:** SSL sertifikası opsiyonel - HTTP üzerinden de çalışabilir.

---

#### 4. **appsettings.Production.json** - Production Ayarları
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  }
}
```

**Özellikler:**
- ✅ Loglama seviyesi: Warning (production için optimal)
- ✅ SQLite connection string
- ✅ SMTP ayarları appsettings.json'dan alınıyor

---

### 🔧 Publish Prosedürü

**Komut:**
```powershell
cd EgeControlWebApp
Remove-Item ..\publish -Recurse -Force -ErrorAction SilentlyContinue
dotnet publish -c Release -o "..\publish" --self-contained false
```

**wwwroot Manuel Kopyalama (Gerekli!):**
```powershell
Copy-Item -Path EgeControlWebApp\wwwroot -Destination publish\wwwroot -Recurse -Force

# Yanlış iç içe yapıyı düzelt
Move-Item "publish\wwwroot\wwwroot\*" "publish\wwwroot\" -Force
Remove-Item "publish\wwwroot\wwwroot" -Recurse -Force
```

**Sonuç:**
- 📦 Boyut: ~172 MB
- 📁 Dosya sayısı: ~670 dosya
- ✅ Bootstrap, jQuery, CSS, JS dahil

---

### 📋 IIS Application Pool Ayarları

**Kritik Ayarlar:**
```
.NET CLR Version: No Managed Code
Enable 32-Bit Applications: False
Identity: ApplicationPoolIdentity
Start Mode: AlwaysRunning (opsiyonel)
```

**PowerShell:**
```powershell
Import-Module WebAdministration
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false
```

---

### 🔒 Dosya İzinleri

**IIS_IUSRS grubu için:**
```powershell
$sitePath = "C:\inetpub\wwwroot\egecontrol"

# Tüm site için Full Control
icacls $sitePath /grant "IIS_IUSRS:(OI)(CI)F" /T

# app.db yazılabilir olmalı
icacls "$sitePath\app.db" /grant "IIS_IUSRS:(M)" /T

# wwwroot/teklifler yazılabilir (PDF kaydetme)
icacls "$sitePath\wwwroot\teklifler" /grant "IIS_IUSRS:(OI)(CI)M" /T

# logs klasörü
New-Item -ItemType Directory -Path "$sitePath\logs" -Force
icacls "$sitePath\logs" /grant "IIS_IUSRS:(OI)(CI)M" /T
```

---

### ⚠️ Bilinen Sorunlar ve Çözümler

#### 1. **HTTP Error 500.32** - Bitness Mismatch
**Sebep:** IIS 32-bit çalışıyorken uygulama 64-bit veya tersi.

**Çözüm:**
```powershell
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false
Restart-WebAppPool -Name EgeControlAppPool
```

---

#### 2. **CSS/JS 404 Hatası** - Statik Dosyalar Eksik
**Sebep:** `<Content Remove="wwwroot\**" />` nedeniyle wwwroot publish edilmemiş.

**Çözüm:** Manuel wwwroot kopyalama (yukarıdaki publish prosedürü)

---

#### 3. **MSBuild StaticWebAssets Hatası**
```
error : System.InvalidOperationException: No file exists for the asset at either location 'wwwroot\**' or 'wwwroot\**'.
```

**Sebep:** MSBuild cache'inde `Content Remove` kalıntısı.

**Çözüm:**
```powershell
# obj/bin temizle
Remove-Item obj,bin -Recurse -Force

# Build server kapat
dotnet build-server shutdown

# Temiz build
dotnet build -c Release
```

**Kalıcı Çözüm:** `.csproj`'de `<Content Remove="wwwroot\**" />` satırını KORUMAK ve wwwroot'u manuel kopyalamak.

---

### 📚 Doküman Referansları

| Dosya | Açıklama |
|-------|----------|
| `DEPLOYMENT_GUIDE.md` | 300+ satır kapsamlı deployment rehberi |
| `TROUBLESHOOTING_500_32.md` | HTTP 500.32 adım adım çözüm |
| `UPLOAD_INSTRUCTIONS.md` | Sunucuya yükleme talimatları |
| `app_offline.htm` | Bakım modu sayfası (güncelleme sırasında kullan) |

---

### 🎯 Başarı Kriterleri

Site çalışıyorsa şunları görmelisiniz:

1. ✅ `https://www.egecontrol.com/health` → `OK` (200)
2. ✅ Ana sayfa → Bootstrap CSS yükleniyor
3. ✅ Console → **404 hatası YOK**
4. ✅ Login → `admin@egecontrol.com` / `Admin123!`
5. ✅ Teklif oluşturma → PDF oluşturuluyor
6. ✅ Email gönderimi → SMTP çalışıyor

---

### 🔐 Güvenlik Notları

**Production'a geçmeden önce:**

1. ✅ Admin şifresini değiştir (`Admin123!` → güçlü şifre)
2. ✅ SMTP şifresini `appsettings.Production.json`'a taşı
3. ⚠️ app.db dosyası yedekleniyor mu kontrol et
4. ⚠️ SSL sertifikası kurulacaksa `app.UseHttpsRedirection()` aktif et
5. ⚠️ logs/ klasörünü düzenli temizle (disk dolmaması için)

---

### 📊 Performans Notları

- **InProcess hosting:** ~30% daha hızlı (vs OutOfProcess)
- **Framework-dependent:** Daha küçük publish (~172 MB vs ~400 MB)
- **SQLite:** Küçük/orta ölçekli projeler için yeterli
- **QuestPDF:** Community license (ticari kullanım için lisans gerekebilir)

---

## 🎉 Tamamlandı!

Bu yapılandırma **2 Ekim 2025** tarihinde test edilmiş ve **çalışır durumda** doğrulanmıştır.

**GitHub Repository:** https://github.com/ozdemirTolga/egecontrol-webapp

**Site:** https://www.egecontrol.com

---

### 💡 İletişim

Sorular için: tolga.ozdemir@egecontrol.com
