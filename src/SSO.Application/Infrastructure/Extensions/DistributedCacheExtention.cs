using Microsoft.Extensions.Caching.Distributed;
using SSO.Application.Contracts;
using System.Text;
using System.Text.Json;

namespace SSO.Application.Extensions
{
    public static class DistributedCacheExtention
    {
        private static readonly string prefix = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
        private static readonly SemaphoreSlim semaphore = new(1, 1);

        private static string GetCacheKey(string key) => String.Format(DistributedCacheContract.CacheKeyFormat, prefix, key);

        public static async Task RemoveCacheAsync(this IDistributedCache cache, string key, CancellationToken token = default)
        {
            await cache.RemoveAsync(GetCacheKey(key), token);
        }

        public static async Task<T> GetOrSetCacheAsync<T>(this IDistributedCache cache, string key, Func<Task<T>> func, DistributedCacheEntryOptions options = default)
        {
            string _key = GetCacheKey(key);
            await semaphore.WaitAsync();
            T output = default;
            try
            {
                var tmp = await cache.GetAsync(_key);
                if (tmp != null)
                {
                    var data = Encoding.UTF8.GetString(tmp);
                    output = JsonSerializer.Deserialize<T>(data);
                }
                else if (func != null)
                {
                    output = await func();
                    if (output != null)
                    {
                        var data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(output, DistributedCacheContract.JsonOptions));
                        await cache.SetAsync(_key, data, options);
                    }
                }
            }
            finally
            {
                semaphore.Release();
            }
            return output;
        }
    }
}
