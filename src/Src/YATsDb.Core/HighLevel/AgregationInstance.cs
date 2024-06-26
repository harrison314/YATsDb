namespace YATsDb.Core.HighLevel;

public record struct AggregationInstance(int Index, IAggregationFunction Aggregation);