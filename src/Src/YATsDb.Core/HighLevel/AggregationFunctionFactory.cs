using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.HighLevel;

internal static class AggregationFunctionFactory
{
    public static IAggregationFunction Create(AggregationType aggregationType)
    {
        return aggregationType switch
        {
            AggregationType.Avg => new AvgAggregationFunction(),
            AggregationType.Min => new MinAggregationFunction(),
            AggregationType.Max => new MaxAggregationFunction(),
            AggregationType.Count => new CountAggregationFunction(),
            AggregationType.Variance => new OnlineVarianceAggregationFunction(),
            AggregationType.StdDev => new OnlineStdDevAggregationFunction(),
            AggregationType.Sum => new SumAggregationFunction(),
            AggregationType.Identity => new IdentityAggregationFunction(),
            AggregationType.Remedian => new RemedianAggregationFunction(250),
            AggregationType.Sign => new SignAggregationFunction(),
            AggregationType.ChangePerSec => new LinearAggregationFunction(1000),
            AggregationType.ChangePerHour => new LinearAggregationFunction(60 * 60 * 1000),
            AggregationType.ChangePerDay => new LinearAggregationFunction(24 * 60 * 60 * 1000),
            _ => throw new InvalidProgramException($"Enum value {aggregationType} is not supported.")
        };
    }
}
