using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class CountAggregationFunctionTests
{
    [Fact]
    public void Count_Execute()
    {
        CountAggregationFunction fn = new CountAggregationFunction();
        fn.Reset();
        fn.Insert(1, 1.0);
        fn.Insert(2, double.NaN);
        fn.Insert(1, 7.0);

        Assert.Equal(DbValue.CreateFromLong(2), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromLong(0), fn.GetAggregated());

        fn.Reset();
        fn.Insert(15, 1.0);
        Assert.Equal(DbValue.CreateFromLong(1), fn.GetAggregated());
    }
}
