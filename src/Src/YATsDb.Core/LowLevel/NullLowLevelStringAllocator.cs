namespace YATsDb.Core.LowLevel;

internal struct NullLowLevelStringAllocator : ILowLevelStringAllocator
{

    public NullLowLevelStringAllocator()
    {

    }

    public string AllocateUtf8(ReadOnlyMemory<byte> utf8Bytes)
    {
        return System.Text.Encoding.UTF8.GetString(utf8Bytes.Span);
    }
}
