using Common.DTO.V2;
using Microsoft.Extensions.Caching.Memory;

namespace WebListener.Utils
{
    public class SimpleMemoryCache
    {
        private MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public GenericMessage GetOrCreate(object key, GenericMessage cacheEntry)
        {
            //GenericMessage cacheEntry;
            if (!_cache.TryGetValue(key, out cacheEntry))// Look for cache key.
            {
                // Key not in cache, so get data.
                //cacheEntry = createItem();

                // Save data in cache.
                _cache.Set(key, cacheEntry);
            }
            return cacheEntry;
        }
    }
}
