# Plesk Git Entegrasyonu Kurulum Rehberi

## 1. Plesk Panel'de Git Extension Kurulumu

1. Plesk panel'e giriş yapın
2. **Extensions** sekmesine gidin
3. **Git** extension'ını arayın ve kurun
4. Eğer yoksa, **Catalog** > **Featured Extensions** > **Git** 

## 2. Git Repository Bağlantısı

1. **Websites & Domains** > Sitenizi seçin
2. **Git** sekmesine tıklayın
3. **Add Repository** butonuna tıklayın
4. Repository bilgilerini girin:
   - **Repository URL**: `https://github.com/ozdemirTolga/egecontrol-webapp.git`
   - **Repository path**: `/egecontrol-webapp` (veya istediğiniz path)
   - **Branch**: `main`

## 3. Deployment Ayarları

1. **Actions** sekmesinde deployment script'i ayarlayın:

```bash
#!/bin/bash

# .NET 9 kurulu mu kontrol et
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET 9 runtime bulunamadı"
    exit 1
fi

# Proje dizinine git
cd /var/www/vhosts/your-domain.com/httpdocs/egecontrol-webapp/EgeControlWebApp

# Dependencies'leri restore et
dotnet restore

# Publish et
dotnet publish -c Release -o ../published --no-restore

# Published dosyaları site root'a kopyala
cp -r ../published/* /var/www/vhosts/your-domain.com/httpdocs/

# app_offline.htm kaldır (eğer varsa)
rm -f /var/www/vhosts/your-domain.com/httpdocs/app_offline.htm

echo "Deployment başarılı!"
```

## 4. SSH Key Authentication (Önerilen)

1. Plesk'te **SSH Keys** sekmesine gidin
2. Yeni SSH key oluşturun
3. Public key'i GitHub hesabınıza ekleyin:
   - GitHub > Settings > SSH and GPG keys > New SSH key

## 5. Webhook Kurulumu (Otomatik Deploy)

1. Plesk Git ayarlarında **Webhook URL**'yi kopyalayın
2. GitHub repository'de:
   - Settings > Webhooks > Add webhook
   - Payload URL: Plesk'ten kopyaladığınız URL
   - Content type: application/json
   - Events: "Just the push event"

## 6. İlk Deployment

1. Plesk Git panel'de **Pull & Deploy** butonuna tıklayın
2. Logları kontrol edin
3. Site çalışıyor mu test edin

## 7. Connection String Güncelleme

Deployment sonrası `appsettings.json` dosyasında SQL Server connection string'ini güncelleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-sql-server;Database=EgeControlWebApp;Trusted_Connection=false;User Id=your-username;Password=your-password;TrustServerCertificate=true;"
  }
}
```

## Troubleshooting

- **Dosya izinleri**: Site dosyalarının doğru izinlere sahip olduğundan emin olun
- **IIS restart**: Deployment sonrası IIS'i restart edin
- **Logs**: Plesk logs ve IIS logs'ları kontrol edin
