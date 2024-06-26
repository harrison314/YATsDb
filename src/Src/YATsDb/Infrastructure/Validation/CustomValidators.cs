using Cronos;
using FluentValidation;

namespace YATsDb.Infrastructure.Validation;

internal static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> MustByCronExpression<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(t => CronExpression.TryParse(t, out _))
            .WithMessage("Expression is not valid cron expression.");
    }
}
