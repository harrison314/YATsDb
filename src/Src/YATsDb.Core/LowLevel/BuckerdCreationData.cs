namespace YATsDb.Core.LowLevel;

public class BucketCreationData
{
    public string Name
    {
        get;
        set;
    }

    public string? Description
    {
        get;
        set;
    }

    public long CreateTimeUnixTimestampInMs
    {
        get;
        set;
    }

    public BucketCreationData()
    {
        this.Name = string.Empty;
    }
}