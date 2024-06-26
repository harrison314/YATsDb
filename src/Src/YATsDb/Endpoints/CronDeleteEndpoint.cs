using Microsoft.AspNetCore.Http.HttpResults;
using YATsDb.Services.Contracts;

namespace YATsDb.Endpoints;

internal static class CronDeleteEndpoint
{
    public static void AddCronDeleteEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/Cron/{bucketName}/{name}", Results<Ok, NotFound> (string bucketName, string name, ICronManagement management) =>
        {
            management.DeleteCronJob(bucketName, name);

            return TypedResults.Ok();
        }).WithTags(TagNames.Cron);
    }
}
