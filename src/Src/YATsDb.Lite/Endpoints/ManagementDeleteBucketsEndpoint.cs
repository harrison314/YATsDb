using YATsDb.Core.Services;

namespace YATsDb.Lite.Endpoints;

public static class ManagementDeleteBucketsEndpoint
{
    public static void AddManagementDeleteBucketsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("/management/bucket/{bucketName}", (string bucketName, IManagementService managementService)
           =>
        {
            managementService.DeleteBucket(bucketName);
            return Results.Ok();
        });
    }
}