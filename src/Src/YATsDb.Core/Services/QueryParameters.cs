namespace YATsDb.Core.Services;

public class QueryParameters
{
    public TimeRepresentation TimeRepresentation
    {
        get;
        set;
    }

    public QueryParameters()
    {
        this.TimeRepresentation = TimeRepresentation.DateTimeOffset;
    }
}
