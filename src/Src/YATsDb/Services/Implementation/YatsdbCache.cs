using Microsoft.Extensions.Caching.Memory;
using YATsDb.Core.Services;

namespace YATsDb.Services.Implementation;

internal sealed class YatsdbCache : ICache
{
    private readonly IMemoryCache memoryCache;

    public YatsdbCache(IMemoryCache memoryCache)
    {
        this.memoryCache = memoryCache;
    }

    public T GetOrCreate<T>(string cacheKey, Func<(T, TimeSpan)> creationFunction)
    {
        T? result = this.memoryCache.GetOrCreate<T>(cacheKey, entry =>
        {
            (T value, TimeSpan lifeTime) = creationFunction();

            entry.SlidingExpiration = lifeTime;
            return value;
        });

        System.Diagnostics.Debug.Assert(result is not null);
        return result;
    }
}
