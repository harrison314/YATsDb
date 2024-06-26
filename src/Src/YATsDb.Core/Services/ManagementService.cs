using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel;

namespace YATsDb.Core.Services;

public class ManagementService : IManagementService
{
    private readonly IYatsdbHighLevelStorage storage;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<ManagementService> logger;

    public ManagementService(IYatsdbHighLevelStorage storage,
        TimeProvider timeProvider,
        ILogger<ManagementService> logger)
    {
        this.storage = storage;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public void CreateBucket(string name, string? description)
    {
        this.logger.LogTrace("Entering to CreateBucket with name {name}", name);
        this.storage.CreateBucket(name, description, this.timeProvider.GetUtcNow());
        this.logger.LogInformation("New bucket created {name}", name);
    }

    public void DeleteBucket(string name)
    {
        this.logger.LogTrace("Entering to DeleteBucket with name {name}", name);
        this.storage.DeleteBucket(name, this.timeProvider.GetUtcNow());
        this.logger.LogInformation("New bucket removed {name}", name);
    }

    public List<HighLevelBucketInfo> ListBuckets()
    {
        this.logger.LogTrace("Entering to ListBuckets");

        return this.storage.GetBuckets();
    }

    public List<string> ListMeasurements(string bucketName)
    {
        this.logger.LogTrace("Entering to ListMeasurements with name {bucketName}", bucketName);

        return this.storage.GetMeasurements(bucketName);
    }
}
