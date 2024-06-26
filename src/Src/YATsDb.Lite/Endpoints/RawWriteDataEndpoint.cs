using YATsDb.Core.Services;
using YATsDb.Lite.Endpoints.Common;

namespace YATsDb.Lite.Endpoints;

public static class RawWriteDataEndpoint
{
    public static void AddRawWriteDataEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/write/{bucketName}", (string bucketName, RawStringDto content, IDalServices dalServices)
            =>
        {
            dalServices.InsertLines(bucketName, content.Value);
            return Results.Created();
        });
    }
}
