using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class SumAggregationFunctionTests
{
    [Fact]
    public void Sum_Execute()
    {
        SumAggregationFunction fn = new SumAggregationFunction();

        fn.Reset();
        fn.Insert(1, 1.0);
        fn.Insert(2, double.NaN);
        fn.Insert(1, 7.0);

        Assert.Equal(DbValue.CreateFromDouble(8.0, false), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());

        fn.Reset();
        fn.Insert(15, 1.0);
        Assert.Equal(DbValue.CreateFromDouble(1.0, false), fn.GetAggregated());
    }
}