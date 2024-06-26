using System.Buffers.Binary;
using System.Text;
using Tenray.ZoneTree;
using YATsDb.Core.TupleEncoding;
using YATsDb.Core.Utils;

namespace YATsDb.Core.LowLevel;

public class YatsdbLowLevelStorage : IYatsdbLowLevelStorage
{
    private readonly IZoneTree<byte[], byte[]> zoneTree;

    public IZoneTree<byte[], byte[]> Db
    {
        get => this.zoneTree;
    }

    public YatsdbLowLevelStorage(IZoneTree<byte[], byte[]> zoneTree)
    {
        this.zoneTree = zoneTree;
    }

    public void InsertDataPoint(uint bucketId,
        uint measurementId,
        long unixTimestampInMsTime,
        ReadOnlySpan<double> values,
        ReadOnlySpan<char> tag)
    {
        uint randomPart = Random.Shared.NextUint32();

        if (tag.IsEmpty)
        {
            byte[] primaryKey = TsKeyTupleEncoding.Encode(bucketId, measurementId, unixTimestampInMsTime, randomPart);
            byte[] value = TsValueTupleEncoding.Encode(values, tag);

            this.zoneTree.Upsert(primaryKey, value);
        }
        else
        {
            byte[] primaryKey = TsKeyTupleEncoding.Encode(bucketId, measurementId, unixTimestampInMsTime, randomPart);
            byte[] tagIndex = TsKeyTupleEncoding.Encode(bucketId, measurementId, unixTimestampInMsTime, tag, randomPart);
            byte[] value = TsValueTupleEncoding.Encode(values, tag);

            this.zoneTree.Upsert(primaryKey, value);
            this.zoneTree.Upsert(tagIndex, value);
        }
    }

    public LowLevelIterator QueryData(uint bucketId,
        uint measurementId,
        long? fromUnixTimestampInMs,
        long? toUnixTimestampInMs,
        ReadOnlySpan<char> tag)
    {
        byte[] from = tag.IsEmpty
            ? TsKeyTupleEncoding.Encode(bucketId, measurementId, fromUnixTimestampInMs ?? 0L, uint.MinValue)
            : TsKeyTupleEncoding.Encode(bucketId, measurementId, fromUnixTimestampInMs ?? 0L, tag, uint.MinValue);

        byte[] to = tag.IsEmpty
            ? TsKeyTupleEncoding.Encode(bucketId, measurementId, toUnixTimestampInMs ?? long.MaxValue, uint.MaxValue)
            : TsKeyTupleEncoding.Encode(bucketId, measurementId, toUnixTimestampInMs ?? long.MaxValue, tag, uint.MaxValue);

        IZoneTreeIterator<byte[], byte[]> iterator = this.zoneTree.CreateIterator(IteratorType.AutoRefresh, includeDeletedRecords: false);

        iterator.Seek(in from);

        return new LowLevelIterator(iterator,
            this.zoneTree.Comparer,
            to,
            TsKeyTupleEncoding.GetTimePosition(tag));
    }

