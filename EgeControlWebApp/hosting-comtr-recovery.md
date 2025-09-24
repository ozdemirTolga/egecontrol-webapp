# Hosting.com.tr Acil Kurtarma Rehberi

## 1. Plesk Kontrol Paneli Erişimi
- URL: https://panel.hosting.com.tr (veya size verilen panel adresi)
- Kullanıcı adı ve şifrenizle giriş yapın

## 2. Veritabanı Kurtarma
### SQLite Kullanıyorsanız:
- Dosya Yöneticisi > httpdocs > app.db dosyasını kontrol edin
- Boyutu 0 KB ise veri kaybolmuş
- Backup Manager'dan eski versiyonu geri yükleyin

### SQL Server Kullanıyorsanız:
- Veritabanları menüsüne gidin
- SQL Server Management Studio ile bağlanın
- Yedek dosyasını (.bak) geri yükleyin

## 3. IIS Ayarları
Hosting.com.tr Windows hosting'de:
```xml
<!-- web.config dosyasında -->
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" arguments="EgeControlWebApp.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" />
  </system.webServer>
</configuration>
```

## 4. Connection String Kontrolü
appsettings.json'da:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  }
}
```

## 5. Acil Geri Yükleme Adımları
1. Backup Manager'dan son yedeği seçin
2. "Geri Yükle" butonuna tıklayın
3. Sadece dosyaları seçin (e-mail hesapları değil)
4. Geri yükleme işlemini başlatın

## 6. FTP ile Manuel Yedek
Eğer panel çalışmıyorsa:
- FTP istemcisi (FileZilla) ile bağlanın
- app.db dosyasını bilgisayarınıza indirin
- Son çalışan versiyonu tekrar yükleyin

## 7. Hosting.com.tr Destek İletişim
- Telefon: 0850 532 0 532
- E-mail: destek@hosting.com.tr
- Canlı Destek: Panel üzerinden
- Ticket: Müşteri panelinden açın

## 8. Acil Durum Kontrol Listesi
- [ ] Plesk paneline erişim sağlandı
- [ ] Veritabanı durumu kontrol edildi
- [ ] Yedek dosyaları listelendi
- [ ] app.db dosyası kontrol edildi
- [ ] Hosting desteği ile iletişime geçildi
- [ ] Geri yükleme işlemi başlatıldı
