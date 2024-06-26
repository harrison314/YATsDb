namespace YATsDb.Services.Implementation;

internal class CronJobData
{
    public string BucketName
    {
        get;
        set;
    }

    public string Name
    {
        get;
        set;
    }

    public string CronExpression
    {
        get;
        set;
    }

    public string Code
    {
        get;
        set;
    }

    public DateTimeOffset Created
    {
        get;
        set;
    }

    public DateTimeOffset? Updated
    {
        get;
        set;
    }

    public CronJobData()
    {
        this.BucketName = string.Empty;
        this.Name = string.Empty;
        this.CronExpression = string.Empty;
        this.Code = string.Empty;
    }
}
