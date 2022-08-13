using Microsoft.Extensions.Caching.Memory;

namespace LanesBackend.Services
{
    public class InMemoryCacheService : ICacheService
    {
        public IMemoryCache MemoryCache { get; set; }

        public InMemoryCacheService(IMemoryCache memoryCache)
        {
            MemoryCache = memoryCache;
        }

        public async Task<object?> GetValue(string key)
        {
            var value = MemoryCache.Get(key);
            return value;
        }

        public async Task SetValue(string key, string value)
        {
            MemoryCache.Set(key, value);
        }
    }
}
