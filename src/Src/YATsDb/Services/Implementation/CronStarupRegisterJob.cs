using NCronJob;
using YATsDb.Services.Contracts;

namespace YATsDb.Services.Implementation;

public class CronStartupRegisterJob : IJob
{
    private readonly ILogger<CronStartupRegisterJob> logger;
    private readonly IServiceProvider serviceProvider;

    public CronStartupRegisterJob(ILogger<CronStartupRegisterJob> logger, IServiceProvider serviceProvider)
    {
        this.logger = logger;
        this.serviceProvider = serviceProvider;
    }

    public Task RunAsync(JobExecutionContext context, CancellationToken token)
    {
        this.logger.LogTrace("Entering to RunAsync");

        ICronManagement cronManagement = this.serviceProvider.GetRequiredService<ICronManagement>();
        cronManagement.RegisterJobsOnStartup();

        return Task.CompletedTask;
    }
}