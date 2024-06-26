

using System.Diagnostics.CodeAnalysis;

namespace YATsDb.Lite.Infrastructure.Validation;

public interface ISimpleValidatedObject
{
    bool IsValid([NotNullWhen(true)] out IDictionary<string, string[]>? errors);
}