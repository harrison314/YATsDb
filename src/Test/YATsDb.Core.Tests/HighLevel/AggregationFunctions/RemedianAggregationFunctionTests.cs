using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class RemedianAggregationFunctionTests
{
    [Fact]
    public void Remedian_Execute()
    {
        RemedianAggregationFunction fn = new RemedianAggregationFunction(25);

        fn.Reset();

        for (int i = 0; i < 345; i++)
        {
            fn.Insert(i + 100, i + 1);
        }

        Assert.Equal(DbValue.CreateFromDouble(188.0, false), fn.GetAggregated());
    }

    [Fact]
    public void Remedian_Execute_Null()
    {
        RemedianAggregationFunction fn = new RemedianAggregationFunction(25);

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());
    }

    [Fact]
    public void Remedian_Execute_LowCount()
    {
        RemedianAggregationFunction fn = new RemedianAggregationFunction(25);

        fn.Reset();

        for (int i = 0; i < 12; i++)
        {
            fn.Insert(i + 100, i + 1);
        }

        Assert.Equal(DbValue.CreateFromDouble(7.0, false), fn.GetAggregated());
    }
}
