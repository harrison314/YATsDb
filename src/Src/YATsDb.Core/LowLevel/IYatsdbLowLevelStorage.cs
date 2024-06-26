using Tenray.ZoneTree;

namespace YATsDb.Core.LowLevel;

public interface IYatsdbLowLevelStorage
{
    IZoneTree<byte[], byte[]> Db
    {
        get; 
    }

    void InsertDataPoint(uint bucketId,
        uint measurementId,
        long unixTimestampInMsTime,
        ReadOnlySpan<double> values,
        ReadOnlySpan<char> tag);

    LowLevelIterator QueryData(uint bucketId,
        uint measurementId,
        long? fromUnixTimestampInMs,
        long? toUnixTimestampInMs,
        ReadOnlySpan<char> tag);

    uint EnsureHilo(byte[] hiloKey);


    bool TryCreateBucket(BucketCreationData bucketCreationData, out uint bucketId);

    bool TryGetBucketId(ReadOnlySpan<char> bucketName, out uint bucketId);

    List<string> ReadBuckets();

    IEnumerable<BucketCreationData> ReadBucketsComplete();

    bool TryRemoveBucket(ReadOnlySpan<char> bucketName, out uint bucketId);

    bool TryEnsureMeasurementId(uint bucketId, ReadOnlySpan<char> measurement, out uint measurementId);

    bool TryGetMeasurementId(uint bucketId, ReadOnlySpan<char> measurement, out uint measurementId);

    List<string> ReadMeasurements(uint bucketId);

}