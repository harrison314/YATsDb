namespace YATsDb.Core.HighLevel;

public interface IAggregationFunction
{
    void Reset();

    void Insert(long timeStampInMs, double value);

    DbValue GetAggregated();
}
