# Ege Control Web Uygulaması

Bu proje www.egecontrol.com için geliştirilen ASP.NET Core 9 tabanlı bir işletme yönetim sistemidir.

## Özellikler

### 🔐 Admin Yetkilendirme Sistemi
- Güvenli admin girişi
- Rol tabanlı erişim kontrolü
- Otomatik admin kullanıcı oluşturma

### 👥 Müşteri Yönetimi
- Müşteri ekleme, düzenleme, silme (CRUD)
- Müşteri arama ve filtreleme
- Detaylı müşteri bilgileri
- Aktif/Pasif durum yönetimi

### 📄 Teklif Yönetimi
- Dinamik teklif oluşturma
- Teklif kalemleri yönetimi
- Otomatik teklif numarası üretimi
- Teklif durumu takibi (Taslak, Gönderildi, Onaylandı, vb.)
- KDV hesaplama

### 📊 PDF Export
- Profesyonel teklif PDF'leri
- Müşteri listesi PDF raporu
- iText7 kütüphanesi ile yüksek kaliteli PDF oluşturma

### 📈 Dashboard
- Özet istatistikler
- Son eklenen müşteriler
- Son oluşturulan teklifler
- Hızlı erişim menüleri

## Teknolojiler

- **Framework**: ASP.NET Core 9
- **UI**: Razor Pages
- **Veritabanı**: SQLite (geliştirme), SQL Server uyumlu
- **ORM**: Entity Framework Core
- **Kimlik Doğrulama**: ASP.NET Core Identity
- **PDF**: iText7
- **Frontend**: Bootstrap 5, Font Awesome

## Kurulum

### Gereksinimler
- .NET 9 SDK
- Visual Studio Code veya Visual Studio

### Adımlar

1. **Projeyi klonlayın:**
   ```bash
   git clone [repository-url]
   cd EgeControlWebApp
   ```

2. **Bağımlılıkları yükleyin:**
   ```bash
   dotnet restore
   ```

3. **Veritabanını güncelleyin:**
   ```bash
   dotnet ef database update
   ```

4. **Uygulamayı çalıştırın:**
   ```bash
   dotnet run
   ```

5. **Tarayıcıda açın:**
   - Ana sayfa: `https://localhost:5001`
   - Admin girişi: `https://localhost:5001/Identity/Account/Login`

## Varsayılan Admin Hesabı

- **Email**: admin@egecontrol.com
- **Şifre**: Admin123!

## Proje Yapısı

```
EgeControlWebApp/
├── Areas/
│   └── Admin/              # Admin panel sayfaları
├── Data/                   # Veritabanı context
├── Models/                 # Veri modelleri
├── Services/               # İş mantığı servisleri
├── Pages/                  # Public sayfalar
├── wwwroot/                # Statik dosyalar
└── Program.cs              # Uygulama yapılandırması
```

## Kullanım

### Admin Paneline Erişim
1. `/Identity/Account/Login` sayfasından giriş yapın
2. Varsayılan admin hesabını kullanın
3. Dashboard'a yönlendirileceksiniz

### Müşteri Ekleme
1. Admin panelinde "Müşteriler" menüsünü tıklayın
2. "Yeni Müşteri" butonunu kullanın
3. Gerekli bilgileri doldurun

### Teklif Oluşturma
1. "Teklifler" menüsünden "Yeni Teklif" oluşturun
2. Müşteri seçin (otomatik bilgi doldurma)
3. Teklif kalemlerini ekleyin
4. PDF olarak export edin

## Geliştirme

### Yeni Migration Ekleme
```bash
dotnet ef migrations add [MigrationName]
dotnet ef database update
```

### Build
```bash
dotnet build
```

### Test
```bash
dotnet test
```

## Güvenlik

- HTTPS zorunlu
- CSRF koruması aktif
- SQL Injection koruması (EF Core)
- XSS koruması (Razor Pages)
- Admin alanları role tabanlı korumalı

## Hosting

Proje Windows tabanlı hosting'de çalışacak şekilde yapılandırılmıştır:
- IIS uyumlu
- SQL Server desteği
- Production ortamı ayarları

## Destek

Proje ile ilgili sorular için:
- Email: admin@egecontrol.com
- Website: www.egecontrol.com

## Lisans

Bu proje Ege Control için özel olarak geliştirilmiştir.
