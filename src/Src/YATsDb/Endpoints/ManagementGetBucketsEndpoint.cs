using YATsDb.Core.HighLevel;
using YATsDb.Core.Services;

namespace YATsDb.Endpoints;

public static class ManagementGetBucketsEndpoint
{
    public record BucketInfoDto(string Name, string? Description, DateTimeOffset Created);

    public static void AddManagementGetBucketsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/management/bucket", (IManagementService managementService)
            =>
        {
            List<HighLevelBucketInfo> buckets = managementService.ListBuckets();
            List<BucketInfoDto> model = buckets.Select(t => new BucketInfoDto(t.Name, t.Description, t.Created)).ToList();
            return Results.Ok(model);
        }).WithTags(TagNames.Management);
    }
}
