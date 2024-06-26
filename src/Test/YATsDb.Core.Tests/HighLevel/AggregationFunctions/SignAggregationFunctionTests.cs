using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class SignAggregationFunctionTests
{
    [Fact]
    public void Sign_Execute()
    {
        SignAggregationFunction fn = new SignAggregationFunction();

        fn.Reset();
        fn.Insert(1, 1.0);
        fn.Insert(2, double.NaN);
        fn.Insert(1, 7.0);

        Assert.Equal(DbValue.CreateFromLong(1), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());

        fn.Reset();
        fn.Insert(15, 1.0);
        Assert.Equal(DbValue.CreateFromLong(0), fn.GetAggregated());

        fn.Reset();
        fn.Insert(1, 17.0);
        fn.Insert(2, double.NaN);
        fn.Insert(1, 7.0);

        Assert.Equal(DbValue.CreateFromLong(-1), fn.GetAggregated());

    }
}
