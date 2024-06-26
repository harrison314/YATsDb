using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.Utils;

internal class ReadOnlyMemoryEqualityComparer : IEqualityComparer<ReadOnlyMemory<byte>>
{
    public ReadOnlyMemoryEqualityComparer()
    {

    }

    public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
    {
        if (x.Length != y.Length)
        {
            return false;
        }

        return x.Span.SequenceEqual(y.Span);
    }

    public int GetHashCode([DisallowNull] ReadOnlyMemory<byte> obj)
    {
        HashCode hc = new HashCode();
        hc.AddBytes(obj.Span);
        return hc.ToHashCode();
    }
}
