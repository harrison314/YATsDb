using System.Text.RegularExpressions;

namespace YATsDb.Lite.Endpoints.Common;

internal static partial class RegexHolder
{
    [GeneratedRegex("^[A-Za-z0-9_-]+$", RegexOptions.Multiline| RegexOptions.Singleline, 1000)]
    public static partial Regex GetIdentfierRegex();
}
