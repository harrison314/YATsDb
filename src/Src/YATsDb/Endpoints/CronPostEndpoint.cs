using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using YATsDb.Infrastructure.Validation;
using YATsDb.Services.Contracts;

namespace YATsDb.Endpoints;

internal static class CronPostEndpoint
{
    public record CreateCronDto(string Name,
        string CronExpression,
        string Code,
        bool Enabled);

    public static void AddCronPostEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/Cron/{bucketName}", Results<Created, BadRequest> (string bucketName, CreateCronDto model, ICronManagement management) =>
        {
            management.CreateCronJob(bucketName, 
                new CreateCronJobRequest(model.Name,
                model.CronExpression,
                model.Code,
                model.Enabled));

            return TypedResults.Created();
        })
        .WithTags(TagNames.Cron)
        .AddEndpointFilter<ValidationFilter<CreateCronDto>>();
    }

    public class CreateCronDtoValidator : AbstractValidator<CreateCronDto>
    {
        public CreateCronDtoValidator()
        {
            this.RuleFor(t => t.Name)
                .NotEmpty()
                .NotNull()
                .MaximumLength(150)
                .Matches("^[A-Za-z0-9_-]+$");

            this.RuleFor(t => t.CronExpression)
                .NotEmpty()
                .NotNull()
                .MaximumLength(150)
                .MustByCronExpression();

            this.RuleFor(t => t.Code)
                .NotEmpty()
                .NotNull()
                .MaximumLength(1024 * 1024 * 4);
        }
    }
}
