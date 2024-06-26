namespace YATsDb.Services.Contracts;

public interface ICronManagement
{
    void CreateCronJob(string bucketName, CreateCronJobRequest request);

    void DeleteCronJob(string bucketName, string jobName);

    List<CronJobInfo> ListJobs(string bucketName);
    
    CronJob? TryGetCronJob(string bucketName, string jobName);

    void UpdateCronJob(string bucketName, CreateCronJobRequest request);

    void RegisterJobsOnStartup();

    void ExecuteJob(string bucketName, string jobName);

    void DeleteCronJobs(string bucketName);
}
