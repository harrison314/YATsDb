using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class OnlineVarianceAggregationFunction : IAggregationFunction
{
    private int n;
    private double mean;
    private double m2;

    public OnlineVarianceAggregationFunction()
    {

    }

    public void Reset()
    {
        this.n = 0;
        this.mean = 0.0;
        this.m2 = 0.0;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            this.n++;

            double delta = value - this.mean;
            this.mean += delta / this.n;

            double delta2 = value - this.mean;
            this.m2 = delta * delta2;
        }
    }

    public DbValue GetAggregated()
    {
        if (this.n < 2)
        {
            return DbValue.CreateFromNull();
        }

        return DbValue.CreateFromDouble(this.m2 / (this.n - 1), false);
    }
}
