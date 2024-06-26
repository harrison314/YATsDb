
using YATsDb.Core.Services;

namespace YATsDb.Services.Implementation.JsEngine;

internal class DatabaseProvider
{
    private readonly IDalServices dalServices;

    public DatabaseProvider(IDalServices dalServices)
    {
        this.dalServices = dalServices;
    }

    public List<object?[]> ExecuteSql(string bucketName, string query)
    {
        return this.dalServices.Query(bucketName, query, new QueryParameters()
        {
            TimeRepresentation = TimeRepresentation.DateTimeOffset
        });
    }

    public void InsertLines(string bucketName, string lines)
    {
        this.dalServices.InsertLines(bucketName, lines);
    }

    public void Insert(string bucketName, string measurement, string? tag, object?[] values, DateTime? time)
    {
        //TODO: Implements
        throw new NotImplementedException("Insert is not implement yet");
    }
}
