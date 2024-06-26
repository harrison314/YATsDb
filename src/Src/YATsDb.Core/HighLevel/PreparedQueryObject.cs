namespace YATsDb.Core.HighLevel;

public class PreparedQueryObject
{
    public PreparedQueryType Type
    {
        get;
        set;
    }

    public uint BucketNameId
    {
        get;
        set;
    }

    public uint MeasurementId
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

    public AggregationQuery[] Aggregations
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

    public int[] ValueIndexes
    {
        get;
        set;
    }

    public PreparedQueryObject()
    {
        this.Type = PreparedQueryType.None;
        this.Aggregations = Array.Empty<AggregationQuery>();
        this.ValueIndexes = Array.Empty<int>();
    }
}