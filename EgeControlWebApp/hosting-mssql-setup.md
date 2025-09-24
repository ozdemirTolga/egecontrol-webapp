# Hosting.com.tr SQL Server Yapılandırma Rehberi

## 1. Plesk Üzerinden SQL Server Veritabanı Oluşturma

### Adım 1: Plesk'e Giriş
- https://panel.hosting.com.tr adresine gidin
- Kullanıcı adı ve şifrenizle giriş yapın

### Adım 2: Veritabanı Oluşturma
1. Sol menüden **"Veritabanları"** (Databases) seçin
2. **"Veritabanı Ekle"** (Add Database) butonuna tıklayın
3. Veritabanı adı: `egecontr1_` (zaten mevcut görünüyor)
4. Kullanıcı adı: `egekontrol` (zaten mevcut)
5. Şifre: `Baba1420**` (zaten ayarlı)

### Adım 3: Bağlantı Bilgileri Kontrolü
Veritabanı oluşturduktan sonra:
- Server: `.\MSSQLSERVER2019` veya sunucu IP'si
- Database: `egecontr1_`
- Username: `egekontrol`
- Password: `Baba1420**`

## 2. Web.config Dosyası Güncellemesi

Hosting.com.tr Windows hosting'de web.config dosyası gerekli:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments="EgeControlWebApp.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" />
    </system.webServer>
  </location>
</configuration>
```

## 3. Connection String Güncelleme

appsettings.json dosyasında:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\MSSQLSERVER2019;Database=egecontr1_;User Id=egekontrol;Password=Baba1420**;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

Alternatif connection string (eğer yukarıdaki çalışmazsa):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=.\\MSSQLSERVER2019;Initial Catalog=egecontr1_;User ID=egekontrol;Password=Baba1420**;Encrypt=False;TrustServerCertificate=true"
  }
}
```

## 4. Migration Komutları

Local'de:
```bash
# Eski migration'ları sil (opsiyonel)
dotnet ef database drop --force

# Yeni migration oluştur
dotnet ef migrations add "InitialMSSQLMigration"

# Veritabanını güncelle
dotnet ef database update
```

## 5. Publish ve Upload

```bash
# Publish
dotnet publish -c Release -o ./publish

# FTP ile hosting'e yükle:
# - publish klasöründeki tüm dosyaları httpdocs'a kopyala
# - web.config dosyasını kontrol et
# - appsettings.json'ı kontrol et
```

## 6. Hosting.com.tr Özel Ayarlar

### IIS Application Pool
- .NET Core Runtime gerekli
- Hosting.com.tr'de genelde önceden yüklü

### Log Dosyaları
- logs klasörü oluşturulmalı
- stdout logları için write permission gerekli

### Troubleshooting
1. Site açılmıyorsa: logs/stdout loglarını kontrol edin
2. Veritabanı bağlantı hatası: Connection string'i kontrol edin
3. 500 hataları: web.config ve IIS ayarlarını kontrol edin

## 7. Test Connection
SQL Server Management Studio ile test:
- Server: hosting sunucu IP'si
- Authentication: SQL Server Authentication
- Login: egekontrol
- Password: Baba1420**

Uzaktan bağlantı genelde kapalıdır, hosting desteğinden açtırabilirsiniz.

## 8. Acil Durum
Eğer bir şeyler ters giderse:
1. app_backup_before_mssql.db dosyasını app.db olarak rename edin
2. appsettings.json'da SQLite connection string'e geri dönün
3. Program.cs'de UseSqlite() kullanın
