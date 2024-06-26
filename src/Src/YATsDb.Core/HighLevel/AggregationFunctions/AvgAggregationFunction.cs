namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class AvgAggregationFunction : IAggregationFunction
{
    private double value;
    private int count;

    public AvgAggregationFunction()
    {

    }

    public void Reset()
    {
        this.value = 0;
        this.count = 0;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            this.value += value;
            this.count++;
        }
    }

    public DbValue GetAggregated()
    {
        if (this.count == 0)
        {
            return DbValue.CreateFromNull();
        }

        return DbValue.CreateFromDouble(this.value / this.count, false);
    }
}
