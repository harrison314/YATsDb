using System.Diagnostics.CodeAnalysis;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree;
using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core.LowLevel;

public class LowLevelIterator : IDisposable
{
    private readonly IZoneTreeIterator<Memory<byte>, Memory<byte>> iterator;
    private readonly IRefComparer<Memory<byte>> comparer;
    private readonly Memory<byte> endKey;
    private readonly int timePosition;

    public LowLevelIterator(IZoneTreeIterator<Memory<byte>, Memory<byte>> iterator,
        IRefComparer<Memory<byte>> comparer,
        Memory<byte> endKey,
        int timePosition)
    {
        this.iterator = iterator;
        this.comparer = comparer;
        this.endKey = endKey;
        this.timePosition = timePosition;
    }

    public bool MoveNext(ReadOnlySpan<int> indexes, out long unixTimestampInMs, Span<double> values)
    {
        if (!this.MoveNextInternal())
        {
            unixTimestampInMs = default;
            return false;
        }

        if (!TsKeyTupleEncoding.TryDecodeTime(this.iterator.CurrentKey.Span, this.timePosition, out unixTimestampInMs))
        {
            throw new InvalidDataException("Can not read time.");
        }

        if (!indexes.IsEmpty)
        {
            TsValueTupleEncoding.DecodeValues(this.iterator.CurrentValue.Span, indexes, values);
        }

        return true;
    }

    public bool MoveNext(ReadOnlySpan<int> indexes, out long unixTimestampInMs, Span<double> values, out ReadOnlyMemory<byte> utf8Tag)
    {
        if (!this.MoveNextInternal())
        {
            unixTimestampInMs = default;
            utf8Tag = Memory<byte>.Empty;
            return false;
        }

        if (!TsKeyTupleEncoding.TryDecodeTime(this.iterator.CurrentKey.Span, this.timePosition, out unixTimestampInMs))
        {
            throw new InvalidDataException("Can not read time.");
        }

        if (!indexes.IsEmpty)
        {
            TsValueTupleEncoding.DecodeValues(this.iterator.CurrentValue.Span, indexes, values);
        }

        utf8Tag = TsValueTupleEncoding.DecodeTagData(this.iterator.CurrentValue);

        return true;
    }

    public bool MoveNext<TAllocator>(ref TAllocator stringAllocator, [NotNullWhen(true)] out LowLevelDataPoint? point)
        where TAllocator : ILowLevelStringAllocator
    {
        if (!this.MoveNextInternal())
        {
            point = null;
            return false;
        }

        if (!TsKeyTupleEncoding.TryDecodeTime(this.iterator.CurrentKey.Span, this.timePosition, out long unixTimestampInMs))
        {
            throw new InvalidDataException("Can not read time.");
        }

        double[] values = TsValueTupleEncoding.DecodeAllValues(this.iterator.CurrentValue.Span);
        ReadOnlyMemory<byte> utf8Tag = TsValueTupleEncoding.DecodeTagData(this.iterator.CurrentValue);
        string? tag = utf8Tag.IsEmpty ? null : stringAllocator.AllocateUtf8(utf8Tag);

        point = new LowLevelDataPoint(unixTimestampInMs,
            values,
            tag);

        return true;
    }

    private bool MoveNextInternal()
    {
        if (!this.iterator.Next())
        {
            return false;
        }

        int result = this.comparer.Compare(this.iterator.CurrentKey, this.endKey);
        return result < 0;
    }

    public Memory<byte> GetCurrentKey()
    {
        return this.iterator.CurrentKey;
    }

    public void Dispose()
    {
        this.iterator.Dispose();
    }
}