using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SSO.Application.Contracts
{
    public static class DistributedCacheContract
    {
        public static string CacheKeyFormat = "{0}-{1}:";

        public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null,
        };

        public static DistributedCacheEntryOptions Cache5M = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        public static DistributedCacheEntryOptions Cache10M = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        public static DistributedCacheEntryOptions Cache30M = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        public static DistributedCacheEntryOptions Cache1H = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }
}
