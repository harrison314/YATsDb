using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using YATsDb.Core.Services;
using static YATsDb.Endpoints.ManagementPostBucketsEndpoint;
using YATsDb.Infrastructure.Validation;
using YATsDb.Endpoints.Common;

namespace YATsDb.Endpoints;

public static class PostQueryEndpoint
{
    public record QueryDal(string BucketName, string Query);

    public static void AddPostQueryEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/query", (QueryDal model,
            [FromQuery(Name = "timeUnit")] TimeRepresentation? timeUnit,
            IDalServices dalServices)
            =>
        {
            QueryParameters queryParameters = new QueryParameters();
            if (timeUnit.HasValue)
            {
                queryParameters.TimeRepresentation = timeUnit.Value;
            }

            List<object?[]> r = dalServices.Query(model.BucketName, model.Query.Trim(), queryParameters);
            return Results.Ok(new QueryResult(r));
        })
        .WithTags(TagNames.Query)
        .AddEndpointFilter<ValidationFilter<QueryDal>>();
    }

    public class QueryDalValidator : AbstractValidator<QueryDal>
    {
        public QueryDalValidator()
        {
            this.RuleFor(t => t.BucketName)
                .NotEmpty()
                .NotNull()
                .MaximumLength(150)
                .Matches("^[A-Za-z0-9_-]+$");

            this.RuleFor(t => t.Query)
                .MaximumLength(2048);
        }
    }
}