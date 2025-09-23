# Hosting.com.tr SQL Server Kurulum ve Veri Geçiş Rehberi

## 📋 **ADIM ADIM KURULUM**

### 1. Plesk Kontrol Paneline Giriş
- URL: https://panel.hosting.com.tr
- Kullanıcı adı ve şifrenizle giriş yapın

### 2. SQL Server Veritabanı Kontrolü
1. Sol menüden **"Veritabanları"** (Databases) seçin
2. Mevcut veritabanlarını listeleyin
3. `egecontr1_` veritabanının mevcut olduğunu doğrulayın
4. Eğer yoksa **"Veritabanı Ekle"** ile oluşturun

### 3. GitHub'dan Dosyaları İndirme
**Seçenek A - Manuel İndirme:**
1. https://github.com/ozdemirTolga/egecontrol-webapp adresine gidin
2. Yeşil **"Code"** butonuna tıklayın
3. **"Download ZIP"** seçin
4. ZIP'i açın

**Seçenek B - Git Clone:**
```bash
git clone https://github.com/ozdemirTolga/egecontrol-webapp.git
cd egecontrol-webapp/EgeControlWebApp
dotnet publish -c Release -o ./publish
```

### 4. Dosyaları Hosting'e Yükleme
**Plesk File Manager ile:**
1. Plesk'te **"Dosya Yöneticisi"** açın
2. `httpdocs` klasörüne gidin
3. Mevcut dosyaları silin (yedekledikten sonra)
4. `publish` klasöründeki tüm dosyaları yükleyin

**FTP ile:**
1. FileZilla gibi FTP istemcisi kullanın
2. Hosting FTP bilgilerinizle bağlanın
3. `httpdocs` klasörüne dosyaları yükleyin

### 5. SQL Server Migration İşlemi

#### A. Manuel SQL Script ile (Önerilen)
1. Local'de migration script oluşturun:
```bash
dotnet ef migrations script --context ApplicationDbContext --output migration.sql
```

2. Plesk'te **SQL Server Management** açın
3. **"Query"** sekmesine gidin
4. `migration.sql` içeriğini yapıştırın
5. **"Execute"** ile çalıştırın

#### B. Otomatik Migration (Riskli)
1. `appsettings.Production.json` dosyasını kontrol edin
2. Site ilk açıldığında otomatik migration çalışacak
3. Bu yöntem hosting'te bazen sorun çıkarabilir

### 6. Veri Geçişi (SQLite'dan SQL Server'a)

#### Yöntem 1: Manuel Veri Girişi
- Kritik veriler az ise manuel olarak yeniden girin
- Admin kullanıcısını tekrar oluşturun
- Önemli müşteri ve teklif verilerini aktarın

#### Yöntem 2: Programatik Geçiş
Local'de çalıştırılacak script:
```csharp
// Bu kodu local'de çalıştırarak SQL Server'a veri aktarın
// Program.cs'e geçici olarak ekleyebilirsiniz
```

### 7. Test ve Doğrulama
1. Site açıldığında hata var mı kontrol edin
2. Giriş yapabildiğinizi test edin
3. Temel fonksiyonları deneyin
4. Log dosyalarını kontrol edin

### 8. Acil Durum Planı
Eğer bir şeyler ters giderse:
1. Eski SQLite versiyonunu geri yükleyin
2. `appsettings.json`'ı SQLite'a geri çevirin
3. Hosting desteğini arayın: 0850 532 0 532

## 🛠️ **HAZIR KOMUTLAR**

### Local'de Migration Script Oluşturma:
```bash
cd EgeControlWebApp
dotnet ef migrations script --output migration.sql
```

### Connection String Kontrolü:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\MSSQLSERVER2019;Database=egecontr1_;User Id=egekontrol;Password=Baba1420**;TrustServerCertificate=true;MultipleActiveResultSets=true"
  }
}
```

### Hosting'te Çalıştırılacak SQL (Acil Durum):
```sql
-- Eğer migration çalışmazsa manuel tablo oluşturma
CREATE DATABASE egecontr1_;
USE egecontr1_;
-- (Migration script içeriğini buraya yapıştırın)
```

## 📞 **DESTEK İLETİŞİM**
- **Hosting.com.tr:** 0850 532 0 532
- **GitHub Repository:** https://github.com/ozdemirTolga/egecontrol-webapp
- **Acil Durum:** SQLite'a geri dönüş her zaman mümkün

## ✅ **KONTROL LİSTESİ**
- [ ] Plesk'e giriş yapıldı
- [ ] SQL Server veritabanı kontrol edildi
- [ ] GitHub'dan son versiyon indirildi
- [ ] Dosyalar hosting'e yüklendi
- [ ] Migration çalıştırıldı
- [ ] Site test edildi
- [ ] Veriler kontrol edildi
- [ ] Backup sistemi aktif edildi
