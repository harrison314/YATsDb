using NCronJob;
using System.Text.Json;
using YATsDb.Core;
using YATsDb.Core.LowLevel;
using YATsDb.Services.Contracts;

namespace YATsDb.Services.Implementation;

public class CronManagement : ICronManagement
{
    private readonly IKvStorage kvStorage;
    private readonly IRuntimeJobRegistry runtimeJobRegistry;
    private readonly IJsInternalEngine jsInternalEngine;
    private readonly TimeProvider timeProvider;
    private readonly ILogger<CronManagement> logger;

    #region Keys
    private const string MainKey = "CronJob:enabled";
    private const string DataKey = "CronJob:data";
    #endregion

    public CronManagement(IKvStorage kvStorage,
        IRuntimeJobRegistry runtimeJobRegistry,
        IJsInternalEngine jsInternalEngine,
        TimeProvider timeProvider,
        ILogger<CronManagement> logger)
    {
        this.kvStorage = kvStorage;
        this.runtimeJobRegistry = runtimeJobRegistry;
        this.jsInternalEngine = jsInternalEngine;
        this.timeProvider = timeProvider;
        this.logger = logger;
    }

    public void CreateCronJob(string bucketName, CreateCronJobRequest request)
    {
        this.logger.LogTrace("Entering to CreateCronJob with name {bucketName}/{name}.", bucketName, request.Name);

        string fullName = this.BuildFullJobName(bucketName, request.Name);
        CronJobData cronJobData = new CronJobData()
        {
            Code = request.Code,
            BucketName = bucketName,
            Created = this.timeProvider.GetUtcNow(),
            Name = request.Name.Trim(),
            CronExpression = request.CronExpression.Trim(),
            Updated = null
        };

        if (this.kvStorage.TryGet(MainKey, fullName, out _))
        {
            throw new YatsdbDataException($"Job {bucketName}/{request.Name} already exists.");
        }

        string jsonData = JsonSerializer.Serialize(cronJobData);

        this.kvStorage.Upsert(MainKey, fullName, request.Enabled.ToString());
        this.kvStorage.Upsert(DataKey, fullName, jsonData);

        this.logger.LogInformation("Created a new CronJob {bucketName}/{name}.", bucketName, cronJobData.Name);

        if (request.Enabled)
        {
            this.EnableCronJob(bucketName, cronJobData.Name, cronJobData.CronExpression);
        }
    }

    public void DeleteCronJob(string bucketName, string jobName)
    {
        this.logger.LogTrace("Entering to DeleteCronJob with name {bucketName}/{name}.", bucketName, jobName);

        jobName = jobName.Trim();
        string jobFullName = this.BuildFullJobName(bucketName, jobName);
        if (!this.kvStorage.TryGet(MainKey, jobFullName, out string? isEnabledStr))
        {
            throw new YatsdbDataException($"Job {bucketName}/{jobName} does not exists");
        }

        if (Convert.ToBoolean(isEnabledStr))
        {
            this.DisableCronJob(bucketName, jobName);
        }

        this.kvStorage.Remove(MainKey, jobFullName);
        this.kvStorage.Remove(DataKey, jobFullName);

        this.logger.LogInformation("Remove Cron json {bucketName}/{name}.", bucketName, jobName);
        //TODO: check removing
    }

    public void DeleteCronJobs(string bucketName)
    {
        this.logger.LogTrace("Entering to DeleteCronJobs with name {bucketName}.", bucketName);

        foreach ((string fullName, string enableStr) in this.kvStorage.EnumerateKeyValues(MainKey))
        {
            if (Convert.ToBoolean(enableStr))
            {
                if (!this.kvStorage.TryGet(DataKey, fullName, out string? dataString))
                {
                    this.logger.LogWarning("Incompatible data for cron job {name}.", fullName);
                    continue;
                }

                CronJobData? cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
                System.Diagnostics.Debug.Assert(cronJobData != null);

                this.DeleteCronJob(cronJobData.BucketName, cronJobData.Name);
            }
        }

    }

    public void UpdateCronJob(string bucketName, CreateCronJobRequest request)
    {
        this.logger.LogTrace("Entering to UpdateCronJob with name {bucketName}/{name}.", bucketName, request.Name);

        string jobName = request.Name.Trim();
        string jobFullName = this.BuildFullJobName(bucketName, jobName);
        if (!this.kvStorage.TryGet(MainKey, jobFullName, out string? isEnabledStr))
        {
            throw new YatsdbDataException("Job does not exists"); //TODO
        }

        if (!this.kvStorage.TryGet(DataKey, jobFullName, out string? dataString))
        {
            this.logger.LogError("Incompatible data for cron job {bucketName}/{name}.", bucketName, jobName);
            throw new YatsdbDataException("Job does not exists"); //TODO
        }

        if (Convert.ToBoolean(isEnabledStr))
        {
            this.DisableCronJob(bucketName, jobName);
        }

        CronJobData? cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
        System.Diagnostics.Debug.Assert(cronJobData != null);

        cronJobData.CronExpression = request.CronExpression.Trim();
        cronJobData.Code = request.Code.Trim();
        cronJobData.Updated = this.timeProvider.GetUtcNow();

        string jsonData = JsonSerializer.Serialize(cronJobData);

        this.kvStorage.Upsert(MainKey, jobName, request.Enabled.ToString());
        this.kvStorage.Upsert(DataKey, jobName, jsonData);

        if (request.Enabled)
        {
            this.EnableCronJob(bucketName, jobName, cronJobData.CronExpression);
        }
    }

