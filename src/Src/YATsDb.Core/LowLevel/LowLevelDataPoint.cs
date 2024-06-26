namespace YATsDb.Core.LowLevel;

public record LowLevelDataPoint(long UnixTimestampInMs, double[] Values, string? Tag);