namespace YATsDb.Core.Utils;

internal class MemStringEqualityComparer : IEqualityComparer<ReadOnlyMemory<char>>
{
    private readonly StringComparison comparison;

    public MemStringEqualityComparer(bool ignoreCase = false)
    {
        this.comparison = ignoreCase
             ? StringComparison.OrdinalIgnoreCase
             : StringComparison.Ordinal;
    }

    public int GetHashCode(ReadOnlyMemory<char> obj)
    {
        return string.GetHashCode(obj.Span, this.comparison);
    }

    public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y)
    {
        return MemoryExtensions.Equals(x.Span, y.Span, this.comparison);
    }
}