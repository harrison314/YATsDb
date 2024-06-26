namespace YATsDb.Core.HighLevel.AggregationFunctions;

internal class RemedianAggregationFunction : IAggregationFunction
{
    private readonly List<double> buffer;
    private readonly List<double> medians;
    private readonly int bufferSize;

    public RemedianAggregationFunction(int bufferSize)
    {
        this.bufferSize = bufferSize;
        this.buffer = new List<double>(bufferSize);
        this.medians = new List<double>();
    }

    public void Reset()
    {
        this.buffer.Clear();
        this.medians.Clear();
    }

    public void Insert(long timeStampInMs, double value)
    {
        if (!double.IsNaN(value))
        {
            this.buffer.Add(value);
            if (this.buffer.Count >= this.bufferSize)
            {
                this.medians.Add(this.GetMedian(this.buffer));
                this.buffer.Clear();
            }
        }
    }

    public DbValue GetAggregated()
    {
        if (this.buffer.Count > 0)
        {
            this.medians.Add(this.GetMedian(this.buffer));
            this.buffer.Clear();
        }

        if (this.medians.Count == 0)
        {
            return DbValue.CreateFromNull();
        }

        double median = this.GetMedian(this.medians);
        return DbValue.CreateFromDouble(median, false);
    }

    private double GetMedian(List<double> list)
    {
        list.Sort();
        return list[list.Count / 2];
    }
}
