using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using YATsDb.Core.Services;
using YATsDb.Lite.Endpoints.Common;
using YATsDb.Lite.Infrastructure.Validation;

namespace YATsDb.Lite.Endpoints;

public static class ManagementPostBucketsEndpoint
{
    public record CreateBucketDto(string BucketName, string? Description) : ISimpleValidatedObject
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

            if (this.Description != null)
            {
                if (this.Description.Length > 2048)
                {
                    this.AddError(localErrors, nameof(this.Description), "Max length is 150.");
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
    }

    public static void AddManagementPostBucketsEndpoint(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("/management/bucket", ([FromBody] CreateBucketDto dto, IManagementService managementService)
            =>
        {
            managementService.CreateBucket(dto.BucketName, dto.Description);
            return Results.Ok();
        })
        .AddEndpointFilter<ValidationFilter<CreateBucketDto>>();
    }

    //public class CreateBucketDtoValidator : AbstractValidator<CreateBucketDto>
    //{
    //    public CreateBucketDtoValidator()
    //    {
    //        this.RuleFor(t => t.BucketName)
    //            .NotEmpty()
    //            .NotNull()
    //            .MaximumLength(150)
    //            .Matches("^[A-Za-z0-9_-]+$");

    //        this.RuleFor(t => t.Description)
    //            .MaximumLength(2048);
    //    }
    //}
}
