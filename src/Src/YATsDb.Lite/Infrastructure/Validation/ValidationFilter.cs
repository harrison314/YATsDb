namespace YATsDb.Lite.Infrastructure.Validation;

public class ValidationFilter<T> : IEndpointFilter
    where T : ISimpleValidatedObject
{
    public ValidationFilter()
    {

    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ILogger<ValidationFilter<T>>? logger = context.HttpContext.RequestServices.GetService<ILogger<ValidationFilter<T>>>();
        System.Diagnostics.Debug.Assert(logger != null);

        T? entity = context.Arguments
              .OfType<T>()
              .FirstOrDefault(a => a?.GetType() == typeof(T));
        if (entity is ISimpleValidatedObject simpleValidatedObject)
        {
            if (simpleValidatedObject.IsValid(out IDictionary<string, string[]>? errors))
            {
                return Results.ValidationProblem(errors.ToDictionary());
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
