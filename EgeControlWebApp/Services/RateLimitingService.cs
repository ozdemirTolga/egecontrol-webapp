using System.Collections.Concurrent;

namespace EgeControlWebApp.Services
{
    public class RateLimitingService
    {
        private readonly ConcurrentDictionary<string, List<DateTime>> _requests = new();
        private readonly int _maxRequests;
        private readonly TimeSpan _timeWindow;

        public RateLimitingService()
        {
            _maxRequests = 5;               // Aynı IP'den max 5 istek
            _timeWindow = TimeSpan.FromMinutes(10);  // 10 dakika içinde
        }

        public bool IsRateLimited(string? clientIp)
        {
            if (string.IsNullOrEmpty(clientIp))
                return false;

            var now = DateTime.UtcNow;
            var timestamps = _requests.GetOrAdd(clientIp, _ => new List<DateTime>());

            lock (timestamps)
            {
                // Eski kayıtları temizle
                timestamps.RemoveAll(t => now - t > _timeWindow);
                
                if (timestamps.Count >= _maxRequests)
                    return true;

                timestamps.Add(now);
                return false;
            }
        }

        /// <summary>
        /// Eski kayıtları periyodik temizle (bellek sızıntısını önler)
        /// </summary>
        public void Cleanup()
        {
            var now = DateTime.UtcNow;
            var keysToRemove = _requests
                .Where(kvp =>
                {
                    lock (kvp.Value) { return kvp.Value.All(t => now - t > _timeWindow); }
                })
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
                _requests.TryRemove(key, out _);
        }
    }
}
