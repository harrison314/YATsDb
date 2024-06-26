namespace YATsDb.Endpoints.Common;

public class QueryResult
{
    public List<object?[]> Result 
    { 
        get;
    }

    public QueryResult(List<object?[]> result)
    {
        this.Result = result;
    }

    private QueryResult()
    {
        this.Result = default!;
    }
}