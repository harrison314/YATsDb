using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class OnlineVarianceAggregationFunctionTests
{
    [Fact]
    public void Var_Execute()
    {
        OnlineVarianceAggregationFunction fn = new OnlineVarianceAggregationFunction();

        fn.Reset();
        fn.Insert(100, 12.5);
        fn.Insert(200, 15.6);
        fn.Insert(300, 18.89);
        fn.Insert(400, 13.5);
        fn.Insert(500, 19.478);
        fn.Insert(600, 32.1);
        fn.Insert(700, 12.5);

        Assert.Equal(DbValue.CreateFromDouble(5.452526285714288, false), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());
    }
}
