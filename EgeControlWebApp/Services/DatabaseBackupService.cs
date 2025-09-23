using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EgeControlWebApp.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EgeControlWebApp.Services
{
    public class DatabaseBackupService : BackgroundService
    {
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromHours(6); // 6 saatte bir yedek

        public DatabaseBackupService(ILogger<DatabaseBackupService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await BackupDatabase();
                    await Task.Delay(_period, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Veritabanı yedekleme hatası");
                    await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Hata durumunda 30 dk bekle
                }
            }
        }

        private async Task BackupDatabase()
        {
            try
            {
                // SQLite için
                var dbPath = "app.db";
                if (File.Exists(dbPath))
                {
                    await BackupSqliteDatabase(dbPath);
                }
                
                // SQL Server için
                await BackupSqlServerDatabase();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Veritabanı yedekleme işlemi başarısız");
            }
        }

        private Task BackupSqliteDatabase(string dbPath)
        {
            var backupDir = "backups";
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"app_backup_{timestamp}.db");

            File.Copy(dbPath, backupPath, true);
            _logger.LogInformation("SQLite yedeği oluşturuldu: {BackupPath}", backupPath);
            
            return Task.CompletedTask;
        }

        private async Task BackupSqlServerDatabase()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            if (context.Database.IsSqlServer())
            {
                var backupDir = "backups";
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupPath = Path.Combine(backupDir, $"sqlserver_backup_{timestamp}.bak");
                
                // SQL Server backup komutu
                var sql = $"BACKUP DATABASE [egecontr1_] TO DISK = '{Path.GetFullPath(backupPath)}'";
                
                try
                {
                    await context.Database.ExecuteSqlRawAsync(sql);
                    _logger.LogInformation("SQL Server yedeği oluşturuldu: {BackupPath}", backupPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SQL Server yedeği oluşturulamadı, sunucu yetkisi gerekebilir");
                }
            }
        }

        private Task CleanOldBackups(string backupDir)
        {
            try
            {
                var files = Directory.GetFiles(backupDir, "app_backup_*.db");
                var cutoffDate = DateTime.Now.AddDays(-7);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation("Eski yedek dosyası silindi: {FileName}", file);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eski yedek dosyaları temizlenirken hata");
            }
            
            return Task.CompletedTask;
        }
    }
}
