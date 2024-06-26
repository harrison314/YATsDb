using YATsDb.Core.Services;

namespace YATsDb.Lite.Endpoints;

public static class ManagementGetMeasurementsEndpoint
{
    public static void AddManagementGetMeasurementsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/management/bucket/{bucketName}/measurement", (string bucketName, IManagementService managementService)
            =>
        {
            return Results.Ok(managementService.ListMeasurements(bucketName));
        });
    }
}