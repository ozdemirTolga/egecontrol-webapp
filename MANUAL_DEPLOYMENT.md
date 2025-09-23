# Manuel Deployment Rehberi

## FileZilla ile Upload

1. **FileZilla İndir**: https://filezilla-project.org/download.php?type=client
2. **Bağlan**:
   - Host: ftp.hosting.com.tr (veya sitenizin FTP adresi)
   - Username: FTP kullanıcı adınız
   - Password: FTP şifreniz  
   - Port: 21

3. **Upload**:
   - Sol panel: `D:\users\ar709568\OneDrive - ARÇELİK A.Ş\Downloads\Yeni klasör (5)\egecontrol-webapp\publish`
   - Sağ panel: `/httpdocs` (site root klasörü)
   - Tüm dosyaları sürükle-bırak

## Plesk File Manager ile Upload

1. Plesk Panel > **Files**
2. **httpdocs** klasörüne git
3. **Upload Files** > Browse
4. `publish` klasöründeki dosyaları seç
5. Upload et

## Connection String Güncelleme

Upload sonrası `appsettings.json`'ı düzenle:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=sql-server-address;Database=EgeControlWebApp;User Id=username;Password=password;TrustServerCertificate=true;"
  }
}
```

## Test

- Site URL'ini ziyaret et
- Database bağlantısını kontrol et
- Admin panel erişimini test et
