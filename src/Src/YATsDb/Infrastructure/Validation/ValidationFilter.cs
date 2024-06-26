
using FluentValidation;

namespace YATsDb.Infrastructure.Validation;

public class ValidationFilter<T> : IEndpointFilter
{
    public ValidationFilter()
    {

    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        IValidator<T>? validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        ILogger<ValidationFilter<T>>? logger = context.HttpContext.RequestServices.GetService<ILogger<ValidationFilter<T>>>();
        System.Diagnostics.Debug.Assert(logger != null);

        if (validator == null)
        {
            logger.LogCritical("Validator for type {validatorType} not found.", typeof(T).FullName);
            throw new InvalidProgramException($"Validator for type {typeof(T).FullName} not found.");
        }

        T? entity = context.Arguments
              .OfType<T>()
              .FirstOrDefault(a => a?.GetType() == typeof(T));
        if (entity is not null)
        {
            FluentValidation.Results.ValidationResult results = await validator.ValidateAsync((entity));
            if (!results.IsValid)
            {
                logger.LogDebug("Validation error for {type} {error}.",
                    typeof(T).FullName,
                    results);

                return Results.ValidationProblem(results.ToDictionary());
            }
        }
        else
        {
            logger.LogDebug("DTO of type {type} not found.",
                    typeof(T).FullName);

            return Results.Problem("DTO not found.");
        }

        return await next(context);
    }
}
