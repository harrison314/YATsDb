using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class IdentityAggregationFunctionTests
{
    [Fact]
    public void Identity_Execute()
    {
        IdentityAggregationFunction fn = new IdentityAggregationFunction();
        fn.Reset();
        fn.Insert(1, 1.0);

        Assert.Equal(DbValue.CreateFromDouble(1.0, false), fn.GetAggregated());
    }
}
