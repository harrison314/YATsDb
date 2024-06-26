using FsCheck.Xunit;
using FsCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenray.ZoneTree.Comparers;
using YATsDb.Core.Tests.FsCheckInfrastructure;
using YATsDb.Core.TupleEncoding;

namespace YATsDb.Core.Tests.TupleEncoding;

public class TsKeyTupleEncodingTests
{
    private readonly IRefComparer<byte[]> comparer = YATsDb.Core.ZoneTreeFactory.RefComparer;

    [Property(StartSize = 200)]
    public Property KeyTupleEncoding_Endcode_BucketId(uint bucketId, uint bucketId2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(bucketId, 123, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(bucketId2, 123, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), 11);

        return (this.comparer.Compare(key1, key2) == bucketId.CompareTo(bucketId2))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Property(StartSize = 200)]
    public Property KeyTupleEncoding_Endcode_MeasurmentId(uint measurmentId1, uint measurmentId2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(12, measurmentId1, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(12, measurmentId2, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), 11);

        return (this.comparer.Compare(key1, key2) == measurmentId1.CompareTo(measurmentId2))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_Endcode_Dates(DateTimeOffset date1, DateTimeOffset date2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(12, 123, date1, 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(12, 123, date2, 11);

        return (this.comparer.Compare(key1, key2) == date1.CompareTo(date2))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Property(StartSize = 200)]
    public Property KeyTupleEncoding_EndcodeWithTag_BucketId(uint bucketId, uint bucketId2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(bucketId, 123, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), "device=device1", 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(bucketId2, 123, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), "device=device1", 11);

        return (this.comparer.Compare(key1, key2) == bucketId.CompareTo(bucketId2))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Property(StartSize = 200)]
    public Property KeyTupleEncoding_EndcodeWithTag_MeasurmentId(uint measurmentId1, uint measurmentId2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(12, measurmentId1, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), "device=device1", 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(12, measurmentId2, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), "device=device1", 11);

        return (this.comparer.Compare(key1, key2) == measurmentId1.CompareTo(measurmentId2))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_EndcodeWithTag_Dates(DateTimeOffset date1, DateTimeOffset date2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(12, 123, date1, "device=device1", 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(12, 123, date2, "device=device1", 11);

        return (this.comparer.Compare(key1, key2) == date1.CompareTo(date2))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_EndcodeWithTag_Tag(NonZeroNotEmptyString tag1, NonZeroNotEmptyString tag2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(12, 457, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), tag1.Value, 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(12, 457, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), tag2.Value, 11);

        return (Math.Sign(this.comparer.Compare(key1, key2)) == Math.Sign(string.CompareOrdinal(tag1.Value, tag2.Value)))
            .ToProperty()
            .Label($"{Convert.ToHexString(key1)} - {Convert.ToHexString(key2)}");
    }

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("World", "ak123")]
    [InlineData("AAAA", "AAAbca")]
    [InlineData("AAAAbca", "AAAA")]
    [InlineData("AAAAbca", "AAAAbca")]
    [InlineData("Kôň z nejakou divnou vecou.", "且女姐")]
    [InlineData("女姐", "且女姐")]
    [InlineData("三体", "三体")]
    [InlineData("žšdr", "zsdr")]
    public void KeyTupleEncoding_EndcodeWithTag_TagExplicit(string tag1, string tag2)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(12, 457, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), tag1, 11);
        byte[] key2 = TsKeyTupleEncoding.Encode(12, 457, new DateTimeOffset(2012, 9, 18, 12, 32, 0, TimeSpan.Zero), tag2, 11);

        Assert.Equal(Math.Sign(string.CompareOrdinal(tag1, tag2)), Math.Sign(this.comparer.Compare(key1, key2)));
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_TryDecode(uint bucketId, uint measureId, DateTimeOffset time)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(bucketId, measureId, time, 11);

