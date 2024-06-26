namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class LinearAggregationFunction : IAggregationFunction
{
    private double startValue;
    private long startTime;
    private bool startValueExists;
    private double endValue;
    private long endTime;
    private readonly long timeUnitInMs;

    public LinearAggregationFunction(long timeUnitInMs)
    {
        this.timeUnitInMs = timeUnitInMs;
    }

    public void Reset()
    {
        this.startValue = 0.0;
        this.startTime = 0;
        this.startValueExists = false;
        this.endValue = 0.0;
        this.endTime = 0;
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            if (!this.startValueExists)
            {
                this.startValue = value;
                this.startTime = timeStampInMs;
                this.startValueExists = true;
            }

            this.endValue = value;
            this.endTime = timeStampInMs;
        }
    }

    public DbValue GetAggregated()
    {
        if (!this.startValueExists)
        {
            return DbValue.CreateFromNull();
        }

        long deltaT = this.endTime - this.startTime;
        if (deltaT == 0L)
        {
            return DbValue.CreateFromDouble(0.0, false);
        }

        decimal deltaVd = this.timeUnitInMs * (decimal)(this.endValue - this.startValue);
        decimal deltaTv = deltaT;
        double linear = (double)(deltaVd / deltaTv);

        return DbValue.CreateFromDouble(linear, false);
    }
}