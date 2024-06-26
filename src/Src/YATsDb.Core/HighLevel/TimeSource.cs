namespace YATsDb.Core.HighLevel;

public struct TimeSource
{
    private enum InternalMode
    {
        None,
        Nil,
        Constant,
        Relative
    }

    private InternalMode mode;
    private long time;

    public TimeSource()
    {
        this.mode = InternalMode.None;
        this.time = 0;
    }

    public static TimeSource CreateNull()
    {
        return new TimeSource()
        {
            mode = InternalMode.Nil
        };
    }

    public static TimeSource CreateConstant(DateTimeOffset dateTime)
    {
        return new TimeSource()
        {
            mode = InternalMode.Constant,
            time = dateTime.ToUnixTimeMilliseconds()
        };
    }

    public static TimeSource CreateRelative(TimeSpan timeFromNow)
    {
        return new TimeSource()
        {
            mode = InternalMode.Relative,
            time = (long)timeFromNow.TotalMilliseconds
        };
    }

    public long? GetTimeValue(TimeProvider timeProvider)
    {
        return this.mode switch
        {
            InternalMode.Nil => null,
            InternalMode.Constant => this.time,
            InternalMode.Relative => timeProvider.GetUtcNow().ToUnixTimeMilliseconds() + this.time,
            InternalMode.None => throw new InvalidOperationException("TimeSource is not initialized"),
            _ => throw new InvalidProgramException($"Enum vale {this.mode} is not supported.")
        };
    }

    public override string ToString()
    {
        return $"TimeSource mode:{this.mode} time: {this.time}";
    }
}
