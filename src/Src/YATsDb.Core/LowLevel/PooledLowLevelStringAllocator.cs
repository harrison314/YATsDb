using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YATsDb.Core.Utils;

namespace YATsDb.Core.LowLevel;

internal struct PooledLowLevelStringAllocator : ILowLevelStringAllocator
{
    private Dictionary<ReadOnlyMemory<byte>, string> pool;

    public PooledLowLevelStringAllocator()
    {
        this.pool = new Dictionary<ReadOnlyMemory<byte>, string>(new ReadOnlyMemoryEqualityComparer());
    }

    public string AllocateUtf8(ReadOnlyMemory<byte> utf8Bytes)
    {
        if (this.pool.TryGetValue(utf8Bytes, out string? result))
        {
            return result;
        }
        else
        {
            result = System.Text.Encoding.UTF8.GetString(utf8Bytes.Span);
            this.pool.Add(utf8Bytes, result);
            return result;
        }
    }
}
