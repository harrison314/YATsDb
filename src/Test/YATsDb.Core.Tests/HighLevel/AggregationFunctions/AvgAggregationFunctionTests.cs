﻿using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class AvgAggregationFunctionTests
{
    [Fact]
    public void Avg_Execute()
    {
        AvgAggregationFunction fn = new AvgAggregationFunction();

        fn.Reset();
        fn.Insert(1, 1.0);
        fn.Insert(2, double.NaN);
        fn.Insert(1, 7.0);

        Assert.Equal(DbValue.CreateFromDouble(4.0, false), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());

        fn.Reset();
        fn.Insert(15, 1.0);
        Assert.Equal(DbValue.CreateFromDouble(1.0, false), fn.GetAggregated());
    }
}
