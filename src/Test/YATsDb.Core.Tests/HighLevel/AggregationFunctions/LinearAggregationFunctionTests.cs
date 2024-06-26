using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel;
using YATsDb.Core.HighLevel.AggregationFunctions;

namespace YATsDb.Core.Tests.HighLevel.AggregationFunctions;

public class LinearAggregationFunctionTests
{
    [Fact]
    public void Linear_Execute()
    {
        LinearAggregationFunction fn = new LinearAggregationFunction(1000);

        fn.Reset();
        fn.Insert(100, 156.0);
        fn.Insert(153, 187.0);
        fn.Insert(200, double.NaN);
        fn.Insert(300, 236.0);

        Assert.Equal(DbValue.CreateFromDouble(400.0, false), fn.GetAggregated());

        fn.Reset();
        Assert.Equal(DbValue.CreateFromNull(), fn.GetAggregated());

        fn.Reset();
        fn.Insert(300, 236.0);
        Assert.Equal(DbValue.CreateFromDouble(0.0, false), fn.GetAggregated());
    }
}
