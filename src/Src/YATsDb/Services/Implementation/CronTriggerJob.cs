using NCronJob;
using YATsDb.Services.Contracts;

namespace YATsDb.Services.Implementation;

public class CronTriggerJob : IJob
{
    private readonly ICronManagement cronManagement;
    private readonly ILogger<CronTriggerJob> logger;

    public CronTriggerJob(ICronManagement cronManagement, ILogger<CronTriggerJob> logger)
    {
        this.cronManagement = cronManagement;
        this.logger = logger;
    }

    public Task RunAsync(JobExecutionContext context, CancellationToken token)
    {
        this.logger.LogInformation("Run JOB {name}", context.Parameter);
        System.Diagnostics.Debug.Assert(context.Parameter is string);

        string jobName = context.Parameter.ToString()!;
        string[] parts = jobName.Split(':', 2);

        try
        {
            this.cronManagement.ExecuteJob(parts[0], parts[1]);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during processing job {name}.", jobName);
        }

        return Task.CompletedTask;
    }
}
