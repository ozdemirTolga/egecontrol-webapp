# 🔧 HTTP 500.32 Hatası - Çözüm Rehberi

## ❌ Hata: Failed to load .NET Core host

Bu hata, uygulama ile IIS'in çalıştığı mimari uyumsuzluğundan kaynaklanır.

---

## ✅ ÇÖZÜM ADIMLARı

### 1️⃣ Application Pool Bitness'i Kontrol Et

**IIS Manager'da:**
1. **Application Pools** → `EgeControlAppPool` seç
2. **Advanced Settings** → **Process Model** bölümü
3. **Enable 32-Bit Applications** ayarını kontrol et

**❗ ÖNEMLİ:**
- Eğer **Enable 32-Bit Applications = True** ise, **False** yap
- Modern sunucularda **64-bit** kullanılmalı

**PowerShell ile kontrol:**
```powershell
Import-Module WebAdministration
Get-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64
```

**PowerShell ile düzelt (64-bit yap):**
```powershell
Import-Module WebAdministration
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false
Restart-WebAppPool -Name EgeControlAppPool
```

---

### 2️⃣ .NET Hosting Bundle Yüklü mü Kontrol Et

**.NET 9.0 Hosting Bundle yüklü olmalı!**

**Kontrol için PowerShell:**
```powershell
# .NET Runtime versiyonlarını listele
dotnet --list-runtimes

# Aranan satırlar:
# Microsoft.AspNetCore.App 9.0.x
# Microsoft.NETCore.App 9.0.x
```

**Yüklü değilse:**
1. [.NET 9.0 Hosting Bundle İndir](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Kur
3. **Sunucuyu yeniden başlat** (önemli!)
4. `iisreset` komutunu çalıştır

---

### 3️⃣ AspNetCoreModuleV2 Yüklü mü?

**Kontrol:**
```powershell
# IIS Modules'de AspNetCoreModuleV2 var mı?
Get-WebConfigurationProperty -PSPath 'MACHINE/WEBROOT/APPHOST' -Filter "system.webServer/globalModules/add[@name='AspNetCoreModuleV2']" -Name "."
```

**Yoksa:**
- .NET Hosting Bundle kurunca otomatik yüklenir
- Sunucu restart gerekir

---

### 4️⃣ Application Pool Identity Doğru mu?

**IIS Manager:**
1. Application Pool → **Advanced Settings**
2. **Identity** = `ApplicationPoolIdentity` olmalı

**Alternatif çözüm (yerel test için):**
- Identity'yi `LocalSystem` yap (sadece test için!)
- Production'da `ApplicationPoolIdentity` kullan

---

### 5️⃣ Dosya İzinleri

**IIS_IUSRS grubuna tam izin ver:**
```powershell
$path = "C:\inetpub\wwwroot\egecontrol"  # Site yolunuz
icacls $path /grant "IIS_IUSRS:(OI)(CI)F" /T
```

---

### 6️⃣ Event Viewer'ı Kontrol Et

**Windows Event Viewer:**
1. **Windows Logs** → **Application**
2. **Source** = `IIS AspNetCore Module V2` filtrele
3. Son hatayı oku

**Yaygın hatalar:**
- `Failed to start application` → .NET Runtime yok
- `Could not load file or assembly` → Bağımlılık eksik
- `Access is denied` → İzin sorunu

---

### 7️⃣ Stdout Logları Aktif Et

Eğer `logs/` klasörü yoksa:

**web.config'e ekle:**
```xml
<aspNetCore ... stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout">
```

**Klasör oluştur:**
```powershell
New-Item -ItemType Directory -Path "C:\inetpub\wwwroot\egecontrol\logs" -Force
icacls "C:\inetpub\wwwroot\egecontrol\logs" /grant "IIS_IUSRS:(OI)(CI)M" /T
```

**IIS Restart:**
```powershell
iisreset
```

Sonra `logs/stdout_*.log` dosyasını aç ve hatayı oku.

---

### 8️⃣ Sunucu 32-bit mi 64-bit mi?

**Kontrol:**
```powershell
# İşlemci mimarisi
$env:PROCESSOR_ARCHITECTURE

# 64-bit: AMD64
# 32-bit: x86
```

**Eğer sunucu 32-bit ise:**
```powershell
# Application Pool'u 32-bit yap
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $true
Restart-WebAppPool -Name EgeControlAppPool
```

---

### 9️⃣ Hızlı Çözüm Komutu (Tümü)

**PowerShell (Admin olarak):**
```powershell
# 1. Application Pool'u 64-bit yap
Import-Module WebAdministration
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false

# 2. Dosya izinleri
$sitePath = "C:\inetpub\wwwroot\egecontrol"
icacls $sitePath /grant "IIS_IUSRS:(OI)(CI)F" /T

# 3. Logs klasörü oluştur
New-Item -ItemType Directory -Path "$sitePath\logs" -Force
icacls "$sitePath\logs" /grant "IIS_IUSRS:(OI)(CI)M" /T

# 4. IIS Restart
iisreset

# 5. Application Pool Restart
Restart-WebAppPool -Name EgeControlAppPool

Write-Host "Tamamlandı! Şimdi siteyi deneyin."
```

---

### 🔟 Son Kontrol Listesi

- [ ] .NET 9.0 Hosting Bundle yüklü
- [ ] Application Pool: **Enable 32-Bit Applications = False**
- [ ] Application Pool: **.NET CLR Version = No Managed Code**
- [ ] web.config: `processPath="dotnet"` ve `arguments=".\EgeControlWebApp.dll"`
- [ ] Dosya izinleri: IIS_IUSRS → Modify
- [ ] logs/ klasörü var ve yazılabilir
- [ ] Sunucu restart yapıldı (Hosting Bundle sonrası)

---

## 📞 Hala Çalışmıyor mu?

**Event Viewer'daki tam hata mesajını paylaşın:**
1. Event Viewer → Application
2. Son IIS AspNetCore hatası
3. Mesajı kopyalayın

**Veya stdout log'unu gönderin:**
```
logs/stdout_20251002_123456.log
```

---

## ✅ Başarı Testi

**Çalışıyorsa şunları görmelisiniz:**

1. **Health Check:**
   ```
   http://www.egecontrol.com/health
   → OK (200)
   ```

2. **Ana Sayfa:**
   ```
   http://www.egecontrol.com/
   → Ege Control ana sayfası
   ```

3. **Login:**
   ```
   http://www.egecontrol.com/Identity/Account/Login
   → Login formu
   ```

**Başarılar! 🎉**
