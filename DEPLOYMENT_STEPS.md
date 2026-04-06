# 🚀 Deployment Adımları (14 Şubat 2026)

## ✅ TAMAMLANDI: Build & Publish

Aşağıdaki komutlar başarıyla çalıştırıldı:

```powershell
cd "E:\OneDrive\EgeOtomasyon\egecontrol.com\egecontrol-webapp\EgeControlWebApp"

# 1. Temizle
dotnet clean
✅ Başarılı

# 2. Release Build
dotnet build -c Release
✅ Başarılı (19 uyarı var ama sorun değil - debug warnings)

# 3. Publish
dotnet publish -c Release -o publish
✅ Başarılı
```

---

## 📁 Dosya Yapısı

### Publish Klasörü İçeriği:
```
publish/
├── EgeControlWebApp.dll              ← Ana uygulama dosyası
├── EgeControlWebApp.exe              ← Executable
├── EgeControlWebApp.pdb              ← Debug info (production'dan çıkarılabilir)
├── appsettings.json                  ← Development ayarları
├── appsettings.Production.json       ← Production ayarları (24h session)
├── app.db                            ← SQLite veritabanı (KORUMA ALT!)
├── web.config                        ← IIS yapılandırması
├── wwwroot/                          ← Statik dosyalar (CSS, JS, resimler)
├── runtimes/                         ← Platform-specific dependencies
└── [diğer DLL dosyaları]            ← Framework bağımlılıkları
```

---

## 🚨 ÖNEMLİ: Hangi Dosyalar KORUMA ALTINDA TUTULACAK

### ❌ HİÇBİR ŞEKILDE DEĞİŞTİRİLMEYECEK:
1. **`app.db`** - Veritabanı dosyası
   - Tüm müşteri bilgileri
   - Tüm teklifler
   - Kullanıcı hesapları
   
2. **`wwwroot/teklifler/`** - PDF dosyaları
   - Oluşturduğunuz tüm PDF'ler
   - Müşteri teklifleri

3. **Özel PDF dosyaları** - Eğer başka yerlerde depoluyorsanız

---

## 🖥️ HOSTING'E KOPYALAMA

### Seçenek 1: **FTP ile Manuel Kopyalama** (Önerilen)

```powershell
# 1. Publish klasörünün tüm içeriğini seç
cd "E:\OneDrive\EgeOtomasyon\egecontrol.com\egecontrol-webapp\EgeControlWebApp\publish"

# 2. Tüm dosyaları FTP'ye kopyala (VEYA sıkıştırıp yükle)

# 3. Hosting üzerinde:
#    - Eski bin/ klasörünü yedekle
#    - Publish klasörünün tüm içeriğini hostingin web root'una kopyala

# 4. İlişkili dosyaları SAKLA:
#    ✗ app.db (TUTMA)
#    ✗ wwwroot/teklifler/ klasörü (TUTMA)
#    ✗ logs/ klasörü (TUTMA)
```

### Seçenek 2: **PowerShell Script ile Kopyalama**

```powershell
# Hosting'e bağlanıp, publish klasörünü kopyala
$sourceFolder = "E:\OneDrive\EgeOtomasyon\egecontrol.com\egecontrol-webapp\EgeControlWebApp\publish"
$destinationFolder = "\\[HOSTING_SERVER]\[SITE_PATH]\bin\"

# Mevcut bin yedekle
if (Test-Path $destinationFolder) {
    Rename-Item $destinationFolder "$destinationFolder.backup_$(Get-Date -Format 'yyyyMMdd_HHmm')" -Force
}

# Yeni dosyaları kopyala
Copy-Item -Path "$sourceFolder\*" -Destination "\\[HOSTING_SERVER]\[SITE_PATH]\" -Recurse -Force `
    -Exclude @("app.db", "logs", "teklifler")
```

---

## 🔄 Hosting'de Yapılacak İşlemler

### 1. **IIS Application Pool Restart**

```powershell
# PowerShell'de (Administrator olarak):
Restart-WebAppPool -Name "EgeControlAppPool"

