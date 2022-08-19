using Common.DTO.V2;
using Microsoft.Extensions.Caching.Memory;

namespace WebListener.Utils
{
    public class SimpleMemoryCache
    {
        // private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        public static MemoryCache? Cache = null;
        public GenericMessage GetOrCreate(object key, GenericMessage pCacheEntry)
        {
            if (Cache == null)
            {
                Console.WriteLine("Creating cache");
                Cache = new MemoryCache(
            new MemoryCacheOptions
            {
                SizeLimit = 12024
            });
            }
            Console.WriteLine("Count:" + Cache.Count);
            GenericMessage cacheEntry;
            if (!Cache.TryGetValue(key, out cacheEntry))// Look for cache key.
            {

                // Save data in cache.
                if (pCacheEntry != null)
                    Cache.Set(key, pCacheEntry);
            }
            return cacheEntry;
        }
    }
}
