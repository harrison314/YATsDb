namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class MaxAggregationFunction : IAggregationFunction
{
    private double value;
    private bool hasValues;

    public MaxAggregationFunction()
    {

    }

    public void Reset()
    {
        this.value = 0.0;
        this.hasValues = false;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value) && (!this.hasValues || this.value < value))
        {
            this.value = value;
            this.hasValues = true;
        }
    }

    public DbValue GetAggregated()
    {
        if (this.hasValues)
        {
            return DbValue.CreateFromDouble(this.value, false);
        }

        return DbValue.CreateFromNull();
    }
}
