# MSDeploy Secrets Setup

GitHub repository > Settings > Secrets and variables > Actions > New repository secret

## Secrets Ekleyin:

### DEPLOY_URL
- Name: `DEPLOY_URL`
- Value: `https://siteniz.hosting.com.tr:8172/msdeploy.axd?site=siteniz`

### DEPLOY_USERNAME  
- Name: `DEPLOY_USERNAME`
- Value: Plesk'ten aldığınız WebDeploy kullanıcı adı

### DEPLOY_PASSWORD
- Name: `DEPLOY_PASSWORD` 
- Value: Plesk'ten aldığınız WebDeploy şifresi

### SITE_NAME
- Name: `SITE_NAME`
- Value: Site adınız (örn: `egecontrol.com`)

## Plesk'te WebDeploy Bilgilerini Alma:

1. Plesk Panel > Websites & Domains > siteniz
2. **Web Publishing** > **WebDeploy**  
3. **Enable WebDeploy** aktifleştirin
4. Şu bilgileri kopyalayın:
   - Server URL
   - Username
   - Password
   - Site name

Bu bilgileri GitHub secrets'a ekleyin.
