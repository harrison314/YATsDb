using Microsoft.AspNetCore.Http.HttpResults;
using YATsDb.Services.Contracts;

namespace YATsDb.Endpoints;

internal static class CronGetEndpoint
{
    public record CronJobInfoDto(string Name, string CronExpression, bool Enabled);

    public static void AddCronGetEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/Cron/{bucketName}", Results<Ok<IEnumerable<CronJobInfoDto>>, BadRequest> (string bucketName, ICronManagement management) =>
        {
            IEnumerable<CronJobInfoDto> result = management.ListJobs(bucketName).Select(t => new CronJobInfoDto(t.Name,
                  t.CronExpression,
                  t.Enabled));

            return TypedResults.Ok(result);
        }).WithTags(TagNames.Cron);
    }
}