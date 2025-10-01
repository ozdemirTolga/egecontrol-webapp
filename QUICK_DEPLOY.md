# 🚀 Hızlı Deployment Rehberi

## Otomatik Deployment (Önerilen)

### Yöntem 1: PowerShell Script

**Terminal'de çalıştır:**
```powershell
.\deploy.ps1
```

**Script ne yapar?**
1. ✅ Eski publish klasörünü temizler
2. ✅ Release modda build yapar
3. ✅ Framework-dependent publish oluşturur
4. ✅ wwwroot'u manuel kopyalar (StaticWebAssets workaround)
5. ✅ Kritik dosyaları kontrol eder:
   - EgeControlWebApp.dll
   - web.config
   - app.db
   - Bootstrap CSS/JS
   - Site CSS/JS
6. ✅ Boyut ve dosya sayısını gösterir
7. ✅ Sunucu setup talimatlarını gösterir

**Örnek Çıktı:**
```
=== EgeControl Deployment Starting ===

Cleaning old publish folder...
  [OK] Old files removed

Building (Release)...
  [OK] Build successful

Publishing...
  [OK] Publish completed

Copying wwwroot...
  [OK] wwwroot copied

=== Publish Statistics ===
  Size: 159.48 MB
  Files: 287
  Location: F:\egecontrol-webapp-main\publish

=== Critical File Checks ===
  [OK] EgeControlWebApp.dll
  [OK] web.config
  [OK] Bootstrap CSS

=== DEPLOYMENT SUCCESSFUL ===

Next step: Upload publish folder to server
```

---

### Yöntem 2: VS Code Task (Klavye Kısayolu)

**Adımlar:**
1. **Ctrl+Shift+P** (Komut Paleti)
2. **"Tasks: Run Task"** yaz
3. **"🚀 Deploy to Production"** seç
4. Script otomatik çalışır

**Veya:**
- **Ctrl+Shift+B** (Default Build Task) → Otomatik deploy başlar

---

## Manuel Deployment (Gelişmiş)

Eğer script kullanmak istemezsen:

```powershell
# 1. Temizle
cd EgeControlWebApp
Remove-Item ..\publish -Recurse -Force -ErrorAction SilentlyContinue

# 2. Build
dotnet build -c Release

# 3. Publish
dotnet publish -c Release -o "..\publish" --self-contained false

# 4. wwwroot kopyala
Copy-Item wwwroot ..\publish\wwwroot -Recurse -Force

# 5. İç içe wwwroot düzelt (gerekirse)
if (Test-Path "..\publish\wwwroot\wwwroot") {
    Move-Item "..\publish\wwwroot\wwwroot\*" "..\publish\wwwroot\" -Force
    Remove-Item "..\publish\wwwroot\wwwroot" -Recurse -Force
}

cd ..
```

---

## Sunucuya Yükleme

### 1. Dosyaları Yükle

**FTP/FileZilla ile:**
```
Kaynak: f:\egecontrol-webapp-main\publish\
Hedef: C:\inetpub\wwwroot\egecontrol\
```

**Önemli:** TÜM alt klasörleri yükle!
- wwwroot/lib/bootstrap/
- wwwroot/css/
- wwwroot/js/
- wwwroot/teklifler/

---

### 2. IIS Application Pool

**PowerShell (Admin):**
```powershell
Import-Module WebAdministration

# 64-bit yap
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false

# .NET CLR yok
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name managedRuntimeVersion -Value ""

# Restart
Restart-WebAppPool -Name EgeControlAppPool
```

---

### 3. Dosya İzinleri

**PowerShell (Admin):**
```powershell
$sitePath = "C:\inetpub\wwwroot\egecontrol"

# Full Control
icacls $sitePath /grant "IIS_IUSRS:(OI)(CI)F" /T

# app.db yazılabilir
icacls "$sitePath\app.db" /grant "IIS_IUSRS:(M)" /T

# teklifler yazılabilir (PDF kaydetme)
icacls "$sitePath\wwwroot\teklifler" /grant "IIS_IUSRS:(OI)(CI)M" /T

# logs klasörü
New-Item -ItemType Directory -Path "$sitePath\logs" -Force
icacls "$sitePath\logs" /grant "IIS_IUSRS:(OI)(CI)M" /T
```

---

### 4. IIS Restart

```powershell
iisreset
```

---

## Test

### Health Check
```
https://www.egecontrol.com/health
```
**Beklenen:** `OK` (200)

### Ana Sayfa
```
https://www.egecontrol.com/
```
**Beklenen:** Bootstrap CSS yüklü, menüler çalışıyor

### Console Kontrolü
**F12** → **Console** → **404 hatası olmamalı!**

### Login
```
https://www.egecontrol.com/Identity/Account/Login
```
**Kullanıcı:** admin@egecontrol.com  
**Şifre:** Admin123!

---

## Güncelleme (Var Olan Site İçin)

### 1. Bakım Modu Aktif Et

**Sunucuda:**
```powershell
Copy-Item app_offline.htm C:\inetpub\wwwroot\egecontrol\app_offline.htm
```

Site şimdi **"Bakım Modu"** mesajı gösterir.

---

### 2. Yeni Dosyaları Yükle

Eski dosyaların üzerine yaz (app.db hariç!).

**app.db'yi korumak için:**
```powershell
# Sunucuda yedek al
Copy-Item C:\inetpub\wwwroot\egecontrol\app.db C:\inetpub\wwwroot\egecontrol\app.db.backup

# Yeni dosyaları yükle (app.db hariç)
# FTP ile publish\* yükle
```

---

### 3. Bakım Modunu Kapat

**Sunucuda:**
```powershell
Remove-Item C:\inetpub\wwwroot\egecontrol\app_offline.htm -Force
```

Site tekrar aktif!

---

## Sorun Giderme

### ❌ CSS/JS 404 Hatası

**Sebep:** wwwroot kopyalanmamış.

**Çözüm:**
```powershell
# Publish klasöründe wwwroot kontrolü
Test-Path publish\wwwroot\lib\bootstrap

# Yoksa deploy.ps1 tekrar çalıştır
.\deploy.ps1
```

---

### ❌ HTTP 500.32

**Sebep:** IIS 32-bit, uygulama 64-bit (veya tersi).

**Çözüm:**
```powershell
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false
Restart-WebAppPool -Name EgeControlAppPool
iisreset
```

Detay: [TROUBLESHOOTING_500_32.md](TROUBLESHOOTING_500_32.md)

---

### ❌ HTTP 500.30

**Sebep:** .NET 9.0 Runtime yok.

**Çözüm:**
1. [.NET 9.0 Hosting Bundle İndir](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Kur
3. **Sunucuyu restart et**
4. `iisreset`

---

## Yardım Dokümanları

| Dosya | Ne Zaman Kullan |
|-------|----------------|
| **DEPLOYMENT_GUIDE.md** | İlk deployment, detaylı adımlar |
| **TROUBLESHOOTING_500_32.md** | HTTP 500.32 hatası |
| **UPLOAD_INSTRUCTIONS.md** | Sunucuya yükleme detayları |
| **DEPLOYMENT_NOTES.md** | Tüm yapılandırma özeti |

---

## 📞 İletişim

Sorular için: tolga.ozdemir@egecontrol.com

**GitHub:** https://github.com/ozdemirTolga/egecontrol-webapp
