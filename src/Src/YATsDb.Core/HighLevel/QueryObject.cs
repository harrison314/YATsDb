namespace YATsDb.Core.HighLevel;

public class QueryObject
{
    public ReadOnlyMemory<char> BucketName
    {
        get;
        set;
    }

    public ReadOnlyMemory<char> MeasurementName
    {
        get;
        set;
    }
    public TimeSource From
    {
        get;
        set;
    }

    public TimeSource To
    {
        get;
        set;
    }

    public int Skip
    {
        get;
        set;
    }

    public int? Take
    {
        get;
        set;
    }

    public List<AggregationQuery>? Aggregations
    {
        get;
        set;
    }

    public long? GroupByMs
    {
        get;
        set;
    }

    public ReadOnlyMemory<char> RequestedTag
    {
        get;
        set;
    }

    public QueryObject()
    {
    }
}