namespace YATsDb.Core.HighLevel;

public record HighLevelInputDataPoint(ReadOnlyMemory<char> MeasurementName,
    long UnixTimeStampInMs,
    ReadOnlyMemory<double> Values,
    ReadOnlyMemory<char> Tag);