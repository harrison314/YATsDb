using YATsDb.Core.HighLevel;

namespace YATsDb.Core.Services;

public interface IManagementService
{
    void CreateBucket(string name, string? description);

    void DeleteBucket(string name);

    List<HighLevelBucketInfo> ListBuckets();
    
    List<string> ListMeasurements(string bucketName);
}
