using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YATsDb.Core.HighLevel;

namespace YATsDb.Core.Tests.HighLevel;

public class AggregationFunctionFactoryTests
{
    [Fact]
    public void Create_Execute_All()
    {
        foreach(AggregationType type in Enum.GetValues<AggregationType>())
        {
            IAggregationFunction fn = AggregationFunctionFactory.Create(type);
            Assert.NotNull(fn);
        }
    }
}
