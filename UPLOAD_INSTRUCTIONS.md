# 📤 Sunucuya Yükleme Talimatları

## ✅ Publish Hazır: 172 MB

**Yüklenecek klasör:**
```
f:\egecontrol-webapp-main\egecontrol-webapp-main\publish\
```

---

## 🚀 ADIM ADIM YÜKLEME

### 1️⃣ Eski Dosyaları Sil (Sunucuda)

**IIS'de uygulamayı durdur:**
```
C:\inetpub\wwwroot\egecontrol
```
Bu klasördeki **TÜM** dosyaları sil (app_offline.htm hariç)

**VEYA** `app_offline.htm` koy (bakım modu):
```html
<!DOCTYPE html>
<html>
<head>
    <title>Bakım Modu</title>
    <meta charset="utf-8" />
</head>
<body>
    <h1>🔧 Site Güncelleniyor</h1>
    <p>Lütfen birkaç dakika sonra tekrar deneyin.</p>
</body>
</html>
```

---

### 2️⃣ Publish Klasörünü Yükle

**Tüm dosyaları kopyala:**
```
f:\egecontrol-webapp-main\egecontrol-webapp-main\publish\
↓
C:\inetpub\wwwroot\egecontrol\
```

**FTP ile yüklüyorsan:**
- FileZilla kullan
- Binary mode (ikili mod) seç
- **BÜTÜN ALT KLASÖRLERE DİKKAT ET:**
  - `wwwroot/lib/bootstrap/`
  - `wwwroot/css/`
  - `wwwroot/js/`
  - `wwwroot/teklifler/` (PDF'ler burada)

---

### 3️⃣ Dosya İzinleri Ayarla (Sunucuda)

**PowerShell (Admin):**
```powershell
$sitePath = "C:\inetpub\wwwroot\egecontrol"

# IIS_IUSRS tam izin ver
icacls $sitePath /grant "IIS_IUSRS:(OI)(CI)F" /T

# app.db yazılabilir olmalı
icacls "$sitePath\app.db" /grant "IIS_IUSRS:(M)" /T

# teklifler klasörü yazılabilir olmalı (PDF kaydetmek için)
icacls "$sitePath\wwwroot\teklifler" /grant "IIS_IUSRS:(OI)(CI)M" /T

# logs klasörü oluştur (yoksa)
New-Item -ItemType Directory -Path "$sitePath\logs" -Force
icacls "$sitePath\logs" /grant "IIS_IUSRS:(OI)(CI)M" /T
```

---

### 4️⃣ IIS Application Pool Ayarları

**IIS Manager:**
1. **Application Pools** → `EgeControlAppPool`
2. **Advanced Settings:**
   - **.NET CLR Version** = `No Managed Code` ✅
   - **Enable 32-Bit Applications** = `False` ✅
   - **Identity** = `ApplicationPoolIdentity` ✅

**Veya PowerShell:**
```powershell
Import-Module WebAdministration

# Application Pool ayarları
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false

# Restart
Restart-WebAppPool -Name EgeControlAppPool
```

---

### 5️⃣ IIS Restart

```powershell
iisreset
```

**VEYA** sadece site restart:
```powershell
Restart-WebAppPool -Name EgeControlAppPool
Restart-WebItem "IIS:\Sites\EgeControl"
```

---

### 6️⃣ app_offline.htm Sil

Eğer bakım modu için `app_offline.htm` koyduysan, **ŞİMDİ SİL:**
```powershell
Remove-Item "C:\inetpub\wwwroot\egecontrol\app_offline.htm" -Force
```

---

### 7️⃣ Test Et

#### ✅ Health Check:
```
https://www.egecontrol.com/health
```
**Beklenen:** `OK` (200)

#### ✅ Ana Sayfa:
```
https://www.egecontrol.com/
```
**Beklenen:** Admin paneli yüklenmeli, **CSS çalışmalı** (artık 404 hatası yok!)

#### ✅ Statik Dosyalar:
```
https://www.egecontrol.com/lib/bootstrap/dist/css/bootstrap.min.css
```
**Beklenen:** CSS dosyası indirilmeli (404 OLMAMALI!)

#### ✅ Login:
```
https://www.egecontrol.com/Identity/Account/Login
```
**Kullanıcı adı:** admin@egecontrol.com  
**Şifre:** Admin123!

---

## 🔧 Hata Alırsan

### ❌ Hala 404 (CSS/JS bulunamıyor)

**Kontrol et:**
```powershell
# wwwroot var mı?
Test-Path "C:\inetpub\wwwroot\egecontrol\wwwroot"

# Bootstrap var mı?
Test-Path "C:\inetpub\wwwroot\egecontrol\wwwroot\lib\bootstrap\dist\css\bootstrap.min.css"
```

**Yoksa:**
- FTP ile yüklerken alt klasörler atlanmış olabilir
- Manuel kopyala/yapıştır ile yükle

---

### ❌ HTTP 500.32 Tekrar Gelirse

```powershell
# Application Pool 64-bit yap
Import-Module WebAdministration
Set-ItemProperty IIS:\AppPools\EgeControlAppPool -Name enable32BitAppOnWin64 -Value $false
Restart-WebAppPool -Name EgeControlAppPool
iisreset
```

---

### ❌ HTTP 500.30 (Runtime bulunamıyor)

**.NET 9.0 Hosting Bundle eksik!**

1. [İndir: .NET 9.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Kur
3. **Sunucuyu restart et** (önemli!)
4. `iisreset` çalıştır

---

## 📊 Yükleme Sonrası Kontrol Listesi

- [ ] `app_offline.htm` silindi mi?
- [ ] `wwwroot/lib/bootstrap/` klasörü var mı?
- [ ] `wwwroot/css/site.css` var mı?
- [ ] IIS Application Pool: Enable 32-Bit = False
- [ ] IIS Application Pool: .NET CLR = No Managed Code
- [ ] Dosya izinleri: IIS_IUSRS → Full Control
- [ ] https://www.egecontrol.com/health → OK
- [ ] Ana sayfa CSS yükleniyor mu?
- [ ] Login çalışıyor mu?

---

## ✅ Başarılı Görünüm

**Artık şunları görmelisiniz:**

1. ✅ Ana sayfa düzgün CSS ile yükleniyor
2. ✅ Bootstrap menüler çalışıyor
3. ✅ Console'da **404 hatası YOK**
4. ✅ Login formu düzgün görünüyor

**Başarılar! 🎉**

---

## 📞 Yardım

Hata devam ederse:

1. **Event Viewer** → **Application** → IIS AspNetCore loglarını kontrol et
2. `logs/stdout_*.log` dosyasını aç
3. Browser Console'daki **tam hata mesajını** paylaş