    public List<CronJobInfo> ListJobs(string bucketName)
    {
        this.logger.LogTrace("Entering to ListJobs by {bucketName}.", bucketName);

        string prefix = string.Concat(bucketName, ":");
        List<CronJobInfo> result = new List<CronJobInfo>();
        foreach ((string fullName, string enableStr) in this.kvStorage.EnumerateKeyValues(MainKey))
        {
            if (fullName.StartsWith(prefix))
            {
                if (!this.kvStorage.TryGet(DataKey, fullName, out string? dataString))
                {
                    this.logger.LogError("Incompatible data for cron job {name}.", fullName);
                    throw new YatsdbDataException("invalid data");
                }

                CronJobData? cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
                System.Diagnostics.Debug.Assert(cronJobData != null);

                result.Add(new CronJobInfo(cronJobData.Name,
                    cronJobData.CronExpression,
                    Convert.ToBoolean(enableStr)));
            }
        }

        return result;
    }

    public CronJob? TryGetCronJob(string bucketName, string jobName)
    {
        this.logger.LogTrace("Entering to TryGetCronJob with name {bucketName}/{name}.", bucketName, jobName);

        jobName = jobName.Trim();
        string jobFullName = this.BuildFullJobName(bucketName, jobName);
        if (!this.kvStorage.TryGet(MainKey, jobFullName, out string? isEnabledStr))
        {
            this.logger.LogDebug("CronJob {bucketName}/{name} not found.", bucketName, jobName);
            return null;
        }

        if (!this.kvStorage.TryGet(DataKey, jobFullName, out string? dataString))
        {
            this.logger.LogError("Incompatible data for cron job {bucketName}/{name}.", bucketName, jobName);
            throw new YatsdbDataException("Incompatible data");
        }

        CronJobData? cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
        System.Diagnostics.Debug.Assert(cronJobData != null);

        return new CronJob(cronJobData.BucketName,
            cronJobData.Name,
            cronJobData.CronExpression,
            cronJobData.Code,
            Convert.ToBoolean(isEnabledStr),
            cronJobData.Created,
            cronJobData.Updated);
    }

    public void RegisterJobsOnStartup()
    {
        this.logger.LogTrace("Entering to RegisterJobsOnStartup.");

        foreach ((string fullName, string enableStr) in this.kvStorage.EnumerateKeyValues(MainKey))
        {
            if (Convert.ToBoolean(enableStr))
            {
                if (!this.kvStorage.TryGet(DataKey, fullName, out string? dataString))
                {
                    this.logger.LogWarning("Incompatible data for cron job {name}.", fullName);
                    continue;
                }

                CronJobData? cronJobData = JsonSerializer.Deserialize<CronJobData>(dataString);
                System.Diagnostics.Debug.Assert(cronJobData != null);

                this.EnableCronJob(cronJobData.BucketName, cronJobData.Name, cronJobData.CronExpression);
            }
        }
    }

    public void ExecuteJob(string bucketName, string jobName)
    {
        CronJob? jobDefinition = this.TryGetCronJob(bucketName, jobName);
        if (jobDefinition == null)
        {
            this.logger.LogDebug("CronJob {bucketName}/{name} not found.", bucketName, jobName);
            throw new YatsdbDataException($"CronJob {bucketName}/{jobName} not found."); //TODO
        }

        JsExecutionContext ctx = new JsExecutionContext(jobDefinition.BucketName,
            jobDefinition.Name,
            jobDefinition.Code,
            false);

        this.jsInternalEngine.ExecuteModule(ctx);
    }

    private void EnableCronJob(string bucketName, string name, string cronExpression)
    {
        string jobFullName = this.BuildFullJobName(bucketName, name);

        bool ttt = this.runtimeJobRegistry.TryGetSchedule(jobFullName, out string? aaa, out _);
        if (ttt)
        {
            this.runtimeJobRegistry.EnableJob(jobFullName);
            
        }
        else
        {
            this.runtimeJobRegistry.AddJob(builder =>
            {
                builder.AddJob<CronTriggerJob>(opt => opt.WithCronExpression(cronExpression)
                    .WithName(jobFullName)
                    .WithParameter(jobFullName));
            });
        }


        this.logger.LogInformation("Enabled CronJob {bucketName}/{name}.", bucketName, name);
    }

    private void DisableCronJob(string bucketName, string name)
    {
        this.runtimeJobRegistry.RemoveJob(this.BuildFullJobName(bucketName, name));
        this.logger.LogInformation("Remove Cron json scheduling {bucketName}/{name}.", bucketName, name);
    }

    private string BuildFullJobName(string bucketName, string name)
    {
        return string.Concat(bucketName, ":", name);
    }
}
