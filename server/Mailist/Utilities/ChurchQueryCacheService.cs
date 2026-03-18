using ChurchTools;
using ChurchTools.Model;
using Mailist.EmailRelay;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mailist.Utilities;

public class ChurchQueryCacheService
{
    private readonly IChurchToolsApi churchTools;
    private readonly IMemoryCache memoryCache;

    public ChurchQueryCacheService(IChurchToolsApi churchTools, IMemoryCache memoryCache)
    {
        this.churchTools = churchTools;
        this.memoryCache = memoryCache;
    }

    public async ValueTask<int> GetCountAsync(JsonElement query)
    {
        if (query.ValueKind == JsonValueKind.Null)
            return 0;

        return await memoryCache.GetOrCreateAsync(GetCacheKey(query), async cacheEntry =>
        {
            cacheEntry.SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            ChurchQueryRequest<IdNameEmail> request = new(query);
            var result = await churchTools.ChurchQuery(request);
            return result.Count;
        });
    }

    private static string GetCacheKey(JsonElement query)
    {
        byte[] queryBytes = Encoding.UTF8.GetBytes(query.GetRawText());
        byte[] hashBytes = SHA256.HashData(queryBytes);
        return $"ChurchQuery-{Convert.ToHexString(hashBytes)}";
    }
}
