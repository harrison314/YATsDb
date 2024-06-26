using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenray.ZoneTree.Comparers;
using Tenray.ZoneTree.Serializers;
using Tenray.ZoneTree;
using Xunit.Abstractions;
using YATsDb.Core.LowLevel;
using YATsDb.Core.TupleEncoding;
using YATsDb.Core.Utils;

namespace YATsDb.Core.Tests.LowLevel;

public sealed class YatsdbLowLevelStorageTests : IDisposable
{
    private string directory;
    private readonly ITestOutputHelper output;

    public YatsdbLowLevelStorageTests(ITestOutputHelper output)
    {
        this.directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.directory);
        this.output = output;
    }

    public void Dispose()
    {
        Directory.Delete(this.directory, true);
    }

    [Fact]
    public void YatsdbLowLevelStorage_InsertDataPoint_SimpleValue()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);

        double[] values = new double[] { 14.3 };
        storage.InsertDataPoint(12, 45, 1716143258000, values, ReadOnlySpan<char>.Empty);
    }

    [Fact]
    public void YatsdbLowLevelStorage_InsertDataPoint_SimpleValueWithTag()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);

        double[] values = new double[] { 14.3 };
        storage.InsertDataPoint(12, 45, 1716143258000, values, "home/temperature/45");
    }

    [Fact]
    public void YatsdbLowLevelStorage_InsertDataPoint_MultipleValue()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);

        double[] values = new double[] { 14.3, 24.0, 32.0, 0.1145, -54.0, -8.5 };
        storage.InsertDataPoint(12, 45, 1716143258000, values, ReadOnlySpan<char>.Empty);
    }

    [Fact]
    public void YatsdbLowLevelStorage_InsertDataPoint_MultipleValueWithTag()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);

        double[] values = new double[] { 14.3, 24.0, 32.0, 0.1145, -54.0, -8.5 };
        storage.InsertDataPoint(12, 45, 1716143258000, values, "home/temperature/45");
    }

    [Fact]
    public void YatsdbLowLevelStorage_Iterate()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        using LowLevelIterator iterator = storage.QueryData(1,
            1,
            null,
            null,
            ReadOnlySpan<char>.Empty);

        bool result;

        NullLowLevelStringAllocator nlsa = new NullLowLevelStringAllocator();
        result = iterator.MoveNext(ref nlsa, out LowLevelDataPoint? point0);
        Assert.True(result);
        Assert.Equal(1010L, point0!.UnixTimestampInMs);
        Assert.Equal("first", point0!.Tag);
        Assert.Equal(10.0, point0!.Values[0]);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out LowLevelDataPoint? point2);
        Assert.True(result);

        Assert.Equal(1030L, point2!.UnixTimestampInMs);
        Assert.Null(point2!.Tag);
        Assert.Equal(30.0, point2!.Values[0]);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.True(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.False(result);

        result = iterator.MoveNext(ref nlsa, out _);
        Assert.False(result);
    }

    [Fact]
    public void YatsdbLowLevelStorage_IterateWithTag()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg =>
        {
            cfg.SetDataDirectory(this.directory);
        });

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        using LowLevelIterator iterator = storage.QueryData(1,
            1,
            null,
            null,
            "even");

        int count = 0;
        NullLowLevelStringAllocator nlsa = new NullLowLevelStringAllocator();
        while (iterator.MoveNext(ref nlsa, out LowLevelDataPoint? point))
        {
            Assert.Equal("even", point.Tag);
            count++;
        }

        Assert.Equal(4, count);
    }


    [Theory]
    [InlineData("", -1L, -1L, 9)]
    [InlineData("", 1000L, -1L, 9)]
    [InlineData("", -1L, 1051, 5)]
    [InlineData("first", -1L, 1050, 1)]
    [InlineData("even", -1L, 1050, 2)]
    public void YatsdbLowLevelStorage_IterateTimestamps(string? tag, long from, long to, int count)
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg =>
        {
            cfg.SetDataDirectory(this.directory);
        });

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        using LowLevelIterator iterator = storage.QueryData(1,
            1,
            (from == -1) ? null : from,
             (to == -1) ? null : to,
            tag);

        int countAct = 0;
        Span<int> indexes = stackalloc int[] { 0, 1 };
        Span<double> values = stackalloc double[2];
        while (iterator.MoveNext(indexes, out long timestamp, values))
        {
            countAct++;
        }

        Assert.Equal(count, countAct);
    }

    [Fact]
    public void YatsdbLowLevelStorage_IterateWithConditions()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        using LowLevelIterator iterator = storage.QueryData(1,
            1,
            1035,
            1080,
            null);

        int count = 0;
        NullLowLevelStringAllocator nlsa = new NullLowLevelStringAllocator();
        while (iterator.MoveNext(ref nlsa, out LowLevelDataPoint? point))
        {
            Assert.NotInRange(point.UnixTimestampInMs, 35, 80);
            count++;
        }

        Assert.NotEqual(0, count);
    }

    [Fact]
    public void YatsdbLowLevelStorage_IterateWithConditionsAndTag()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        using LowLevelIterator iterator = storage.QueryData(1,
            1,
            1035,
            1080,
            "even");

        int count = 0;
        NullLowLevelStringAllocator nlsa = new NullLowLevelStringAllocator();
        while (iterator.MoveNext(ref nlsa, out LowLevelDataPoint? point))
        {
            Assert.Equal("even", point.Tag);
            Assert.NotInRange(point.UnixTimestampInMs, 35, 80);
            count++;
        }

        Assert.NotEqual(0, count);
    }


    [Fact]
    public void Debug()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        using IZoneTreeIterator<byte[], byte[]> iterator = db.CreateIterator();
        iterator.SeekFirst();

        while (iterator.Next())
        {
            string val = TsKeyTupleEncoding.ToString(iterator.CurrentKey);
            this.output.WriteLine("> {0}", val);
        }
    }

    [Fact]
    public void YatsdbLowLevelStorage_BucketApi()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        List<string> buckets;
        bool rv;

        buckets = storage.ReadBuckets();
        Assert.Empty(buckets);


        rv = storage.TryCreateBucket(new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = 4589,
            Description = null,
            Name = "test1"
        }, out uint bucketId1);

        Assert.True(rv);

        rv = storage.TryCreateBucket(new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = 4589,
            Description = null,
            Name = "test2"
        }, out uint bucketId2);

        Assert.True(rv);
        Assert.NotEqual(bucketId1, bucketId2);


        rv = storage.TryCreateBucket(new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = 145899,
            Description = "Nekjaky popis predtym",
            Name = "moj_bucket"
        }, out uint bucketId3);


        rv = storage.TryCreateBucket(new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = 4589,
            Description = "Nekjaky popis",
            Name = "moj_bucket"
        }, out uint bucketId4);

        Assert.False(rv);


        //this.DebugFlush(db);

        buckets = storage.ReadBuckets();
        Assert.Equal(3, buckets.Count);
        Assert.Contains("test1", buckets);
        Assert.Contains("test2", buckets);
        Assert.Contains("moj_bucket", buckets);
    }

    [Fact]
    public void YatsdbLowLevelStorage_RemoveBucket()
    {
        using IZoneTree<byte[], byte[]> db = ZoneTreeFactory.Build(cfg => cfg.SetDataDirectory(this.directory));

        YatsdbLowLevelStorage storage = new YatsdbLowLevelStorage(db);
        this.InsertTestData(storage);

        List<string> buckets;
        bool rv;

        buckets = storage.ReadBuckets();
        Assert.Empty(buckets);


        rv = storage.TryCreateBucket(new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = 4589,
            Description = null,
            Name = "test1"
        }, out uint bucketId1);


        rv = storage.TryCreateBucket(new BucketCreationData()
        {
            CreateTimeUnixTimestampInMs = 4589,
            Description = null,
            Name = "test2"
        }, out uint bucketId2);


        rv = storage.TryRemoveBucket("test3", out uint val3);
        Assert.False(rv);

        rv = storage.TryRemoveBucket("test1", out uint val1);
        Assert.True(rv);
        Assert.Equal(bucketId1, val1);


        buckets = storage.ReadBuckets();
        Assert.Single(buckets);
        Assert.Contains("test2", buckets);
    }

    private void DebugFlush(IZoneTree<byte[], byte[]> db)
    {
        using IZoneTreeIterator<byte[], byte[]> iterator = db.CreateIterator();
        iterator.SeekFirst();

        while (iterator.Next())
        {
            this.output.WriteLine("> {0}\t = {1}",
                Convert.ToHexString(iterator.CurrentKey),
                Convert.ToHexString(iterator.CurrentValue));
        }
    }

    private void InsertTestData(YatsdbLowLevelStorage storage)
    {
        storage.InsertDataPoint(404, 404, 1007, new double[] { 17 }, "");
        storage.InsertDataPoint(404, 404, 1017, new double[] { 27 }, "");

        storage.InsertDataPoint(1, 1, 1010, new double[] { 10 }, "first");
        storage.InsertDataPoint(1, 1, 1020, new double[] { 20 }, "even");
        storage.InsertDataPoint(1, 1, 1030, new double[] { 30 }, "");
        storage.InsertDataPoint(1, 1, 1040, new double[] { 40 }, "even");
        storage.InsertDataPoint(1, 1, 1050, new double[] { 50 }, "");
        storage.InsertDataPoint(1, 1, 1060, new double[] { 60 }, "even");
        storage.InsertDataPoint(1, 1, 1070, new double[] { 70 }, "");
        storage.InsertDataPoint(1, 1, 1080, new double[] { 80 }, "even");
        storage.InsertDataPoint(1, 1, 1090, new double[] { 90 }, "");
    }

    [Fact]
    public void TestData()
    {
        using IZoneTree<string, string> zoneTree = new ZoneTreeFactory<string, string>()
    .SetComparer(new StringOrdinalComparerAscending())
    .SetKeySerializer(new Utf8StringSerializer())
    .SetValueSerializer(new Utf8StringSerializer())
    .SetIsValueDeletedDelegate((in string x) => string.IsNullOrEmpty(x))
  .SetMarkValueDeletedDelegate((ref string x) => x = string.Empty)
    .SetDataDirectory(this.directory)
    .OpenOrCreate();

        zoneTree.Upsert("D:01:01:0010", "cas 10");
        zoneTree.Upsert("D:01:01:0020", "cas 20");
        zoneTree.Upsert("D:01:01:0030", "cas 30");
        zoneTree.Upsert("D:01:01:0040", "cas 40");
        zoneTree.Upsert("D:01:01:0050", "cas 50");

        zoneTree.Upsert("I:01:01#even:0020", "cas 20");
        zoneTree.Upsert("I:01:01#even:0040", "cas 40");
        zoneTree.Upsert("I:01:01#first:0010", "cas 10");



        using IZoneTreeIterator<string, string> iterator = zoneTree.CreateIterator();
        iterator.SeekFirst();

        iterator.Seek("I:01:01#even:0030");

        //iterator.Next() complexity is O(1)
        while (iterator.Next())
        {
            string key = iterator.CurrentKey;
            string value = iterator.CurrentValue;

            this.output.WriteLine("{0} - {1}", key, value);
        }
    }

    [Fact]
    public void TestData2()
    {
        using IZoneTree<string, string> zoneTree = new ZoneTreeFactory<string, string>()
    .SetComparer(new StringOrdinalComparerAscending())
    .SetKeySerializer(new Utf8StringSerializer())
    .SetValueSerializer(new Utf8StringSerializer())
    .SetIsValueDeletedDelegate((in string x) => string.IsNullOrEmpty(x))
  .SetMarkValueDeletedDelegate((ref string x) => x = string.Empty)
    .SetDataDirectory(this.directory)
    .OpenOrCreate();

        void InsertDataPoint2(uint bucketId,
        uint measurmentId,
        long unixTimestampInMsTime,
        ReadOnlySpan<double> values,
        ReadOnlySpan<char> tag)
        {
            uint randomPart = System.Random.Shared.NextUint32();
            if (tag.IsEmpty)
            {
                byte[] primaryKey = TsKeyTupleEncoding.Encode(bucketId, measurmentId, unixTimestampInMsTime, randomPart);
                byte[] value = TsValueTupleEncoding.Encode(values, tag);

                zoneTree.Upsert(Convert.ToHexString(primaryKey), Convert.ToHexString(value));
            }
            else
            {
                byte[] primaryKey = TsKeyTupleEncoding.Encode(bucketId, measurmentId, unixTimestampInMsTime, randomPart);
                byte[] tagIndex = TsKeyTupleEncoding.Encode(bucketId, measurmentId, unixTimestampInMsTime, tag, randomPart);
                byte[] value = TsValueTupleEncoding.Encode(values, tag);

                zoneTree.Upsert(Convert.ToHexString(primaryKey), Convert.ToHexString(value));
                zoneTree.Upsert(Convert.ToHexString(tagIndex), Convert.ToHexString(value));
            }
        }


        InsertDataPoint2(404, 404, 1007, new double[] { 17 }, "");
        InsertDataPoint2(404, 404, 1017, new double[] { 27 }, "");

        InsertDataPoint2(1, 1, 1010, new double[] { 10 }, "first");
        InsertDataPoint2(1, 1, 1020, new double[] { 20 }, "even");
        InsertDataPoint2(1, 1, 1030, new double[] { 30 }, "");
        InsertDataPoint2(1, 1, 1040, new double[] { 40 }, "even");
        InsertDataPoint2(1, 1, 1050, new double[] { 50 }, "");
        InsertDataPoint2(1, 1, 1060, new double[] { 60 }, "even");
        InsertDataPoint2(1, 1, 1070, new double[] { 70 }, "");
        InsertDataPoint2(1, 1, 1080, new double[] { 80 }, "even");
        InsertDataPoint2(1, 1, 1090, new double[] { 90 }, "");

        InsertDataPoint2(1, 1, 1101, new double[] { 90 }, "zend");
        InsertDataPoint2(1, 1, 1102, new double[] { 90 }, "zend");
        InsertDataPoint2(1, 1, 1103, new double[] { 90 }, "zend");

        using IZoneTreeIterator<string, string> iterator = zoneTree.CreateIterator();
        // iterator.SeekFirst();

        string lk = Convert.ToHexString(TsKeyTupleEncoding.Encode(1, 1, 1030, "even", 45));
        string mk = Convert.ToHexString(TsKeyTupleEncoding.Encode(1, 1, 1070, "even", 45));
        this.output.WriteLine("Seek: {0}", lk);
        this.output.WriteLine("Max : {0}", mk);

        iterator.Seek(lk);

        //iterator.Next() complexity is O(1)
        while (iterator.Next())
        {
            string key = iterator.CurrentKey;
            string value = iterator.CurrentValue;

            if (new StringOrdinalComparerAscending().Compare(in key, in mk) > 0)
            {
                break;
            }

            this.output.WriteLine("{0} - {1}", key, TsKeyTupleEncoding.ToString(Convert.FromHexString(iterator.CurrentKey)));
        }
    }
}
