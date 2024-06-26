namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class IdentityAggregationFunction : IAggregationFunction
{
    private double value;
    private bool hasValue;

    public IdentityAggregationFunction()
    {

    }

    public void Reset()
    {
        this.value = 0.0;
        this.hasValue = true;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            this.value = value;
            this.hasValue = true;
        }
    }

    public DbValue GetAggregated()
    {
        if (this.hasValue)
        {
            return DbValue.CreateFromDouble(this.value, false);
        }

        return DbValue.CreateFromNull();
    }
}