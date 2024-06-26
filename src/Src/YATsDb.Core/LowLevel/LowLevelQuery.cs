namespace YATsDb.Core.LowLevel;

internal struct LowLevelQuery
{
    public long TimeFrom;
    public long TimeTo;

    public ReadOnlyMemory<char> Tag;
}