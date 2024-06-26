namespace YATsDb.Core.LowLevel;

public interface ILowLevelStringAllocator
{
    string AllocateUtf8(ReadOnlyMemory<byte> utf8Bytes);
}
