using YATsDb.Lite.Endpoints;

namespace YATsDb.Lite;

internal static class EndpointsExtensions
{
    public static void AddAppEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.AddRawWriteDataEndpoint();
        endpointRouteBuilder.AddRawQueryEndpoint();

        endpointRouteBuilder.AddManagementGetBucketsEndpoint();
        endpointRouteBuilder.AddManagementGetMeasurementsEndpoint();
        endpointRouteBuilder.AddManagementPostBucketsEndpoint();
        endpointRouteBuilder.AddManagementDeleteBucketsEndpoint();

        endpointRouteBuilder.AddPostQueryEndpoint();
    }
}