    public uint EnsureHilo(byte[] hiloKey)
    {
        byte[] rawValue = new byte[] { 1, 0, 0, 0, 0, 1 };
        ObjRef<uint> value = new ObjRef<uint>(1);

        this.zoneTree.TryAtomicAddOrUpdate(in hiloKey, rawValue, bool (ref byte[] data) =>
        {
            uint localValue = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(2, 4));
            localValue++;

            value.Value = localValue;
            BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(2, 4), localValue);
            return true;
        });

        return value.Value;
    }

    #region Bucket API
    public bool TryCreateBucket(BucketCreationData bucketCreationData, out uint bucketId)
    {
        byte[] bucketKey = BucketKeyEncoder.Encode(bucketCreationData.Name);

        if (this.zoneTree.TryGet(in bucketKey, out _))
        {
            bucketId = 0;
            return false;
        }

        byte[] hiLoKey = HiloKeyEncoding.Encode(HiloType.Bucket);
        uint bucketIdInternal = this.EnsureHilo(hiLoKey);
        byte[] bucketValue = TupleEncoder.Create(1,
            bucketIdInternal,
            bucketCreationData.Name,
            bucketCreationData.Description ?? string.Empty,
            bucketCreationData.CreateTimeUnixTimestampInMs);

        if (this.zoneTree.TryAtomicAdd(in bucketKey, in bucketValue))
        {
            bucketId = bucketIdInternal;
            return true;
        }

        bucketId = 0;
        return false;
    }

    public bool TryRemoveBucket(ReadOnlySpan<char> bucketName, out uint bucketId)
    {
        byte[] bucketKey = BucketKeyEncoder.Encode(bucketName);

        if (this.zoneTree.TryGet(in bucketKey, out byte[] bucketValue))
        {
            if (!TupleEncoder.TryDeconstruct(bucketValue,
                1,
                out bucketId))
            {
                return false;
            }

            return this.zoneTree.TryDelete(in bucketKey);
        }

        bucketId = 0;
        return false;
    }

    public bool TryGetBucketId(ReadOnlySpan<char> bucketName, out uint bucketId)
    {
        byte[] bucketKey = BucketKeyEncoder.Encode(bucketName);

        if (this.zoneTree.TryGet(in bucketKey, out byte[] bucketValue))
        {
            return TupleEncoder.TryDeconstruct(bucketValue,
                1,
                out bucketId);
        }

        bucketId = 0;
        return false;
    }

    public List<string> ReadBuckets()
    {
        byte[] bucketPrefix = BucketKeyEncoder.GetPrefix();

        using IZoneTreeIterator<byte[], byte[]> iterator = this.zoneTree.CreateReverseIterator();
        iterator.Seek(in bucketPrefix);

        List<string> names = new List<string>();

        while (iterator.Next())
        {
            if (BucketKeyEncoder.TryDecode(iterator.CurrentKey, out string name))
            {
                names.Add(name);
            }
            else
            {
                break;
            }
        }

        return names;
    }

    public IEnumerable<BucketCreationData> ReadBucketsComplete()
    {
        byte[] bucketPrefix = BucketKeyEncoder.GetPrefix();

        using IZoneTreeIterator<byte[], byte[]> iterator = this.zoneTree.CreateReverseIterator();
        iterator.Seek(in bucketPrefix);


        while (iterator.Next())
        {
            if (BucketKeyEncoder.TryDecode(iterator.CurrentKey, out string name))
            {
                bool canReadBucket = TupleEncoder.TryDeconstruct(iterator.CurrentValue,
                     1,
                     out uint _,
                     out string _,
                     out string description,
                     out long unixTimestampInMs);

                System.Diagnostics.Debug.Assert(canReadBucket);

                yield return new BucketCreationData()
                {
                    Name = name,
                    CreateTimeUnixTimestampInMs = unixTimestampInMs,
                    Description = description,
                };
            }
            else
            {
                break;
            }
        }

    }

    #endregion

    #region Measurement API

    public bool TryEnsureMeasurementId(uint bucketId, ReadOnlySpan<char> measurement, out uint measurementId)
    {
        byte[] measurementKey = TupleEncoder.Create(DataType.KeyMeasurementPrefix, bucketId, measurement.ToString());

        if (this.zoneTree.TryGet(in measurementKey, out byte[] value))
        {
            return TupleEncoder.TryDeconstruct(value, 1, out measurementId);
        }
        else
        {
            measurementId = this.EnsureHilo(HiloKeyEncoding.Encode(HiloType.Measurement, bucketId));
            value = TupleEncoder.Create(1, measurementId);

            return this.zoneTree.TryAtomicAdd(in measurementKey, in value);
        }
    }

    public bool TryGetMeasurementId(uint bucketId, ReadOnlySpan<char> measurement, out uint measurementId)
    {
        byte[] measurementKey = TupleEncoder.Create(DataType.KeyMeasurementPrefix, bucketId, measurement.ToString());

        if (this.zoneTree.TryGet(in measurementKey, out byte[] value))
        {
            return TupleEncoder.TryDeconstruct(value, 1, out measurementId);
        }

        measurementId = default;
        return false;
    }

    public List<string> ReadMeasurements(uint bucketId)
    {
        byte[] bucketPrefix = TupleEncoder.Create(DataType.KeyMeasurementPrefix, bucketId);

        using IZoneTreeIterator<byte[], byte[]> iterator = this.zoneTree.CreateReverseIterator();
        iterator.Seek(in bucketPrefix);

        List<string> names = new List<string>();

        while (iterator.Next())
        {
            if (TupleEncoder.TryDeconstruct(iterator.CurrentKey, DataType.KeyMeasurementPrefix, out uint _, out string name))
            {
                names.Add(name);
            }
            else
            {
                break;
            }
        }

        return names;
    }
    #endregion
}
