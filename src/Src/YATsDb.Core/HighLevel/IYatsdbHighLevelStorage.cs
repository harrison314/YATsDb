
namespace YATsDb.Core.HighLevel;

public interface IYatsdbHighLevelStorage
{
    void CreateBucket(string bucket, string? description, DateTimeOffset cratedAt);

    void DeleteBucket(string bucket, DateTimeOffset now);

    List<DbValue[]> ExecuteQuery(PreparedQueryObject preparedQueryObject);

    List<HighLevelBucketInfo> GetBuckets();

    List<string> GetMeasurements(string bucketName);

    void Insert(ReadOnlyMemory<char> bucket, IEnumerable<HighLevelInputDataPoint> points);

    PreparedQueryObject PrepareQuery(QueryObject queryObject);
}
