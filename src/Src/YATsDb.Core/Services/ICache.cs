using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel;

namespace YATsDb.Core.Services;

public interface ICache
{
    T GetOrCreate<T>(string cacheKey, Func<(T, TimeSpan)> creationFunction);
}
