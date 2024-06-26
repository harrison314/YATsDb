namespace YATsDb.Core.Services;

public sealed class NullCache : ICache
{
    public NullCache()
    {
        
    }

    public T GetOrCreate<T>(string cacheKey, Func<(T, TimeSpan)> creationFunction)
    {
        (T value, _) = creationFunction();
        return value;
    }
}