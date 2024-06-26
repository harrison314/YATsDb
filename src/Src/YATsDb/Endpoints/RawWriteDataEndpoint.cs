using YATsDb.Core.Services;
using YATsDb.Endpoints.Common;

namespace YATsDb.Endpoints;

public static class RawWriteDataEndpoint
{
    public static void AddRawWriteDataEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/write/{bucketName}", (string bucketName, RawStringDto content, IDalServices dalServices)
            =>
        {
            dalServices.InsertLines(bucketName, content.Value);
            return Results.Created();
        })
            .ExcludeFromDescription();
    }
}
