namespace YATsDb.Services.Contracts;

public record CronJob(string Name,
    string BucketName,
    string CronExpression,
    string Code,
    bool Enabled,
    DateTimeOffset Created,
    DateTimeOffset? Updated);
