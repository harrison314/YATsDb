namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class SignAggregationFunction : IAggregationFunction
{
    private double startValue;
    private bool startValueExists;
    private double endValue;

    public SignAggregationFunction()
    {

    }

    public void Reset()
    {
        this.startValue = 0.0;
        this.startValueExists = false;
        this.endValue = 0.0;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            if (!this.startValueExists)
            {
                this.startValue = value;
                this.startValueExists = true;
            }

            this.endValue = value;
        }
    }

    public DbValue GetAggregated()
    {
        if (this.startValueExists)
        {
            int sign = Math.Sign(this.endValue - this.startValue);
            return DbValue.CreateFromLong(sign);
        }

        return DbValue.CreateFromNull();
    }
}
