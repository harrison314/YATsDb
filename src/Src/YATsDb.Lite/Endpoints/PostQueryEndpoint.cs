using Microsoft.AspNetCore.Mvc;
using YATsDb.Core.Services;
using YATsDb.Lite.Infrastructure.Validation;
using YATsDb.Lite.Endpoints.Common;
using System.Diagnostics.CodeAnalysis;

namespace YATsDb.Lite.Endpoints;

public static class PostQueryEndpoint
{
    public record QueryDal(string BucketName, string Query) : ISimpleValidatedObject
    {
        public bool IsValid([NotNullWhen(true)] out IDictionary<string, string[]>? errors)
        {
            Dictionary<string, List<string>> localErrors = new Dictionary<string, List<string>>();
            if (string.IsNullOrEmpty(this.BucketName))
            {
                this.AddError(localErrors, nameof(this.BucketName), "Can not by null or empty.");
            }
            else
            {
                if (this.BucketName.Length > 150)
                {
                    this.AddError(localErrors, nameof(this.BucketName), "Max length is 150.");
                }

                if (!RegexHolder.GetIdentfierRegex().IsMatch(this.BucketName))
                {
                    this.AddError(localErrors, nameof(this.BucketName), "BucketName is not valid.");
                }
            }

            if (string.IsNullOrEmpty(this.Query))
            {
                this.AddError(localErrors, nameof(this.Query), "Can not by null or empty.");
            }
            else
            {
                if (this.Query.Length > 2048)
                {
                    this.AddError(localErrors, nameof(this.Query), "Max length is 2048.");
                }
            }

            if (localErrors.Count > 0)
            {
                errors = localErrors.ToDictionary(t => t.Key, t => t.Value.ToArray());
                return false;
            }

            errors = null;
            return false;
        }

        private void AddError(Dictionary<string, List<string>> dic, string filedName, string message)
        {
            if (dic.TryGetValue(filedName, out List<string>? list))
            {
                list.Add(message);
            }
            else
            {
                dic.Add(filedName, new List<string>() { message });
            }
        }
    };

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
        .AddEndpointFilter<ValidationFilter<QueryDal>>();
    }

    //public class QueryDalValidator : AbstractValidator<QueryDal>
    //{
    //    public QueryDalValidator()
    //    {
    //        this.RuleFor(t => t.BucketName)
    //            .NotEmpty()
    //            .NotNull()
    //            .MaximumLength(150)
    //            .Matches("^[A-Za-z0-9_-]+$");

    //        this.RuleFor(t => t.Query)
    //            .MaximumLength(2048);
    //    }
    //}
}