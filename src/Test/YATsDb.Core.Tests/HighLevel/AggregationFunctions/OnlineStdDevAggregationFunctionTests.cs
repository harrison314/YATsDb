using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class OnlineStdDevAggregationFunctionTests
{
    [Fact]
    public void StdDev_Execute()
    {
        OnlineStdDevAggregationFunction fn = new OnlineStdDevAggregationFunction();

        fn.Reset();
        fn.Insert(100, 12.5);
        fn.Insert(200, 15.6);
        fn.Insert(300, 18.89);
        fn.Insert(400, 13.5);
        fn.Insert(500, 19.478);
        fn.Insert(600, 32.1);
        fn.Insert(700, 12.5);

        Assert.Equal(DbValue.CreateFromDouble(2.3350645142510063, false), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());
    }
}