        bool result = TsKeyTupleEncoding.TryDecodeTime(key1, out long unixTimestampMs);
        Assert.True(result);

        return time.Equals(DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMs))
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_TryDecode_WithTag(uint bucketId, uint measureId, DateTimeOffset time, NonZeroNotEmptyString tag)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(bucketId, measureId, time, tag.Value.AsSpan(), 11);

        bool result = TsKeyTupleEncoding.TryDecodeTime(key1, out long unixTimestampMs);
        Assert.True(result);

        return time.Equals(DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMs))
            .ToProperty();
    }

    [Fact]
    public void KeyTupleEncoding_TryDecode_WithTag2()
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(0, 0, 1716233899123, "a", 11);

        bool result = TsKeyTupleEncoding.TryDecodeTime(key1, out long unixTimestampMs);
        Assert.True(result);

        Assert.Equal(unixTimestampMs, 1716233899123);
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_TryDecode_WithPosition(DateTimeOffset time)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(11, 13, time, 11);

        int position = TsKeyTupleEncoding.GetTimePosition(ReadOnlySpan<char>.Empty);
        bool result = TsKeyTupleEncoding.TryDecodeTime(key1, position, out long unixTimestampMs);
        Assert.True(result);

        return time.Equals(DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMs))
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_TryDecode_WithPositionAndTag(DateTimeOffset time, NonZeroNotEmptyString tag)
    {
        byte[] key1 = TsKeyTupleEncoding.Encode(11, 13, time, tag.Value.AsSpan(), 11);

        int position = TsKeyTupleEncoding.GetTimePosition(tag.Value.AsSpan());
        bool result = TsKeyTupleEncoding.TryDecodeTime(key1, position, out long unixTimestampMs);
        Assert.True(result);

        return time.Equals(DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMs))
            .ToProperty();
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_Endcode_Order(DateTimeOffset time)
    {
        byte[] keyFrom = TsKeyTupleEncoding.Encode(45, 123, 0, 11);
        byte[] keyTo = TsKeyTupleEncoding.Encode(45, 123, long.MaxValue, 11);

        byte[] key = TsKeyTupleEncoding.Encode(45, 123, time, 11);


        return (this.comparer.Compare(keyFrom, key) <= 0 && this.comparer.Compare(key, keyTo) <= 0)
            .ToProperty()
            .Label($"{Convert.ToHexString(keyFrom)} - {Convert.ToHexString(key)} - {Convert.ToHexString(keyTo)}");
    }

    [Property(StartSize = 200, Arbitrary = new Type[] { typeof(DateTimeOffsetArb) })]
    public Property KeyTupleEncoding_Endcode_OrderWithTag(DateTimeOffset time, NonZeroNotEmptyString tag)
    {
        byte[] keyFrom = TsKeyTupleEncoding.Encode(45, 123, 0, tag.Value, 11);
        byte[] keyTo = TsKeyTupleEncoding.Encode(45, 123, long.MaxValue, tag.Value, 11);

        byte[] key = TsKeyTupleEncoding.Encode(45, 123, time, tag.Value, 11);

        byte[] keyOtherKey = TsKeyTupleEncoding.Encode(45, 123, time, 11);
        byte[] keyOtherTag = TsKeyTupleEncoding.Encode(45, 123, time, tag.Value + "/foo", 11);

        if (!this.IsInRange(key, keyFrom, keyTo))
        {
            return false.ToProperty();
        }

        if (this.IsInRange(keyOtherKey, keyFrom, keyTo))
        {
            return false.ToProperty();
        }

        if (this.IsInRange(keyOtherTag, keyFrom, keyTo))
        {
            return false.ToProperty();
        }

        return true.ToProperty();
    }

    private bool IsInRange(byte[] value, byte[] from, byte[] to)
    {
        return (this.comparer.Compare(from, value) <= 0 && this.comparer.Compare(value, to) <= 0);
    }
}
