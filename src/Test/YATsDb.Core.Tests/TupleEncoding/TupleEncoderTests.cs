using FsCheck.Xunit;
using FsCheck;
using YATsDb.Core.Tests.FsCheckInfrastructure;
using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core.Tests.TupleEncoding;

public class TupleEncoderTests
{
    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1uint(uint value1)
    {
        byte[] value = TupleEncoder.Create(4, value1);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out uint rv1);
        Assert.True(result);

        return (value1 == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1long(long value1)
    {

        byte[] value = TupleEncoder.Create(4, value1);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out long rv1);
        Assert.True(result);

        return (value1 == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1int(int value1)
    {
        byte[] value = TupleEncoder.Create(4, value1);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out int rv1);
        Assert.True(result);

        return (value1 == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1Guid(Guid value1)
    {
        byte[] value = TupleEncoder.Create(4, value1);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out Guid rv1);
        Assert.True(result);

        return (value1 == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { })]
    public Property TupleEncoder_EncodeDecode1longCmp(long value1, long value2)
    {
        byte[] key1 = TupleEncoder.Create(4, value1);
        byte[] key2 = TupleEncoder.Create(4, value2);

        return this.CmpEq(key1, key2, value1, value2)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1bool(bool value1)
    {
        byte[] value = TupleEncoder.Create(4, value1);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out bool rv1);
        Assert.True(result);

        return (value1 == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1byte(byte value1)
    {
        byte[] value = TupleEncoder.Create(4, value1);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out byte rv1);
        Assert.True(result);

        return (value1 == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode1string(NonZeroNotEmptyString value1)
    {
        byte[] value = TupleEncoder.Create(4, value1.Value);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out string rv1);
        Assert.True(result);

        return (value1.Value == rv1)
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property TupleEncoder_EncodeDecode4(uint value1, NonZeroNotEmptyString value2, long value3, bool value4)
    {
        byte[] value = TupleEncoder.Create<uint, string, long, bool>((byte)4, value1, value2.Value, value3, value4);
        bool result = TupleEncoder.TryDeconstruct(value, 4, out uint r1, out string r2, out long r3, out bool r4);
        Assert.True(result);

        return (value1 == r1
            && value2.Value == r2
            && value3 == r3
            && value4 == r4)
            .ToProperty();
    }

    private bool CmpEq<T>(byte[] key1, byte[] key2, T v1, T v2)
        where T : IComparable<T>
    {
        int diff = YATsDb.Core.ZoneTreeFactory.RefComparer.Compare(key1, key2);
        int diffValue = v1.CompareTo(v2);

        return Math.Sign(diff) == Math.Sign(diffValue);
    }
}
