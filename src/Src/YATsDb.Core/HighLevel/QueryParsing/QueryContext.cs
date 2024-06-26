namespace YATsDb.Core.HighLevel.QueryParsing;

internal class QueryContext
{
    public string BucketName
    {
        get;
    }

    internal bool IntervalFnApply
    {
        get;
        set;
    }

    internal bool TagFnApply
    {
        get;
        set;
    }

    public QueryContext(string bucketName)
    {
        this.BucketName = bucketName;
    }
}