# EgeControl Web Uygulaması

Modern teklif yönetim sistemi - ASP.NET Core ile geliştirilmiş

## 📋 Özellikler

### 🔐 Kullanıcı Yönetimi
- **6 Farklı Rol Sistemi:**
  - **Admin:** Tam yetki - tüm işlemler
  - **Manager:** Yönetici yetkisi - tüm teklifler üzerinde yetki
  - **QuoteCreator:** Teklif oluşturma ve kendi tekliflerini düzenleme
  - **QuoteEditor:** Tüm teklifleri düzenleme yetkisi
  - **QuoteSender:** Teklif gönderme yetkisi
  - **Viewer:** Sadece görüntüleme yetkisi

### 📊 Teklif Yönetimi
- ✅ **Teklif Oluşturma ve Düzenleme**
- ✅ **Çoklu Para Birimi Desteği** (TRY, EUR, USD)
- ✅ **Otomatik Teklif Numarası** oluşturma
- ✅ **KDV Hesaplaması** ve özelleştirilebilir oran
- ✅ **İndirim Yönetimi** (yüzde bazlı)
- ✅ **PDF Export** özelliği
- ✅ **E-posta Gönderimi** teklif müşteriye

### 👥 Kullanıcı Takibi
- ✅ **Teklifi Kim Oluşturdu** - tam kullanıcı bilgisi
- ✅ **Son Değiştiren Kim** - değişiklik tarihi ile birlikte
- ✅ **Ownership-based Editing** - QuoteCreator sadece kendi tekliflerini düzenleyebilir
- ✅ **Departman ve Pozisyon** takibi

### 👤 Müşteri Yönetimi
- ✅ **Müşteri Bilgileri** (şirket, iletişim, adres)
- ✅ **Vergi Bilgileri** (vergi no, vergi dairesi)
- ✅ **Aktif/Pasif** durum yönetimi

## 🛠️ Teknolojiler

- **Backend:** ASP.NET Core 9.0
- **Database:** SQLite (Entity Framework Core)
- **Authentication:** ASP.NET Core Identity
- **Frontend:** Razor Pages + Bootstrap 5
- **PDF:** Custom PDF generation
- **Email:** SMTP Email Service
- **Icons:** Font Awesome

## 📦 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Visual Studio Code / Visual Studio

### Adımlar
1. **Repository'yi klonlayın:**
   ```bash
   git clone https://github.com/[your-username]/egecontrol-webapp.git
   cd egecontrol-webapp
   ```

2. **Bağımlılıkları yükleyin:**
   ```bash
   cd EgeControlWebApp
   dotnet restore
   ```

3. **Veritabanını oluşturun:**
   ```bash
   dotnet ef database update
   ```

4. **Uygulamayı çalıştırın:**
   ```bash
   dotnet run
   ```

5. **Tarayıcıda açın:**
   ```
   https://localhost:5238
   ```

## 👨‍💻 Varsayılan Kullanıcı

İlk kurulumda sistem otomatik olarak bir admin kullanıcısı oluşturur:
- **Email:** admin@egecontrol.com
- **Password:** Admin123!

## 🏗️ Proje Yapısı

```
EgeControlWebApp/
├── Areas/
│   ├── Admin/           # Admin paneli sayfaları
│   │   ├── Pages/
│   │   │   ├── Quotes/  # Teklif yönetimi
│   │   │   ├── Customers/ # Müşteri yönetimi
│   │   │   └── Users/   # Kullanıcı yönetimi
│   └── Identity/        # Login/Register sayfaları
├── Data/                # Entity Framework DbContext
├── Models/              # Veri modelleri
├── Services/            # Business logic katmanı
├── Migrations/          # Veritabanı migration'ları
└── wwwroot/            # Static dosyalar (CSS, JS, images)
```

## 🔧 Konfigürasyon

### Email Ayarları
`appsettings.json` dosyasında SMTP ayarlarını yapılandırın:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-password",
    "EnableSSL": true
  }
}
```

### Para Birimleri
Desteklenen para birimleri:
- `TRY` - Türk Lirası (₺)
- `EUR` - Euro (€)
- `USD` - Amerikan Doları ($)

## 🔐 Güvenlik

- ✅ **Role-based Authorization** - Sayfa seviyesinde yetki kontrolü
- ✅ **Ownership-based Access Control** - Kullanıcılar sadece kendi kayıtlarını düzenleyebilir
- ✅ **Claims-based Authentication** - Modern kimlik doğrulama
- ✅ **CSRF Protection** - Cross-site request forgery koruması

## 📈 Özellik Roadmap

- [ ] **Dashboard ve Raporlar**
- [ ] **Teklif Şablonları**
- [ ] **Bulk Operations**
- [ ] **Advanced Search & Filtering**
- [ ] **Mobile Responsive Design**
- [ ] **API Endpoints**

## 🤝 Katkıda Bulunma

1. Fork edin
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit edin (`git commit -m 'Add some amazing feature'`)
4. Push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje MIT lisansı ile lisanslanmıştır.

## 📞 İletişim

Tolga - [@your-handle] - email@example.com

Proje Linki: [https://github.com/[your-username]/egecontrol-webapp](https://github.com/[your-username]/egecontrol-webapp)

---

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!
