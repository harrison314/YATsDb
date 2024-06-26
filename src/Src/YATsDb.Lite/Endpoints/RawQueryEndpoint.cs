using YATsDb.Core.Services;
using YATsDb.Lite.Endpoints.Common;

namespace YATsDb.Lite.Endpoints;

public static class RawQueryEndpoint
{
    public static void AddRawQueryEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/query/raw/{bucketName}", (string bucketName, RawStringDto content, IDalServices dalServices)
            =>
        {
            QueryParameters queryParameters = new QueryParameters();

            List<object?[]> r = dalServices.Query(bucketName, content.Value.Trim(), queryParameters);
            return Results.Ok(new QueryResult(r));
        });
    }
}