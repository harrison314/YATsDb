using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class CountAggregationFunction : IAggregationFunction
{
    private int count;

    public CountAggregationFunction()
    {

    }

    public void Reset()
    {
        this.count = 0;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            this.count++;
        }
    }

    public DbValue GetAggregated()
    {
        return DbValue.CreateFromLong(this.count);
    }
}
