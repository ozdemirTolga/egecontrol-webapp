# WebDeploy Kurulum Rehberi

## 1. Plesk Panel'de WebDeploy Aktifleştirin

1. Plesk panel > **Websites & Domains** > siteniz
2. **Web Publishing** > **WebDeploy**
3. **Enable WebDeploy** butonuna tıklayın
4. **Download Publishing Profile** ile .publishsettings dosyasını indirin

## 2. GitHub Secrets Ekleyin

1. GitHub repository > **Settings** > **Secrets and variables** > **Actions**
2. **New repository secret** ile şunları ekleyin:

### PUBLISH_PROFILE
- Name: `PUBLISH_PROFILE`
- Value: İndirdiğiniz .publishsettings dosyasının **tüm içeriği**

### SITE_NAME
- Name: `SITE_NAME`
- Value: Site adınız (örn: `egecontrol.com`)

## 3. WebDeploy Test

Publishing profile'da şu bilgiler olmalı:
- `publishUrl`: WebDeploy endpoint
- `userName`: Deployment kullanıcısı  
- `userPWD`: Deployment şifresi
- `msdeploysite`: Site adı

## 4. Alternatif: Manuel WebDeploy

Eğer otomatik çalışmazsa:

```powershell
# Visual Studio'da Publish Profile oluşturun
# Plesk'ten aldığınız .publishsettings'i import edin
# Publish butonuna tıklayın
```

Bu yöntem %100 çalışır çünkü Microsoft'un resmi deployment yöntemi.