# VEYA IIS Manager'dan:
# IIS Manager → Application Pools → EgeControlAppPool → Right Click → Restart
```

### 2. **Dosya İzinleri (Hosting üzerinde)**

```powershell
# logs klasörü için yazma izni
icacls "C:\inetpub\wwwroot\egecontrol\logs" /grant "IIS_IUSRS:(OI)(CI)M" /T

# wwwroot/teklifler klasörü için yazma izni
icacls "C:\inetpub\wwwroot\egecontrol\wwwroot\teklifler" /grant "IIS_IUSRS:(OI)(CI)M" /T

# app.db için izin
icacls "C:\inetpub\wwwroot\egecontrol\app.db" /grant "IIS_IUSRS:M" /T
```

---

## 📋 Hosting Kopyalama Adımları (ÖZETİ)

### Publish Klasöründe KİM GİDER KİM KALIR?

| Dosya/Klasör | Kopyalanır mı? | Not |
|---|---|---|
| `EgeControlWebApp.dll` | ✅ **KOPYALA** | Yeni versiyon |
| `EgeControlWebApp.exe` | ✅ **KOPYALA** | Yeni versiyon |
| `appsettings.json` | ✅ **KOPYALA** | 24h session ayarı güncellendi |
| `appsettings.Production.json` | ✅ **KOPYALA** | 24h session ayarı güncellendi |
| `web.config` | ✅ **KOPYALA** | (yoksa staging'den al) |
| `wwwroot/` | ⚠️ **SEÇEREK KOPYALA** | wwwroot/teklifler/ TUTMA |
| `runtimes/` | ✅ **KOPYALA** | Dependency dosyaları |
| `app.db` | ❌ **TUTMA** | Veritabanı koruması |
| `logs/` | ❌ **TUTMA** | Eski log'ları koru |
| Diğer `.dll` dosyaları | ✅ **KOPYALA** | Framework dependencies |

---

## ⚡ EN HızLı KOPYALAMA YÖNTEMİ

### Eğer FTP'niz varsa:
1. Publish klasörünü `.zip` ile sıkıştır
2. FTP'ye yükle
3. Hosting üzerinde bir kla sörde aç
4. `app.db`, `logs/`, `wwwroot/teklifler/` koruma altında tut
5. Diğer dosyaları kopyala

### PowerShell Command (Dosyaları hızlı kopyala):
```powershell
# Publish klasörünün tüm dosyalarını sıkıştır
cd "E:\OneDrive\EgeOtomasyon\egecontrol.com\egecontrol-webapp\EgeControlWebApp"
Compress-Archive -Path "publish\*" -DestinationPath "publish.zip" -Force

# Çıktu: publish.zip oluşturuldu
```

---

## 🧪 Test Edilecekler

Deployment sonrası:

- [ ] Site açılıyor mu? (http://egecontrol.com)
- [ ] Login yapılıyor mu?
- [ ] 24 saat oturum kalıyor mu? (Teklif kaydet → login sayfasına gitmiyor mu?)
- [ ] PDF oluşturuluyor mu?
- [ ] Eski teklifler/PDF'ler var mı?
- [ ] Veritabanı bağlantısı çalışıyor mu?

---

## 📞 Sorunlar Olursa

1. **IIS Manager** → Logs → Hataları kontrol et
2. **Event Viewer** → Application logs
3. **web.config** hatasının olup olmadığını kontrol et
4. **app.db** dosyası var mı ve erişime açık mı?

---

## ✅ ÖZETLE

✅ Build tamamlandı → `publish/` klasöründe hazır
✅ Cookie ömrü 24 saate ayarlandı
✅ Şu an yapman gereken:
   1. `publish/` klasörünü hostinge kopyala
   2. `app.db`, `logs/`, `teklifler/` TUTMA
   3. IIS App Pool restart et
   4. Site'yi test et

Başarılı deploymentler! 🚀
