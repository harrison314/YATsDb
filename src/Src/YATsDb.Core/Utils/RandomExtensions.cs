using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace YATsDb.Core.Utils;

internal static class RandomExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint NextUint32(this Random random)
    {
        Span<byte> buffer = stackalloc byte[sizeof(uint)];
        random.NextBytes(buffer);

        return MemoryMarshal.Cast<byte, uint>(buffer)[0];
    }
}