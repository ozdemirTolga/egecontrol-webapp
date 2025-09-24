# Acil Veri Geri Yükleme Rehberi

## 1. Hosting Yedek Kontrolü
- Hosting kontrol paneline giriş yapın
- Backup/Yedekleme menüsünü açın
- Son yedekleme tarihini kontrol edin
- Yedek dosyasını indirin

## 2. Veritabanı Geri Yükleme
```bash
# Eğer SQLite kullanıyorsanız
# Yedek app.db dosyasını sunucuya yükleyin

# Eğer MySQL kullanıyorsanız
# phpMyAdmin'den Import ile .sql dosyasını yükleyin
```

## 3. Acil Durum Komutları
```bash
# Uygulamayı durdur
dotnet ef database drop --force

# Yeni veritabanı oluştur
dotnet ef database update

# Eğer yedek varsa, eski app.db'yi yerine koy
```

## 4. Gelecek İçin Otomatik Yedekleme
```csharp
// Startup.cs veya Program.cs'de
services.AddHostedService<DatabaseBackupService>();
```

## 5. Acil İletişim
- Hosting sağlayıcısının teknik desteğini HEMEN arayın
- Ticket açın
- Telefon desteği varsa arayın

## 6. Veri Kaybını Minimize Etmek İçin
- Hemen kullanıcılara duyuru yapın
- Siteyi geçici olarak kapatmayı düşünün
- Yedek planını devreye sokun
